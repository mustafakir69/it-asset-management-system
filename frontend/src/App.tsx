import { App as AntdApp, ConfigProvider } from 'antd'
import trTR from 'antd/locale/tr_TR'
import { RouterProvider } from 'react-router-dom'
import { AuthProvider } from './contexts/AuthProvider'
import { router } from './router'
import { appTheme } from './theme'

function App() {
  return (
    <ConfigProvider locale={trTR} theme={appTheme}>
      <AntdApp>
        <AuthProvider>
          <RouterProvider router={router} />
        </AuthProvider>
      </AntdApp>
    </ConfigProvider>
  )
}

export default App
