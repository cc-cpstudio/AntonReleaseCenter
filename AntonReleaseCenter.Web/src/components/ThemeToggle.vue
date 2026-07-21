<script setup lang="ts">
import { useTheme } from '../composables/useTheme'
import {Moon, Sunny} from "@element-plus/icons-vue";

const { isDark, toggleTheme } = useTheme()
</script>

<template>
  <el-button circle :icon="isDark ? Sunny : Moon" @click="toggleTheme"/>
</template>
