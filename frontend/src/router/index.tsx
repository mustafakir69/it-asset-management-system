import { Navigate, createBrowserRouter } from 'react-router-dom'
import MainLayout from '../layouts/MainLayout'
import AssignmentCreatePage from '../pages/Assignments/AssignmentCreatePage'
import AssignmentDetailPage from '../pages/Assignments/AssignmentDetailPage'
import AssignmentHistoryPage from '../pages/Assignments/AssignmentHistoryPage'
import AssignmentsPage from '../pages/Assignments/AssignmentsPage'
import AssetCreatePage from '../pages/Assets/AssetCreatePage'
import AssetDetailPage from '../pages/Assets/AssetDetailPage'
import AssetEditPage from '../pages/Assets/AssetEditPage'
import AssetsPage from '../pages/Assets/AssetsPage'
import DashboardPage from '../pages/Dashboard/DashboardPage'
import NotFoundPage from '../pages/NotFoundPage'
import PlaceholderPage from '../pages/PlaceholderPage'

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
      { path: 'assignments/returns', element: <PlaceholderPage title="İade İşlemleri" /> },
      { path: 'assignments/history', element: <AssignmentHistoryPage /> },
      { path: 'assignments/:id', element: <AssignmentDetailPage /> },
      { path: 'my-assignments', element: <PlaceholderPage title="Zimmetlerim" /> },
      { path: 'stock', element: <PlaceholderPage title="Stok Durumu" /> },
      { path: 'stock/movements', element: <PlaceholderPage title="Stok Hareketleri" /> },
      { path: 'stock/critical', element: <PlaceholderPage title="Kritik Stoklar" /> },
      { path: 'warranties', element: <PlaceholderPage title="Garanti Listesi" /> },
      { path: 'warranties/expiring', element: <PlaceholderPage title="Süresi Yaklaşan Garantiler" /> },
      { path: 'licenses', element: <PlaceholderPage title="Lisans Listesi" /> },
      { path: 'licenses/expiring', element: <PlaceholderPage title="Süresi Yaklaşan Lisanslar" /> },
      { path: 'licenses/:licenseId', element: <PlaceholderPage title="Lisans Detayı" /> },
      { path: 'my-licenses', element: <PlaceholderPage title="Lisanslarım" /> },
      { path: 'maintenance/plans', element: <PlaceholderPage title="Bakım Planları" /> },
      { path: 'maintenance/tasks', element: <PlaceholderPage title="Bakım Görevleri" /> },
      { path: 'maintenance/history', element: <PlaceholderPage title="Bakım Geçmişi" /> },
      { path: 'maintenance/tasks/:taskId', element: <PlaceholderPage title="Bakım Görevi Detayı" /> },
      { path: 'my-maintenance-requests', element: <PlaceholderPage title="Bakım Taleplerim" /> },
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
