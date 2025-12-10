
<template>
  <div class="permi-manage-container">
    <h2>Reveived permission request</h2>
    <div v-if="permissionRequests.length > 0">
      <table>
        <thead>
          <tr>
            <th>dataset name</th>
            <th>type</th>
            <th>applicant</th>
            <th>operation</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(request, index) in permissionRequests" :key="index">
            <td>{{ request.datasetName }}</td>
            <td>{{ request.datasetType }}</td>
            <td>{{ request.requesterUsername }}</td>
            <td>
              <button @click="openConfirmation(request.requestId, index, true)">同意</button>
              <button @click="openConfirmation(request.requestId, index, false)">拒绝</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
    <div v-else>
      <p>none</p>
    </div>

    <div v-if="showDialog" class="modal-overlay">
      <div class="modal">
        <p>Are you sure to {{ isApprove ? 'approve to' : 'reject' }} the permission request？</p>
        <div class="modal-buttons">
          <button @click="confirmAction">confirm</button>
          <button @click="cancelAction">cancel</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import axios from '@/services/axios';

export default {
  data() {
    return {
      permissionRequests: [],
      showDialog: false,
      currentRequestId: null,
      currentIndex: null,
      isApprove: true
    };
  },
  mounted() {
    this.fetchPermissionRequests();
  },
  methods: {
    async fetchPermissionRequests() {
      try {
        const token = localStorage.getItem('jwtToken');
        const response = await axios.get('/Permissions/received-requests', {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        });
        this.permissionRequests = response.data.filter(r => r.status === 'Pending');
      } catch (error) {
        console.error('获取权限请求失败:', error);
      }
    },
    openConfirmation(requestId, index, approve) {
      this.currentRequestId = requestId;
      this.currentIndex = index;
      this.isApprove = approve;
      this.showDialog = true;
    },
    async confirmAction() {
      try {
        const token = localStorage.getItem('jwtToken');
        const response = await axios.post('/Permissions/handle-request', {
          requestId: this.currentRequestId,
          approve: this.isApprove
        }, {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        });

        this.permissionRequests.splice(this.currentIndex, 1);
        alert(response.data.message);
      } catch (error) {
        console.error('处理权限请求失败:', error);
        alert('处理失败，请稍后重试。');
      } finally {
        this.cancelAction();
      }
    },
    cancelAction() {
      this.showDialog = false;
      this.currentRequestId = null;
      this.currentIndex = null;
    }
  },
};
</script>

<style scoped>
.permi-manage-container {
  padding: 30px;
  background-color: #f9f9f9;
  max-width: 1000px;
  margin: auto;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0,0,0,0.1);
  margin-top: 100px;
}

h2 {
  text-align: center;
  margin-bottom: 20px;
}

table {
  width: 100%;
  border-collapse: collapse;
  background: white;
}

th, td {
  padding: 12px;
  text-align: center;
  border-bottom: 1px solid #ddd;
}

tr:hover {
  background-color: #f2f2f2;
}

button {
  padding: 6px 12px;
  margin: 0 4px;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  color: white;
}

button:nth-child(1) {
  background-color: #4CAF50;
}

button:nth-child(2) {
  background-color: #F44336;
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.4);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 999;
}

.modal {
  background: white;
  padding: 20px 30px;
  border-radius: 10px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.2);
  text-align: center;
  width: 300px;
}

.modal-buttons button {
  margin: 10px 8px 0;
  padding: 6px 14px;
  border-radius: 8px;
  border: none;
  cursor: pointer;
}

.modal-buttons button:first-child {
  background-color: #4CAF50;
  color: white;
}

.modal-buttons button:last-child {
  background-color: #bbb;
  color: white;
}
</style>
