<script setup lang="ts">

import axios from "axios";
import {getCurrentInstance, ref, watch} from "vue";
import ReleaseItem from "./ReleaseItem.vue";
import type { Software } from "../types/Software"
import type { Release } from "../types/Release"

const instance = getCurrentInstance()
const serverUrl = instance?.appContext.config.globalProperties.$serverUrl as string

const props = defineProps({
  softwareUuid: {
    type: String,
    required: true
  }
})

const software = ref<Software | null>(null)
const releases = ref<Release[]>([])

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
  const res = await axios.get(`${serverUrl}/api/software/${props.softwareUuid}/releases`)
  const data = res.data
  releases.value = (data || []).map((item: any) => ({
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
}

watch(() => props.softwareUuid, (newVal) => {
  if (newVal) {
    loadSoftware()
    loadReleases()
  }
}, { immediate: true })

</script>

<template>
  <div class="main-container">
    <div class="operation-container">
      <div>
        <p>{{ releases.length }} 条发布</p>
      </div>
      <div>
        <el-button type="default">
          筛选发布
        </el-button>
        <el-button type="primary">
          新建发布
        </el-button>
      </div>
    </div>
    <el-scrollbar wrap-style="display: flex; flex-direction: column; gap: 16px;">
      <ReleaseItem v-for="item in releases" :releaseUuid="item.softwareReleaseId"/>
    </el-scrollbar>
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
</style>