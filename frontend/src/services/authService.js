import axios from './axios'

export default {
  async login(username, password) {
    return axios.post('/auth/login', { username, password })
  },

  async register(username, password) {
    return axios.post('/auth/register', { username, password })
  }
}

