import type { Metadata } from 'next'
import './globals.css'

export const metadata: Metadata = {
  title: 'OpenAI Chat',
  description: 'Real-time chat with OpenAI GPT-4 using Server-Sent Events',
}

export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <html lang="en">
      <body>{children}</body>
    </html>
  )
}
