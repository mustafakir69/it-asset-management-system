export type StockStatus = 'Normal' | 'Kritik'

export interface StockItem {
  id: string
  itemCode: string
  name: string
  category: string
  brandModel: string
  unit: string
  currentQuantity: number
  minimumQuantity: number
  location: string
  isActive: boolean
  isCritical: boolean
}

export interface StockItemInput {
  itemCode: string
  name: string
  category: string
  brandModel: string
  unit: string
  currentQuantity: number
  minimumQuantity: number
  location: string
}

export type StockTransactionType = 'Giriş' | 'Çıkış'

export interface StockTransaction {
  id: string
  stockItemId: string
  transactionType: StockTransactionType
  quantity: number
  transactionDate: string
  performedByUserId: string
  performedByName: string
  recipientEmployeeId: string | null
  recipientEmployeeName: string | null
  note?: string
}

export interface StockTransactionListItem extends StockTransaction {
  itemCode: string
  itemName: string
}

export interface StockTransactionInput {
  transactionType: StockTransactionType
  quantity: number
  transactionDate: string
  recipientEmployeeId?: string
  note?: string
}
