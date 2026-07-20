<script setup lang="ts">
import { ref, onMounted, getCurrentInstance } from 'vue'
import Cookies from 'js-cookie'
import axios from 'axios'

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
  <div v-if="jwtTokenAvailable">
    <el-text>您好，{{ username }}</el-text>
    <el-button type="primary">进入管理页</el-button>
  </div>
  <div v-else>
    <el-text>请先登录</el-text>
    <el-button type="primary">登录</el-button>
  </div>
</template>

<style scoped>

</style>