import { Navigate, createBrowserRouter } from 'react-router-dom'
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
import DashboardPage from '../pages/Dashboard/DashboardPage'
import LicenseCreatePage from '../pages/Licenses/LicenseCreatePage'
import LicenseDetailPage from '../pages/Licenses/LicenseDetailPage'
import LicenseEditPage from '../pages/Licenses/LicenseEditPage'
import ExpiringLicensesPage from '../pages/Licenses/ExpiringLicensesPage'
import LicensesPage from '../pages/Licenses/LicensesPage'
import MaintenancePage from '../pages/Maintenance/MaintenancePage'
import MaintenanceHistoryPage from '../pages/Maintenance/MaintenanceHistoryPage'
import MaintenancePlanCreatePage from '../pages/Maintenance/MaintenancePlanCreatePage'
import MaintenancePlanDetailPage from '../pages/Maintenance/MaintenancePlanDetailPage'
import MaintenancePlanEditPage from '../pages/Maintenance/MaintenancePlanEditPage'
import MaintenancePlansPage from '../pages/Maintenance/MaintenancePlansPage'
import MaintenanceRequestCreatePage from '../pages/Maintenance/MaintenanceRequestCreatePage'
import MaintenanceRequestDetailPage from '../pages/Maintenance/MaintenanceRequestDetailPage'
import MaintenanceRequestEditPage from '../pages/Maintenance/MaintenanceRequestEditPage'
import MaintenanceRequestsPage from '../pages/Maintenance/MaintenanceRequestsPage'
import MaintenanceTaskDetailPage from '../pages/Maintenance/MaintenanceTaskDetailPage'
import MaintenanceTasksPage from '../pages/Maintenance/MaintenanceTasksPage'
import MyMaintenanceRequestsPage from '../pages/Maintenance/MyMaintenanceRequestsPage'
import NotFoundPage from '../pages/NotFoundPage'
import PlaceholderPage from '../pages/PlaceholderPage'
import StockItemCreatePage from '../pages/Stock/StockItemCreatePage'
import StockItemDetailPage from '../pages/Stock/StockItemDetailPage'
import StockItemsPage from '../pages/Stock/StockItemsPage'
import CriticalStockPage from '../pages/Stock/CriticalStockPage'
import StockTransactionsPage from '../pages/Stock/StockTransactionsPage'
import ExpiringWarrantiesPage from '../pages/Warranties/ExpiringWarrantiesPage'
import WarrantiesPage from '../pages/Warranties/WarrantiesPage'

export const router = createBrowserRouter([
  {
    path: '/login',
    element: <PlaceholderPage title="Giriş" />,
  },
  {
    path: '/unauthorized',
    element: <PlaceholderPage title="Yetkisiz Erişim" />,
  },
  {
    path: '/',
    element: <MainLayout />,
    children: [
      { index: true, element: <Navigate to="/dashboard" replace /> },
      { path: 'dashboard', element: <DashboardPage /> },
      { path: 'assets', element: <AssetsPage /> },
      { path: 'assets/new', element: <AssetCreatePage /> },
      { path: 'assets/:id/edit', element: <AssetEditPage /> },
      { path: 'assets/:id', element: <AssetDetailPage /> },
      { path: 'inventory', element: <Navigate to="/assets" replace /> },
      { path: 'inventory/new', element: <Navigate to="/assets/new" replace /> },
      { path: 'inventory/:deviceId', element: <PlaceholderPage title="Cihaz Detayı" /> },
      { path: 'inventory/:deviceId/edit', element: <PlaceholderPage title="Cihaz Düzenleme" /> },
      { path: 'assignments', element: <AssignmentsPage /> },
      { path: 'assignments/new', element: <AssignmentCreatePage /> },
      { path: 'assignments/returns', element: <AssignmentReturnsPage /> },
      { path: 'assignments/history', element: <AssignmentHistoryPage /> },
      { path: 'assignments/:id', element: <AssignmentDetailPage /> },
      { path: 'my-assignments', element: <MyAssignmentsPage /> },
      { path: 'stock', element: <StockItemsPage /> },
      { path: 'stock/new', element: <StockItemCreatePage /> },
      { path: 'stock/transactions', element: <StockTransactionsPage /> },
      { path: 'stock/movements', element: <Navigate to="/stock/transactions" replace /> },
      { path: 'stock/critical', element: <CriticalStockPage /> },
      { path: 'stock/:id', element: <StockItemDetailPage /> },
      { path: 'warranties', element: <WarrantiesPage /> },
      { path: 'warranties/expiring', element: <ExpiringWarrantiesPage /> },
      { path: 'licenses', element: <LicensesPage /> },
      { path: 'licenses/new', element: <LicenseCreatePage /> },
      { path: 'licenses/expiring', element: <ExpiringLicensesPage /> },
      { path: 'licenses/:id/edit', element: <LicenseEditPage /> },
      { path: 'licenses/:id', element: <LicenseDetailPage /> },
      { path: 'my-licenses', element: <PlaceholderPage title="Lisanslarım" /> },
      { path: 'maintenance', element: <MaintenancePage /> },
      { path: 'maintenance/plans', element: <MaintenancePlansPage /> },
      { path: 'maintenance/plans/new', element: <MaintenancePlanCreatePage /> },
      { path: 'maintenance/plans/:id/edit', element: <MaintenancePlanEditPage /> },
      { path: 'maintenance/plans/:id', element: <MaintenancePlanDetailPage /> },
      { path: 'maintenance/tasks', element: <MaintenanceTasksPage /> },
      { path: 'maintenance/requests', element: <MaintenanceRequestsPage /> },
      { path: 'maintenance/requests/new', element: <MaintenanceRequestCreatePage /> },
      { path: 'maintenance/requests/:id/edit', element: <MaintenanceRequestEditPage /> },
      { path: 'maintenance/requests/:id', element: <MaintenanceRequestDetailPage /> },
      { path: 'maintenance/history', element: <MaintenanceHistoryPage /> },
      { path: 'maintenance/tasks/:taskId', element: <MaintenanceTaskDetailPage /> },
      { path: 'maintenance/my-requests', element: <MyMaintenanceRequestsPage /> },
      { path: 'reports', element: <PlaceholderPage title="Raporlar" /> },
      { path: 'admin/users', element: <PlaceholderPage title="Kullanıcılar" /> },
      { path: 'admin/categories', element: <PlaceholderPage title="Kategoriler" /> },
      { path: 'admin/locations', element: <PlaceholderPage title="Lokasyonlar" /> },
      { path: 'admin/suppliers', element: <PlaceholderPage title="Tedarikçiler" /> },
      { path: 'admin/audit-logs', element: <PlaceholderPage title="Audit Logları" /> },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
])
