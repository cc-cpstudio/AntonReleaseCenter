<script setup lang="ts">

import {House, Menu, SwitchButton, UserFilled} from "@element-plus/icons-vue";
import axios from "axios";
import {getCurrentInstance, onMounted, ref} from "vue";
import Cookies from "js-cookie";
import router from "../router";
import ThemeToggle from "../components/ThemeToggle.vue";
import Overview from "../components/Overview.vue";
import ReleaseList from "../components/ReleaseList.vue";
import type { Software } from "../types/Software"

const instance = getCurrentInstance()
const serverUrl = instance?.appContext.config.globalProperties.$serverUrl as string

const softwareList = ref<Software[]>([])
const username = ref('')
const currentMenuIndex = ref('__overview')

const loadSoftwareList = async () => {
  const response = await axios.get(`${serverUrl}/api/software`)
  softwareList.value = (response.data || []).map((item: { softwareId: any; appKey: any; name: any; description: any; isEnabled: any; }) => ({
    softwareId: item.softwareId,
    appKey: item.appKey,
    name: item.name,
    description: item.description,
    isEnabled: item.isEnabled,
  }))
}

const loadUserInfo = async () => {
  const token = Cookies.get('jwt_token')
  if (!token) {
    username.value = ''
    return
  }
  try {
    const response = await axios.post(`${serverUrl}/Auth/validate-token`, {token})
    if (response.data.isValid) {
      username.value = response.data.username
    }
  } catch {
    username.value = ''
  }
}

const logout = () => {
  Cookies.remove('jwt_token')
  router.push('/login')
}

const handleMenuSelect = (index: string) => {
  currentMenuIndex.value = index
}

loadSoftwareList()

onMounted(() => {
  loadUserInfo()
})

</script>

<template>
  <el-container style="height: 100vh">
      <el-header class="header-bar">
        <div class="header-left">
          <span class="project-name">AntonReleaseCenter</span>
        </div>
        <div class="header-right">
          <el-link :underline="false">软件</el-link>
          <el-link :underline="false">渠道</el-link>
          <el-link :underline="false">设置</el-link>
          <el-link :underline="false">关于</el-link>
          <span class="header-username">
            <el-icon><UserFilled /></el-icon>
            <span>{{ username }}</span>
          </span>
          <el-link :underline="false" @click="logout">
            <el-icon><SwitchButton /></el-icon>
            <span>退出登录</span>
          </el-link>
          <ThemeToggle />
        </div>
      </el-header>
      <el-container style="flex: 1; overflow: hidden;">
        <el-aside width="200px">
          <el-menu style="width: 100%" :default-active="currentMenuIndex" @select="handleMenuSelect">
            <el-menu-item index="__overview">
              <el-icon><House /></el-icon>
              <span>概览</span>
            </el-menu-item>
            <el-menu-item v-for="item in softwareList" :index="item.softwareId">
              <el-icon><Menu /></el-icon>
              <span>{{ item.name }}</span>
            </el-menu-item>
          </el-menu>
        </el-aside>
        <el-main>
          <el-scrollbar style="height: 100%;">
            <div style="overflow: hidden; margin: 16px">
              <Overview v-if="currentMenuIndex == '__overview'" @select-software="handleMenuSelect"/>
              <ReleaseList v-else :softwareUuid="currentMenuIndex"/>
            </div>
          </el-scrollbar>
        </el-main>
      </el-container>
    </el-container>
</template>

<style scoped>
.header-bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 20px;
}

.header-left {
  display: flex;
  align-items: center;
}

.project-name {
  font-size: 20px;
  font-weight: bold;
  white-space: nowrap;
}

.header-right {
  display: flex;
  align-items: center;
  gap: 16px;
}

.header-username {
  display: flex;
  align-items: center;
  gap: 4px;
}
</style>