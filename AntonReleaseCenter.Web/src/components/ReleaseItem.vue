<script setup lang="ts">

import {getCurrentInstance, ref} from "vue";
import axios from "axios";
import { ElMessage, ElMessageBox } from "element-plus";
import {Delete, Edit} from "@element-plus/icons-vue";
import type { Release } from "../types/Release"
import type { Channel } from "../types/Channel"
import { versionToString } from "../types/Version"
import {platformLabel} from "../types/platformLabel.ts";

const instance = getCurrentInstance()
const serverUrl = instance?.appContext.config.globalProperties.$serverUrl as string

const props = defineProps({
  releaseUuid: {
    type: String,
    required: true
  }
})

const emit = defineEmits(['refresh'])

const release = ref<Release | null>(null)
const channels = ref<Channel[]>([])

const editing = ref(false)
const deleting = ref(false)
const editDialogVisible = ref(false)
const editFormRef = ref()
const editForm = ref({
  platform: null as number | null,
  channelId: null as string | null,
  versionMajor: 0,
  versionMinor: 0,
  versionBuild: 0,
  versionRevision: 0,
  updateLog: '',
  isForceUpdate: false,
  isOnline: true,
})
const editFile = ref<File | null>(null)

const platformOptions = Object.entries(platformLabel).map(([value, label]) => ({
  value: Number(value),
  label,
}))

const loadRelease = async () => {
  const res = await axios.get(`${serverUrl}/api/releases/${props.releaseUuid}`)
  const data = res.data
  release.value = {
    softwareReleaseId: data.releaseId,
    softwareId: data.softwareId,
    channelId: data.channelId,
    platform: data.platform,
    version: data.version,
    updateLog: data.updateLog,
    filePath: data.filePath,
    fileSize: data.filesize,
    fileHash: data.fileHash,
    isForceUpdate: data.isForceUpdate,
    releaseTime: data.releaseTime,
    isOnline: data.isOnline,
  }
}

const loadChannels = async () => {
  if (!release.value?.softwareId) return
  try {
    const res = await axios.get(`${serverUrl}/api/software/${release.value.softwareId}/channels`)
    channels.value = res.data || []
  } catch {
    channels.value = []
  }
}

const openEditDialog = async () => {
  await loadRelease()
  await loadChannels()

  if (release.value) {
    editForm.value = {
      platform: release.value.platform,
      channelId: release.value.channelId,
      versionMajor: release.value.version.major,
      versionMinor: release.value.version.minor,
      versionBuild: release.value.version.build,
      versionRevision: release.value.version.revision,
      updateLog: release.value.updateLog,
      isForceUpdate: release.value.isForceUpdate,
      isOnline: release.value.isOnline,
    }
  }
  editFile.value = null
  editDialogVisible.value = true
}

const handleEditFileChange = (uploadFile: any) => {
  editFile.value = uploadFile.raw ?? null
}

const handleEditFileRemove = () => {
  editFile.value = null
}

const submitEdit = async () => {
  const valid = await editFormRef.value?.validate().catch(() => false)
  if (!valid) return

  editing.value = true
  try {
    const formData = new FormData()
    formData.append('softwareId', release.value?.softwareId ?? '')
    formData.append('platform', String(editForm.value.platform))
    formData.append('channelId', editForm.value.channelId ?? '')
    formData.append('version', `${editForm.value.versionMajor}.${editForm.value.versionMinor}.${editForm.value.versionBuild}.${editForm.value.versionRevision}`)
    formData.append('updateLog', editForm.value.updateLog)
    formData.append('isForceUpdate', String(editForm.value.isForceUpdate))
    formData.append('isOnline', String(editForm.value.isOnline))
    if (editFile.value) {
      formData.append('file', editFile.value)
    }

    await axios.put(`${serverUrl}/api/releases/${props.releaseUuid}`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
    ElMessage.success('发布更新成功')
    editDialogVisible.value = false
    loadRelease()
    emit('refresh')
  } catch (err: any) {
    ElMessage.error(err?.response?.data?.message || '更新失败')
  } finally {
    editing.value = false
  }
}

const handleDelete = async () => {
  try {
    await ElMessageBox.confirm(
      '确定要删除该发布吗？此操作不可撤销，关联的文件也将被删除。',
      '删除确认',
      { confirmButtonText: '确定删除', cancelButtonText: '取消', type: 'warning' }
    )
  } catch {
    return
  }

  deleting.value = true
  try {
    await axios.delete(`${serverUrl}/api/releases/${props.releaseUuid}`)
    ElMessage.success('删除成功')
    emit('refresh')
  } catch {
    ElMessage.error('删除失败')
  } finally {
    deleting.value = false
  }
}

const editFormRules = {
  platform: [{ required: true, message: '请选择平台', trigger: 'change' }],
  channelId: [{ required: true, message: '请选择渠道', trigger: 'change' }],
}

const formatFileSize = (bytes: number) => {
  const mb = bytes / 1024 / 1024
  if (isNaN(mb) || mb < 1) return '< 1 MB'
  return `${mb.toFixed(2)} MB`
}

loadRelease()

</script>

<template>
  <el-card v-if="release" shadow="hover" style="width: 100%;">
    <div class="release-container">
      <div style="display: flex; flex-direction: row; gap: 4px; align-items: center;">
        <p>{{ versionToString(release.version) }}</p>
        <el-tag type="primary">支持平台：{{ platformLabel[release.platform] }}</el-tag>
      </div>
      <el-text truncated line-clamp="1">{{ release.updateLog }}</el-text>
      <div class="release-actions">
        <el-button round @click="openEditDialog">
          <el-icon><Edit /></el-icon>
          编辑
        </el-button>
        <el-button round type="danger" :loading="deleting" @click="handleDelete">
          <el-icon><Delete /></el-icon>
          删除
        </el-button>
      </div>
    </div>
  </el-card>

  <el-dialog v-model="editDialogVisible" title="编辑发布" width="600px" :close-on-click-modal="false">
    <el-form ref="editFormRef" :model="editForm" :rules="editFormRules" label-width="100px">
      <el-form-item label="平台" prop="platform">
        <el-select v-model="editForm.platform" placeholder="请选择平台" style="width: 100%;">
          <el-option v-for="opt in platformOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
        </el-select>
      </el-form-item>
      <el-form-item label="渠道" prop="channelId">
        <el-select v-model="editForm.channelId" placeholder="请选择渠道" style="width: 100%;">
          <el-option v-for="ch in channels" :key="ch.channelId" :label="ch.channelName" :value="ch.channelId" />
        </el-select>
      </el-form-item>
      <el-form-item label="版本号">
        <div style="display: flex; gap: 4px; align-items: center; width: 100%;">
          <el-input-number v-model="editForm.versionMajor" :min="0" style="flex: 1;" />
          <span>.</span>
          <el-input-number v-model="editForm.versionMinor" :min="0" style="flex: 1;" />
          <span>.</span>
          <el-input-number v-model="editForm.versionBuild" :min="0" style="flex: 1;" />
          <span>.</span>
          <el-input-number v-model="editForm.versionRevision" :min="0" style="flex: 1;" />
        </div>
      </el-form-item>
      <el-form-item label="更新日志">
        <el-input v-model="editForm.updateLog" type="textarea" :rows="4" placeholder="请输入更新日志" />
      </el-form-item>
      <el-form-item label="发布文件">
        <div v-if="release?.filePath && !editFile" style="margin-bottom: 8px;">
          <el-tag type="info">当前文件：{{ release.filePath }} ({{ formatFileSize(release.fileSize) }})</el-tag>
        </div>
        <el-upload
          :auto-upload="false"
          :limit="1"
          :file-list="editFile ? [{ name: editFile.name, size: editFile.size }] : []"
          :on-change="handleEditFileChange"
          :on-remove="handleEditFileRemove"
          accept="*"
        >
          <el-button type="primary">替换文件</el-button>
        </el-upload>
      </el-form-item>
      <el-form-item label="强制更新">
        <el-switch v-model="editForm.isForceUpdate" />
      </el-form-item>
      <el-form-item label="上线状态">
        <el-switch v-model="editForm.isOnline" />
      </el-form-item>
    </el-form>
    <template #footer>
      <el-button @click="editDialogVisible = false">取消</el-button>
      <el-button type="primary" :loading="editing" @click="submitEdit">保存修改</el-button>
    </template>
  </el-dialog>
</template>

<style scoped>
.release-container {
  display: flex;
  flex-direction: row;
  justify-content: space-between;
  align-content: center;
}

.release-actions {
  display: flex;
  flex-direction: row;
  gap: 8px;
  flex-shrink: 0;
}
</style>
