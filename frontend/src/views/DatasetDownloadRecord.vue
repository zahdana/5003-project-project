<template>
  <div class="record-container">
    <h2 class="title">Download record</h2>
    <div v-if="records.length === 0" class="empty-message">no download record</div>
    <table v-else class="record-table">
      <thead>
        <tr>
          <th>dataset name</th>
          <th>type</th>
          <th>permission</th>
          <th>uploader</th>
          <th>download time</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="record in records" :key="record.downloadTime">
          <td>{{ record.datasetName }}</td>
          <td>{{ record.datasetType }}</td>
          <td>{{ record.datasetPermission }}</td>
          <td>{{ record.uploaderUsername }}</td>
          <td>{{ formatDate(record.downloadTime) }}</td>
        </tr>
      </tbody>
    </table>

    <div class="pagination">
      <button @click="prevPage" :disabled="pageNumber === 1">last page</button>
      <span>page {{ pageNumber }} </span>
      <button @click="nextPage" :disabled="pageNumber * pageSize >= totalCount">next page</button>
    </div>
  </div>
</template>

<script>
import axios from 'axios';
export default {
  data() {
    return {
      records: [],
      pageNumber: 1,
      pageSize: 10,
      totalCount: 0,
      token: ''
    };
  },
  methods: {
    async fetchRecords() {
      try {
        const response = await axios.get('https://localhost:7275/api/UserActivity/download-records', {
          params: {
            pageNumber: this.pageNumber,
            pageSize: this.pageSize
          },
          headers: {
            Authorization: `Bearer ${this.token}`
          }
        });
        this.records = response.data.records;
        this.totalCount = response.data.totalCount;
      } catch (error) {
        console.error('获取下载记录失败：', error);
      }
    },
    prevPage() {
      if (this.pageNumber > 1) {
        this.pageNumber--;
        this.fetchRecords();
      }
    },
    nextPage() {
      if (this.pageNumber * this.pageSize < this.totalCount) {
        this.pageNumber++;
        this.fetchRecords();
      }
    },
    formatDate(dateString) {
      const date = new Date(dateString);
      return date.toLocaleString();
    }
  },
  mounted() {
    this.token = localStorage.getItem('jwtToken');
    if (this.token) {
      this.fetchRecords();
    } else {
      console.error('未找到登录凭证');
    }
  }
};
</script>

<style scoped>
.record-container {
  padding: 20px;
}

.title {
  font-size: 24px;
  margin-bottom: 20px;
}

.record-table {
  width: 100%;
  border-collapse: collapse;
}

.record-table th,
.record-table td {
  border: 1px solid #ddd;
  padding: 8px;
  text-align: center;
}

.pagination {
  margin-top: 20px;
  display: flex;
  justify-content: center;
  align-items: center;
}

.pagination button {
  margin: 0 10px;
  padding: 5px 15px;
}

.empty-message {
  text-align: center;
  font-size: 18px;
  color: gray;
}
</style>
