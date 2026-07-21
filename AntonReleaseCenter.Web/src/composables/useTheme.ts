import { ref, onMounted } from 'vue'

const isDark = ref(false)

function applyTheme(dark: boolean) {
  if (dark) {
    document.documentElement.classList.add('dark')
  } else {
    document.documentElement.classList.remove('dark')
  }
  localStorage.setItem('theme', dark ? 'dark' : 'light')
}

export function useTheme() {
  onMounted(() => {
    const saved = localStorage.getItem('theme')
    if (saved) {
      isDark.value = saved === 'dark'
    } else {
      isDark.value = window.matchMedia('(prefers-color-scheme: dark)').matches
    }
    applyTheme(isDark.value)
  })

  function toggleTheme() {
    isDark.value = !isDark.value
    applyTheme(isDark.value)
  }

  return { isDark, toggleTheme }
}
