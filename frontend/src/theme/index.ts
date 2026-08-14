import type { ThemeConfig } from 'antd'

export const appTheme: ThemeConfig = {
  token: {
    colorPrimary: '#1677ff',
    colorBgLayout: '#f0f2f5',
    borderRadius: 8,
    controlHeight: 36,
    fontFamily: "Inter, -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif",
  },
  components: {
    Button: {
      fontWeight: 500,
    },
    Card: {
      headerFontSize: 16,
    },
    Layout: {
      headerBg: '#ffffff',
    },
  },
}
