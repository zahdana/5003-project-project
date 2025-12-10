
<template>
  <div class="restricted-manager">
    <h2>Authority management</h2>
    <table>
      <thead>
        <tr>
          <th>name</th>
          <th>tyepe</th>
          <th>upload time</th>
          <th>Authorized username</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="dataset in datasets" :key="dataset.id">
          <td>{{ dataset.name }}</td>
          <td>{{ dataset.type }}</td>
          <td>{{ formatDate(dataset.uploadTime) }}</td>
          <td>
            <div class="auth-list">
              <span 
                v-for="user in dataset.authorizedUsers" 
                :key="user.userId"
                class="auth-user"
                @mouseenter="hoveredUser = { datasetId: dataset.id, userId: user.userId }"
                @mouseleave="hoveredUser = null"
              >
                {{ user.username }}
                <span 
                  v-if="hoveredUser && hoveredUser.datasetId === dataset.id && hoveredUser.userId === user.userId"
                  class="remove-icon"
                  @click="openRemoveConfirm(dataset.id, user.userId, user.username)"
                >－</span>
              </span>
              <button @click="openAddModal(dataset.id)">＋</button>
            </div>
          </td>
        </tr>
      </tbody>
    </table>

    <!-- 添加授权弹窗 -->
    <div v-if="showModal" class="modal">
      <div class="modal-content">
        <h3>add users</h3>
        <input v-model="newUserId" placeholder="input userID" />
        <button @click="submitPermission">Authorize</button>
        <button @click="closeModal">cancel</button>
      </div>
    </div>

    <!-- 删除授权确认弹窗 -->
    <div v-if="showRemoveModal" class="modal">
      <div class="modal-content">
        <h3>confirm</h3>
        <p>Are you sure to delete the authority of <strong>{{ removingUsername }}</strong> ？</p>
        <button @click="confirmRemove">yes</button>
        <button @click="closeRemoveModal">no</button>
      </div>
    </div>
  </div>
</template>
<script>
import axios from '@/services/axios';
import { jwtDecode } from 'jwt-decode';

export default {
  data() {
    return {
      datasets: [],
      showModal: false,
      selectedDatasetId: null,
      newUserId: '',
      hoveredUser: null,
      showRemoveModal: false,
      removingDatasetId: null,
      removingUserId: null,
      removingUsername: ''
    };
  },
  methods: {
    async fetchData() {
      const token = localStorage.getItem('jwtToken');
      if (!token) {
        this.$router.push({ name: 'Login' });
        return;
      }
      const res = await axios.get('Permissions/my-restricted-datasets', {
        headers: { Authorization: `Bearer ${token}` },
      });
      this.datasets = res.data;
    },
    openAddModal(datasetId) {
      this.selectedDatasetId = datasetId;
      this.showModal = true;
    },
    closeModal() {
      this.showModal = false;
      this.newUserId = '';
    },
    async submitPermission() {
      try {
        const token = localStorage.getItem('jwtToken');
        if (!token) {
          this.$router.push({ name: 'Login' });
          return;
        }
        const decodedToken = jwtDecode(token);
        const currentUserId = decodedToken['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];

        if (this.newUserId === currentUserId) {
          alert('不能为自己添加下载权限。');
          return;
        }

        await axios.post('/Permissions/add-permission', {
          datasetId: this.selectedDatasetId,
          userId: parseInt(this.newUserId)
        }, {
          headers: { Authorization: `Bearer ${token}` }
        });

        this.closeModal();
        await this.fetchData();
      } catch (err) {
        alert(err.response?.data || '添加失败');
      }
    },
    openRemoveConfirm(datasetId, userId, username) {
      this.removingDatasetId = datasetId;
      this.removingUserId = userId;
      this.removingUsername = username;
      this.showRemoveModal = true;
    },
    closeRemoveModal() {
      this.showRemoveModal = false;
      this.removingDatasetId = null;
      this.removingUserId = null;
      this.removingUsername = '';
    },
    async confirmRemove() {
      try {
        const token = localStorage.getItem('jwtToken');
        await axios.post('/Permissions/remove-permission', {
          datasetId: this.removingDatasetId,
          userId: this.removingUserId
        }, {
          headers: { Authorization: `Bearer ${token}` }
        });
        this.closeRemoveModal();
        await this.fetchData();
      } catch (err) {
        alert(err.response?.data || '删除失败');
      }
    },
    formatDate(date) {
      return new Date(date).toLocaleString();
    }
  },
  mounted() {
    this.fetchData();
  }
};
</script>
<style scoped>
.restricted-manager {
  margin-top: 100px;
  padding: 24px;
  font-family: "Segoe UI", sans-serif;
  background-color: #f9fafb;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0,0,0,0.05);
}

h2 {
  margin-bottom: 20px;
  font-size: 22px;
  color: #333;
}

.dataset-table {
  width: 100%;
  border-collapse: collapse;
  background-color: white;
  border-radius: 8px;
  overflow: hidden;
}

.dataset-table th, .dataset-table td {
  padding: 12px 16px;
  text-align: left;
  border-bottom: 1px solid #e0e0e0;
}

.dataset-table tbody tr:hover {
  background-color: #f1f5f9;
}

.auth-list {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 6px;
}

.auth-user {
  background-color: #e2e8f0;
  padding: 4px 10px;
  border-radius: 16px;
  font-size: 13px;
}

.add-btn {
  background-color: #3b82f6;
  color: white;
  border: none;
  padding: 4px 10px;
  border-radius: 50%;
  cursor: pointer;
  font-size: 16px;
  transition: background 0.2s;
}

.add-btn:hover {
  background-color: #2563eb;
}

.modal {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0,0,0,0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal-content {
  background: #fff;
  width: 320px;
  padding: 24px;
  border-radius: 10px;
  box-shadow: 0 8px 24px rgba(0,0,0,0.2);
  text-align: center;
}

.input {
  width: 100%;
  padding: 8px 10px;
  margin: 12px 0;
  border: 1px solid #ccc;
  border-radius: 6px;
  font-size: 14px;
}

.modal-buttons {
  display: flex;
  justify-content: space-between;
  margin-top: 16px;
}

.btn {
  padding: 8px 14px;
  font-size: 14px;
  border-radius: 6px;
  cursor: pointer;
  border: none;
  transition: background-color 0.2s;
}

.btn.primary {
  background-color: #3b82f6;
  color: white;
}

.btn.primary:hover {
  background-color: #2563eb;
}

.btn.cancel {
  background-color: #e5e7eb;
}

.btn.cancel:hover {
  background-color: #d1d5db;
}
.restricted-manager {
  padding: 20px;
}

table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 10px;
}

th, td {
  border-bottom: 1px solid #ccc;
  padding: 10px;
  text-align: left;
}

.auth-list {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.auth-user {
  position: relative;
  padding-right: 10px;
  background-color: #f5f5f5;
  border-radius: 6px;
  padding: 4px 8px;
}

.remove-icon {
  color: red;
  margin-left: 4px;
  cursor: pointer;
  font-weight: bold;
}
.remove-icon:hover {
  color: darkred;
}

.modal {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0,0,0,0.5);
}
.modal-content {
  background: white;
  width: 300px;
  margin: 120px auto;
  padding: 20px;
  border-radius: 10px;
  text-align: center;
}
.modal-content input {
  width: 100%;
  margin-bottom: 10px;
  padding: 8px;
}
.modal-content button {
  margin: 5px;
  padding: 6px 12px;
}

</style>