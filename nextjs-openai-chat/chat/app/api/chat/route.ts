import OpenAI from 'openai'
import { NextRequest } from 'next/server'

export const runtime = 'nodejs'

// Lazy initialization - OpenAI client created on first request, not at build time
let openai: OpenAI | null = null
function getOpenAI() {
  if (!openai) {
    openai = new OpenAI({
      apiKey: process.env.OPENAI_API_KEY,
    })
  }
  return openai
}

export async function POST(req: NextRequest) {
  try {
    const { messages } = await req.json()

    if (!messages || !Array.isArray(messages)) {
      return new Response('Invalid request body', { status: 400 })
    }

    // Create a streaming response using OpenAI's streaming API
    const stream = await getOpenAI().chat.completions.create({
      model: 'gpt-4',
      messages: messages,
      stream: true,
    })

    // Convert OpenAI stream to ReadableStream for SSE
    const encoder = new TextEncoder()
    const readable = new ReadableStream({
      async start(controller) {
        try {
          for await (const chunk of stream) {
            const content = chunk.choices[0]?.delta?.content
            if (content) {
              // Send as Server-Sent Event format
              const data = `data: ${JSON.stringify({ content })}\n\n`
              controller.enqueue(encoder.encode(data))
            }
          }
          // Send done event
          controller.enqueue(encoder.encode('data: [DONE]\n\n'))
          controller.close()
        } catch (error) {
          controller.error(error)
        }
      },
    })

    return new Response(readable, {
      headers: {
        'Content-Type': 'text/event-stream',
        'Cache-Control': 'no-cache, no-transform',
        Connection: 'keep-alive',
      },
    })
  } catch (error) {
    console.error('Error in chat API:', error)
    return new Response('Internal server error', { status: 500 })
  }
}
