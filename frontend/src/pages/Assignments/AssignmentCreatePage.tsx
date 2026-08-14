import { ArrowLeftOutlined } from '@ant-design/icons'
import { App as AntdApp, Button, Col, DatePicker, Form, Input, Row, Select, Space } from 'antd'
import dayjs from 'dayjs'
import type { Dayjs } from 'dayjs'
import { useCallback, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ContentCard, ErrorState, LoadingState, PageHeader } from '../../components'
import { assignmentService } from '../../services/assignmentService'
import type { Asset } from '../../types/asset'
import type { CreateAssignmentInput, Employee } from '../../types/assignment'
import './AssignmentCreatePage.css'

interface AssignmentFormValues {
  assetId: string
  employeeId: string
  assignedAt: Dayjs
  assignedBy: string
  notes?: string
}

function AssignmentCreatePage() {
  const navigate = useNavigate()
  const { message } = AntdApp.useApp()
  const [form] = Form.useForm<AssignmentFormValues>()
  const [assignableAssets, setAssignableAssets] = useState<Asset[]>([])
  const [employees, setEmployees] = useState<Employee[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [loadError, setLoadError] = useState<string | null>(null)

  const loadAssignableAssets = useCallback(async () => {
    setIsLoading(true)
    setLoadError(null)

    try {
      const [assets, activeEmployees] = await Promise.all([
        assignmentService.getAssignableAssets(),
        assignmentService.getEmployees(),
      ])
      setAssignableAssets(assets)
      setEmployees(activeEmployees)
    } catch (error: unknown) {
      setLoadError(
        error instanceof Error
          ? error.message
          : 'Zimmet formu için gerekli veriler yüklenirken bir hata oluştu.',
      )
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    void loadAssignableAssets()
  }, [loadAssignableAssets])

  const handleSubmit = async (values: AssignmentFormValues) => {
    setIsSubmitting(true)

    const input: CreateAssignmentInput = {
      assetId: values.assetId,
      employeeId: values.employeeId,
      assignedAt: values.assignedAt.toISOString(),
      assignedBy: values.assignedBy.trim(),
      notes: values.notes?.trim() || null,
    }

    try {
      await assignmentService.createAssignment(input)
      message.success('Zimmet başarıyla oluşturuldu.')
      void navigate('/assignments')
    } catch (error: unknown) {
      message.error(error instanceof Error ? error.message : 'Zimmet oluşturulamadı.')
      await loadAssignableAssets()
    } finally {
      setIsSubmitting(false)
    }
  }

  const assetOptions = assignableAssets.map((asset) => ({
    label: `${asset.assetCode} · ${asset.brand} ${asset.model}`,
    value: asset.id,
  }))
  const employeeOptions = employees.map((employee) => ({
    label: `${employee.employeeNo} · ${employee.fullName} · ${employee.department}`,
    value: employee.id,
  }))

  return (
    <section className="assignment-create-page">
      <PageHeader
        title="Yeni Zimmet"
        description="Stokta bulunan uygun bir cihazı çalışana zimmetleyin."
        actions={
          <Button icon={<ArrowLeftOutlined />} onClick={() => void navigate('/assignments')}>
            Aktif Zimmetlere Dön
          </Button>
        }
      />

      <ContentCard>
        {loadError ? (
          <ErrorState message={loadError} onRetry={() => void loadAssignableAssets()} />
        ) : isLoading ? (
          <LoadingState message="Zimmete uygun cihazlar yükleniyor..." />
        ) : (
          <Form<AssignmentFormValues>
            form={form}
            initialValues={{ assignedAt: dayjs() }}
            layout="vertical"
            onFinish={(values) => void handleSubmit(values)}
            requiredMark="optional"
          >
            <Row gutter={[16, 0]}>
              <Col xs={24} md={12}>
                <Form.Item
                  label="Cihaz"
                  name="assetId"
                  rules={[{ required: true, message: 'Zimmetlenecek cihazı seçin.' }]}
                >
                  <Select<string>
                    disabled={assetOptions.length === 0}
                    optionFilterProp="label"
                    options={assetOptions}
                    placeholder={
                      assetOptions.length > 0
                        ? 'Cihaz seçin'
                        : 'Zimmete uygun cihaz bulunamadı'
                    }
                    showSearch
                  />
                </Form.Item>
              </Col>

              <Col xs={24} md={12}>
                <Form.Item
                  label="Çalışan"
                  name="employeeId"
                  rules={[{ required: true, message: 'Zimmetlenecek çalışanı seçin.' }]}
                >
                  <Select<string>
                    disabled={employeeOptions.length === 0}
                    optionFilterProp="label"
                    options={employeeOptions}
                    placeholder={
                      employeeOptions.length > 0 ? 'Çalışan seçin' : 'Aktif çalışan bulunamadı'
                    }
                    showSearch
                  />
                </Form.Item>
              </Col>

              <Col xs={24} md={12}>
                <Form.Item
                  label="Zimmet Tarihi"
                  name="assignedAt"
                  rules={[{ required: true, message: 'Zimmet tarihini seçin.' }]}
                >
                  <DatePicker
                    disabledDate={(current) => current.isAfter(dayjs(), 'day')}
                    format="DD.MM.YYYY"
                    placeholder="Zimmet tarihini seçin"
                  />
                </Form.Item>
              </Col>

              <Col xs={24} md={12}>
                <Form.Item
                  label="Zimmetleyen"
                  name="assignedBy"
                  rules={[{ required: true, whitespace: true, message: 'Zimmetleyeni girin.' }]}
                >
                  <Input placeholder="İşlemi yapan personel" />
                </Form.Item>
              </Col>

              <Col xs={24}>
                <Form.Item label="Açıklama / Not" name="notes">
                  <Input.TextArea
                    maxLength={500}
                    placeholder="Zimmetle ilgili isteğe bağlı açıklama"
                    rows={4}
                    showCount
                  />
                </Form.Item>
              </Col>
            </Row>

            <div className="assignment-create-actions">
              <Space wrap>
                <Button disabled={isSubmitting} onClick={() => void navigate('/assignments')}>
                  İptal
                </Button>
                <Button
                  disabled={assetOptions.length === 0 || employeeOptions.length === 0}
                  htmlType="submit"
                  loading={isSubmitting}
                  type="primary"
                >
                  Zimmeti Oluştur
                </Button>
              </Space>
            </div>
          </Form>
        )}
      </ContentCard>
    </section>
  )
}

export default AssignmentCreatePage
