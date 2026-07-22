<script setup lang="ts">

import type { RecentReleaseItem } from "../../types/RecentReleaseItem"
import { platformLabel } from "../../types/platformLabel"

const props = defineProps<{
  releases: RecentReleaseItem[]
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

const truncateLog = (log: string, maxLen: number = 40) => {
  if (!log) return "-"
  return log.length > maxLen ? log.slice(0, maxLen) + "..." : log
}

</script>

<template>
  <el-card shadow="hover">
    <template #header>
      <span>最近发布记录</span>
    </template>
    <el-table :data="releases" stripe size="small" style="width: 100%">
      <el-table-column prop="softwareName" label="软件" width="140" />
      <el-table-column prop="version" label="版本" width="120" />
      <el-table-column label="平台" width="160">
        <template #default="{ row }">
          {{ platformLabel[row.platform] ?? `平台 ${row.platform}` }}
        </template>
      </el-table-column>
      <el-table-column label="更新日志" min-width="160">
        <template #default="{ row }">
          <el-text truncated line-clamp="1">{{ truncateLog(row.updateLog) }}</el-text>
        </template>
      </el-table-column>
      <el-table-column label="发布时间" width="160">
        <template #default="{ row }">
          {{ formatTime(row.releaseTime) }}
        </template>
      </el-table-column>
      <el-table-column label="状态" width="80">
        <template #default="{ row }">
          <el-tag :type="row.isOnline ? 'success' : 'info'" size="small">
            {{ row.isOnline ? "在线" : "下线" }}
          </el-tag>
        </template>
      </el-table-column>
    </el-table>
  </el-card>
</template>
