<!-- 用于管理员 -->

<template>  
  <div class="dataset-detail-container">
    <h2>Dataset details</h2>
    <div v-if="dataset">
      <div class="detail-item"><strong>name:</strong> {{ dataset.name }}</div>
      <div class="detail-item"><strong>type:</strong> {{ dataset.type }}</div>
      <div class="detail-item"><strong>permission:</strong> {{ dataset.downloadPermission }}</div>
      <div class="detail-item"><strong>status:</strong> {{ dataset.status }}</div>
      <div class="detail-item"><strong>description:</strong> {{ dataset.description }}</div>
      <div class="detail-item">
        <strong>label:</strong>
        <span v-if="tags.length">
          <span v-for="tag in tags" :key="tag" class="tag">{{ tag.name }}</span>
        </span>
        <span v-else>none</span>
      </div>
      <div class="detail-item"><strong>upload date:</strong> {{ formatDate(dataset.uploadTime) }}</div>
      <div class="button-group">
        <div v-if="canDownload">
          <p v-if="dataset.downloadPermission === 'restricted'" class="info-message">
            ✅ you have got the download permission
          </p>
          <button @click="downloadDataset">download</button>
        </div>

        <div v-else>
          <p class="warning-message">{{ warningMessage }}</p>
          <button
          v-if="dataset.downloadPermission !== 'private'"
          @click="requestPermission"
          :disabled="requestStatus === 'Pending' || isRequesting"
          >
          {{ requestStatus === 'Pending' ? 'wait for permission' : 'call for permission' }}
          </button>
        </div>
      </div>
    </div>
    <div v-else>
      <p>fail to load the dataset</p>
    </div>

    <div v-if="showReportDialog" class="report-dialog-overlay">
      <div class="report-dialog">
        <h3>质量分析报告</h3>
        
        <div class="detail-item">
          <strong>缺失值比例：</strong>
          <span>{{ reportContent.missingValueRate * 100 }}%</span>
        </div>

        <div class="detail-item">
          <strong>重复记录数：</strong>
          <span>{{ reportContent.duplicateRowCount }}</span>
        </div>

        <div class="detail-item">
          <strong>总行数：</strong>
          <span>{{ reportContent.rowCount }}</span>
        </div>

        <div class="detail-item">
          <strong>总列数：</strong>
          <span>{{ reportContent.columnCount }}</span>
        </div>

        <div class="detail-item">
          <strong>质量评分：</strong>
          <span>{{ reportContent.qualityScore.toFixed(2) }}</span>
        </div>

        <button @click="closeReportDialog">关闭</button>
      </div>
    </div>  

    <button @click="goBack" class="back-button">back</button>
  </div>
  <!-- 评论输入区 -->
  <div class="comment-section">
    <h3>Comments</h3>
    <textarea
      v-model="newComment"
      placeholder="Write your comments..."
      rows="4"
    ></textarea>
    <div v-if="commentError" class="comment-error">{{ commentError }}</div>
    <button @click="submitComment">submit</button>
  </div>

  <!-- 评论展示区 -->
  <div class="comment-list" v-if="comments.length">
    <h3>comment list</h3>
    <div v-for="comment in comments" :key="comment.id" class="comment-item">
      <p class="comment-content">{{ comment.content }}</p>
      <p class="comment-meta">
        from <strong>{{ comment.username }}</strong>   date {{ formatDate(comment.createdAt) }}
      </p>
    </div>
  </div>
</template>

<script>
import axios from '@/services/axios';

export default {
  data() {
    return {
      dataset: null,
      errorMessage: '',
      canDownload: false,
      warningMessage: '',
      hasRequested: false,
      isRequesting: false,
      requestStatus:'None',
      isRequestStatusChecked: false,
      comments: [],
      newComment: '',
      commentError: '',
      tags: [],
      showReportDialog: false,
      reportContent: '',
    };
  },
  mounted() {
    this.fetchDatasetDetail();
    this.checkRequestStatus();
    this.loadTags(); 
    this.loadComments();
  },
  methods: {
    async handleCheckQualityReport() {
      const datasetId = this.$route.query.id;
      const token = localStorage.getItem('jwtToken');
      try {
        // 第一步：尝试获取已有报告
        const res = await axios.get(`/DatasetQuality/${datasetId}`, {
          headers: { Authorization: `Bearer ${token}` }
        });
        this.reportContent = res.data;
        this.showReportDialog = true;
      } catch (error) {
        if (error.response && error.response.status === 404) {
          // 如果报告不存在，调用分析接口
          try {
            const analysisRes = await axios.post(`/DatasetQuality/analyze/${datasetId}`, null, {
              headers: { Authorization: `Bearer ${token}` }
            });
            this.reportContent = analysisRes.data;
            this.showReportDialog = true;
          } catch (err) {
            console.error('生成质量分析失败', err);
            alert('生成质量分析报告失败，请稍后重试');
          }
        } else {
          console.error('获取质量报告失败', error);
          alert('获取质量分析报告失败，请稍后重试');
        }
      }
    },

    closeReportDialog() {
      this.showReportDialog = false;
      this.reportContent = '';
    },

    formatDate(dateString) {
      const date = new Date(dateString);
      return date.toLocaleString('zh-CN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit'
      });
    },
    async loadTags() {
      const datasetId = this.$route.query.id;
      try {
        const response = await axios.get(`/tag/dataset/${datasetId}/tags`);
        this.tags = response.data; // 假设返回的是字符串数组
      } catch (error) {
        console.error('加载标签失败:', error);
      }
    },
    async loadComments() {
      const datasetId = this.$route.query.id;
      try {
        const response = await axios.get(`/dataset/${datasetId}/comments`);
        this.comments = response.data;
      } catch (error) {
        console.error('加载评论失败:', error);
      }
    },

    async submitComment() {
      const datasetId = this.$route.query.id;
      const token = localStorage.getItem('jwtToken');
      if (!token) {
        this.commentError = '请先登录后再发表评论';
        return;
      }

      if (!this.newComment.trim()) {
        this.commentError = '评论内容不能为空';
        return;
      }

      try {
        const response = await axios.post(
          `/dataset/${datasetId}/add-comments`,
          { datasetId: datasetId, content: this.newComment },
          { headers: { Authorization: `Bearer ${token}` } }
        );
        console.log(response.data);
        this.newComment = '';
        this.commentError = '';
        this.loadComments(); // 重新加载评论列表
      } catch (error) {
        console.error('发表评论失败:', error);
        this.commentError = '发表评论失败，请稍后再试';
      }
    },

    async fetchDatasetDetail() {
      const datasetId = this.$route.query.id;
      if (!datasetId) {
        this.errorMessage = '没有找到数据集 ID';
        return;
      }

      try {
        const response = await axios.get(`/dataset/${datasetId}`);
        if (response.data) {
          this.dataset = response.data;
          await this.checkDownloadPermission(datasetId);
        }
      } catch (error) {
        console.error('获取数据集详情失败:', error);
        this.errorMessage = '加载数据集详情失败，请稍后重试。';
      }
    },

    async checkDownloadPermission(datasetId) {
      if (this.isRequestStatusChecked) return;  // 避免覆盖已更新的状态
      const token = localStorage.getItem('jwtToken');
      if (!token) {
        this.warningMessage = '未登录，无法验证权限';
        return;
      }

      try {
        const response = await axios.get(`/Permissions/check-download-permission?datasetId=${datasetId}`, {
          headers: { Authorization: `Bearer ${token}` }
        });

        const { canDownload, message, hasRequested } = response.data;
        this.canDownload = canDownload;
        this.hasRequested = hasRequested;

        if (!canDownload) {
          this.warningMessage = hasRequested
            ? '请求已提交，等待上传者处理'
            : (message || '无法下载此数据集');
        }
      } catch (error) {
        console.error('检查下载权限失败:', error);
        this.warningMessage = '权限检查失败，请稍后重试。';
      }
    },

   downloadDataset() {
      const datasetId = this.$route.query.id;
      const token = localStorage.getItem('jwtToken');
      if (!datasetId || !token) {
        this.errorMessage = '下载失败：缺失数据集 ID 或未登录';
        return;
      }

      axios.get(`/dataset/download/${datasetId}`, {
        responseType: 'blob',
        headers: { Authorization: `Bearer ${token}` }
      }).then(response => {
        const blob = response.data;
        const link = document.createElement('a');
        const url = window.URL.createObjectURL(blob);
        link.href = url;

        // 如果有 dataset.name，就用名字；没有就用 dataset_123
        const baseName = this.dataset?.name || `dataset_${datasetId}`;
        const ext = this.dataset?.type === 'csv' ? 'csv' : 'dat'; 
        link.download = `${baseName}.${ext}`;

        link.click();
        window.URL.revokeObjectURL(url);
      }).catch(error => {
        console.error('下载数据集失败:', error);
        this.errorMessage = '下载数据集失败，请稍后重试。';
      });
    },


    async requestPermission() {
      const datasetId = this.$route.query.id;
      const token = localStorage.getItem('jwtToken');
      if (!token) {
        alert('请先登录以请求权限');
        return;
      }

      this.isRequesting = true;

      try {
        const response = await axios.post('/permissions/request', { datasetId }, {
          headers: { Authorization: `Bearer ${token}` }
        });

        if (response.data && response.data.message) {
          this.hasRequested = true;
          this.warningMessage = '请求已提交，等待上传者处理';
        }
      } catch (error) {
        console.error('请求权限失败', error);
        alert('请求权限失败，请稍后再试');
      } finally {
        this.isRequesting = false;
      }
    },
    async checkRequestStatus() {
      const datasetId = this.$route.query.id;
      const token = localStorage.getItem('jwtToken');
      if (!token) return;

      try {
          const response = await axios.get(`/permissions/request-status?datasetId=${datasetId}`, {
          headers: { Authorization: `Bearer ${token}` }
          });

          this.requestStatus = response.data.status;
          console.log('请求状态:',this.requestStatus);
          if (this.requestStatus === 'Pending') {
          this.hasRequested = true;
          this.warningMessage = '请求已提交，等待上传者处理';
          } else if (this.requestStatus === 'Accepted') {
          this.warningMessage = '权限已授予，可下载';
          } else if (this.requestStatus === 'Rejected') {
          this.warningMessage = '请求被拒绝，您可以重新提交';
          this.hasRequested = false; // 允许重新请求
          } else {
          this.hasRequested = false;
          }
      } catch (error) {
          console.error('状态查询失败', error);
      }
      this.isRequestStatusChecked = true;  // 更新状态，避免重复覆盖
    },
    goBack() {
      this.$router.push({ name: 'Mainpage', query: { activeView: 'UploadChecking' } });
    }
  }
};
</script>

<style scoped>
.dataset-detail-container {
  max-width: 900px;
  margin: 30px auto;
  background-color: #ffffff;
  border-radius: 10px;
  padding: 25px 30px;
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.1);
}

h2, h3 {
  font-weight: 700;
  font-size: 28px;
  color: #222;
  margin-bottom: 25px;
  text-align: center;
}

.detail-item {
  display: flex;
  margin-bottom: 16px;
  font-size: 16px;
  color: #333;
}

.detail-item strong {
  width: 150px;
  color: #111;
}

button {
  padding: 12px 30px;
  background: linear-gradient(45deg, #4caf50, #388e3c);
  color: white;
  border: none;
  border-radius: 25px;
  font-size: 1.1rem;
  cursor: pointer;
  box-shadow: 0 4px 10px rgba(76, 175, 80, 0.4);
  transition: all 0.3s ease;
  display: block;
  margin: 15px auto 0;
  min-width: 150px;
}

button:hover:not(:disabled) {
  background: #357a38;
  box-shadow: 0 6px 15px rgba(53, 122, 56, 0.7);
}

button:disabled {
  background-color: #bdbdbd;
  cursor: not-allowed;
  box-shadow: none;
}

.warning-message, .info-message {
  border-radius: 6px;
  padding: 10px 15px;
  font-weight: 600;
  text-align: center;
  margin-bottom: 15px;
  max-width: 300px;
  margin-left: auto;
  margin-right: auto;
}

.warning-message {
  background-color: #fff3e0;
  color: #f57c00;
  border: 1px solid #f57c00;
}

.info-message {
  background-color: #e8f5e9;
  color: #388e3c;
  border: 1px solid #388e3c;
}

.back-button {
  margin: 30px auto 0;
  display: block;
  padding: 12px 28px;
  background: linear-gradient(45deg, #4caf50, #388e3c);
  color: white;
  border: none;
  border-radius: 25px;
  cursor: pointer;
  font-size: 1rem;
  box-shadow: 0 4px 12px rgba(76, 175, 80, 0.5);
  transition: background-color 0.3s ease;
  min-width: 120px;
}

.back-button:hover {
  background-color: #357a38;
}
.comment-section {
  margin-top: 40px;
  max-width: 900px;
  margin-left: auto;
  margin-right: auto;
}

.comment-section textarea {
  width: 100%;
  padding: 15px 18px;
  font-size: 1.1rem;
  border-radius: 12px;
  border: 1.5px solid #ccc;
  resize: vertical;
  transition: border-color 0.3s ease;
}

.comment-section textarea:focus {
  outline: none;
  border-color: #4caf50;
  background-color: #f9fff9;
}

.comment-error {
  background-color: #fbe9e7;
  color: #d32f2f;
  border-radius: 8px;
  padding: 8px 12px;
  margin-bottom: 15px;
  font-weight: 600;
}

.comment-section button {
  margin-top: 15px;
  padding: 12px 30px;
  background: linear-gradient(45deg, #4caf50, #388e3c);
  border-radius: 25px;
  font-size: 1.1rem;
  box-shadow: 0 4px 10px rgba(76, 175, 80, 0.4);
  cursor: pointer;
  color: white;
  border: none;
  transition: all 0.3s ease;
  display: block;
  min-width: 150px;
  margin-left: auto;
  margin-right: auto;
}

.comment-section button:hover {
  background-color: #357a38;
  box-shadow: 0 6px 15px rgba(53, 122, 56, 0.7);
}
.comment-list {
  margin-top: 30px;
  background-color: #f7f9fc;
  border-radius: 12px;
  padding: 25px 30px;
  max-width: 900px;
  margin-left: auto;
  margin-right: auto;
}

.comment-item {
  background-color: #ffffff;
  border-radius: 10px;
  padding: 18px 22px;
  margin-bottom: 20px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.07);
}

.comment-content {
  font-size: 1.1rem;
  margin-bottom: 8px;
  line-height: 1.5;
  color: #222;
}

.comment-meta {
  font-size: 0.9rem;
  color: #5a6b8a;
}
.tag {
  display: inline-block;
  background-color: #00bcd4;
  color: white;
  padding: 5px 14px;
  margin: 0 8px 8px 0;
  border-radius: 20px;
  font-size: 0.9rem;
  cursor: default;
  transition: background-color 0.3s;
}

.tag:hover {
  background-color: #0097a7;
}
.report-dialog-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0,0,0,0.4);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 999;
}

.report-dialog {
  background-color: #fff;
  padding: 20px;
  width: 600px;
  max-height: 80%;
  overflow-y: auto;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.3);
}

.report-dialog pre {
  white-space: pre-wrap;
  font-family: "Courier New", monospace;
}

</style>