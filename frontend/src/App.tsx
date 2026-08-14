import { App as AntdApp, ConfigProvider } from 'antd'
import trTR from 'antd/locale/tr_TR'
import { RouterProvider } from 'react-router-dom'
import { router } from './router'
import { appTheme } from './theme'

function App() {
  return (
    <ConfigProvider locale={trTR} theme={appTheme}>
      <AntdApp>
        <RouterProvider router={router} />
      </AntdApp>
    </ConfigProvider>
  )
}

export default App
