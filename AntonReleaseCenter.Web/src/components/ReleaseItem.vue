<script setup lang="ts">

import {getCurrentInstance, ref} from "vue";
import axios from "axios";
import {Edit} from "@element-plus/icons-vue";

interface Release {
  releaseUuid: string
  softwareUuid: string
  channelUuid: string
  platform: number
  version: string
  updateLog: string
  filePath: string
  fileSize: number
  fileHash: string
  isForceUpdate: boolean
  releaseTime: string
  isOnline: boolean
}

const instance = getCurrentInstance()
const serverUrl = instance?.appContext.config.globalProperties.$serverUrl as string

const props = defineProps({
  releaseUuid: {
    type: String,
    required: true
  }
})

const release = ref<Release | null>(null)

const loadRelease = async () => {
  const res = await axios.get(`${serverUrl}/api/releases/${props.releaseUuid}`)
  const data = res.data
  release.value = {
    releaseUuid: data.releaseId,
    softwareUuid: data.softwareId,
    channelUuid: data.channelId,
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

loadRelease()

</script>

<template>
  <el-card v-if="release" shadow="hover" style="width: 100%;">
    <div class="release-container">
      <div style="display: flex; flex-direction: row; gap: 4px; align-items: center;">
        <p>{{ release.version }}</p>
        <el-tag type="primary">支持平台：{{ release.platform }}</el-tag>
      </div>
      <el-text truncated line-clamp="1">{{ release.updateLog }}</el-text>
      <el-button round>
        <el-icon><Edit /></el-icon>
        编辑
      </el-button>
    </div>
  </el-card>
</template>

<style scoped>
.release-container {
  display: flex;
  flex-direction: row;
  justify-content: space-between;
  align-content: center;
}
</style>