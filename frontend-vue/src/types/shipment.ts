export interface Route {
  id: number
  source: string
  destination: string
  averageDuration: number
}

export interface Customer {
  id: number
  name: string
  email: string
  phone: string
}

export interface Shipment {
  id: number
  trackingNo: string
  sender: string
  receiver: string
  routeId: number
  route?: Route
  customerId?: number
  customer?: Customer
  weight: number
  status: string
  createdAt: string
  riskScore?: number
  riskLevel?: string
}

export interface CreateShipmentInput {
  trackingNo: string
  sender: string
  receiver: string
  routeId: number
  customerId: number
  weight: number
  status: string
}
