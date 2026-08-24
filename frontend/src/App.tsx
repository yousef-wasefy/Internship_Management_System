import { Route, Routes } from 'react-router-dom'
import { Navbar } from './components/Navbar'
import { InternshipDetailPage } from './pages/InternshipDetailPage'
import { InternshipListPage } from './pages/InternshipListPage'
import { LoginPage } from './pages/LoginPage'
import { NotFoundPage } from './pages/NotFoundPage'
import { RegisterPage } from './pages/RegisterPage'

function App() {
  return (
    <>
      <Navbar />
      <main>
        <Routes>
          <Route path="/" element={<InternshipListPage />} />
          <Route path="/internships/:id" element={<InternshipDetailPage />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="*" element={<NotFoundPage />} />
        </Routes>
      </main>
    </>
  )
}

export default App
