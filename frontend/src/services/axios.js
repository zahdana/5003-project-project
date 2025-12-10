import axios from 'axios';

// 创建 axios 实例
const apiClient = axios.create({
  baseURL: 'https://localhost:7275/api',  // 后端 API 基础 URL
  headers: {
    'Content-Type': 'application/json',  // 设置请求头
  },
  timeout: 10000,  // 设置请求超时
});

// 请求和响应拦截器
apiClient.interceptors.response.use(
  response => response,
  error => {
    // 处理请求错误
    console.error('API error:', error);
    return Promise.reject(error);
  }
);

export default apiClient;
