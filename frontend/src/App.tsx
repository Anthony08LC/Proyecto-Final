import { createBrowserRouter, RouterProvider } from 'react-router'
import { RedirectIfAuthenticated, RequireAdministrator, RequireAuth } from './auth/guards'
import { CategoriesPage } from './features/categories/CategoriesPage'
import { CategoryFormPage } from './features/categories/CategoryFormPage'
import { CustomerDetailPage } from './features/customers/CustomerDetailPage'
import { CustomerFormPage } from './features/customers/CustomerFormPage'
import { CustomersPage } from './features/customers/CustomersPage'
import { ProductFormPage } from './features/products/ProductFormPage'
import { ProductsPage } from './features/products/ProductsPage'
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
          { path: 'products', element: <ProductsPage /> },
          { path: 'customers', element: <CustomersPage /> },
          { path: 'customers/new', element: <CustomerFormPage /> },
          { path: 'customers/:id', element: <CustomerDetailPage /> },
          {
            element: <RequireAdministrator />,
            children: [
              { path: 'categories', element: <CategoriesPage /> },
              { path: 'categories/new', element: <CategoryFormPage /> },
              { path: 'categories/:id/edit', element: <CategoryFormPage /> },
              { path: 'products/new', element: <ProductFormPage /> },
              { path: 'products/:id/edit', element: <ProductFormPage /> },
              { path: 'customers/:id/edit', element: <CustomerFormPage /> },
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
