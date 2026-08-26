import { Route, Routes } from 'react-router-dom'
import { Navbar } from './components/Navbar'
import { ProtectedRoute } from './components/ProtectedRoute'
import { AdminDashboardPage } from './pages/admin/AdminDashboardPage'
import { ApplicantsPage } from './pages/company/ApplicantsPage'
import { CompanyDashboardPage } from './pages/company/CompanyDashboardPage'
import { InternshipFormPage } from './pages/company/InternshipFormPage'
import { DashboardRedirectPage } from './pages/DashboardRedirectPage'
import { InternshipDetailPage } from './pages/InternshipDetailPage'
import { InternshipListPage } from './pages/InternshipListPage'
import { LoginPage } from './pages/LoginPage'
import { NotFoundPage } from './pages/NotFoundPage'
import { RegisterPage } from './pages/RegisterPage'
import { StudentDashboardPage } from './pages/student/StudentDashboardPage'

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

          <Route
            path="/dashboard"
            element={
              <ProtectedRoute>
                <DashboardRedirectPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/student/dashboard"
            element={
              <ProtectedRoute role="Student">
                <StudentDashboardPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/company/dashboard"
            element={
              <ProtectedRoute role="Company">
                <CompanyDashboardPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/company/internships/new"
            element={
              <ProtectedRoute role="Company">
                <InternshipFormPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/company/internships/:id/edit"
            element={
              <ProtectedRoute role="Company">
                <InternshipFormPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/company/internships/:id/applicants"
            element={
              <ProtectedRoute role="Company">
                <ApplicantsPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/admin/dashboard"
            element={
              <ProtectedRoute role="Admin">
                <AdminDashboardPage />
              </ProtectedRoute>
            }
          />

          <Route path="*" element={<NotFoundPage />} />
        </Routes>
      </main>
    </>
  )
}

export default App
