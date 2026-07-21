<script setup lang="ts">
import { ref, onMounted, getCurrentInstance } from 'vue'
import Cookies from 'js-cookie'
import axios from 'axios'
import router from "../router";
import ThemeToggle from "../components/ThemeToggle.vue";

const jwtTokenAvailable = ref(false)
const username = ref('')

const instance = getCurrentInstance()
const serverUrl = instance?.appContext.config.globalProperties.$serverUrl as string

onMounted(async () => {
  const token = Cookies.get('jwt_token')
  if (!token) {
    jwtTokenAvailable.value = false
    username.value = ''
    return
  }

  try {
    const response = await axios.post(`${serverUrl}/Auth/validate-token`, { token })
    jwtTokenAvailable.value = response.data.isValid
    username.value = response.data.isValid ? response.data.username : ''
  } catch {
    jwtTokenAvailable.value = false
    username.value = ''
  }
})
</script>

<template>
  <div class="main-container">
    <div class="theme-toggle-wrapper">
      <ThemeToggle />
    </div>
    <div v-if="jwtTokenAvailable" class="content">
      <p class="title">您好，{{ username }}</p>
      <el-button class="button" type="primary" size="large" round @click="router.push('/release')">
        进入管理页
      </el-button>
    </div>
    <div v-else class="content">
      <p class="title">您好，请先登录</p>
      <el-button class="button" type="primary" size="large" round @click="router.push('/login')">
        登录
      </el-button>
    </div>
  </div>
</template>

<style scoped>
.main-container {
  width: 100%;
  height: 100vh;
  display: flex;
  justify-content: center;
  align-items: center;
  flex-direction: column;
  margin: 0;
  overflow: hidden;
  position: relative;
}

.main-container p {
  margin-bottom: 24px;
}

.theme-toggle-wrapper {
  position: absolute;
  top: 16px;
  right: 16px;
}

.content {
  text-align: center;
}

.title {
  font-size: 32px;
  font-weight: bold;
}

.button {
  width: 108px;
}
</style>