'use client'

import { useState, useRef, useEffect } from 'react'
import ChatMessage from '@/components/ChatMessage'
import styles from './page.module.css'

interface Message {
  role: 'user' | 'assistant'
  content: string
}

export default function Home() {
  const [messages, setMessages] = useState<Message[]>([])
  const [input, setInput] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const messagesEndRef = useRef<HTMLDivElement>(null)

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' })
  }

  useEffect(() => {
    scrollToBottom()
  }, [messages])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!input.trim() || isLoading) return

    const userMessage: Message = { role: 'user', content: input }
    const newMessages = [...messages, userMessage]
    setMessages(newMessages)
    setInput('')
    setIsLoading(true)

    try {
      const response = await fetch('/api/chat', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ messages: newMessages }),
      })

      if (!response.ok) {
        throw new Error('Failed to get response')
      }

      const reader = response.body?.getReader()
      const decoder = new TextDecoder()
      let assistantMessage = ''

      if (reader) {
        while (true) {
          const { done, value } = await reader.read()
          if (done) break

          const chunk = decoder.decode(value)
          const lines = chunk.split('\n')

          for (const line of lines) {
            if (line.startsWith('data: ')) {
              const data = line.slice(6)
              if (data === '[DONE]') {
                break
              }
              try {
                const parsed = JSON.parse(data)
                if (parsed.content) {
                  assistantMessage += parsed.content
                  setMessages([...newMessages, { role: 'assistant', content: assistantMessage }])
                }
              } catch (e) {
                // Ignore parse errors
              }
            }
          }
        }
      }
    } catch (error) {
      console.error('Error:', error)
      setMessages([...newMessages, { role: 'assistant', content: 'Sorry, there was an error processing your request.' }])
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <main className={styles.main}>
      <div className={styles.container}>
        <h1 className={styles.title}>OpenAI Chat</h1>
        <div className={styles.chatContainer}>
          <div className={styles.messages}>
            {messages.length === 0 && (
              <div className={styles.emptyState}>
                <h2>Start a conversation</h2>
                <p>Ask me anything and I'll respond in real-time!</p>
              </div>
            )}
            {messages.map((message, index) => (
              <ChatMessage key={index} role={message.role} content={message.content} />
            ))}
            {isLoading && messages[messages.length - 1]?.role === 'user' && (
              <div className={styles.typing}>GPT-4 is typing...</div>
            )}
            <div ref={messagesEndRef} />
          </div>
          <form onSubmit={handleSubmit} className={styles.form}>
            <input
              type="text"
              value={input}
              onChange={(e) => setInput(e.target.value)}
              placeholder="Type your message..."
              className={styles.input}
              disabled={isLoading}
            />
            <button type="submit" disabled={isLoading || !input.trim()} className={styles.button}>
              Send
            </button>
          </form>
        </div>
      </div>
    </main>
  )
}
