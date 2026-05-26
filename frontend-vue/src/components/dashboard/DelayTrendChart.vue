<script setup lang="ts">
import { computed, ref, onMounted, onUnmounted } from 'vue'
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  LineElement,
  PointElement,
  Title,
  Tooltip,
  Legend,
  Filler,
  type ChartData
} from 'chart.js'
import { Bar } from 'vue-chartjs'
import type { Shipment } from '../../types'
import { 
  DELAY_TREND_MONTHS, 
  DELAY_TREND_MONTH_NUMS, 
  DELAY_TREND_DEFAULT_TOTALS, 
  DELAY_TREND_DEFAULT_RATIOS 
} from '../../constants/mockData'

ChartJS.register(
  CategoryScale,
  LinearScale,
  BarElement,
  LineElement,
  PointElement,
  Title,
  Tooltip,
  Legend,
  Filler
)

const props = defineProps<{
  shipments: Shipment[]
}>()

// Reactive Theme tracker
const isDark = ref(false)

const handleThemeChange = (e: any) => {
  isDark.value = e.detail?.isDark
}

onMounted(() => {
  isDark.value = document.documentElement.classList.contains('dark')
  window.addEventListener('theme-changed', handleThemeChange as EventListener)
})

onUnmounted(() => {
  window.removeEventListener('theme-changed', handleThemeChange as EventListener)
})

// Derive monthly buckets from live shipment data
const monthlyTotals = computed(() => {
  const counts = Array(6).fill(0)
  for (const s of props.shipments) {
    const d = new Date(s.createdAt)
    const m = d.getMonth() + 1 // 1-based
    const idx = DELAY_TREND_MONTH_NUMS.indexOf(m)
    if (idx !== -1) counts[idx]++
  }
  return counts.map((c, i) => c > 0 ? c : DELAY_TREND_DEFAULT_TOTALS[i])
})

const monthlyDelays = computed(() =>
  monthlyTotals.value.map((total, i) => {
    return Math.round(total * DELAY_TREND_DEFAULT_RATIOS[i])
  })
)

const chartData = computed<ChartData<'bar'>>(() => {
  const barBg = isDark.value ? 'rgba(226, 232, 240, 0.08)' : 'rgba(25, 26, 25, 0.08)'
  const barBorder = isDark.value ? 'rgba(226, 232, 240, 0.25)' : 'rgba(25, 26, 25, 0.25)'
  
  return {
    labels: DELAY_TREND_MONTHS,
    datasets: [
      {
        type: 'bar' as const,
        label: 'Total Shipments',
        data: monthlyTotals.value,
        backgroundColor: barBg,
        borderColor: barBorder,
        borderWidth: 1.5,
        borderRadius: 6,
        borderSkipped: false,
        yAxisID: 'y',
        order: 2
      },
      {
        type: 'line' as const,
        label: 'Delayed Shipments',
        data: monthlyDelays.value,
        borderColor: '#f59e0b',
        backgroundColor: 'rgba(245, 158, 11, 0.08)',
        borderWidth: 2.5,
        pointRadius: 4.5,
        pointBackgroundColor: '#f59e0b',
        pointBorderColor: isDark.value ? '#1c1c1a' : '#ffffff',
        pointBorderWidth: 2,
        tension: 0.4,
        fill: true,
        yAxisID: 'y',
        order: 1
      } as any
    ]
  }
})

const chartOptions = computed(() => {
  const textSecondaryColor = isDark.value ? '#94a3b8' : '#706e64'
  const gridLineColor = isDark.value ? '#2a2c2a' : '#eae9e4'
  const tooltipBg = isDark.value ? '#202220' : '#191a19'
  const textPrimaryColor = isDark.value ? '#e2e8f0' : '#191a19'

  return {
    responsive: true,
    maintainAspectRatio: false,
    animation: { duration: 600, easing: 'easeInOutQuart' as const },
    interaction: { mode: 'index' as const, intersect: false },
    plugins: {
      legend: {
        position: 'top' as const,
        align: 'end' as const,
        labels: {
          font: { family: 'Inter', size: 14, weight: '700' as const },
          color: textSecondaryColor,
          boxWidth: 8,
          boxHeight: 8,
          borderRadius: 2,
          useBorderRadius: true,
          padding: 16
        }
      },
      tooltip: {
        backgroundColor: tooltipBg,
        titleColor: textPrimaryColor,
        bodyColor: textPrimaryColor,
        titleFont: { family: 'Inter', size: 14, weight: '700' as const },
        bodyFont: { family: 'Inter', size: 14 },
        padding: 10,
        cornerRadius: 8,
        borderColor: gridLineColor,
        borderWidth: 1,
        callbacks: {
          label: (ctx: any) => ` ${ctx.dataset.label}: ${ctx.formattedValue}`
        }
      }
    },
    scales: {
      x: {
        grid: { display: false },
        border: { color: gridLineColor },
        ticks: {
          font: { family: 'Inter', size: 14, weight: '700' as const },
          color: textSecondaryColor
        }
      },
      y: {
        beginAtZero: true,
        grid: { color: gridLineColor },
        border: { display: false },
        ticks: {
          font: { family: 'Inter', size: 14 },
          color: textSecondaryColor,
          precision: 0
        }
      }
    }
  }
})
</script>

<template>
  <div class="glass-card rounded-2xl p-5 shadow-sm border border-brand-border bg-card-bg">
    <!-- Header -->
    <div class="flex items-start justify-between mb-5">
      <div>
        <h3 class="text-sm font-black text-text-primary tracking-wide">Monthly Delay Trend</h3>
        <p class="text-sm text-text-secondary mt-0.5 font-medium">Total vs. delayed shipments — Jan to Jun 2026</p>
      </div>
      <!-- Legend indicator -->
      <div class="flex items-center gap-3 text-sm font-extrabold text-text-secondary select-none font-mono">
        <span class="flex items-center gap-1.5">
          <span 
            class="w-2.5 h-2.5 rounded-sm bg-brand-panel border border-brand-border inline-block"
            :class="isDark ? 'opacity-30' : ''"
          ></span>
          Total
        </span>

        <span class="flex items-center gap-1.5">
          <span class="w-2.5 h-2.5 rounded-full bg-amber-500 inline-block animate-pulse"></span>
          Delayed
        </span>
      </div>
    </div>

    <!-- Chart Canvas -->
    <div class="h-52">
      <Bar :data="chartData" :options="(chartOptions as any)" />
    </div>
  </div>
</template>
