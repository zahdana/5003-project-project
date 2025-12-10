<template>
    <div class="admin-container">
      <div class="sidebar">
        <h2>Admin Bar</h2>
        <ul>
          <li @click="setActiveView('UploadChecking')">DatasetAudit</li>
          <li @click="setActiveView('AuditRecord')">AuditRecord</li>
        </ul>  
      </div>

      <!-- 右上方退出登录按钮 -->
      <div class="logout-button-container">
        <button @click="logout">Logout</button>
      </div>

      <div class="content">
        <UploadChecking v-if="activeView === 'UploadChecking'" />
        <AuditRecord v-if="activeView === 'AuditRecord'" />
      </div>

    </div>
  </template>
  
  <script>
  import UploadChecking from './UploadChecking.vue'
  import AuditRecord from './AuditRecord.vue'
  
  export default {
    name: 'AdminMainPage',
    components: {
      UploadChecking,
      AuditRecord
    },
    data() {
      return {
        activeView: 'UploadChecking'
      }
    },
    mounted() {
    // 根据路由的 query 参数设置 activeView
    const activeView = this.$route.query.activeView || 'UploadChecking';
    this.setActiveView(activeView);
    },
    methods: {
    setActiveView(view) {
      this.activeView = view; // 设置当前展示的视图
    },
    logout() {
      // 删除 JWT token 或清除其他认证信息
      localStorage.removeItem('token'); // localStorage 存储 JWT
      this.$router.push({ name: 'Login' }); // 跳转到登录界面
    }
    },
  }
  </script>
  
  <style scoped>
  .logout-button-container {
  position: absolute;
  top: 20px;
  right: 20px;
  z-index: 1000; /* 确保按钮在最上层 */
  }

  .logout-button-container button {
    padding: 8px 16px;
    background-color: #ff4d4d;
    color: white;
    border: none;
    cursor: pointer;
    border-radius: 5px;
    font-size: 14px;
  }

  .logout-button-container button:hover {
    background-color: #ff1a1a;
  }
  .admin-container {
    display: flex;
    height: 100vh;
  }
  .sidebar {
  width: 220px;
  height: 100vh;
  background-color: #2c3e50;
  color: white;
  padding: 1rem;
  box-shadow: 2px 0 8px rgba(0, 0, 0, 0.1);
  border-top-right-radius: 12px;
  border-bottom-right-radius: 12px;
  display: flex;
  flex-direction: column;
}

.sidebar h2 {
  margin-bottom: 1.5rem;
  font-size: 1.4rem;
  font-weight: bold;
  text-align: center;
  border-bottom: 1px solid rgba(255, 255, 255, 0.2);
  padding-bottom: 0.5rem;
}

.sidebar ul {
  list-style: none;
  padding: 0;
  margin: 0;
  flex-grow: 1;
}

.sidebar li {
  padding: 0.6rem 1rem;
  cursor: pointer;
  border-radius: 8px;
  transition: background-color 0.3s, transform 0.2s;
}

.sidebar li:hover {
  background-color: #34495e;
  transform: translateX(4px);
}

  .content {
    flex: 1;
    padding: 2rem;
    background-color: #f4f4f4;
  }
  </style>
  