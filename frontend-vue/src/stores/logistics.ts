import { ref } from 'vue'
import { defineStore } from 'pinia'
import { shipmentService } from '../services/shipmentService'
import type { Shipment, Customer, CreateShipmentInput } from '../types'

export const useLogisticsStore = defineStore('logistics', () => {
  const shipments = ref<Shipment[]>([])
  const customers = ref<Customer[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  const fetchShipments = async () => {
    loading.value = true
    error.value = null
    try {
      shipments.value = await shipmentService.getShipments()
    } catch (err: any) {
      error.value = err.message || 'Error loading shipments'
      console.error(err)
    } finally {
      loading.value = false
    }
  }

  const fetchCustomers = async () => {
    try {
      customers.value = await shipmentService.getCustomers()
    } catch (err) {
      console.error(err)
    }
  }

  const addShipment = async (shipmentData: CreateShipmentInput) => {
    loading.value = true
    try {
      const newShipment = await shipmentService.createShipment(shipmentData)
      shipments.value.unshift(newShipment)
      return newShipment
    } catch (err: any) {
      error.value = err.message || 'Error creating shipment'
      throw err
    } finally {
      loading.value = false
    }
  }

  const deleteShipment = async (id: number) => {
    try {
      await shipmentService.deleteShipment(id)
      shipments.value = shipments.value.filter(s => s.id !== id)
    } catch (err: any) {
      error.value = err.message || 'Error deleting shipment'
      throw err
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
