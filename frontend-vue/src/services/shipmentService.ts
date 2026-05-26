import { apiClient } from './apiClient'
import type { Shipment, Customer, CreateShipmentInput } from '../types'

const API_NET = import.meta.env.VITE_API_NET || 'http://localhost:5000'

export const shipmentService = {
  async getShipments(): Promise<Shipment[]> {
    return apiClient.get<Shipment[]>(`${API_NET}/api/shipments`)
  },

  async getCustomers(): Promise<Customer[]> {
    return apiClient.get<Customer[]>(`${API_NET}/api/customers`)
  },

  async createShipment(shipmentData: CreateShipmentInput): Promise<Shipment> {
    return apiClient.post<Shipment>(`${API_NET}/api/shipments`, shipmentData)
  },

  async deleteShipment(id: number): Promise<void> {
    return apiClient.delete<void>(`${API_NET}/api/shipments/${id}`)
  }
}
