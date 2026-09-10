import {
  Navigate,
  Route,
  Routes,
} from 'react-router-dom'
import { ProtectedRoute }
  from '../auth/ProtectedRoute'
import { AppLayout }
  from '../layouts/AppLayout'
import { LoadsPage }
  from '../pages/LoadsPage'
import { LoginPage }
  from '../pages/LoginPage'

import { LoadDetailPage }
  from '../pages/LoadDetailPage'
import { UploadLoadPage }
  from '../pages/UploadLoadPage'

export function AppRouter() {
  return (
    <Routes>
      <Route
        path="/login"
        element={<LoginPage />}
      />

      <Route
        element={
          <ProtectedRoute />
        }
      >
        <Route
          element={
            <AppLayout />
          }
        >
          <Route
            index
            element={
                <LoadsPage />
            }
            />

            <Route
            path="loads/new"
            element={
                <UploadLoadPage />
            }
            />

            <Route
            path="loads/:loadId"
            element={
                <LoadDetailPage />
            }
            />
        </Route>
      </Route>

      <Route
        path="*"
        element={
          <Navigate
            to="/"
            replace
          />
        }
      />
    </Routes>
  )
}