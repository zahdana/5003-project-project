
   <template>
    <div class="audit-record">
      <h2>audit record</h2>
  
      <table class="audit-record-table">
        <thead>
          <tr>
            <th>dataset name</th>
            <th>Auditor</th>
            <th>Operation</th>
            <th>Audit time</th>
            <th>comment</th>
          </tr>
        </thead>
        <tbody>
          <!-- ✅ 修改为分页后的展示 -->
          <tr v-for="log in paginatedLogs" :key="log.id">
            <td>{{ log.dataset.name }}</td>
            <td>{{ log.reviewerName }}</td>
            <td>{{ log.action }}</td>
            <td>{{ formatDate(log.timestamp) }}</td>
            <td>{{ log.comment }}</td>
          </tr>
        </tbody>
      </table>
  
      <!-- 分页控件 -->
      <div class="pagination">
        <button @click="changePage(currentPage - 1)" :disabled="currentPage === 1">«</button>
        <span>第 {{ currentPage }} / {{ totalPages }} 页</span>
        <button @click="changePage(currentPage + 1)" :disabled="currentPage === totalPages">»</button>
      </div>
    </div>
  </template>
  
  <script>
  import axios from '@/services/axios';
  
  export default {
    data() {
      return {
        auditLogs: [],
        currentPage: 1,       
        pageSize: 10          
      };
    },
    computed: {
      // 
      paginatedLogs() {
        const start = (this.currentPage - 1) * this.pageSize;
        return this.auditLogs.slice(start, start + this.pageSize);
      },
      // 计算总页数
      totalPages() {
        return Math.ceil(this.auditLogs.length / this.pageSize);
      }
    },
    async mounted() {
      await this.fetchAuditRecords();
    },
    methods: {
      async fetchAuditRecords() {
        try {
          const response = await axios.get('/dataset/audit-records');
          this.auditLogs = response.data;
        } catch (err) {
          console.error('加载审核记录失败:', err);
        }
      },
      formatDate(timestamp) {
        const date = new Date(timestamp);
        return date.toLocaleString();
      },
      //  页码切换
      changePage(newPage) {
        if (newPage >= 1 && newPage <= this.totalPages) {
          this.currentPage = newPage;
        }
      }
    }
  };
  </script>
  
  <style scoped>
  .audit-record {
  padding: 24px;
  background-color: #f8f9fa;
  border-radius: 16px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.audit-record h2 {
  margin-bottom: 16px;
  font-size: 24px;
  color: #333;
}

/* 表格样式美化 */
.audit-record-table {
  width: 100%;
  border-collapse: separate;
  border-spacing: 0;
  background-color: #fff;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.08);
}

.audit-record-table th {
  background-color: #007bff;
  color: white;
  padding: 12px;
  font-weight: 500;
  text-align: center;
}

.audit-record-table td {
  padding: 12px;
  border-top: 1px solid #e0e0e0;
  text-align: center;
}

.audit-record-table tr:hover {
  background-color: #f2f8ff;
  transition: background-color 0.3s ease;
}

/* 分页样式美化 */
.pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 12px;
  margin-top: 16px;
  font-size: 15px;
}

.pagination button {
  padding: 6px 14px;
  border: none;
  background-color: #007bff;
  color: white;
  border-radius: 20px;
  cursor: pointer;
  font-weight: bold;
  transition: background-color 0.2s ease;
}

.pagination button:hover:not(:disabled) {
  background-color: #0056b3;
}

.pagination button:disabled {
  background-color: #d0d0d0;
  cursor: not-allowed;
}

.pagination span {
  font-weight: bold;
  color: #333;
}

  </style>
  