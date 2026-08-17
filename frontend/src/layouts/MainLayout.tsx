import {
  AuditOutlined,
  BarChartOutlined,
  CustomerServiceOutlined,
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
  SolutionOutlined,
  SwapOutlined,
  TeamOutlined,
  ToolOutlined,
  WarningOutlined,
} from '@ant-design/icons'
import { Button, Layout, Menu, Typography } from 'antd'
import type { MenuProps } from 'antd'
import { useState } from 'react'
import { Outlet, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../contexts/useAuth'
import type { UserRole } from '../types/auth'
import './MainLayout.css'

const { Header, Content, Sider } = Layout
type MenuItem = NonNullable<MenuProps['items']>[number]

const operationalItems: MenuItem[] = [
  { key: '/dashboard', icon: <DashboardOutlined />, label: 'Dashboard' },
  { key: '/assets', icon: <LaptopOutlined />, label: 'Envanter' },
  {
    key: 'assignments-menu',
    icon: <SolutionOutlined />,
    label: 'Zimmetler',
    children: [
      { key: '/assignments', icon: <SolutionOutlined />, label: 'Aktif Zimmetler' },
      { key: '/assignments/new', icon: <PlusOutlined />, label: 'Yeni Zimmet' },
      { key: '/assignments/returns', icon: <SwapOutlined />, label: 'İade İşlemleri' },
      { key: '/assignments/history', icon: <HistoryOutlined />, label: 'Zimmet Geçmişi' },
    ],
  },
  {
    key: 'stock-menu',
    icon: <DatabaseOutlined />,
    label: 'Stok',
    children: [
      { key: '/stock', icon: <DatabaseOutlined />, label: 'Stok Durumu' },
      { key: '/stock/transactions', icon: <SwapOutlined />, label: 'Stok Hareketleri' },
      { key: '/stock/critical', icon: <WarningOutlined />, label: 'Kritik Stok' },
    ],
  },
  {
    key: 'warranties-menu',
    icon: <SafetyCertificateOutlined />,
    label: 'Garanti',
    children: [
      { key: '/warranties', icon: <SafetyCertificateOutlined />, label: 'Garanti Listesi' },
      { key: '/warranties/expiring', icon: <WarningOutlined />, label: 'Yaklaşan Garantiler' },
    ],
  },
  {
    key: 'licenses-menu',
    icon: <FileProtectOutlined />,
    label: 'Lisanslar',
    children: [
      { key: '/licenses', icon: <FileProtectOutlined />, label: 'Lisans Listesi' },
      { key: '/licenses/expiring', icon: <WarningOutlined />, label: 'Yaklaşan Lisanslar' },
    ],
  },
  { key: '/maintenance', icon: <ToolOutlined />, label: 'Periyodik Bakım' },
  { key: '/support-requests', icon: <CustomerServiceOutlined />, label: 'Teknik Destek' },
  { key: '/reports', icon: <BarChartOutlined />, label: 'Raporlar' },
  { key: '/admin/users', icon: <TeamOutlined />, label: 'Kullanıcılar' },
]

const itemsByRole: Record<UserRole, MenuProps['items']> = {
  Admin: [
    ...operationalItems,
    { key: '/admin/audit-logs', icon: <AuditOutlined />, label: 'Audit Log' },
  ],
  IT: operationalItems,
  Employee: [
    { key: '/dashboard', icon: <DashboardOutlined />, label: 'Dashboard' },
    { key: '/assignments/mine', icon: <SolutionOutlined />, label: 'Zimmetlerim' },
    { key: '/support-requests', icon: <CustomerServiceOutlined />, label: 'Teknik Destek' },
  ],
}

const getSelectedKey = (path: string, role: UserRole): string => {
  const exactPaths = [
    '/dashboard',
    '/assets',
    '/assignments',
    '/assignments/new',
    '/assignments/returns',
    '/assignments/history',
    '/assignments/mine',
    '/stock',
    '/stock/transactions',
    '/stock/critical',
    '/warranties',
    '/warranties/expiring',
    '/licenses',
    '/licenses/expiring',
    '/maintenance',
    '/support-requests',
    '/reports',
    '/admin/users',
    '/admin/audit-logs',
  ]

  if (exactPaths.includes(path)) return path
  if (path.startsWith('/assets/')) return '/assets'
  if (path.startsWith('/assignments/')) {
    return role === 'Employee' ? '/assignments/mine' : '/assignments'
  }
  if (path.startsWith('/stock/')) return '/stock'
  if (path.startsWith('/warranties/')) return '/warranties'
  if (path.startsWith('/licenses/')) return '/licenses'
  if (path.startsWith('/maintenance/')) return '/maintenance'
  if (path.startsWith('/support-requests/')) return '/support-requests'
  if (path.startsWith('/reports/')) return '/reports'
  if (path.startsWith('/admin/users/')) return '/admin/users'

  return path
}

const getOpenMenuKeys = (path: string): string[] => {
  if (path.startsWith('/assignments')) return ['assignments-menu']
  if (path.startsWith('/stock')) return ['stock-menu']
  if (path.startsWith('/warranties')) return ['warranties-menu']
  if (path.startsWith('/licenses')) return ['licenses-menu']
  return []
}

function MainLayout() {
  const [collapsed, setCollapsed] = useState(false)
  const { logout, user } = useAuth()
  const location = useLocation()
  const navigate = useNavigate()

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
        <div className="brand">{collapsed ? 'TS' : 'Takip Sistemi'}</div>
        <Menu
          defaultOpenKeys={getOpenMenuKeys(location.pathname)}
          items={user ? itemsByRole[user.role] : []}
          mode="inline"
          onClick={({ key }) => void navigate(key)}
          selectedKeys={user ? [getSelectedKey(location.pathname, user.role)] : []}
          theme="dark"
        />
      </Sider>

      <Layout>
        <Header className="main-header">
          <Button
            aria-label="Menüyü aç/kapat"
            icon={collapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
            onClick={() => setCollapsed((value) => !value)}
            type="text"
          />
          <Typography.Text className="header-title" strong>
            Donanım ve Lisans Takip Sistemi
          </Typography.Text>
          {user && (
            <div className="header-user">
              <div className="header-user-details">
                <Typography.Text strong>{user.fullName}</Typography.Text>
                {user.department && (
                  <Typography.Text type="secondary">{user.department}</Typography.Text>
                )}
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
