import { Navigate, createBrowserRouter } from 'react-router-dom'
import type { ReactNode } from 'react'
import { ProtectedRoute, PublicOnlyRoute, RoleRoute } from '../components/AuthRoutes/AuthRoutes'
import MainLayout from '../layouts/MainLayout'
import AssignmentCreatePage from '../pages/Assignments/AssignmentCreatePage'
import AssignmentDetailPage from '../pages/Assignments/AssignmentDetailPage'
import AssignmentHistoryPage from '../pages/Assignments/AssignmentHistoryPage'
import AssignmentReturnsPage from '../pages/Assignments/AssignmentReturnsPage'
import AssignmentsPage from '../pages/Assignments/AssignmentsPage'
import MyAssignmentsPage from '../pages/Assignments/MyAssignmentsPage'
import AssetCreatePage from '../pages/Assets/AssetCreatePage'
import AssetDetailPage from '../pages/Assets/AssetDetailPage'
import AssetEditPage from '../pages/Assets/AssetEditPage'
import AssetsPage from '../pages/Assets/AssetsPage'
import AuditLogsPage from '../pages/AuditLogs/AuditLogsPage'
import DashboardPage from '../pages/Dashboard/DashboardPage'
import LicenseCreatePage from '../pages/Licenses/LicenseCreatePage'
import LicenseDetailPage from '../pages/Licenses/LicenseDetailPage'
import LicenseEditPage from '../pages/Licenses/LicenseEditPage'
import LicensesPage from '../pages/Licenses/LicensesPage'
import ExpiringLicensesPage from '../pages/Licenses/ExpiringLicensesPage'
import LoginPage from '../pages/Login/LoginPage'
import MaintenancePage from '../pages/Maintenance/MaintenancePage'
import MaintenancePlanCreatePage from '../pages/Maintenance/MaintenancePlanCreatePage'
import MaintenanceRequestCreatePage from '../pages/Maintenance/MaintenanceRequestCreatePage'
import MaintenanceRequestDetailPage from '../pages/Maintenance/MaintenanceRequestDetailPage'
import MaintenanceRequestsPage from '../pages/Maintenance/MaintenanceRequestsPage'
import MaintenanceTaskDetailPage from '../pages/Maintenance/MaintenanceTaskDetailPage'
import NotFoundPage from '../pages/NotFoundPage'
import AssignmentReportPage from '../pages/Reports/AssignmentReportPage'
import InventoryReportPage from '../pages/Reports/InventoryReportPage'
import MaintenanceReportPage from '../pages/Reports/MaintenanceReportPage'
import WarrantyReportPage from '../pages/Reports/WarrantyReportPage'
import LicenseReportPage from '../pages/Reports/LicenseReportPage'
import SupportReportPage from '../pages/Reports/SupportReportPage'
import ReportsPage from '../pages/Reports/ReportsPage'
import StockReportPage from '../pages/Reports/StockReportPage'
import StockItemCreatePage from '../pages/Stock/StockItemCreatePage'
import CriticalStockPage from '../pages/Stock/CriticalStockPage'
import StockItemDetailPage from '../pages/Stock/StockItemDetailPage'
import StockItemsPage from '../pages/Stock/StockItemsPage'
import StockTransactionsPage from '../pages/Stock/StockTransactionsPage'
import UnauthorizedPage from '../pages/UnauthorizedPage'
import UserCreatePage from '../pages/Users/UserCreatePage'
import UsersPage from '../pages/Users/UsersPage'
import WarrantiesPage from '../pages/Warranties/WarrantiesPage'
import ExpiringWarrantiesPage from '../pages/Warranties/ExpiringWarrantiesPage'

const operation = (element: ReactNode) => <RoleRoute allowedRoles={['Admin', 'IT']}>{element}</RoleRoute>
const adminOnly = (element: ReactNode) => <RoleRoute allowedRoles={['Admin']}>{element}</RoleRoute>

export const router = createBrowserRouter([
  { path: '/login', element: <PublicOnlyRoute><LoginPage /></PublicOnlyRoute> },
  { path: '/unauthorized', element: <UnauthorizedPage /> },
  {
    path: '/', element: <ProtectedRoute><MainLayout /></ProtectedRoute>, children: [
      { index: true, element: <Navigate to="/dashboard" replace /> },
      { path: 'dashboard', element: <DashboardPage /> },
      { path: 'assets', element: operation(<AssetsPage />) },
      { path: 'assets/new', element: operation(<AssetCreatePage />) },
      { path: 'assets/:id/edit', element: operation(<AssetEditPage />) },
      { path: 'assets/:id', element: operation(<AssetDetailPage />) },
      { path: 'assignments', element: operation(<AssignmentsPage />) },
      { path: 'assignments/new', element: operation(<AssignmentCreatePage />) },
      { path: 'assignments/returns', element: operation(<AssignmentReturnsPage />) },
      { path: 'assignments/history', element: operation(<AssignmentHistoryPage />) },
      { path: 'assignments/mine', element: <RoleRoute allowedRoles={['Employee']}><MyAssignmentsPage /></RoleRoute> },
      { path: 'assignments/:id', element: <AssignmentDetailPage /> },
      { path: 'stock', element: operation(<StockItemsPage />) },
      { path: 'stock/new', element: operation(<StockItemCreatePage />) },
      { path: 'stock/transactions', element: operation(<StockTransactionsPage />) },
      { path: 'stock/critical', element: operation(<CriticalStockPage />) },
      { path: 'stock/:id', element: operation(<StockItemDetailPage />) },
      { path: 'warranties', element: operation(<WarrantiesPage />) },
      { path: 'warranties/expiring', element: operation(<ExpiringWarrantiesPage />) },
      { path: 'licenses', element: operation(<LicensesPage />) },
      { path: 'licenses/new', element: operation(<LicenseCreatePage />) },
      { path: 'licenses/expiring', element: operation(<ExpiringLicensesPage />) },
      { path: 'licenses/:id/edit', element: operation(<LicenseEditPage />) },
      { path: 'licenses/:id', element: operation(<LicenseDetailPage />) },
      { path: 'maintenance', element: operation(<MaintenancePage />) },
      { path: 'maintenance/plans/new', element: operation(<MaintenancePlanCreatePage />) },
      { path: 'maintenance/tasks/:taskId', element: operation(<MaintenanceTaskDetailPage />) },
      { path: 'support-requests', element: <MaintenanceRequestsPage /> },
      { path: 'support-requests/new', element: <RoleRoute allowedRoles={['Employee']}><MaintenanceRequestCreatePage /></RoleRoute> },
      { path: 'support-requests/:id', element: <MaintenanceRequestDetailPage /> },
      { path: 'reports', element: operation(<ReportsPage />) },
      { path: 'reports/inventory', element: operation(<InventoryReportPage />) },
      { path: 'reports/assignments', element: operation(<AssignmentReportPage />) },
      { path: 'reports/stock', element: operation(<StockReportPage />) },
      { path: 'reports/maintenance', element: operation(<MaintenanceReportPage />) },
      { path: 'reports/warranties', element: operation(<WarrantyReportPage />) },
      { path: 'reports/licenses', element: operation(<LicenseReportPage />) },
      { path: 'reports/support-requests', element: operation(<SupportReportPage />) },
      { path: 'admin/users', element: operation(<UsersPage />) },
      { path: 'admin/users/new', element: operation(<UserCreatePage />) },
      { path: 'admin/audit-logs', element: adminOnly(<AuditLogsPage />) },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
])
