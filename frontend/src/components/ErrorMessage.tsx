interface ErrorMessageProps {
  message: string | null
  fieldErrors?: Record<string, string[]>
}

export function ErrorMessage({ message, fieldErrors }: ErrorMessageProps) {
  if (!message) return null

  return (
    <div className="error-box" role="alert">
      <p>{message}</p>
      {fieldErrors && (
        <ul>
          {Object.entries(fieldErrors).map(([field, messages]) =>
            messages.map((msg) => <li key={`${field}-${msg}`}>{msg}</li>),
          )}
        </ul>
      )}
    </div>
  )
}
