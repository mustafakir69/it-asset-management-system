import { Navigate, createBrowserRouter } from 'react-router-dom'
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
import LegacyAssetRedirect from '../pages/Assets/LegacyAssetRedirect'
import AuditLogsPage from '../pages/AuditLogs/AuditLogsPage'
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
import LoginPage from '../pages/Login/LoginPage'
import NotFoundPage from '../pages/NotFoundPage'
import UnauthorizedPage from '../pages/UnauthorizedPage'
import FeatureInfoPage from '../pages/Informational/FeatureInfoPage'
import AssignmentReportPage from '../pages/Reports/AssignmentReportPage'
import InventoryReportPage from '../pages/Reports/InventoryReportPage'
import MaintenanceReportPage from '../pages/Reports/MaintenanceReportPage'
import ReportsPage from '../pages/Reports/ReportsPage'
import StockReportPage from '../pages/Reports/StockReportPage'
import StockItemCreatePage from '../pages/Stock/StockItemCreatePage'
import StockItemDetailPage from '../pages/Stock/StockItemDetailPage'
import StockItemsPage from '../pages/Stock/StockItemsPage'
import CriticalStockPage from '../pages/Stock/CriticalStockPage'
import StockTransactionsPage from '../pages/Stock/StockTransactionsPage'
import ExpiringWarrantiesPage from '../pages/Warranties/ExpiringWarrantiesPage'
import WarrantiesPage from '../pages/Warranties/WarrantiesPage'
import UserCreatePage from '../pages/Users/UserCreatePage'
import UsersPage from '../pages/Users/UsersPage'

export const router = createBrowserRouter([
  {
    path: '/login',
    element: (
      <PublicOnlyRoute>
        <LoginPage />
      </PublicOnlyRoute>
    ),
  },
  {
    path: '/unauthorized',
    element: <UnauthorizedPage />,
  },
  {
    path: '/',
    element: (
      <ProtectedRoute>
        <MainLayout />
      </ProtectedRoute>
    ),
    children: [
      { index: true, element: <Navigate to="/dashboard" replace /> },
      { path: 'dashboard', element: <DashboardPage /> },
      { path: 'assets', element: <AssetsPage /> },
      { path: 'assets/new', element: <AssetCreatePage /> },
      { path: 'assets/:id/edit', element: <AssetEditPage /> },
      { path: 'assets/:id', element: <AssetDetailPage /> },
      { path: 'inventory', element: <Navigate to="/assets" replace /> },
      { path: 'inventory/new', element: <Navigate to="/assets/new" replace /> },
      { path: 'inventory/:deviceId', element: <LegacyAssetRedirect /> },
      { path: 'inventory/:deviceId/edit', element: <LegacyAssetRedirect edit /> },
      {
        path: 'assignments',
        element: <RoleRoute allowedRoles={['Admin', 'IT', 'Auditor']}><AssignmentsPage /></RoleRoute>,
      },
      {
        path: 'assignments/new',
        element: <RoleRoute allowedRoles={['Admin', 'IT']}><AssignmentCreatePage /></RoleRoute>,
      },
      {
        path: 'assignments/returns',
        element: <RoleRoute allowedRoles={['Admin', 'IT']}><AssignmentReturnsPage /></RoleRoute>,
      },
      {
        path: 'assignments/history',
        element: <RoleRoute allowedRoles={['Admin', 'IT', 'Auditor']}><AssignmentHistoryPage /></RoleRoute>,
      },
      { path: 'assignments/:id', element: <AssignmentDetailPage /> },
      {
        path: 'assignments/mine',
        element: <RoleRoute allowedRoles={['Employee']}><MyAssignmentsPage /></RoleRoute>,
      },
      {
        path: 'my-assignments',
        element: <RoleRoute allowedRoles={['Employee']}><MyAssignmentsPage /></RoleRoute>,
      },
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
      { path: 'my-licenses', element: <RoleRoute allowedRoles={['Employee']}><FeatureInfoPage title="Lisanslarım" description="Kullanıcıya atanmış lisanslar." message="Lisanslarım özelliği lisans kullanıcı/cihaz atama altyapısı eklendiğinde aktif olacaktır." /></RoleRoute> },
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
      { path: 'reports', element: <RoleRoute allowedRoles={['Admin', 'IT', 'Auditor']}><ReportsPage /></RoleRoute> },
      { path: 'reports/inventory', element: <RoleRoute allowedRoles={['Admin', 'IT', 'Auditor']}><InventoryReportPage /></RoleRoute> },
      { path: 'reports/assignments', element: <RoleRoute allowedRoles={['Admin', 'IT', 'Auditor']}><AssignmentReportPage /></RoleRoute> },
      { path: 'reports/stock', element: <RoleRoute allowedRoles={['Admin', 'IT', 'Auditor']}><StockReportPage /></RoleRoute> },
      { path: 'reports/maintenance', element: <RoleRoute allowedRoles={['Admin', 'IT', 'Auditor']}><MaintenanceReportPage /></RoleRoute> },
      { path: 'users', element: <Navigate to="/admin/users" replace /> },
      {
        path: 'admin/users',
        element: <RoleRoute allowedRoles={['Admin', 'IT']}><UsersPage /></RoleRoute>,
      },
      {
        path: 'admin/users/new',
        element: <RoleRoute allowedRoles={['Admin', 'IT']}><UserCreatePage /></RoleRoute>,
      },
      { path: 'admin/categories', element: <RoleRoute allowedRoles={['Admin']}><FeatureInfoPage title="Kategoriler" description="Envanter ve stok kategori yönetimi." message="Bu özellik sonraki sürüm kapsamındadır." /></RoleRoute> },
      { path: 'admin/locations', element: <RoleRoute allowedRoles={['Admin']}><FeatureInfoPage title="Lokasyonlar" description="Kurum lokasyonlarının yönetimi." message="Bu özellik sonraki sürüm kapsamındadır." /></RoleRoute> },
      { path: 'admin/suppliers', element: <RoleRoute allowedRoles={['Admin']}><FeatureInfoPage title="Tedarikçiler" description="Tedarikçi bilgilerinin yönetimi." message="Bu özellik sonraki sürüm kapsamındadır." /></RoleRoute> },
      { path: 'admin/audit-logs', element: <RoleRoute allowedRoles={['Admin', 'Auditor']}><AuditLogsPage /></RoleRoute> },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
])
