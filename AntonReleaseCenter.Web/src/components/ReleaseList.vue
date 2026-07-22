<script setup lang="ts">

import axios from "axios";
import {computed, getCurrentInstance, ref, watch} from "vue";
import { ElMessage } from "element-plus";
import ReleaseItem from "./ReleaseItem.vue";
import type { Software } from "../types/Software"
import type { Release } from "../types/Release"
import type { Channel } from "../types/Channel"
import { platformLabel } from "../types/platformLabel"

const instance = getCurrentInstance()
const serverUrl = instance?.appContext.config.globalProperties.$serverUrl as string

const props = defineProps({
  softwareUuid: {
    type: String,
    required: true
  }
})

const software = ref<Software | null>(null)
const allReleases = ref<Release[]>([])
const channels = ref<Channel[]>([])
const releasing = ref(false)

const filterVisible = ref(false)
const filterPlatform = ref<number | null>(null)
const filterChannelId = ref<string | null>(null)
const filterIsOnline = ref<boolean | null>(null)
const filterIsForceUpdate = ref<boolean | null>(null)

const filteredReleases = computed(() => {
  return allReleases.value.filter(r => {
    if (filterPlatform.value != null && r.platform !== filterPlatform.value) return false
    if (filterChannelId.value && r.channelId !== filterChannelId.value) return false
    if (filterIsOnline.value != null && r.isOnline !== filterIsOnline.value) return false
    if (filterIsForceUpdate.value != null && r.isForceUpdate !== filterIsForceUpdate.value) return false
    return true
  })
})

const creating = ref(false)
const createDialogVisible = ref(false)
const createFormRef = ref()
const createForm = ref({
  platform: null as number | null,
  channelId: null as string | null,
  versionMajor: 1,
  versionMinor: 0,
  versionBuild: 0,
  versionRevision: 0,
  updateLog: '',
  isForceUpdate: false,
  isOnline: true,
})
const createFile = ref<File | null>(null)

const platformOptions = Object.entries(platformLabel).map(([value, label]) => ({
  value: Number(value),
  label,
}))

const loadSoftware = async () => {
  const res = await axios.get(`${serverUrl}/api/software/${props.softwareUuid}`)
  const data = res.data
  software.value = {
    softwareId: data.softwareId,
    appKey: data.appKey,
    name: data.name,
    description: data.description,
    isEnabled: data.isEnabled,
  }
}

const loadReleases = async () => {
  releasing.value = true
  try {
    const res = await axios.get(`${serverUrl}/api/software/${props.softwareUuid}/releases`)
    const data = res.data
    allReleases.value = (data || []).map((item: any) => ({
      softwareReleaseId: item.softwareReleaseId,
      softwareId: item.softwareId,
      channelId: item.channelId,
      platform: item.platform,
      version: item.version,
      updateLog: item.updateLog,
      filePath: item.filePath,
      fileSize: item.fileSize,
      fileHash: item.fileHash,
      isForceUpdate: item.isForceUpdate,
      releaseTime: item.releaseTime,
      isOnline: item.isOnline,
    }))
  } finally {
    releasing.value = false
  }
}

const loadChannels = async () => {
  try {
    const res = await axios.get(`${serverUrl}/api/software/${props.softwareUuid}/channels`)
    channels.value = res.data || []
  } catch {
    channels.value = []
  }
}

const toggleFilter = () => {
  filterVisible.value = !filterVisible.value
}

const resetFilter = () => {
  filterPlatform.value = null
  filterChannelId.value = null
  filterIsOnline.value = null
  filterIsForceUpdate.value = null
}

const resetCreateForm = () => {
  createForm.value = {
    platform: null,
    channelId: null,
    versionMajor: 1,
    versionMinor: 0,
    versionBuild: 0,
    versionRevision: 0,
    updateLog: '',
    isForceUpdate: false,
    isOnline: true,
  }
  createFile.value = null
}

const openCreateDialog = () => {
  resetCreateForm()
  createDialogVisible.value = true
}

const handleCreateFileChange = (uploadFile: any) => {
  createFile.value = uploadFile.raw ?? null
}

const handleCreateFileRemove = () => {
  createFile.value = null
}

const submitCreate = async () => {
  const valid = await createFormRef.value?.validate().catch(() => false)
  if (!valid) return

  creating.value = true
  try {
    const formData = new FormData()
    formData.append('softwareId', props.softwareUuid)
    formData.append('platform', String(createForm.value.platform))
    formData.append('channelId', createForm.value.channelId ?? '')
    formData.append('version', `${createForm.value.versionMajor}.${createForm.value.versionMinor}.${createForm.value.versionBuild}.${createForm.value.versionRevision}`)
    formData.append('updateLog', createForm.value.updateLog)
    formData.append('isForceUpdate', String(createForm.value.isForceUpdate))
    formData.append('isOnline', String(createForm.value.isOnline))
    if (createFile.value) {
      formData.append('file', createFile.value)
    }

    await axios.post(`${serverUrl}/api/releases`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
    ElMessage.success('发布创建成功')
    createDialogVisible.value = false
    loadReleases()
  } catch (err: any) {
    ElMessage.error(err?.response?.data?.message || '创建失败')
  } finally {
    creating.value = false
  }
}

const createFormRules = {
  platform: [{ required: true, message: '请选择平台', trigger: 'change' }],
  channelId: [{ required: true, message: '请选择渠道', trigger: 'change' }],
  versionMajor: [{ required: true, message: '请输入版本号', trigger: 'blur' }],
}

watch(() => props.softwareUuid, (newVal) => {
  if (newVal) {
    resetFilter()
    filterVisible.value = false
    loadSoftware()
    loadReleases()
    loadChannels()
  }
}, { immediate: true })

</script>

<template>
  <div class="main-container">
    <div class="operation-container">
      <div>
        <p>{{ filteredReleases.length }} 条发布</p>
      </div>
      <div style="display: flex; gap: 8px;">
        <el-button @click="toggleFilter">
          {{ filterVisible ? '收起筛选' : '筛选发布' }}
        </el-button>
        <el-button type="primary" @click="openCreateDialog">
          新建发布
        </el-button>
      </div>
    </div>

    <div v-if="filterVisible" class="filter-panel">
      <el-form :inline="true">
        <el-form-item label="平台">
          <el-select v-model="filterPlatform" placeholder="全部" clearable style="width: 160px;">
            <el-option v-for="opt in platformOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
          </el-select>
        </el-form-item>
        <el-form-item label="渠道">
          <el-select v-model="filterChannelId" placeholder="全部" clearable style="width: 160px;">
            <el-option v-for="ch in channels" :key="ch.channelId" :label="ch.channelName" :value="ch.channelId" />
          </el-select>
        </el-form-item>
        <el-form-item label="上线状态">
          <el-select v-model="filterIsOnline" placeholder="全部" clearable style="width: 120px;">
            <el-option label="已上线" :value="true" />
            <el-option label="未上线" :value="false" />
          </el-select>
        </el-form-item>
        <el-form-item label="强制更新">
          <el-select v-model="filterIsForceUpdate" placeholder="全部" clearable style="width: 120px;">
            <el-option label="是" :value="true" />
            <el-option label="否" :value="false" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button @click="resetFilter">重置</el-button>
        </el-form-item>
      </el-form>
    </div>

    <div v-if="releasing" class="loading-placeholder">
      <el-skeleton :rows="3" animated />
    </div>
    <el-scrollbar v-else wrap-style="display: flex; flex-direction: column; gap: 16px;">
      <ReleaseItem
        v-for="item in filteredReleases"
        :key="item.softwareReleaseId"
        :releaseUuid="item.softwareReleaseId"
        @refresh="loadReleases"
      />
    </el-scrollbar>

    <el-dialog v-model="createDialogVisible" title="新建发布" width="600px" :close-on-click-modal="false">
      <el-form ref="createFormRef" :model="createForm" :rules="createFormRules" label-width="100px">
        <el-form-item label="平台" prop="platform">
          <el-select v-model="createForm.platform" placeholder="请选择平台" style="width: 100%;">
            <el-option v-for="opt in platformOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
          </el-select>
        </el-form-item>
        <el-form-item label="渠道" prop="channelId">
          <el-select v-model="createForm.channelId" placeholder="请选择渠道" style="width: 100%;">
            <el-option v-for="ch in channels" :key="ch.channelId" :label="ch.channelName" :value="ch.channelId" />
          </el-select>
        </el-form-item>
        <el-form-item label="版本号" prop="versionMajor">
          <div style="display: flex; gap: 4px; align-items: center; width: 100%;">
            <el-input-number v-model="createForm.versionMajor" :min="0" style="flex: 1;" />
            <span>.</span>
            <el-input-number v-model="createForm.versionMinor" :min="0" style="flex: 1;" />
            <span>.</span>
            <el-input-number v-model="createForm.versionBuild" :min="0" style="flex: 1;" />
            <span>.</span>
            <el-input-number v-model="createForm.versionRevision" :min="0" style="flex: 1;" />
          </div>
        </el-form-item>
        <el-form-item label="更新日志">
          <el-input v-model="createForm.updateLog" type="textarea" :rows="4" placeholder="请输入更新日志" />
        </el-form-item>
        <el-form-item label="发布文件">
          <el-upload
            :auto-upload="false"
            :limit="1"
            :file-list="createFile ? [{ name: createFile.name, size: createFile.size }] : []"
            :on-change="handleCreateFileChange"
            :on-remove="handleCreateFileRemove"
            accept="*"
          >
            <el-button type="primary">选择文件</el-button>
          </el-upload>
        </el-form-item>
        <el-form-item label="强制更新">
          <el-switch v-model="createForm.isForceUpdate" />
        </el-form-item>
        <el-form-item label="立即上线">
          <el-switch v-model="createForm.isOnline" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="createDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="creating" @click="submitCreate">确认创建</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.main-container {
  display: flex;
  flex-direction: column;
  gap: 16px;
  justify-content: center;
  align-items: stretch;
  width: 100%;
}

.operation-container {
  display: flex;
  flex-direction: row;
  justify-content: space-between;
  align-items: center;
  width: 100%;
}

.filter-panel {
  padding: 16px;
  background: var(--el-fill-color-light);
  border-radius: 8px;
}

.loading-placeholder {
  padding: 16px 0;
}
</style>
