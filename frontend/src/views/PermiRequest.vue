<template>
    <div class="permi-request-container">
      <h2>The permission request I made</h2>
      <div v-if="permissionRequests.length > 0">
        <table>
          <thead>
            <tr>
              <th>dataset name</th>
              <th>type</th>
              <th>uploader</th>
              <th>request status</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="(request, index) in permissionRequests"
              :key="index"
              @click="goToDatasetDetail(request.datasetId)"
            >
              <td>{{ request.datasetName }}</td>
              <td>{{ request.datasetType }}</td>
              <td>{{ request.ownerUsername }}</td>
              <td :class="statusClass(request.status)">
                {{ translateStatus(request.status) }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-else>
        <p>none</p>
      </div>
    </div>
  </template>
  
  <script>
  import axios from '@/services/axios';
  
  export default {
    data() {
      return {
        permissionRequests: [],
      };
    },
    mounted() {
      this.fetchPermissionRequests();
    },
    methods: {
      async fetchPermissionRequests() {
        const token = localStorage.getItem('jwtToken');
        if (!token) {
          this.$router.push({ name: 'Login' });
          return;
        }
        try {
          const response = await axios.get('Permissions/my-requests', {
            headers: {
              Authorization: `Bearer ${token}`,
            },
          });
          this.permissionRequests = response.data;
        } catch (error) {
          console.error('获取权限请求列表失败:', error);
        }
      },
      goToDatasetDetail(datasetId) {
        this.$router.push({ name: 'DataSetDetail_3', query: { id: datasetId } });
      },
      translateStatus(status) {
        switch (status) {
          case 'Pending':
            return '等待处理';
          case 'Accepted':
            return '已同意';
          case 'Rejected':
            return '已拒绝';
          default:
            return '未知';
        }
      },
      statusClass(status) {
        switch (status) {
          case 'Accepted':
            return 'status-approved';
          case 'Rejected':
            return 'status-rejected';
          case 'Pending':
            return 'status-pending';
          default:
            return '';
        }
      },
    },
  };
  </script>
  
  <style scoped>
  .permi-request-container {
    padding: 20px;
    background-color: #f9f9f9;
    border-radius: 8px;
    max-width: 900px;
    margin: auto;
    margin-top: 100px;
  }
  
  h2 {
    text-align: center;
    margin-bottom: 20px;
  }
  
  table {
    width: 100%;
    border-collapse: collapse;
    margin-top: 30px;
    background-color: white;
    box-shadow: 0 2px 6px rgba(0,0,0,0.05);
  }
  
  th, td {
    padding: 12px;
    text-align: left;
    border-bottom: 1px solid #ddd;
  }
  
  tr:hover {
    background-color: #f1f1f1;
    cursor: pointer;
  }
  
  .status-approved {
    color: #4CAF50;
    font-weight: bold;
  }
  
  .status-rejected {
    color: #F44336;
    font-weight: bold;
  }
  
  .status-pending {
    color: #FF9800;
    font-weight: bold;
  }
  
  p {
    text-align: center;
    font-size: 1.1rem;
    color: #777;
  }
  </style>
  