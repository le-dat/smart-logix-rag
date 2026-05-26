import { createRouter, createWebHistory } from 'vue-router'
const Dashboard = () => import('../views/Dashboard.vue')
const RiskPredictor = () => import('../views/RiskPredictor.vue')
const AiChatbot = () => import('../views/AiChatbot.vue')

const routes = [
  {
    path: '/',
    name: 'Dashboard',
    component: Dashboard
  },
  {
    path: '/predict',
    name: 'RiskPredictor',
    component: RiskPredictor
  },
  {
    path: '/chat',
    name: 'AiChatbot',
    component: AiChatbot
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router
