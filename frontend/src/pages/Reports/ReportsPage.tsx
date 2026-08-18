import { ApartmentOutlined, CustomerServiceOutlined, DatabaseOutlined, FileProtectOutlined, LaptopOutlined, SafetyCertificateOutlined, ToolOutlined } from '@ant-design/icons'
import { Button, Col, Row, Typography } from 'antd'
import { useNavigate } from 'react-router-dom'
import { ContentCard, PageHeader } from '../../components'
import './ReportsPage.css'

const reports = [
  { path: '/reports/inventory', title: 'Envanter Raporu', description: 'Cihazları kategori, durum ve lokasyona göre inceleyin.', icon: <LaptopOutlined /> },
  { path: '/reports/assignments', title: 'Zimmet Raporu', description: 'Aktif ve iade edilmiş zimmet kayıtlarını raporlayın.', icon: <ApartmentOutlined /> },
  { path: '/reports/stock', title: 'Stok Raporu', description: 'Stok seviyelerini ve kritik ürünleri görüntüleyin.', icon: <DatabaseOutlined /> },
  { path: '/reports/maintenance', title: 'Bakım Raporu', description: 'Bakım görev ve taleplerini metrikleriyle değerlendirin.', icon: <ToolOutlined /> },
  { path: '/reports/warranties', title: 'Garanti Raporu', description: 'Garanti ve cihaz kullanım durumlarını birlikte inceleyin.', icon: <SafetyCertificateOutlined /> },
  { path: '/reports/licenses', title: 'Lisans Raporu', description: 'Lisans haklarını ve gerçek atama kullanımını raporlayın.', icon: <FileProtectOutlined /> },
  { path: '/reports/support-requests', title: 'Teknik Destek Raporu', description: 'Destek taleplerini durum, öncelik ve aktörleriyle değerlendirin.', icon: <CustomerServiceOutlined /> },
]

function ReportsPage() {
  const navigate = useNavigate()
  return <section><PageHeader title="Rapor Merkezi" description="Operasyonel verileri filtreleyin, inceleyin ve CSV olarak dışa aktarın." />
    <Row gutter={[16, 16]}>{reports.map((report) => <Col key={report.path} xs={24} md={12}><ContentCard><div className="report-center-card"><div className="report-center-icon">{report.icon}</div><div><Typography.Title level={4}>{report.title}</Typography.Title><Typography.Paragraph type="secondary">{report.description}</Typography.Paragraph><Button onClick={() => void navigate(report.path)} type="primary">Raporu Aç</Button></div></div></ContentCard></Col>)}</Row>
  </section>
}
export default ReportsPage
