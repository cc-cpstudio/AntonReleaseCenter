<script setup lang="ts">

import { computed, getCurrentInstance, onMounted, ref } from "vue"
import axios from "axios"
import StatCard from "./overview/StatCard.vue"
import PlatformChart from "./overview/PlatformChart.vue"
import RecentReleases from "./overview/RecentReleases.vue"
import SoftwareStatusTable from "./overview/SoftwareStatusTable.vue"
import type { Software } from "../types/Software"
import type { Release } from "../types/Release"
import type { Channel } from "../types/Channel"
import type { SoftwareStatus } from "../types/SoftwareStatus"
import type { RecentReleaseItem } from "../types/RecentReleaseItem"
import { versionToString } from "../types/Version"

const emit = defineEmits<{
  selectSoftware: [softwareId: string]
}>()

const instance = getCurrentInstance()
const serverUrl = instance?.appContext.config.globalProperties.$serverUrl as string

const loading = ref(true)
const softwareList = ref<Software[]>([])
const allReleases = ref<Release[]>([])
const allChannels = ref<Channel[]>([])
const softwareNameMap = ref<Record<string, string>>({})

const softwareCount = computed(() => softwareList.value.length)
const totalReleases = computed(() => allReleases.value.length)
const onlineReleases = computed(() => allReleases.value.filter((r) => r.isOnline).length)
const totalChannels = computed(() => allChannels.value.length)

const platformGroup = (platform: number): string => {
  if (platform <= 2) return "Windows"
  if (platform <= 4) return "macOS"
  return "Linux"
}

const platformDistribution = computed(() => {
  const map: Record<string, number> = {}
  for (const r of allReleases.value) {
    const group = platformGroup(r.platform)
    map[group] = (map[group] || 0) + 1
  }
  return Object.entries(map).map(([name, value]) => ({ name, value }))
})

const recentReleases = computed<RecentReleaseItem[]>(() => {
  const sorted = [...allReleases.value].sort(
    (a, b) => new Date(b.releaseTime).getTime() - new Date(a.releaseTime).getTime()
  )
  return sorted.slice(0, 10).map((r) => ({
    softwareName: softwareNameMap.value[r.softwareId] ?? r.softwareId,
    version: versionToString(r.version),
    platform: r.platform,
    updateLog: r.updateLog,
    releaseTime: r.releaseTime,
    isOnline: r.isOnline,
  }))
})

const softwareStatusList = computed<SoftwareStatus[]>(() => {
  return softwareList.value.map((sw) => {
    const swReleases = allReleases.value.filter((r) => r.softwareId === sw.softwareId)
    const sorted = swReleases.sort(
      (a, b) => new Date(b.releaseTime).getTime() - new Date(a.releaseTime).getTime()
    )
    const swChannels = allChannels.value.filter((c) => c.softwareId === sw.softwareId)
    return {
      softwareId: sw.softwareId,
      name: sw.name,
      description: sw.description,
      isEnabled: sw.isEnabled,
      latestVersion: sorted.length > 0 ? versionToString(sorted[0].version) : "",
      latestReleaseTime: sorted.length > 0 ? sorted[0].releaseTime : "",
      releaseCount: swReleases.length,
      onlineCount: swReleases.filter((r) => r.isOnline).length,
      channelCount: swChannels.length,
    }
  })
})

const loadAllData = async () => {
  loading.value = true
  try {
    const swResponse = await axios.get(`${serverUrl}/api/software`)
    const rawSoftware = swResponse.data as any[]
    softwareList.value = rawSoftware.map((item) => ({
      softwareId: item.softwareId,
      appKey: item.appKey,
      name: item.name,
      description: item.description,
      isEnabled: item.isEnabled,
    }))

    for (const sw of softwareList.value) {
      softwareNameMap.value[sw.softwareId] = sw.name
    }

    const releasePromises = softwareList.value.map((sw) =>
      axios.get(`${serverUrl}/api/software/${sw.softwareId}/releases`)
    )
    const channelPromises = softwareList.value.map((sw) =>
      axios.get(`${serverUrl}/api/software/${sw.softwareId}/channels`)
    )

    const releaseResults = await Promise.allSettled(releasePromises)
    const channelResults = await Promise.allSettled(channelPromises)

    const releases: Release[] = []
    for (const result of releaseResults) {
      if (result.status === "fulfilled") {
        const items = (result.value.data || []) as any[]
        releases.push(
          ...items.map((item) => ({
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
        )
      }
    }
    allReleases.value = releases

    const channels: Channel[] = []
    for (const result of channelResults) {
      if (result.status === "fulfilled") {
        const items = (result.value.data || []) as any[]
        channels.push(
          ...items.map((item) => ({
            channelId: item.channelId,
            softwareId: item.softwareId,
            channelCode: item.channelCode,
            channelName: item.channelName,
            grayScalePercent: item.grayScalePercent,
          }))
        )
      }
    }
    allChannels.value = channels
  } finally {
    loading.value = false
  }
}

const handleSoftwareSelect = (softwareId: string) => {
  emit("selectSoftware", softwareId)
}

onMounted(() => {
  loadAllData()
})

</script>

<template>
  <div v-loading="loading" class="overview-container">
    <template v-if="!loading">
      <div class="stat-cards-row">
        <StatCard title="软件总数" :value="softwareCount" color="#409eff" />
        <StatCard title="总发布数" :value="totalReleases" color="#67c23a" />
        <StatCard title="在线发布" :value="onlineReleases" color="#e6a23c" />
        <StatCard title="渠道总数" :value="totalChannels" color="#f56c6c" />
      </div>
      <div class="chart-and-releases">
        <PlatformChart :data="platformDistribution" />
        <RecentReleases :releases="recentReleases" />
      </div>
      <SoftwareStatusTable :software-list="softwareStatusList" @select="handleSoftwareSelect" />
    </template>
  </div>
</template>

<style scoped>
.overview-container {
  display: flex;
  flex-direction: column;
  gap: 16px;
  min-height: 200px;
}

.stat-cards-row {
  display: flex;
  flex-direction: row;
  gap: 16px;
  flex-wrap: wrap;
}

.chart-and-releases {
  display: flex;
  flex-direction: row;
  gap: 16px;
  flex-wrap: wrap;
}
</style>
