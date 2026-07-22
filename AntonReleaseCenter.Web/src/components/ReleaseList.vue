<script setup lang="ts">

import axios from "axios";
import {getCurrentInstance} from "vue";

const instance = getCurrentInstance()
const serverUrl = instance?.appContext.config.globalProperties.$serverUrl as string

const props = defineProps({
  softwareUuid: {
    type: String,
    required: true
  }
})

const software = axios.get(`${serverUrl}/software/${props.softwareUuid}`).then(res => {
  const data = res.data
  return {
    uuid: data.softwareId,
    appKey: data.appKey,
    name: data.name,
    description: data.description,
    isEnabled: data.isEnabled,
  }
})

</script>

<template>

</template>

<style scoped>

</style>