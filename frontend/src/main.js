// main.js 
import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import process from 'process';  // 引入 process

const app = createApp(App)
window.process = process;  // 将 process 添加到 window 上，确保可以访问

app.use(router)
app.mount('#app')