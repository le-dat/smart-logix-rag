import { ref } from 'vue'
import { defineStore } from 'pinia'

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

export const useLogisticsStore = defineStore('logistics', () => {
  const shipments = ref<Shipment[]>([])
  const customers = ref<Customer[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  const apiNet = import.meta.env.VITE_API_NET || 'http://localhost:5000'

  const fetchShipments = async () => {
    loading.value = true
    error.value = null
    try {
      const response = await fetch(`${apiNet}/api/shipments`)
      if (!response.ok) throw new Error('Failed to fetch shipments')
      shipments.value = await response.json()
    } catch (err: any) {
      error.value = err.message || 'Error loading shipments'
      console.error(err)
    } finally {
      loading.value = false
    }
  }

  const fetchCustomers = async () => {
    try {
      const response = await fetch(`${apiNet}/api/customers`)
      if (!response.ok) throw new Error('Failed to fetch customers')
      customers.value = await response.json()
    } catch (err) {
      console.error(err)
    }
  }

  const addShipment = async (shipmentData: any) => {
    loading.value = true
    try {
      const response = await fetch(`${apiNet}/api/shipments`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(shipmentData)
      })
      if (!response.ok) {
        const errBody = await response.json()
        throw new Error(errBody.message || 'Failed to create shipment')
      }
      const newShipment = await response.json()
      shipments.value.unshift(newShipment)
      return newShipment
    } catch (err: any) {
      error.value = err.message || 'Error creating shipment'
      throw err;
    } finally {
      loading.value = false
    }
  }

  const deleteShipment = async (id: number) => {
    try {
      const response = await fetch(`${apiNet}/api/shipments/${id}`, {
        method: 'DELETE'
      })
      if (!response.ok) throw new Error('Failed to delete shipment')
      shipments.value = shipments.value.filter(s => s.id !== id)
    } catch (err: any) {
      error.value = err.message || 'Error deleting shipment'
      throw err;
    }
  }

  return {
    shipments,
    customers,
    loading,
    error,
    fetchShipments,
    fetchCustomers,
    addShipment,
    deleteShipment
  }
})
