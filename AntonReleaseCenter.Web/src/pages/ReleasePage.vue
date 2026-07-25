<script setup lang="ts">

import {House, Menu, UserFilled} from "@element-plus/icons-vue";
import axios from "axios";
import {ElMessage} from "element-plus";
import {getCurrentInstance, onMounted, ref} from "vue";
import Cookies from "js-cookie";
import router from "../router";
import ThemeToggle from "../components/ThemeToggle.vue";
import Overview from "../components/Overview.vue";
import ReleaseList from "../components/ReleaseList.vue";
import type { Software } from "../types/Software"
import type { Channel } from "../types/Channel"

const instance = getCurrentInstance()
const serverUrl = instance?.appContext.config.globalProperties.$serverUrl as string

const softwareList = ref<Software[]>([])
const username = ref('')
const currentMenuIndex = ref('__overview')

const softwareDialogVisible = ref(false)
const channelDialogVisible = ref(false)
const aboutDialogVisible = ref(false)
const settingsDialogVisible = ref(false)

const softwareTableData = ref<Software[]>([])
const softwareSaving = ref(false)
const editingSoftwareId = ref<string | null>(null)
const softwareForm = ref({
  appKey: '',
  name: '',
  description: '',
  isEnabled: true,
})
const softwareFormRef = ref()

const channelSoftwareId = ref('')
const channelTableData = ref<Channel[]>([])
const channelSaving = ref(false)
const channelLoading = ref(false)
const editingChannelId = ref<string | null>(null)
const channelForm = ref({
  channelCode: 0,
  channelName: '',
  grayScalePercent: 100,
})
const channelFormRef = ref()

async function sha256(message: string): Promise<string> {
  const encoder = new TextEncoder()
  const data = encoder.encode(message)
  const hashBuffer = await crypto.subtle.digest('SHA-256', data)
  const hashArray = Array.from(new Uint8Array(hashBuffer))
  return hashArray.map(b => b.toString(16).padStart(2, '0')).join('')
}

const adminTableData = ref<{ adminId: string; username: string }[]>([])
const adminLoading = ref(false)
const adminSaving = ref(false)
const editingAdminId = ref<string | null>(null)
const adminForm = ref({ username: '', password: '' })
const adminFormRef = ref()

const passwordForm = ref({ oldPassword: '', newPassword: '', confirmPassword: '' })
const passwordSaving = ref(false)
const passwordFormRef = ref()
const passwordRules = {
  oldPassword: [{ required: true, message: '请输入当前密码', trigger: 'blur' }],
  newPassword: [{ required: true, message: '请输入新密码', trigger: 'blur' }, { min: 6, message: '密码至少6位', trigger: 'blur' }],
  confirmPassword: [{ required: true, message: '请确认新密码', trigger: 'blur' }],
}

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

const loadSoftwareTable = async () => {
  const response = await axios.get(`${serverUrl}/api/software`)
  softwareTableData.value = response.data || []
}

const openSoftwareDialog = () => {
  resetSoftwareForm()
  loadSoftwareTable()
  softwareDialogVisible.value = true
}

const resetSoftwareForm = () => {
  editingSoftwareId.value = null
  softwareForm.value = { appKey: '', name: '', description: '', isEnabled: true }
  softwareFormRef.value?.resetFields()
}

const handleSoftwareEdit = (row: Software) => {
  editingSoftwareId.value = row.softwareId
  softwareForm.value = {
    appKey: row.appKey,
    name: row.name,
    description: row.description,
    isEnabled: row.isEnabled,
  }
}

const handleSoftwareDelete = async (row: Software) => {
  try {
    await axios.delete(`${serverUrl}/api/software/${row.softwareId}`)
    await loadSoftwareTable()
    await loadSoftwareList()
  } catch {
    // silently fail
  }
}

const submitSoftwareForm = async () => {
  softwareSaving.value = true
  try {
    if (editingSoftwareId.value) {
      await axios.put(`${serverUrl}/api/software/${editingSoftwareId.value}`, {
        name: softwareForm.value.name,
        description: softwareForm.value.description,
        isEnabled: softwareForm.value.isEnabled,
      })
    } else {
      await axios.post(`${serverUrl}/api/software`, softwareForm.value)
    }
    await loadSoftwareTable()
    await loadSoftwareList()
    resetSoftwareForm()
    softwareDialogVisible.value = false
  } catch {
    // silently fail
  } finally {
    softwareSaving.value = false
  }
}

const loadChannelTable = async () => {
  if (!channelSoftwareId.value) {
    channelTableData.value = []
    return
  }
  channelLoading.value = true
  try {
    const response = await axios.get(`${serverUrl}/api/software/${channelSoftwareId.value}/channels`)
    channelTableData.value = response.data || []
  } catch {
    channelTableData.value = []
  } finally {
    channelLoading.value = false
  }
}

const openChannelDialog = () => {
  channelSoftwareId.value = ''
  channelTableData.value = []
  resetChannelForm()
  channelDialogVisible.value = true
}

const onChannelSoftwareChange = () => {
  resetChannelForm()
  loadChannelTable()
}

const resetChannelForm = () => {
  editingChannelId.value = null
  channelForm.value = { channelCode: 0, channelName: '', grayScalePercent: 100 }
  channelFormRef.value?.resetFields()
}

const handleChannelEdit = (row: Channel) => {
  editingChannelId.value = row.channelId
  channelForm.value = {
    channelCode: row.channelCode,
    channelName: row.channelName,
    grayScalePercent: row.grayScalePercent,
  }
}

const handleChannelDelete = async (row: Channel) => {
  try {
    await axios.delete(`${serverUrl}/api/software/${channelSoftwareId.value}/channels/${row.channelId}`)
    await loadChannelTable()
  } catch {
    // silently fail
  }
}

const submitChannelForm = async () => {
  channelSaving.value = true
  try {
    if (editingChannelId.value) {
      await axios.put(`${serverUrl}/api/software/${channelSoftwareId.value}/channels/${editingChannelId.value}`, {
        channelName: channelForm.value.channelName,
        grayScalePercent: channelForm.value.grayScalePercent,
      })
    } else {
      await axios.post(`${serverUrl}/api/software/${channelSoftwareId.value}/channels`, {
        channelCode: channelForm.value.channelCode,
        channelName: channelForm.value.channelName,
        grayScalePercent: channelForm.value.grayScalePercent,
      })
    }
    await loadChannelTable()
    resetChannelForm()
  } catch {
    // silently fail
  } finally {
    channelSaving.value = false
  }
}

const loadAdminTable = async () => {
  adminLoading.value = true
  try {
    const response = await axios.get(`${serverUrl}/api/admin`)
    adminTableData.value = response.data || []
  } catch {
    adminTableData.value = []
  } finally {
    adminLoading.value = false
  }
}

const resetAdminForm = () => {
  editingAdminId.value = null
  adminForm.value = { username: '', password: '' }
  adminFormRef.value?.resetFields()
}

const handleAdminEdit = (row: { adminId: string; username: string }) => {
  editingAdminId.value = row.adminId
  adminForm.value = { username: row.username, password: '' }
}

const handleAdminDelete = async (row: { adminId: string }) => {
  try {
    await axios.delete(`${serverUrl}/api/admin/${row.adminId}`)
    await loadAdminTable()
    ElMessage.success('删除成功')
  } catch {
    ElMessage.error('删除失败')
  }
}

const submitAdminForm = async () => {
  if (!adminForm.value.username) {
    ElMessage.warning('请输入用户名')
    return
  }
  if (!adminForm.value.password) {
    ElMessage.warning('请输入密码')
    return
  }
  adminSaving.value = true
  try {
    const passwordHash = await sha256(adminForm.value.password)
    if (editingAdminId.value) {
      await axios.put(`${serverUrl}/api/admin/${editingAdminId.value}`, {
        username: adminForm.value.username,
        passwordHash,
      })
    } else {
      await axios.post(`${serverUrl}/api/admin`, {
        username: adminForm.value.username,
        passwordHash,
      })
    }
    await loadAdminTable()
    resetAdminForm()
    ElMessage.success(editingAdminId.value ? '修改成功' : '添加成功')
  } catch {
    ElMessage.error('操作失败')
  } finally {
    adminSaving.value = false
  }
}

const resetPasswordForm = () => {
  passwordForm.value = { oldPassword: '', newPassword: '', confirmPassword: '' }
  passwordFormRef.value?.resetFields()
}

const submitChangePassword = async () => {
  const valid = await passwordFormRef.value?.validate().catch(() => false)
  if (!valid) return
  if (passwordForm.value.newPassword !== passwordForm.value.confirmPassword) {
    ElMessage.error('两次输入的新密码不一致')
    return
  }
  passwordSaving.value = true
  try {
    const oldHash = await sha256(passwordForm.value.oldPassword)
    const newHash = await sha256(passwordForm.value.newPassword)
    await axios.post(`${serverUrl}/api/admin/change-password`, {
      oldPasswordHash: oldHash,
      newPasswordHash: newHash,
    })
    ElMessage.success('密码修改成功')
    resetPasswordForm()
  } catch (error: any) {
    const message = error.response?.data || '密码修改失败'
    ElMessage.error(message)
  } finally {
    passwordSaving.value = false
  }
}

const openSettingsDialog = () => {
  loadAdminTable()
  resetPasswordForm()
  resetAdminForm()
  settingsDialogVisible.value = true
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
          <el-link :underline="false" @click="openSoftwareDialog">软件</el-link>
          <el-link :underline="false" @click="openChannelDialog">渠道</el-link>
          <el-link :underline="false" @click="openSettingsDialog">设置</el-link>
          <el-link :underline="false" @click="aboutDialogVisible = true">关于</el-link>
          <span class="header-username">
            <el-icon><UserFilled /></el-icon>
            <span>{{ username }}</span>
          </span>
          <el-link :underline="false" @click="logout">
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

    <el-dialog v-model="softwareDialogVisible" title="软件管理" width="700px" :close-on-click-modal="false">
      <div style="margin-bottom: 16px;">
        <el-form ref="softwareFormRef" :model="softwareForm" inline>
          <el-form-item label="标识">
            <el-input v-model="softwareForm.appKey" :disabled="!!editingSoftwareId" style="width: 120px;" />
          </el-form-item>
          <el-form-item label="名称">
            <el-input v-model="softwareForm.name" style="width: 140px;" />
          </el-form-item>
          <el-form-item label="描述">
            <el-input v-model="softwareForm.description" style="width: 140px;" />
          </el-form-item>
          <el-form-item label="启用">
            <el-switch v-model="softwareForm.isEnabled" />
          </el-form-item>
          <el-form-item>
            <el-button type="primary" :loading="softwareSaving" @click="submitSoftwareForm">
              {{ editingSoftwareId ? '保存' : '新增' }}
            </el-button>
            <el-button v-if="editingSoftwareId" @click="resetSoftwareForm">取消编辑</el-button>
          </el-form-item>
        </el-form>
      </div>
      <el-table :data="softwareTableData" stripe size="small" style="width: 100%;">
        <el-table-column prop="name" label="名称" width="140" />
        <el-table-column prop="appKey" label="标识" width="140" />
        <el-table-column prop="description" label="描述" min-width="160" />
        <el-table-column label="启用" width="80">
          <template #default="{ row }">
            <el-tag :type="row.isEnabled ? 'success' : 'danger'" size="small">
              {{ row.isEnabled ? '是' : '否' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="140">
          <template #default="{ row }">
            <el-button type="primary" size="small" @click="handleSoftwareEdit(row)">编辑</el-button>
            <el-button type="danger" size="small" @click="handleSoftwareDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-dialog>

    <el-dialog v-model="channelDialogVisible" title="渠道管理" width="700px" :close-on-click-modal="false">
      <div style="margin-bottom: 16px;">
        <el-form ref="channelFormRef" :model="channelForm" inline>
          <el-form-item label="软件">
            <el-select
              v-model="channelSoftwareId"
              placeholder="请选择软件"
              style="width: 180px;"
              @change="onChannelSoftwareChange"
            >
              <el-option
                v-for="sw in softwareList"
                :key="sw.softwareId"
                :label="sw.name"
                :value="sw.softwareId"
              />
            </el-select>
          </el-form-item>
          <el-form-item label="渠道号">
            <el-input-number
              v-model="channelForm.channelCode"
              :min="0"
              :disabled="!!editingChannelId"
              style="width: 120px;"
            />
          </el-form-item>
          <el-form-item label="渠道名">
            <el-input v-model="channelForm.channelName" style="width: 120px;" />
          </el-form-item>
          <el-form-item label="灰度百分比">
            <el-input-number v-model="channelForm.grayScalePercent" :min="0" :max="100" style="width: 120px;" />
          </el-form-item>
          <el-form-item>
            <el-button
              type="primary"
              :loading="channelSaving"
              :disabled="!channelSoftwareId"
              @click="submitChannelForm"
            >
              {{ editingChannelId ? '保存' : '新增' }}
            </el-button>
            <el-button v-if="editingChannelId" @click="resetChannelForm">取消编辑</el-button>
          </el-form-item>
        </el-form>
      </div>
      <el-table
        :data="channelTableData"
        stripe
        size="small"
        style="width: 100%;"
        v-loading="channelLoading"
        empty-text="请先选择软件"
      >
        <el-table-column prop="channelCode" label="渠道号" width="100" />
        <el-table-column prop="channelName" label="渠道名" min-width="160" />
        <el-table-column label="灰度百分比" width="120">
          <template #default="{ row }"> 
            {{ row.grayScalePercent }}%
          </template>
        </el-table-column>
        <el-table-column label="操作" width="140">
          <template #default="{ row }">
            <el-button type="primary" size="small" @click="handleChannelEdit(row)">编辑</el-button>
            <el-button type="danger" size="small" @click="handleChannelDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-dialog>

    <el-dialog v-model="aboutDialogVisible" title="关于项目" width="500px" :close-on-click-modal="false">
      <el-descriptions :column="1" border>
        <el-descriptions-item label="项目名称">AntonReleaseCenter</el-descriptions-item>
        <el-descriptions-item label="项目简介">软件版本管理中心</el-descriptions-item>
        <el-descriptions-item label="技术栈">Vue 3 + Element Plus + .NET</el-descriptions-item>
      </el-descriptions>
      <template #footer>
        <el-button type="primary" @click="aboutDialogVisible = false">关闭</el-button>
      </template>
    </el-dialog>

    <el-dialog v-model="settingsDialogVisible" title="设置" width="750px" :close-on-click-modal="false">
      <el-tabs>
        <el-tab-pane label="修改密码">
          <el-form ref="passwordFormRef" :model="passwordForm" :rules="passwordRules" label-width="100px" style="max-width: 420px;">
            <el-form-item label="当前密码" prop="oldPassword">
              <el-input v-model="passwordForm.oldPassword" type="password" show-password />
            </el-form-item>
            <el-form-item label="新密码" prop="newPassword">
              <el-input v-model="passwordForm.newPassword" type="password" show-password />
            </el-form-item>
            <el-form-item label="确认密码" prop="confirmPassword">
              <el-input v-model="passwordForm.confirmPassword" type="password" show-password />
            </el-form-item>
            <el-form-item>
              <el-button type="primary" :loading="passwordSaving" @click="submitChangePassword">修改密码</el-button>
            </el-form-item>
          </el-form>
        </el-tab-pane>
        <el-tab-pane label="管理员管理">
          <div style="margin-bottom: 16px;">
            <el-form ref="adminFormRef" :model="adminForm" inline>
              <el-form-item label="用户名">
                <el-input v-model="adminForm.username" style="width: 160px;" />
              </el-form-item>
              <el-form-item label="密码">
                <el-input v-model="adminForm.password" type="password" show-password style="width: 160px;" />
              </el-form-item>
              <el-form-item>
                <el-button type="primary" :loading="adminSaving" @click="submitAdminForm">
                  {{ editingAdminId ? '保存' : '新增' }}
                </el-button>
                <el-button v-if="editingAdminId" @click="resetAdminForm">取消编辑</el-button>
              </el-form-item>
            </el-form>
          </div>
          <el-table :data="adminTableData" stripe size="small" v-loading="adminLoading">
            <el-table-column prop="username" label="用户名" min-width="200" />
            <el-table-column label="操作" width="160">
              <template #default="{ row }">
                <el-button type="primary" size="small" @click="handleAdminEdit(row)">编辑</el-button>
                <el-button type="danger" size="small" @click="handleAdminDelete(row)">删除</el-button>
              </template>
            </el-table-column>
          </el-table>
        </el-tab-pane>
      </el-tabs>
      <template #footer>
        <el-button type="primary" @click="settingsDialogVisible = false">关闭</el-button>
      </template>
    </el-dialog>
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