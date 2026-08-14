import { useState } from 'react'
import {
  AppstoreOutlined,
  AuditOutlined,
  BankOutlined,
  BarChartOutlined,
  BuildOutlined,
  DashboardOutlined,
  DatabaseOutlined,
  FileProtectOutlined,
  HistoryOutlined,
  LaptopOutlined,
  LogoutOutlined,
  MenuFoldOutlined,
  MenuUnfoldOutlined,
  PlusOutlined,
  SafetyCertificateOutlined,
  SettingOutlined,
  ShopOutlined,
  SolutionOutlined,
  SwapOutlined,
  TeamOutlined,
  ToolOutlined,
  UserOutlined,
  WarningOutlined,
} from '@ant-design/icons'
import { Button, Layout, Menu, Typography } from 'antd'
import type { MenuProps } from 'antd'
import { Outlet, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../contexts/useAuth'
import type { UserRole } from '../types/auth'
import './MainLayout.css'

const { Header, Content, Sider } = Layout

const menuItems: MenuProps['items'] = [
  {
    key: '/dashboard',
    icon: <DashboardOutlined />,
    label: 'Dashboard',
  },
  {
    key: 'inventory-menu',
    icon: <LaptopOutlined />,
    label: 'Envanter',
    children: [
      { key: '/assets', icon: <LaptopOutlined />, label: 'Cihazlar' },
      { key: '/assets/new', icon: <PlusOutlined />, label: 'Yeni Cihaz' },
    ],
  },
  {
    key: 'assignments-menu',
    icon: <SolutionOutlined />,
    label: 'Zimmet',
    children: [
      { key: '/assignments', icon: <SolutionOutlined />, label: 'Aktif Zimmetler' },
      { key: '/assignments/new', icon: <PlusOutlined />, label: 'Yeni Zimmet' },
      { key: '/assignments/returns', icon: <SwapOutlined />, label: 'İade İşlemleri' },
      { key: '/assignments/history', icon: <HistoryOutlined />, label: 'Zimmet Geçmişi' },
      { key: '/my-assignments', icon: <UserOutlined />, label: 'Zimmetlerim' },
    ],
  },
  {
    key: 'stock-menu',
    icon: <DatabaseOutlined />,
    label: 'Stok',
    children: [
      { key: '/stock', icon: <DatabaseOutlined />, label: 'Stok Durumu' },
      { key: '/stock/transactions', icon: <SwapOutlined />, label: 'Stok Hareketleri' },
      { key: '/stock/critical', icon: <WarningOutlined />, label: 'Kritik Stoklar' },
    ],
  },
  {
    key: 'warranties-menu',
    icon: <SafetyCertificateOutlined />,
    label: 'Garantiler',
    children: [
      { key: '/warranties', icon: <SafetyCertificateOutlined />, label: 'Garanti Listesi' },
      { key: '/warranties/expiring', icon: <WarningOutlined />, label: 'Süresi Yaklaşanlar' },
    ],
  },
  {
    key: 'licenses-menu',
    icon: <FileProtectOutlined />,
    label: 'Lisanslar',
    children: [
      { key: '/licenses', icon: <FileProtectOutlined />, label: 'Lisans Listesi' },
      { key: '/licenses/expiring', icon: <WarningOutlined />, label: 'Süresi Yaklaşanlar' },
      { key: '/my-licenses', icon: <UserOutlined />, label: 'Lisanslarım' },
    ],
  },
  {
    key: 'maintenance-menu',
    icon: <ToolOutlined />,
    label: 'Bakım',
    children: [
      { key: '/maintenance', icon: <ToolOutlined />, label: 'Bakım Merkezi' },
      { key: '/maintenance/plans', icon: <BuildOutlined />, label: 'Bakım Planları' },
      { key: '/maintenance/tasks', icon: <ToolOutlined />, label: 'Bakım Görevleri' },
      { key: '/maintenance/requests', icon: <SolutionOutlined />, label: 'Bakım Talepleri' },
      { key: '/maintenance/history', icon: <HistoryOutlined />, label: 'Bakım Geçmişi' },
      { key: '/maintenance/my-requests', icon: <UserOutlined />, label: 'Bakım Taleplerim' },
    ],
  },
  {
    key: 'reports-menu',
    icon: <BarChartOutlined />,
    label: 'Raporlar',
    children: [
      { key: '/reports', icon: <BarChartOutlined />, label: 'Rapor Merkezi' },
      { key: '/reports/inventory', icon: <LaptopOutlined />, label: 'Envanter Raporu' },
      { key: '/reports/assignments', icon: <SolutionOutlined />, label: 'Zimmet Raporu' },
      { key: '/reports/stock', icon: <DatabaseOutlined />, label: 'Stok Raporu' },
      { key: '/reports/maintenance', icon: <ToolOutlined />, label: 'Bakım Raporu' },
    ],
  },
  {
    key: 'admin-menu',
    icon: <SettingOutlined />,
    label: 'Yönetim',
    children: [
      { key: '/admin/users', icon: <TeamOutlined />, label: 'Kullanıcılar' },
      { key: '/admin/categories', icon: <AppstoreOutlined />, label: 'Kategoriler' },
      { key: '/admin/locations', icon: <BankOutlined />, label: 'Lokasyonlar' },
      { key: '/admin/suppliers', icon: <ShopOutlined />, label: 'Tedarikçiler' },
      { key: '/admin/audit-logs', icon: <AuditOutlined />, label: 'Audit Logları' },
    ],
  },
]

const hiddenMenuKeysByRole: Record<UserRole, ReadonlySet<string>> = {
  Admin: new Set(['/my-assignments', '/my-licenses', '/maintenance/my-requests']),
  IT: new Set([
    '/my-assignments',
    '/my-licenses',
    '/maintenance/my-requests',
    '/admin/categories',
    '/admin/locations',
    '/admin/suppliers',
    '/admin/audit-logs',
  ]),
  Employee: new Set([
    'inventory-menu',
    '/assignments',
    '/assignments/new',
    '/assignments/returns',
    '/assignments/history',
    'stock-menu',
    'warranties-menu',
    '/licenses',
    '/licenses/expiring',
    '/maintenance',
    '/maintenance/plans',
    '/maintenance/tasks',
    '/maintenance/requests',
    '/maintenance/history',
    'reports-menu',
    'admin-menu',
  ]),
  Auditor: new Set([
    '/assets/new',
    '/assignments/new',
    '/assignments/returns',
    '/my-assignments',
    '/my-licenses',
    '/maintenance/my-requests',
    '/admin/users',
    '/admin/categories',
    '/admin/locations',
    '/admin/suppliers',
  ]),
}

const filterMenuItemsByRole = (
  items: MenuProps['items'],
  role: UserRole,
): MenuProps['items'] => {
  const hiddenKeys = hiddenMenuKeysByRole[role]

  return (items ?? []).reduce<NonNullable<MenuProps['items']>>((visibleItems, item) => {
    if (!item || hiddenKeys.has(String(item.key))) return visibleItems

    if ('children' in item && item.children) {
      visibleItems.push({
        ...item,
        children: filterMenuItemsByRole(item.children, role) ?? [],
      } as typeof item)
    } else {
      visibleItems.push(item)
    }

    return visibleItems
  }, [])
}

const getSelectedMenuKey = (pathname: string) => {
  const exactMenuPaths = [
    '/dashboard',
    '/assets',
    '/assets/new',
    '/assignments/new',
    '/assignments/returns',
    '/assignments/history',
    '/my-assignments',
    '/stock/transactions',
    '/stock/critical',
    '/warranties/expiring',
    '/licenses/expiring',
    '/my-licenses',
    '/maintenance',
    '/maintenance/plans',
    '/maintenance/tasks',
    '/maintenance/requests',
    '/maintenance/history',
    '/maintenance/my-requests',
    '/reports',
    '/reports/inventory',
    '/reports/assignments',
    '/reports/stock',
    '/reports/maintenance',
    '/admin/users',
    '/admin/categories',
    '/admin/locations',
    '/admin/suppliers',
    '/admin/audit-logs',
  ]

  if (exactMenuPaths.includes(pathname)) {
    return pathname
  }

  if (pathname.startsWith('/assets/')) return '/assets'
  if (pathname.startsWith('/inventory/')) return '/assets'
  if (pathname.startsWith('/assignments/')) return '/assignments'
  if (pathname.startsWith('/licenses/')) return '/licenses'
  if (pathname.startsWith('/maintenance/plans/')) return '/maintenance/plans'
  if (pathname.startsWith('/maintenance/tasks/')) return '/maintenance/tasks'
  if (pathname.startsWith('/maintenance/requests/')) return '/maintenance/requests'
  if (pathname.startsWith('/reports/')) return pathname
  if (pathname.startsWith('/admin/users/')) return '/admin/users'

  return pathname
}

const getOpenMenuKeys = (pathname: string) => {
  if (pathname.startsWith('/assets') || pathname.startsWith('/inventory')) return ['inventory-menu']
  if (pathname.startsWith('/assignments') || pathname === '/my-assignments') return ['assignments-menu']
  if (pathname.startsWith('/stock')) return ['stock-menu']
  if (pathname.startsWith('/warranties')) return ['warranties-menu']
  if (pathname.startsWith('/licenses') || pathname === '/my-licenses') return ['licenses-menu']
  if (pathname.startsWith('/maintenance')) return ['maintenance-menu']
  if (pathname.startsWith('/reports')) return ['reports-menu']
  if (pathname.startsWith('/admin')) return ['admin-menu']

  return []
}

function MainLayout() {
  const [collapsed, setCollapsed] = useState(false)
  const { logout, user } = useAuth()
  const location = useLocation()
  const navigate = useNavigate()
  const visibleMenuItems = user ? filterMenuItemsByRole(menuItems, user.role) : []

  const handleMenuClick: MenuProps['onClick'] = ({ key }) => {
    if (key.startsWith('/')) {
      void navigate(key)
    }
  }

  const handleLogout = () => {
    logout()
    void navigate('/login', { replace: true })
  }

  return (
    <Layout className="main-layout">
      <Sider
        breakpoint="lg"
        className="main-sidebar"
        collapsed={collapsed}
        collapsible
        onBreakpoint={setCollapsed}
        trigger={null}
        width={240}
      >
        <div className="brand" aria-label="Takip Sistemi">
          {collapsed ? 'TS' : 'Takip Sistemi'}
        </div>
        <Menu
          defaultOpenKeys={getOpenMenuKeys(location.pathname)}
          items={visibleMenuItems}
          mode="inline"
          onClick={handleMenuClick}
          selectedKeys={[getSelectedMenuKey(location.pathname)]}
          theme="dark"
        />
      </Sider>

      <Layout>
        <Header className="main-header">
          <Button
            aria-label={collapsed ? 'Menüyü genişlet' : 'Menüyü daralt'}
            icon={collapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
            onClick={() => setCollapsed((current) => !current)}
            type="text"
          />
          <Typography.Text className="header-title" strong>
            Donanım ve Lisans Takip Sistemi
          </Typography.Text>
          {user && (
            <div className="header-user">
              <div className="header-user-details">
                <Typography.Text strong>{user.username}</Typography.Text>
                <Typography.Text type="secondary">{user.roleDisplayName}</Typography.Text>
              </div>
              <Button icon={<LogoutOutlined />} onClick={handleLogout}>
                Çıkış Yap
              </Button>
            </div>
          )}
        </Header>

        <Content className="main-content">
          <div className="content-shell">
            <Outlet />
          </div>
        </Content>
      </Layout>
    </Layout>
  )
}

export default MainLayout
