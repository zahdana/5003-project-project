<template>
  <div class="upload-checking">
    <h2>audit list </h2>
    <table class="dataset-table">
      <thead>
        <tr>
          <th>name</th>
          <th>type</th>
          <th>uploadtime</th>
          <th>uploader</th>
          <th>operation</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="dataset in datasets" :key="dataset.id" @click="goToDetail(dataset.id)">
          <td>{{ dataset.name }}</td>
          <td>{{ dataset.type }}</td>
          <td>{{ formatDate(dataset.uploadTime) }}</td>
          <td>{{ dataset.uploaderUsername }}</td>
          <td @click.stop>
            <button @click="openApproveModal(dataset.id)">accept</button>
            <button @click="openRejectModal(dataset.id)">reject</button>
          </td>
        </tr>
      </tbody>
    </table>

    <!-- 拒绝审核确认弹窗 -->
    <div v-if="showRejectModal" class="modal">
      <div class="modal-content">
        <h3>the reason for refusal</h3>
        <textarea v-model="rejectComment" rows="4" cols="50"></textarea>
        <div class="modal-buttons">
          <button @click="rejectDataset">confirm</button>
          <button @click="closeRejectModal">cancel</button>
        </div>
      </div>
    </div>

    <!-- 通过审核确认弹窗 -->
    <div v-if="showApproveModal" class="modal">
      <div class="modal-content">
        <h3>confirm</h3>
        <div class="modal-buttons">
          <button @click="approveDataset">confirm</button>
          <button @click="closeApproveModal">cancel</button>
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
      datasets: [],
      showRejectModal: false,
      showApproveModal: false,  // 用于显示“通过”确认弹窗
      rejectComment: '',
      rejectId: null,
      approveId: null,  // 存储待审核的通过操作的数据集ID
    };
  },
  async mounted() {
    await this.fetchPendingDatasets();
  },
  methods: {
    async fetchPendingDatasets() {
      try {
        const response = await axios.get('/dataset/pending');
        this.datasets = response.data;
      } catch (err) {
        console.error('加载待审核数据集失败:', err);
      }
    },
    formatDate(timestamp) {
        const date = new Date(timestamp);
        return date.toLocaleString();
      },
    goToDetail(id) {
      this.$router.push({ name: 'DataSetDetail_1', query: { id } });
    },
    openApproveModal(id) {
      this.approveId = id;
      this.showApproveModal = true;
    },
    closeApproveModal() {
      this.showApproveModal = false;
      this.approveId = null;
    },
    async approveDataset() {
      try {
        await axios.post(`/dataset/audit/${this.approveId}`, {
          action: 'approve',
          comment: ''
        });
        this.datasets = this.datasets.filter(ds => ds.id !== this.approveId);
        this.closeApproveModal();
      } catch (err) {
        console.error('审核失败:', err);
      }
    },
    openRejectModal(id) {
      this.rejectId = id;
      this.rejectComment = '';
      this.showRejectModal = true;
    },
    closeRejectModal() {
      this.showRejectModal = false;
      this.rejectId = null;
    },
    async rejectDataset() {
      if (!this.rejectComment) {
        alert('请填写拒绝原因');
        return;
      }
      try {
        await axios.post(`/dataset/audit/${this.rejectId}`, {
          action: 'reject',
          comment: this.rejectComment
        });
        this.datasets = this.datasets.filter(ds => ds.id !== this.rejectId);
        this.closeRejectModal();
      } catch (err) {
        console.error('拒绝失败:', err);
      }
    }
  }
};
</script>

<style scoped>
.upload-checking {
  padding: 24px;
  background-color: #f8f9fa;
  border-radius: 16px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  margin-top: 50px;
}

.upload-checking h2 {
  margin-bottom: 16px;
  font-size: 24px;
  color: #333;
}

/* 表格样式 */
.dataset-table {
  width: 100%;
  border-collapse: separate;
  border-spacing: 0;
  background-color: #fff;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.08);
}

.dataset-table th {
  background-color: #007bff;
  color: white;
  padding: 12px;
  font-weight: 500;
  text-align: center;
}

.dataset-table td {
  padding: 12px;
  border-top: 1px solid #e0e0e0;
  text-align: center;
}

.dataset-table tr:hover {
  background-color: #f2f8ff;
  transition: background-color 0.3s ease;
  cursor: pointer;
}

/* 操作按钮 */
.dataset-table button {
  padding: 6px 12px;
  margin: 0 4px;
  border: none;
  background-color: #007bff;
  color: white;
  border-radius: 20px;
  font-weight: bold;
  cursor: pointer;
  transition: background-color 0.2s ease;
}

.dataset-table button:hover {
  background-color: #0056b3;
}

/* 弹窗样式 */
.modal {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.4);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 999;
}

.modal-content {
  background: #fff;
  padding: 24px;
  width: 400px;
  border-radius: 12px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.2);
}

.modal-content h3 {
  margin-bottom: 12px;
  font-size: 18px;
  color: #333;
}

.modal-content textarea {
  width: 100%;
  border: 1px solid #ccc;
  border-radius: 8px;
  padding: 8px;
  resize: none;
  font-size: 14px;
  margin-bottom: 16px;
}

.modal-buttons {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.modal-buttons button {
  padding: 6px 14px;
  background-color: #007bff;
  color: white;
  border: none;
  border-radius: 20px;
  font-weight: bold;
  cursor: pointer;
  transition: background-color 0.2s ease;
}

.modal-buttons button:hover {
  background-color: #0056b3;
}

</style>
