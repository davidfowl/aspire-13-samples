using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

var builder = Host.CreateApplicationBuilder(args);

builder.AddRabbitMQClient("messaging");

builder.Services.AddHostedService<ReportWorker>();

var host = builder.Build();
host.Run();

class ReportWorker(IConnection connection, ILogger<ReportWorker> logger) : BackgroundService
{
    private const string TasksQueue = "tasks";
    private const string ResultsQueue = "results";
    private const string TaskStatusQueue = "task_status";
    private const string WorkerName = "csharp-worker";

    // Use JsonSerializerDefaults.Web for consistent camelCase JSON serialization
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private async Task PublishTaskStatusAsync(IChannel channel, string taskId, string status, object? additionalData = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var statusMessage = new
            {
                TaskId = taskId,
                Status = status,
                Worker = WorkerName,
                Timestamp = DateTime.Now,
                AdditionalData = additionalData
            };

            var statusJson = JsonSerializer.Serialize(statusMessage, JsonOptions);
            var statusBody = Encoding.UTF8.GetBytes(statusJson);

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: TaskStatusQueue,
                mandatory: false,
                body: statusBody,
                cancellationToken: cancellationToken);

            logger.LogInformation("[{Time}] Status update published: {TaskId} -> {Status}", DateTime.Now, taskId, status);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[{Time}] Error publishing status for task {TaskId}", DateTime.Now, taskId);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("[{Time}] C# worker starting...", DateTime.Now);

        var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        // Declare queues
        await channel.QueueDeclareAsync(
            queue: TasksQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: ResultsQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: TaskStatusQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        // Set prefetch count
        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var messageBody = Encoding.UTF8.GetString(ea.Body.Span);
                logger.LogInformation("[{Time}] Received message: {Message}", DateTime.Now, messageBody);

                var task = JsonSerializer.Deserialize<TaskMessage>(messageBody, JsonOptions);

                if (task is null)
                {
                    logger.LogWarning("[{Time}] Received invalid task message", DateTime.Now);
                    await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                    return;
                }

                logger.LogInformation("[{Time}] Processing task {TaskId} (type: {Type})",
                    DateTime.Now, task.TaskId, task.Type);

                // Publish processing status
                await PublishTaskStatusAsync(channel, task.TaskId!, "processing", cancellationToken: stoppingToken);

                // Only process 'report' tasks
                if (task.Type != "report")
                {
                    logger.LogInformation("[{Time}] Skipping task {TaskId} - not a report task",
                        DateTime.Now, task.TaskId);
                    
                    await PublishTaskStatusAsync(channel, task.TaskId!, "skipped", 
                        new { reason = "not a report task" }, stoppingToken);
                    
                    await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                    return;
                }

                // Process the task
                var result = await ProcessReportTask(task);

                // Send result back
                var resultMessage = new ResultMessage
                {
                    TaskId = task.TaskId,
                    Worker = WorkerName,
                    Result = result,
                    CompletedAt = DateTime.Now
                };

                var resultJson = JsonSerializer.Serialize(resultMessage, JsonOptions);
                var resultBody = Encoding.UTF8.GetBytes(resultJson);

                await channel.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: ResultsQueue,
                    mandatory: false,
                    body: resultBody,
                    cancellationToken: stoppingToken);

                logger.LogInformation("[{Time}] Completed task {TaskId}", DateTime.Now, task.TaskId);

                await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[{Time}] Error processing message", DateTime.Now);
                
                // Try to publish error status if we have task info
                try
                {
                    var messageBody = Encoding.UTF8.GetString(ea.Body.Span);
                    var task = JsonSerializer.Deserialize<TaskMessage>(messageBody, JsonOptions);
                    if (task?.TaskId != null)
                    {
                        await PublishTaskStatusAsync(channel, task.TaskId, "error", 
                            new { error = ex.Message }, stoppingToken);
                    }
                }
                catch
                {
                    // Ignore errors in error handling
                }
                
                await channel.BasicNackAsync(ea.DeliveryTag, false, true, stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(
            queue: TasksQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        logger.LogInformation("[{Time}] C# worker started. Waiting for tasks...", DateTime.Now);

        // Keep the worker running
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task<object> ProcessReportTask(TaskMessage task)
    {
        await Task.Delay(100); // Simulate processing time

        try
        {
            var data = task.Data ?? string.Empty;
            var lines = data.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            // Generate a structured report
            var report = new
            {
                Title = $"Report generated at {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                Summary = new
                {
                    TotalLines = lines.Length,
                    TotalCharacters = data.Length,
                    AverageLineLength = lines.Length > 0 ? data.Length / lines.Length : 0,
                    ProcessedBy = WorkerName,
                    ProcessedAt = DateTime.Now
                },
                Content = new
                {
                    Lines = lines.Take(10).Select((line, index) => new
                    {
                        LineNumber = index + 1,
                        Content = line,
                        Length = line.Length,
                        WordCount = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length
                    }).ToArray()
                },
                Statistics = new
                {
                    ShortestLine = lines.Any() ? lines.MinBy(l => l.Length)?.Length ?? 0 : 0,
                    LongestLine = lines.Any() ? lines.MaxBy(l => l.Length)?.Length ?? 0 : 0,
                    TotalWords = lines.Sum(l => l.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length)
                },
                Metadata = new
                {
                    Format = "Structured Report",
                    Version = "1.0",
                    Generator = "C# Report Worker",
                    Timestamp = DateTime.Now
                }
            };

            return report;
        }
        catch (Exception ex)
        {
            return new
            {
                Error = ex.Message,
                ErrorType = ex.GetType().Name
            };
        }
    }
}

record TaskMessage
{
    public string? TaskId { get; init; }
    public string? Type { get; init; }
    public string? Data { get; init; }
}

record ResultMessage
{
    public string? TaskId { get; init; }
    public string? Worker { get; init; }
    public object? Result { get; init; }
    public DateTime CompletedAt { get; init; }
}
