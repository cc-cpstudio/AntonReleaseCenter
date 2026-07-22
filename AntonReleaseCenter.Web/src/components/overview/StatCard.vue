<script setup lang="ts">

defineProps<{
  title: string
  value: string | number
  color: string
}>()

</script>

<template>
  <el-card shadow="hover" class="stat-card" :style="{ borderLeft: `4px solid ${color}` }">
    <div class="stat-content">
      <span class="stat-value">{{ value }}</span>
      <span class="stat-title">{{ title }}</span>
    </div>
  </el-card>
</template>

<style scoped>
.stat-card {
  flex: 1;
  min-width: 160px;
}

.stat-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  padding: 8px 0;
}

.stat-value {
  font-size: 32px;
  font-weight: 700;
}

.stat-title {
  font-size: 14px;
  color: var(--el-text-color-secondary);
}
</style>
