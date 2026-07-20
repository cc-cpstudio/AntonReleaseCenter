import { createApp } from 'vue'
import './style.css'
import App from './App.vue'
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
import Cookies from 'js-cookie'

const app = createApp(App)
app.use(ElementPlus)
app.config.globalProperties.$serverUrl = "http://localhost:5251"
app.config.globalProperties.$cookies = Cookies
app.mount('#app')
