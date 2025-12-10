<template>  
  <div class="login-container">
    <div class="login-form">
      <h2>login</h2>
      <form @submit.prevent="submitLogin">
        <div class="input-group">
          <input v-model="username" placeholder="username" required />
        </div>
        <div class="input-group">
          <input v-model="password" type="password" placeholder="password" required />
        </div>
        <div class="button-group">
          <button type="submit">login</button>
        </div>
        <div class="footer">
          <p>no account？ <router-link to="/register">register</router-link></p>
        </div>
      </form>
    </div>
  </div>
</template>

<script>
import axios from '@/services/axios';

export default {
  name: 'LoginForm',
  data() {
    return {
      username: '',
      password: '',
      errorMessage: '',  // 用于显示错误信息
    };
  },
  methods: {
    async submitLogin() {
      try {
        // 清空错误信息
        this.errorMessage = '';

        // 校验用户名和密码
        if (!this.username || !this.password) {
          this.errorMessage = '用户名和密码不能为空';
          return;
        }

        const response = await axios.post('/auth/login', {
          username: this.username,
          password: this.password,
        });

        console.log('登录成功:', response.data);

        // 后端返回的数据结构为 { token: 'JWT_TOKEN' }
        const token = response.data.token;

        if (token) {
          // 存储 JWT Token
          localStorage.setItem('jwtToken', token);
          console.log('JWT Token 已保存');

          // 判断是否为管理员，重定向到不同页面
          if (this.username === 'admin') {
            this.$router.push('/adminmainpage');
          } else {
            this.$router.push('/mainpage');
          }
        } else {
          this.errorMessage = '登录失败: 无效的登录凭证';
        }
      } catch (error) {
        console.error('登录失败:', error);
        // 如果登录失败，设置错误信息
        this.errorMessage = '用户名或密码错误';
      }
    },
  },
  mounted() {
    // 如果有用户名参数，预填充用户名字段
    if (this.$route.query.username) {
      this.username = this.$route.query.username;
    }
  },
};
</script>



<style scoped>
.login-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background-color: #f7f7f7;
}

.login-form {
  background-color: #fff;
  padding: 2rem;
  border-radius: 8px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
  width: 100%;
  max-width: 400px;
}

h2 {
  text-align: center;
  margin-bottom: 1.5rem;
}

.input-group {
  margin-bottom: 1rem;
}

input {
  width: 100%;
  padding: 0.8rem;
  margin: 0.4rem 0;
  border: 1px solid #ccc;
  border-radius: 4px;
  font-size: 1rem;
}

button {
  width: 100%;
  padding: 0.8rem;
  background-color: #4CAF50;
  color: white;
  border: none;
  border-radius: 4px;
  font-size: 1.1rem;
  cursor: pointer;
  transition: background-color 0.3s;
}

button:hover {
  background-color: #45a049;
}

.footer {
  text-align: center;
  margin-top: 1rem;
}

.footer a {
  color: #4CAF50;
  text-decoration: none;
}
</style>
