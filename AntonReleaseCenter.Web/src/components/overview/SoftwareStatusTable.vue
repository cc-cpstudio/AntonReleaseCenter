<script setup lang="ts">

interface SoftwareStatus {
  softwareId: string
  name: string
  description: string
  isEnabled: boolean
  latestVersion: string
  latestReleaseTime: string
  releaseCount: number
  onlineCount: number
  channelCount: number
}

defineProps<{
  softwareList: SoftwareStatus[]
}>()

const emit = defineEmits<{
  select: [softwareId: string]
}>()

const formatTime = (time: string) => {
  if (!time) return "-"
  const date = new Date(time)
  return date.toLocaleString("zh-CN", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  })
}

</script>

<template>
  <el-card shadow="hover">
    <template #header>
      <span>软件状态一览</span>
    </template>
    <el-table :data="softwareList" stripe size="small" style="width: 100%">
      <el-table-column prop="name" label="软件名称" width="140" />
      <el-table-column prop="description" label="描述" min-width="160">
        <template #default="{ row }">
          <el-text truncated line-clamp="1">{{ row.description || "-" }}</el-text>
        </template>
      </el-table-column>
      <el-table-column label="启用" width="70">
        <template #default="{ row }">
          <el-tag :type="row.isEnabled ? 'success' : 'danger'" size="small">
            {{ row.isEnabled ? "是" : "否" }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="latestVersion" label="最新版本" width="120">
        <template #default="{ row }">
          {{ row.latestVersion || "-" }}
        </template>
      </el-table-column>
      <el-table-column label="最近发布" width="160">
        <template #default="{ row }">
          {{ formatTime(row.latestReleaseTime) }}
        </template>
      </el-table-column>
      <el-table-column prop="releaseCount" label="发布数" width="80" align="center" />
      <el-table-column prop="onlineCount" label="在线" width="70" align="center" />
      <el-table-column prop="channelCount" label="渠道" width="70" align="center" />
      <el-table-column label="操作" width="80" fixed="right">
        <template #default="{ row }">
          <el-button type="primary" link size="small" @click="emit('select', row.softwareId)">
            查看
          </el-button>
        </template>
      </el-table-column>
    </el-table>
  </el-card>
</template>
