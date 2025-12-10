<template>
    <div class="mainpage-container">
      <!-- 固定的左侧菜单栏 -->
      <div class="sidebar">
        <div class="logo">
          <h2>📊 Dataset Share Platform</h2>
        </div>
        <ul class="menu">
          <li :class="{ active: activeView === 'UploadDataSet' }" @click="setActiveView('UploadDataSet')">
            <Upload class="icon" /> Upload dataset
          </li>
          <li :class="{ active: activeView === 'SearchDataSet' }" @click="setActiveView('SearchDataSet')">
            <Search class="icon" /> Search for dataset
          </li>
          <li @click="togglePermissionMenu" class="has-submenu">
            <Shield class="icon" /> Authority management
            <ul v-if="isPermissionMenuOpen" class="submenu">
              <li @click="setActiveView('PermiManage')">Received</li>
              <li @click="setActiveView('PermiRequest')">Send</li>
              <li @click="setActiveView('PermissionManageRestricted')">Authorization</li>
            </ul>
          </li>
          <li :class="{ active: activeView === 'PersonalCenter' }" @click="setActiveView('PersonalCenter')">
            <User class="icon" /> Personal dataset
          </li>
          <li @click="toggleHistoryMenu" class="has-submenu">
            <Clock class="icon" /> History
            <ul v-if="isHistoryMenuOpen" class="submenu">
              <li @click="setActiveView('DatasetViewRecord')">Browsing record</li>
              <li @click="setActiveView('DatasetDownloadRecord')">Download record</li>
            </ul>
          </li>
        </ul>
      </div>
      
      <!-- 右上方退出登录按钮 -->
      <div class="logout-button-container">
        <button @click="logout">Logout</button>
      </div>
      
      <div class="username-display">
      <!-- 如果用户名存在，显示用户名，否则显示未登录 -->
        <div v-if="username">welcome，{{ username }}</div>
        <div v-else>no account</div>
      </div>
      <!-- 右侧动态内容区域 -->
      <div class="main-content">
        <UploadDataSet v-if="activeView === 'UploadDataSet'" />
        <SearchDataSet v-if="activeView === 'SearchDataSet'" />
        <PermiManage v-if="activeView === 'PermiManage'" />
        <PermiRequest v-if="activeView === 'PermiRequest'" />
        <PermissionManageRestricted v-if="activeView === 'PermissionManageRestricted'" />
        <PersonalCenter v-if="activeView === 'PersonalCenter'" />
        <DatasetViewRecord v-if="activeView === 'DatasetViewRecord'" />
        <DatasetDownloadRecord v-if="activeView === 'DatasetDownloadRecord'" />
      </div>
    </div>
  </template>
  
  <script>
  import UploadDataSet from './UpLoadDataSet.vue';
  import SearchDataSet from './SearchDataSet.vue';
  import PermiManage from './PermiManage.vue';
  import PermiRequest from './PermiRequest.vue';
  import PermissionManageRestricted from './PermissionManageRestricted.vue';
  import PersonalCenter from './PersonalCenter.vue';
  import DatasetViewRecord from './DatasetViewRecord.vue'; 
  import DatasetDownloadRecord from './DatasetDownloadRecord.vue'; 
  import { jwtDecode } from 'jwt-decode';

  export default {
    name: 'MainPage',
    components: {
      UploadDataSet,
      SearchDataSet,
      PermiManage,
      PermiRequest,
      PermissionManageRestricted,
      PersonalCenter,
      DatasetViewRecord,
      DatasetDownloadRecord
    },
    data() {
      return {
        activeView: 'SearchDataSet', // 默认显示上传数据集
        username: '', // 用来存储解码后的用户名
        uploadStatus: '', // 用来存储上传状态消息
        uploadStatusClass: '', // 用来存储上传状态
        isHistoryMenuOpen: false,
        isPermissionMenuOpen: false
      };
  
    },
    mounted() {
    // 根据路由的 query 参数设置 activeView
    const activeView = this.$route.query.activeView || 'SearchDataSet';
    this.setActiveView(activeView);
    this.decodeJwtToken();
  },
    methods: {
      toggleHistoryMenu() {
        this.isHistoryMenuOpen = !this.isHistoryMenuOpen;
      },
      togglePermissionMenu(){
        this.isPermissionMenuOpen = !this.isPermissionMenuOpen;
      },
      decodeJwtToken() {
      const token = localStorage.getItem('jwtToken');
      if (!token) {
        this.uploadStatus = '未找到有效的JWT Token';
        this.uploadStatusClass = 'error';
        return;
      }
      else{
        console.log('获取了未解码JwtToken');
      }

      try {
        // 解码 JWT Token 获取数据
        const decodedToken = jwtDecode(token);
        console.log('解码后的 JWT Token:', decodedToken); // 调试日志
        console.log('解码后的 UserID:', decodedToken.userId); // 调试日志
        // 提取用户名
        const username = decodedToken['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name']; 

        // 如果解码的 JWT 中包含用户名，保存并显示；否则，返回错误消息
        if (username) {
          this.username = username;
        } else {
          this.uploadStatus = '未找到有效的用户信息';
          this.uploadStatusClass = 'error';
        }
      } catch (error) {
        console.error('解码JWT失败', error);
        this.uploadStatus = '解码JWT失败';
        this.uploadStatusClass = 'error';
      }
    },

    setActiveView(view) {
      this.activeView = view; // 设置当前展示的视图
      this.isHistoryMenuOpen = false;
      this.isPermissionMenuOpen = false;
    },
    logout() {
      // 删除 JWT token 或清除其他认证信息
      localStorage.removeItem('JwtToken'); // localStorage 存储 JWT
      this.$router.push({ name: 'Login' }); // 跳转到登录界面
    }
    },
  };
  </script>
  
  <style scoped>
 .mainpage-container {
  display: flex;
  height: 100vh;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
}

/* 左侧菜单栏样式 */
.sidebar {
  width: 240px;
  flex-shrink: 0; /*  防止被压缩 */
  background-color: #1e1e2f;
  color: #fff;
  padding: 1rem;
  display: flex;
  flex-direction: column;
  box-shadow: 2px 0 8px rgba(0, 0, 0, 0.1);
  overflow-y: auto;
}

.logo h2 {
  text-align: center;
  color: #00c9a7;
  margin-bottom: 2rem;
  font-size: 20px;
  letter-spacing: 1px;
}

/* 菜单样式 */
.menu {
  list-style: none;
  padding: 0;
  margin: 0;
}

.menu li {
  padding: 0.8rem 1rem;
  border-radius: 8px;
  cursor: pointer;
  transition: background-color 0.3s ease, transform 0.1s ease;
}

.menu li:hover {
  background-color: #2f4050;
  transform: translateX(5px);
}

.submenu {
  padding-left: 1rem;
  list-style: none;
  margin-top: 0.5rem;
}

.submenu li {
  padding: 0.6rem 1rem;
  font-size: 14px;
  border-radius: 6px;
  cursor: pointer;
  transition: background-color 0.3s ease;
}

.submenu li:hover {
  background-color: #38485a;
}

/* 主内容区域 */
.main-content {
  flex-grow: 1;
  background-color: #f4f7fa;
  padding: 2rem;
  overflow-y: auto;
}

/* 顶部用户名展示 */
.username-display {
  position: absolute;
  top: 20px;
  right: 130px;
  font-size: 14px;
  color: #2dac3e;
  line-height: 34px;
  z-index: 1000;
}

/* 退出按钮样式 */
.logout-button-container {
  position: absolute;
  top: 20px;
  right: 20px;
  z-index: 1000;
}

.logout-button-container button {
  padding: 8px 16px;
  background-color: #ff4d4d;
  color: white;
  border: none;
  cursor: pointer;
  border-radius: 6px;
  font-size: 14px;
  transition: background-color 0.2s ease;
}

.logout-button-container button:hover {
  background-color: #ff1a1a;
}
  </style>
  