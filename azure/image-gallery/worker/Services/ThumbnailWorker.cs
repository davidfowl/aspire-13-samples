using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Text.Json;
using Worker.Data;

namespace Worker.Services;

public class ThumbnailWorker(
    IServiceProvider serviceProvider,
    BlobContainerClient containerClient,
    QueueServiceClient queueService,
    IHostApplicationLifetime hostApplicationLifetime,
    ILogger<ThumbnailWorker> logger) : BackgroundService
{

    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly BlobContainerClient _containerClient = containerClient;
    private readonly QueueServiceClient _queueService = queueService;
    private readonly IHostApplicationLifetime _hostApplicationLifetime = hostApplicationLifetime;
    private readonly ILogger<ThumbnailWorker> _logger = logger;
    private const int ThumbnailWidth = 300;
    private const int ThumbnailHeight = 300;
    private const long MaxImageSizeBytes = 20 * 1024 * 1024; // 20 MB - slightly larger than upload limit
    private const int MaxRetryCount = 3;
    private const int MaxEmptyPolls = 6;        // Poll up to 6 times
    private const int EmptyPollWaitSeconds = 20; // Wait 20 seconds between polls (total: 2 minutes)
    private const int GracePeriodSeconds = 30;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var startTime = DateTime.UtcNow;
        var processedCount = 0;

        try
        {
            var maxRunDuration = TimeSpan.FromMinutes(5);
            var gracePeriod = TimeSpan.FromSeconds(GracePeriodSeconds);

            _logger.LogInformation("Thumbnail worker started, will run for max {Duration} minutes", maxRunDuration.TotalMinutes);

            var queueClient = _queueService.GetQueueClient("thumbnails");
            await queueClient.CreateIfNotExistsAsync(cancellationToken: stoppingToken);

            var emptyPollCount = 0;

            while (!stoppingToken.IsCancellationRequested)
        {
            // Check if we've exceeded max run duration
            var elapsed = DateTime.UtcNow - startTime;
            if (elapsed >= maxRunDuration)
            {
                _logger.LogInformation("Max run duration reached ({Duration} minutes), exiting. Processed {Count} messages",
                    maxRunDuration.TotalMinutes, processedCount);
                break;
            }

            try
            {
                // Receive messages from queue
                QueueMessage[] messages = await queueClient.ReceiveMessagesAsync(
                    maxMessages: 10,
                    visibilityTimeout: TimeSpan.FromMinutes(5),
                    cancellationToken: stoppingToken);

                if (messages.Length == 0)
                {
                    emptyPollCount++;

                    if (emptyPollCount >= MaxEmptyPolls)
                    {
                        // No messages after multiple attempts, exit to save costs
                        _logger.LogInformation("Queue empty after {PollCount} attempts, exiting. Processed {Count} messages in {Elapsed}",
                            emptyPollCount, processedCount, elapsed);
                        break;
                    }

                    // Wait briefly in case new messages arrive
                    _logger.LogInformation("Queue empty, waiting {WaitSeconds}s before retry ({PollCount}/{MaxPolls})",
                        EmptyPollWaitSeconds, emptyPollCount, MaxEmptyPolls);
                    await Task.Delay(TimeSpan.FromSeconds(EmptyPollWaitSeconds), stoppingToken);
                    continue;
                }

                // Reset empty counter when work is found
                emptyPollCount = 0;
                _logger.LogInformation("Found {Count} messages to process", messages.Length);

                foreach (var message in messages)
                {
                    // Check if we have enough time to process another message
                    var currentElapsed = DateTime.UtcNow - startTime;
                    var estimatedTimeRemaining = maxRunDuration - currentElapsed;

                    if (estimatedTimeRemaining < gracePeriod)
                    {
                        _logger.LogInformation("Approaching timeout ({RemainingSeconds}s left), finishing current batch gracefully",
                            estimatedTimeRemaining.TotalSeconds);
                        break;
                    }

                    try
                    {
                        await ProcessMessageAsync(message, queueClient, stoppingToken);
                        processedCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to process message: {MessageId}", message.MessageId);

                        // Check retry count and move to dead letter if exceeded
                        if (message.DequeueCount >= MaxRetryCount)
                        {
                            _logger.LogWarning("Message {MessageId} exceeded max retry count ({MaxRetryCount}), deleting",
                                message.MessageId, MaxRetryCount);
                            await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, stoppingToken);
                        }
                        // Message will become visible again after visibility timeout
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in thumbnail worker main loop");
                // Don't retry on error, just exit to let the next scheduled run handle it
                break;
            }
        }

            _logger.LogInformation("Thumbnail worker stopped. Total processed: {Count}, Duration: {Elapsed}",
                processedCount, DateTime.UtcNow - startTime);
        }
        finally
        {
            // Always signal application shutdown, even if exceptions occurred
            _logger.LogInformation("Shutting down worker application");
            _hostApplicationLifetime.StopApplication();
        }
    }

    private async Task ProcessMessageAsync(
        QueueMessage message,
        QueueClient queueClient,
        CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;

        // Parse message
        var data = JsonSerializer.Deserialize<JsonElement>(message.MessageText);
        var imageId = data.GetProperty("imageId").GetInt32();
        var blobName = data.GetProperty("blobName").GetString()!;

        _logger.LogInformation("Processing thumbnail for image {ImageId}, blob: {BlobName}", imageId, blobName);

        var sourceBlobClient = _containerClient.GetBlobClient(blobName);

        // Check blob size before downloading
        var properties = await sourceBlobClient.GetPropertiesAsync(cancellationToken: cancellationToken);
        if (properties.Value.ContentLength > MaxImageSizeBytes)
        {
            _logger.LogWarning("Image {ImageId} exceeds max size ({Size} bytes), skipping thumbnail generation",
                imageId, properties.Value.ContentLength);
            throw new InvalidOperationException($"Image size {properties.Value.ContentLength} exceeds maximum allowed {MaxImageSizeBytes}");
        }

        // Download original image
        using var originalStream = new MemoryStream();
        await sourceBlobClient.DownloadToAsync(originalStream, cancellationToken);
        originalStream.Position = 0;

        // Generate thumbnail
        using var image = await Image.LoadAsync(originalStream, cancellationToken);
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(ThumbnailWidth, ThumbnailHeight),
            Mode = ResizeMode.Max
        }));

        // Upload thumbnail
        var thumbnailName = $"thumb-{blobName}";
        var thumbnailBlobClient = _containerClient.GetBlobClient(thumbnailName);

        using var thumbnailStream = new MemoryStream();
        await image.SaveAsJpegAsync(thumbnailStream, cancellationToken);
        thumbnailStream.Position = 0;
        await thumbnailBlobClient.UploadAsync(thumbnailStream, overwrite: true, cancellationToken: cancellationToken);

        // Update database
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ImageDbContext>();
        var imageRecord = await db.Images.FindAsync([imageId], cancellationToken: cancellationToken);

        if (imageRecord != null)
        {
            imageRecord.ThumbnailUrl = thumbnailBlobClient.Uri.ToString();
            imageRecord.ThumbnailProcessed = true;
            await db.SaveChangesAsync(cancellationToken);
        }

        // Delete message from queue
        await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, cancellationToken);

        var processingTime = DateTime.UtcNow - startTime;
        _logger.LogInformation(
            "Thumbnail generated for image {ImageId} in {ProcessingTime}ms",
            imageId,
            processingTime.TotalMilliseconds);
    }
}
