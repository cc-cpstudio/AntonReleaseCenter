<script setup lang="ts">

import { computed } from "vue"
import VChart from "vue-echarts"
import { use } from "echarts/core"
import { PieChart } from "echarts/charts"
import { TitleComponent, TooltipComponent, LegendComponent } from "echarts/components"
import { CanvasRenderer } from "echarts/renderers"
import { useTheme } from "../../composables/useTheme"

use([PieChart, TitleComponent, TooltipComponent, LegendComponent, CanvasRenderer])

const { isDark } = useTheme()

const props = defineProps<{
  data: { name: string; value: number }[]
}>()

const option = computed(() => ({
  tooltip: {
    trigger: "item" as const,
    formatter: "{b}: {c} ({d}%)",
  },
  legend: {
    bottom: 0,
    textStyle: {
      color: isDark.value ? "#a3a6ad" : "#606266",
    },
  },
  series: [
    {
      type: "pie" as const,
      radius: ["45%", "70%"],
      center: ["50%", "45%"],
      avoidLabelOverlap: false,
      itemStyle: {
        borderRadius: 4,
        borderColor: isDark.value ? "#1d1e1f" : "#fff",
        borderWidth: 2,
      },
      label: {
        show: false,
      },
      emphasis: {
        label: {
          show: true,
          fontSize: 16,
          fontWeight: "bold",
        },
      },
      data: props.data,
    },
  ],
}))

</script>

<template>
  <el-card v-if="data.length > 0" shadow="hover" class="chart-card">
    <template #header>
      <span>平台分布统计</span>
    </template>
    <VChart class="chart" :option="option" autoresize />
  </el-card>
</template>

<style scoped>
.chart-card {
  flex: 1;
  min-width: 320px;
}

.chart {
  width: 100%;
  height: 300px;
}
</style>
