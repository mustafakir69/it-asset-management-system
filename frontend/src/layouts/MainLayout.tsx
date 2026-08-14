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
      { key: '/maintenance/plans', icon: <BuildOutlined />, label: 'Bakım Planları' },
      { key: '/maintenance/tasks', icon: <ToolOutlined />, label: 'Bakım Görevleri' },
      { key: '/maintenance/history', icon: <HistoryOutlined />, label: 'Bakım Geçmişi' },
      { key: '/my-maintenance-requests', icon: <UserOutlined />, label: 'Bakım Taleplerim' },
    ],
  },
  {
    key: '/reports',
    icon: <BarChartOutlined />,
    label: 'Raporlar',
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
    '/maintenance/plans',
    '/maintenance/tasks',
    '/maintenance/history',
    '/my-maintenance-requests',
    '/reports',
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
  if (pathname.startsWith('/maintenance/tasks/')) return '/maintenance/tasks'

  return pathname
}

const getOpenMenuKeys = (pathname: string) => {
  if (pathname.startsWith('/assets') || pathname.startsWith('/inventory')) return ['inventory-menu']
  if (pathname.startsWith('/assignments') || pathname === '/my-assignments') return ['assignments-menu']
  if (pathname.startsWith('/stock')) return ['stock-menu']
  if (pathname.startsWith('/warranties')) return ['warranties-menu']
  if (pathname.startsWith('/licenses') || pathname === '/my-licenses') return ['licenses-menu']
  if (pathname.startsWith('/maintenance') || pathname === '/my-maintenance-requests') return ['maintenance-menu']
  if (pathname.startsWith('/admin')) return ['admin-menu']

  return []
}

function MainLayout() {
  const [collapsed, setCollapsed] = useState(false)
  const location = useLocation()
  const navigate = useNavigate()

  const handleMenuClick: MenuProps['onClick'] = ({ key }) => {
    if (key.startsWith('/')) {
      void navigate(key)
    }
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
          items={menuItems}
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
          <Typography.Text strong>Donanım ve Lisans Takip Sistemi</Typography.Text>
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
