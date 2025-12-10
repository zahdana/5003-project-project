<template>
  <div class="personal-center">
    <h2>My dataset</h2>

    <!-- 筛选区域 -->
    <div class="filters">
      <input v-model="filters.nameKeyword" placeholder="按名称搜索" />

      <select v-model="filters.type">
        <option value="">all types</option>
        <option value="CSV">CSV</option>
        <option value="文本">text</option>
        <option value="图像">image</option>
        <option value="音频">vedio</option>
        <option value="表格">table</option>
      </select>

      <select v-model="filters.status">
        <option value="">all status</option>
        <option value="待审核">auditing</option>
        <option value="已通过">accepted</option>
        <option value="已拒绝">rejected</option>
      </select>

      <button @click="fetchDatasets">search</button>
    </div>

    <!-- 数据集卡片 -->
    <div class="dataset-card-container">
      <div
        class="dataset-card"
        v-for="dataset in datasetList"
        :key="dataset.id"
        @click="goToDatasetDetail(dataset.id)"
      >
        <h3>{{ dataset.name }}</h3>
        <p>type：{{ dataset.type }}</p>
        <p>status：{{ dataset.status }}</p>
        <p>upload date：{{ formatDate(dataset.uploadTime) }}</p>
      </div>
    </div>

    <!-- 分页 -->
    <div class="pagination">
      <button :disabled="filters.page === 1" @click="prevPage">last page</button>
      <span>page {{ filters.page }} / {{ totalPages }} </span>
      <button :disabled="filters.page >= totalPages" @click="nextPage">next page</button>
    </div>
  </div>
</template>

<script>
import axios from '@/services/axios';

export default {
  name: 'PersonalCenter',
  data() {
    return {
      datasetList: [],
      totalCount: 0,
      filters: {
        nameKeyword: '',
        type: '',
        status: '',
        page: 1,
        pageSize: 10
      }
    };
  },
  computed: {
    totalPages() {
      return Math.ceil(this.totalCount / this.filters.pageSize);
    }
  },
  methods: {
    async fetchDatasets() {
      try {
        const token = localStorage.getItem('jwtToken');
        const response = await axios.get('/dataset/user/self', {
          headers: {
            Authorization: `Bearer ${token}`
          },
          params: {
            nameKeyword: this.filters.nameKeyword,
            type: this.filters.type,
            status: this.filters.status,
            page: this.filters.page,
            pageSize: this.filters.pageSize
          }
        });
        this.datasetList = response.data.items;
        this.totalCount = response.data.totalCount;
      } catch (error) {
        console.error('获取数据集失败:', error);
        alert('无法加载数据集，请稍后重试。');
      }
    },
    formatDate(dateStr) {
      const date = new Date(dateStr);
      return date.toLocaleString();
    },
    goToDatasetDetail(datasetId) {
      this.$router.push({ name: 'DataSetDetail', query: { id: datasetId } });
    },
    prevPage() {
      if (this.filters.page > 1) {
        this.filters.page--;
        this.fetchDatasets();
      }
    },
    nextPage() {
      if (this.filters.page < this.totalPages) {
        this.filters.page++;
        this.fetchDatasets();
      }
    }
  },
  mounted() {
    this.fetchDatasets();
  }
};
</script>

<style scoped>
.personal-center {
  padding: 20px;
}
.filters {
  display: flex;
  gap: 10px;
  margin-bottom: 20px;
  flex-wrap: wrap;
}
.filters input,
.filters select,
.filters button {
  padding: 6px 12px;
  font-size: 14px;
}
.dataset-card-container {
  display: flex;
  flex-wrap: wrap;
  gap: 20px;
}
.dataset-card {
  flex: 1 1 calc(50% - 20px);
  background: #f9f9f9;
  border: 1px solid #ddd;
  padding: 16px;
  border-radius: 8px;
  cursor: pointer;
  transition: box-shadow 0.3s ease;
}
.dataset-card:hover {
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
}
.dataset-card h3 {
  margin-top: 0;
}
.pagination {
  margin-top: 20px;
  display: flex;
  align-items: center;
  gap: 12px;
}
.pagination button {
  padding: 6px 12px;
}
</style>
