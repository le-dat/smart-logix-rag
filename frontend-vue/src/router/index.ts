import { createRouter, createWebHistory } from 'vue-router'
import Dashboard from '../views/Dashboard.vue'
import RiskPredictor from '../views/RiskPredictor.vue'
import AiChatbot from '../views/AiChatbot.vue'

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
