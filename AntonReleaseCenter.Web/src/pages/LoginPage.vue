<script setup lang="ts">
import { ref, reactive, getCurrentInstance } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import Cookies from 'js-cookie'
import axios from 'axios'
import ThemeToggle from '../components/ThemeToggle.vue'

const router = useRouter()
const instance = getCurrentInstance()
const serverUrl = instance?.appContext.config.globalProperties.$serverUrl as string

const loading = ref(false)
const loginForm = reactive({
  username: '',
  passwordHash: '',
})
const rules = {
  username: [{ required: true, message: '请输入用户名', trigger: 'blur' }],
  passwordHash: [{ required: true, message: '请输入密码', trigger: 'blur' }],
}

const formRef = ref()

async function handleLogin() {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  loading.value = true
  try {
    const response = await axios.post(`${serverUrl}/Auth/login`, {
      username: loginForm.username,
      passwordHash: loginForm.passwordHash,
    })
    Cookies.set('jwt_token', response.data.token, { expires: 7 })
    ElMessage.success('登录成功')
    router.push('/')
  } catch (error: any) {
    const message = error.response?.data || '登录失败，请稍后重试'
    ElMessage.error(message)
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="login-container">
    <div class="theme-toggle-wrapper">
      <ThemeToggle />
    </div>
    <div class="login-card">
      <h2 class="login-title">登录</h2>
      <el-form
        ref="formRef"
        :model="loginForm"
        :rules="rules"
        label-width="0"
        @keyup.enter="handleLogin"
      >
        <el-form-item prop="username">
          <el-input
            v-model="loginForm.username"
            placeholder="用户名"
            size="large"
            prefix-icon="User"
          />
        </el-form-item>
        <el-form-item prop="passwordHash">
          <el-input
            v-model="loginForm.passwordHash"
            type="password"
            placeholder="密码"
            size="large"
            prefix-icon="Lock"
            show-password
          />
        </el-form-item>
        <el-form-item>
          <el-button
            class="login-button"
            type="primary"
            size="large"
            round
            :loading="loading"
            @click="handleLogin"
          >
            登录
          </el-button>
        </el-form-item>
      </el-form>
    </div>
  </div>
</template>

<style scoped>
.login-container {
  width: 100%;
  height: 100vh;
  display: flex;
  justify-content: center;
  align-items: center;
  position: relative;
}

.theme-toggle-wrapper {
  position: absolute;
  top: 16px;
  right: 16px;
}

.login-card {
  width: 380px;
  padding: 40px;
  border-radius: 12px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.1);
}

html.dark .login-card {
  background-color: #1d1e1f;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.4);
}

.login-title {
  text-align: center;
  margin-bottom: 32px;
  font-size: 24px;
  font-weight: bold;
}

.login-button {
  width: 100%;
}
</style>
