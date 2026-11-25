import { createBrowserRouter, RouterProvider } from 'react-router'
import { RedirectIfAuthenticated, RequireAdministrator, RequireAuth } from './auth/guards'
import { CategoriesPage } from './features/categories/CategoriesPage'
import { CategoryFormPage } from './features/categories/CategoryFormPage'
import { AppLayout } from './layout/AppLayout'
import { ChangePasswordPage } from './pages/ChangePasswordPage'
import { HomePage } from './pages/HomePage'
import { LoginPage } from './pages/LoginPage'
import { ForbiddenPage, NotFoundPage } from './pages/StatusPages'

/**
 * Rutas (RF-AUT-04): todo exige sesión salvo el inicio de sesión; las de gestión exigen además el
 * rol Administrador. La autoridad es siempre el backend.
 */
const router = createBrowserRouter([
  {
    element: <RedirectIfAuthenticated />,
    children: [{ path: '/login', element: <LoginPage /> }],
  },
  {
    element: <RequireAuth />,
    children: [
      { path: '/change-password', element: <ChangePasswordPage /> },
      {
        element: <AppLayout />,
        children: [
          { index: true, element: <HomePage /> },
          { path: 'forbidden', element: <ForbiddenPage /> },
          {
            element: <RequireAdministrator />,
            children: [
              { path: 'categories', element: <CategoriesPage /> },
              { path: 'categories/new', element: <CategoryFormPage /> },
              { path: 'categories/:id/edit', element: <CategoryFormPage /> },
            ],
          },
          { path: '*', element: <NotFoundPage /> },
        ],
      },
    ],
  },
])

export function App() {
  return <RouterProvider router={router} />
}
