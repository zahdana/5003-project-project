<template>
  <div class="register-container">
    <div class="register-form">
      <h2>register</h2>
      <form @submit.prevent="submitRegister">
        <div class="input-group">
          <input v-model="username" placeholder="username" required />
        </div>
        <div class="input-group">
          <input v-model="password" type="password" placeholder="password" required />
        </div>
        <div class="button-group">
          <button type="submit">register</button>
        </div>
        <div class="footer">
          <p>have account？ <router-link to="/login">login</router-link></p>
        </div>
      </form>
    </div>
  </div>
</template>

<script>
import axios from '@/services/axios';

export default {
  name: 'RegisterForm',
  data() {
    return {
      username: '',
      password: ''
    };
  },
  methods: {
    async submitRegister() {
      try {
        await axios.post('/auth/register', {
          username: this.username,
          password: this.password
        });
        alert('注册成功，请登录！');
        this.$router.push({ path: '/login', query: { username: this.username } });
      } catch (error) {
        console.error('注册失败:', error);
        alert('注册失败，请重试');
      }
    }
  }
};
</script>

<style scoped>
.register-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background-color: #f7f7f7;
}

.register-form {
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
  background-color: #2196F3;
  color: white;
  border: none;
  border-radius: 4px;
  font-size: 1.1rem;
  cursor: pointer;
  transition: background-color 0.3s;
}

button:hover {
  background-color: #1976D2;
}

.footer {
  text-align: center;
  margin-top: 1rem;
}

.footer a {
  color: #2196F3;
  text-decoration: none;
}
</style>
