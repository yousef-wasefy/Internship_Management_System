import { Link } from 'react-router-dom'

export function NotFoundPage() {
  return (
    <div className="page">
      <h1>Page not found</h1>
      <p>
        <Link to="/">Back to internship listings</Link>
      </p>
    </div>
  )
}
