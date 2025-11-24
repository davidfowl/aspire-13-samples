import styles from './ChatMessage.module.css'

interface ChatMessageProps {
  role: 'user' | 'assistant'
  content: string
}

export default function ChatMessage({ role, content }: ChatMessageProps) {
  return (
    <div className={`${styles.message} ${styles[role]}`}>
      <div className={styles.role}>
        {role === 'user' ? 'You' : 'GPT-4'}
      </div>
      <div className={styles.content}>{content}</div>
    </div>
  )
}
