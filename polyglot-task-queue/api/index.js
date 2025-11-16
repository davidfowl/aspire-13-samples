import express from 'express';
import amqp from 'amqplib';
import { fileURLToPath } from 'url';
import { dirname, join } from 'path';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

const app = express();
const port = process.env.PORT || 3000;

app.use(express.json());
app.use(express.static(join(__dirname, 'public')));

// ============================================================================
// STATE MANAGEMENT
// ============================================================================
// RabbitMQ is the source of truth. The Map is an in-memory read cache.
// Background consumers continuously update the Map from RabbitMQ messages.
// HTTP requests ONLY read from the Map - they NEVER query RabbitMQ directly.
// ============================================================================
const tasks = new Map();

// RabbitMQ connections
let consumerChannel = null;  // Background consumers reading from queues
let publisherChannel = null; // Publishing messages (from HTTP requests)
const rabbitmqUrl = process.env.MESSAGING_URI;
const TASKS_QUEUE = 'tasks';
const RESULTS_QUEUE = 'results';
const TASK_STATUS_QUEUE = 'task_status';

// ============================================================================
// RABBITMQ CONNECTION & BACKGROUND MESSAGE CONSUMERS
// ============================================================================
// These consumers run continuously in the background, updating the tasks Map
// as messages arrive. HTTP endpoints never touch RabbitMQ - they only read
// from the Map that these background consumers keep up to date.
// ============================================================================

async function connectRabbitMQ() {
    try {
        console.log('🔌 Connecting to RabbitMQ...');
        const connection = await amqp.connect(rabbitmqUrl);

        // Create two channels for different purposes
        consumerChannel = await connection.createChannel();  // For background consumption
        publisherChannel = await connection.createChannel(); // For publishing from HTTP handlers

        // Declare queues on both channels
        await consumerChannel.assertQueue(TASKS_QUEUE, { durable: true });
        await consumerChannel.assertQueue(RESULTS_QUEUE, { durable: true });
        await consumerChannel.assertQueue(TASK_STATUS_QUEUE, { durable: true });

        await publisherChannel.assertQueue(TASKS_QUEUE, { durable: true });
        await publisherChannel.assertQueue(RESULTS_QUEUE, { durable: true });
        await publisherChannel.assertQueue(TASK_STATUS_QUEUE, { durable: true });

        console.log('✓ Connected to RabbitMQ (consumer + publisher channels)');

        // Start background consumers - these run continuously
        console.log('🔄 Starting background message consumers...');
        await startBackgroundConsumers();
        console.log('✓ Background consumers running');

        await restoreTaskState();
    } catch (error) {
        console.error('✗ RabbitMQ connection error:', error);
        // Retry connection after 5 seconds
        setTimeout(connectRabbitMQ, 5000);
    }
}

async function startBackgroundConsumers() {
    // These consumers run continuously in the background
    // They update the tasks Map as messages arrive from RabbitMQ
    await consumeTaskStatus();
    await consumeResults();
}

async function publishTaskStatus(taskId, status, additionalData = {}) {
    if (!publisherChannel) return;

    const statusMessage = {
        taskId,
        status,
        timestamp: new Date().toISOString(),
        ...additionalData
    };

    try {
        publisherChannel.sendToQueue(TASK_STATUS_QUEUE, Buffer.from(JSON.stringify(statusMessage)), {
            persistent: true
        });
        console.log(`📊 Published status update for task ${taskId}: ${status}`);
    } catch (error) {
        console.error(`Error publishing status for task ${taskId}:`, error);
    }
}

async function consumeTaskStatus() {
    console.log('📥 Background consumer: Listening for task status updates...');

    await consumerChannel.consume(TASK_STATUS_QUEUE, (msg) => {
        if (msg) {
            try {
                const statusUpdate = JSON.parse(msg.content.toString());
                console.log(`📊 [Background] Received status update for task ${statusUpdate.taskId}: ${statusUpdate.status}`);

                // Get or create task in cache
                let task = tasks.get(statusUpdate.taskId);
                if (!task) {
                    // Create task from status update (source of truth is RabbitMQ)
                    task = {
                        id: statusUpdate.taskId,
                        type: statusUpdate.type || 'unknown',
                        data: statusUpdate.data || '',
                        status: statusUpdate.status,
                        createdAt: statusUpdate.createdAt || statusUpdate.timestamp || new Date().toISOString()
                    };
                    tasks.set(statusUpdate.taskId, task);
                    console.log(`📝 [Background] Created new task in cache: ${task.id}`);
                } else {
                    // Update existing task
                    task.status = statusUpdate.status;

                    // Update worker if provided
                    if (statusUpdate.worker) {
                        task.worker = statusUpdate.worker;
                    }

                    // Handle error status
                    if (statusUpdate.status === 'error' && statusUpdate.error) {
                        task.error = statusUpdate.error;
                    }
                    console.log(`🔄 [Background] Updated task in cache: ${task.id} -> ${task.status}`);
                }

                consumerChannel.ack(msg);
            } catch (error) {
                console.error('[Background] Error processing status update:', error);
                consumerChannel.nack(msg, false, false); // Don't requeue malformed messages
            }
        }
    });
}

async function restoreTaskState() {
    console.log('Task state will be built from RabbitMQ messages...');
    // The cache is ephemeral and rebuilt from consuming messages
    // Status updates and results from RabbitMQ will populate the cache
    // For persistent storage, you'd typically use a database alongside RabbitMQ
}

async function consumeResults() {
    console.log('📥 Background consumer: Listening for task results...');

    await consumerChannel.consume(RESULTS_QUEUE, (msg) => {
        if (msg) {
            try {
                const result = JSON.parse(msg.content.toString());
                console.log(`✅ [Background] Received result for task ${result.taskId}`);

                // Update task in cache - RabbitMQ is the source of truth
                let task = tasks.get(result.taskId);
                if (!task) {
                    // Create task from result if it doesn't exist
                    task = {
                        id: result.taskId,
                        type: 'unknown',
                        data: '',
                        status: 'completed',
                        createdAt: new Date().toISOString()
                    };
                    tasks.set(result.taskId, task);
                    console.log(`📝 [Background] Created completed task in cache: ${task.id}`);
                } else {
                    task.status = 'completed';
                }

                task.result = result.result;
                task.completedAt = result.completedAt || new Date().toISOString();
                task.worker = result.worker;

                console.log(`✅ [Background] Task completed: ${task.id} by ${task.worker}`);

                consumerChannel.ack(msg);
            } catch (error) {
                console.error('[Background] Error processing result:', error);
                consumerChannel.nack(msg, false, false);
            }
        }
    });
}

// ============================================================================
// HTTP ENDPOINTS
// ============================================================================
// These endpoints ONLY read from the tasks Map (or publish to RabbitMQ).
// They NEVER query RabbitMQ directly for data.
// The Map is kept up to date by background consumers.
// ============================================================================

// Root endpoint
app.get('/', (req, res) => {
    res.json({
        message: 'Task Queue API',
        endpoints: ['/tasks', '/tasks/:id', '/health']
    });
});

// Health check - only reads from cache
app.get('/health', async (req, res) => {
    try {
        if (!consumerChannel || !publisherChannel) {
            return res.status(503).json({
                status: 'unhealthy',
                rabbitmq: 'disconnected'
            });
        }

        return res.json({
            status: 'healthy',
            rabbitmq: 'connected',
            cachedTasks: tasks.size  // Reading from cache, not RabbitMQ
        });
    } catch (error) {
        res.status(503).json({
            status: 'unhealthy',
            error: error.message
        });
    }
});

// Submit a new task - publishes to RabbitMQ but doesn't touch the cache
app.post('/tasks', async (req, res) => {
    try {
        const { type, data } = req.body;

        if (!type || !data) {
            return res.status(400).json({ error: 'type and data are required' });
        }

        const taskId = `task-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
        const createdAt = new Date().toISOString();

        // IMPORTANT: Don't add to cache here!
        // The background consumer will add it when it consumes the status message

        // 1. Publish status to RabbitMQ (source of truth)
        await publishTaskStatus(taskId, 'queued', {
            type,
            data,
            createdAt
        });

        // 2. Publish task to workers queue
        const message = JSON.stringify({
            taskId,
            type,
            data
        });

        publisherChannel.sendToQueue(TASKS_QUEUE, Buffer.from(message), {
            persistent: true
        });

        console.log(`✓ [HTTP] Task ${taskId} published to queues (type: ${type})`);

        // Return task details immediately for UX
        // Background consumer will add this to cache when it processes the status message
        res.status(201).json({
            id: taskId,
            type,
            data,
            status: 'queued',
            createdAt
        });
    } catch (error) {
        console.error('Error creating task:', error);
        res.status(500).json({ error: error.message });
    }
});

// Get all tasks - reads ONLY from cache (Map), never queries RabbitMQ
app.get('/tasks', (req, res) => {
    const allTasks = Array.from(tasks.values()).sort((a, b) =>
        new Date(b.createdAt) - new Date(a.createdAt)
    );
    res.json(allTasks);
});

// Get task by ID - reads ONLY from cache (Map), never queries RabbitMQ
app.get('/tasks/:id', (req, res) => {
    const task = tasks.get(req.params.id);
    if (!task) {
        return res.status(404).json({ error: 'Task not found' });
    }
    res.json(task);
});

// Clear all tasks - only clears cache (for demo purposes)
// Note: Tasks in RabbitMQ queues are not affected
app.delete('/tasks', (req, res) => {
    const count = tasks.size;
    tasks.clear();
    res.json({ message: `Cleared ${count} tasks from cache` });
});

// Connect to RabbitMQ before starting server
connectRabbitMQ();

app.listen(port, () => {
    console.log(`✓ API listening on port ${port}`);
});
