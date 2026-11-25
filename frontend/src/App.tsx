import { createBrowserRouter, RouterProvider } from 'react-router'
import { RedirectIfAuthenticated, RequireAdministrator, RequireAuth } from './auth/guards'
import { AuditPage } from './features/audit/AuditPage'
import { CategoriesPage } from './features/categories/CategoriesPage'
import { CategoryFormPage } from './features/categories/CategoryFormPage'
import { CustomerDetailPage } from './features/customers/CustomerDetailPage'
import { CustomerFormPage } from './features/customers/CustomerFormPage'
import { CustomersPage } from './features/customers/CustomersPage'
import { DashboardPage } from './features/dashboard/DashboardPage'
import { ProductFormPage } from './features/products/ProductFormPage'
import { NewQuotePage } from './features/quotes/NewQuotePage'
import { QuoteDetailPage } from './features/quotes/QuoteDetailPage'
import { QuotesPage } from './features/quotes/QuotesPage'
import { ProductsPage } from './features/products/ProductsPage'
import { SettingsPage } from './features/settings/SettingsPage'
import { UserFormPage } from './features/users/UserFormPage'
import { UsersPage } from './features/users/UsersPage'
import { AppLayout } from './layout/AppLayout'
import { ChangePasswordPage } from './pages/ChangePasswordPage'
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
          { index: true, element: <DashboardPage /> },
          { path: 'forbidden', element: <ForbiddenPage /> },
          { path: 'products', element: <ProductsPage /> },
          { path: 'quotes', element: <QuotesPage /> },
          { path: 'quotes/new', element: <NewQuotePage /> },
          { path: 'quotes/:id', element: <QuoteDetailPage /> },
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
              { path: 'users', element: <UsersPage /> },
              { path: 'users/new', element: <UserFormPage /> },
              { path: 'users/:id/edit', element: <UserFormPage /> },
              { path: 'audit', element: <AuditPage /> },
              { path: 'settings', element: <SettingsPage /> },
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
