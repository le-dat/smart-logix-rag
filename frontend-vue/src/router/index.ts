import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const Dashboard = () => import('../views/Dashboard.vue')
const RiskPredictor = () => import('../views/RiskPredictor.vue')
const AiChatbot = () => import('../views/AiChatbot.vue')
const Login = () => import('../views/Login.vue')

const routes = [
  {
    path: '/login',
    name: 'Login',
    component: Login,
    meta: { requiresAuth: false }
  },
  {
    path: '/',
    name: 'Dashboard',
    component: Dashboard,
    meta: { requiresAuth: true }
  },
  {
    path: '/predict',
    name: 'RiskPredictor',
    component: RiskPredictor,
    meta: { requiresAuth: true }
  },
  {
    path: '/chat',
    name: 'AiChatbot',
    component: AiChatbot,
    meta: { requiresAuth: true }
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

// ─── Global Navigation Guard ─────────────────────────────────────────────────
router.beforeEach((to) => {
  const authStore = useAuthStore()

  // If route requires auth and user is not authenticated, redirect to login
  if (to.meta.requiresAuth !== false && !authStore.isAuthenticated) {
    return { name: 'Login', query: { redirect: to.fullPath } }
  }

  // If already authenticated and navigating to login, redirect to dashboard
  if (to.name === 'Login' && authStore.isAuthenticated) {
    return { name: 'Dashboard' }
  }
})

export default router
