import { createApp } from 'vue'
import './style.css'
import App from './App.vue'
import ElementPlus from 'element-plus'
import * as ElementPlusIconsVue from '@element-plus/icons-vue'
import 'element-plus/dist/index.css'
import 'element-plus/theme-chalk/dark/css-vars.css'
import axios from 'axios'
import Cookies from 'js-cookie'
import router from "./router";

// 全局 axios 请求拦截：自动带上 JWT 令牌
axios.interceptors.request.use((config) => {
  const token = Cookies.get('jwt_token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

const app = createApp(App)
app.use(ElementPlus)
for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
    app.component(key, component)
}
app.use(router)
app.config.globalProperties.$serverUrl = "http://localhost:5251"
app.config.globalProperties.$cookies = Cookies
app.mount('#app')
