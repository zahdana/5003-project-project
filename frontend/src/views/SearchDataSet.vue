
<template>
  <div class="search-page">
    <div class="left-panel">
      <!-- 左侧标签树 -->
      <div class="tag-tree-list">
        <div v-for="parent in tagTree" :key="parent.id" class="parent-block">
          <div class="parent-title" @click="toggleFilterExpand(parent.id)">
            {{ parent.name }}
            <span>{{ expandedFilterTags.includes(parent.id) ? '▼' : '▶' }}</span>
          </div>
          <div v-if="expandedFilterTags.includes(parent.id)" class="child-list">
            <div v-for="child in parent.children" :key="child.id" class="child-item">
              <label>
                <input
                  type="checkbox"
                  :value="child.id"
                  v-model="selectedSearchTagIds"
                  @change="handleSearch"
                /> {{ child.name }}
              </label>
            </div>
          </div>
        </div>
      </div>
    </div>
    <div class="right-panel">
    <!-- 搜索栏 -->
    <div class="search-bar">
      <select v-model="selectedType" @change="handleSearch">
        <option value="all">All type</option>
        <option value="image">image</option>
        <option value="text">text</option>
        <option value="CSV">CSV</option>
        <option value="audio">vedio</option>
      </select>
      <!-- 下载权限筛选 -->
      <select v-model="selectedPermission" @change="handleSearch">
        <option value="all">all</option>
        <option value="public">public</option>
        <option value="restricted">restricted</option>
        <option value="private">private</option>
      </select>

      <!-- 上传时间筛选 -->
      <select v-model="uploadDateRange" @change="handleSearch">
        <option value="all">all</option>
        <option value="7">7 days ago</option>
        <option value="30">30 days ago</option>
      </select>
      <!-- 上传者搜索 -->
      <input
        v-model="uploader"
        type="text"
        placeholder="Uploader username"
        @input="handleSearch"
      />
      <input
        v-model="searchKeyword"
        type="text"
        placeholder="dataset name"
        @focus="showRecentSearches = true" 
        @change="handleSearch"
      />
      
      <!-- 显示最近搜索记录的下拉框 -->
      <div v-if="showRecentSearches && recentSearches.length > 0" ref="dropdown" class="recent-searches-dropdown">
        <ul>
          <li
            v-for="(item, index) in recentSearches"
            :key="index"
            @click="selectSearch(item)"  
          >
            {{ item }}
          </li>
        </ul>
      </div>

      <button @click="handleSearch">search</button>
    </div>

    

    <div v-if="showRecommendations && recommendedDatasets.length > 0" class="recommendation-section">
      <div class="recommendation-header">
        <h2>recommendation-card</h2>
        <button class="hide-button" @click="hideRecommendations">hide</button>
      </div>

      <!-- 推荐滚动区 -->
      <div class="recommendation-carousel">
        <button class="carousel-btn" @click="goToPrevRecommendPage" :disabled="recommendPageIndex === 0">《</button>

        <div class="recommendation-row">
          <div
            v-for="dataset in pagedRecommendedDatasets"
            :key="dataset.id"
            class="recommendation-card"
            @click="goToDetail(dataset.id)"
          >
            <h4>{{ dataset.name }}</h4>
            <p>type：{{ dataset.type }}</p>
          </div>
        </div>

        <button class="carousel-btn" @click="goToNextRecommendPage" :disabled="isLastRecommendPage">》</button>
      </div>
    </div>
    <div class="dataset-grid">
      <div
        v-for="dataset in datasets"
        :key="dataset.id"
        class="dataset-card"
        @click="goToDetail(dataset.id)"
      >
        <div class="dataset-info-left">
          <h3 v-html="highlightKeyword(dataset.name, searchKeyword)"></h3>
          <p><span class="label">type：</span><span class="value">{{ dataset.type }}</span></p>
          <p><span class="label">permission：</span><span class="value">{{ dataset.downloadPermission }}</span></p>
          <p><span class="label">upload time：</span><span class="value">{{ dataset.uploadTime }}</span></p>
          <p><span class="label">uploader：</span><span class="value">{{ dataset.username }}</span></p>
          <!-- 标签显示区域 -->
          <div class="dataset-tags">
            <span
              v-for="(tag) in dataset.tags.slice(0, 3)"
              :key="tag.id"
              class="tag-item"
            >
              {{ tag.name }}
            </span>
            <span v-if="dataset.tags.length > 3" class="tag-more">...</span>
          </div>
        </div>
        <div class="dataset-info-right">
          <p><span class="label">browsing count：</span><span class="value">{{ dataset.views }}</span></p>
          <p><span class="label">download count：</span><span class="value">{{ dataset.downloads }}</span></p>
        </div>
      </div>
    </div>

    <!-- 分页 -->
    <div class="pagination" v-if="totalPages > 1">
      <button :disabled="currentPage === 1" @click="changePage(currentPage - 1)">last page</button>
      <span>page {{ currentPage }}  /  {{ totalPages }} </span>
      <button :disabled="currentPage === totalPages" @click="changePage(currentPage + 1)">next page</button>
    </div>
    </div>
  </div>
</template>

<script>
import axios from '@/services/axios';
import qs from 'qs';
export default {
  data() {
    return {
      selectedType: 'all',
      selectedPermission: 'all',
      uploadDateRange: 'all',
      uploader: '',
      searchKeyword: '',
      datasets: [],
      currentPage: 1,
      totalPages: 1,
      pageSize: 12,
      recentSearches: [],  // 用于存储最近搜索记录
      showRecentSearches: false, // 控制显示下拉框
      recommendedDatasets: [],
      showRecommendations: true,
      recommendPageIndex: 0,
      recommendPageSize: 4,
      tagTree: [],            // 后端预设标签的树状列表
      selectedSearchTagIds: [], // 搜索时选中的标签 ID 列表
      expandedFilterTags: []
    };
  },
  mounted() {
    const savedState = JSON.parse(localStorage.getItem('searchState'));
    if (savedState) {
      this.selectedType = savedState.selectedType || 'all';
      this.selectedPermission = savedState.selectedPermission || 'all';
      this.uploadDateRange = savedState.uploadDateRange || 'all';
      this.uploader = savedState.uploader || '';
      this.searchKeyword = savedState.searchKeyword || '';
    }
    // 加载标签筛选状态
    const savedTags = JSON.parse(localStorage.getItem('selectedSearchTagIds'));
    if (Array.isArray(savedTags)) {
      this.selectedSearchTagIds = savedTags;
      // 遍历 searchTags，更新对应 checked 和 expanded 状态
    
    }
    this.fetchTagTree();      // 加载预设标签
    this.loadRecentSearches(); // 加载最近搜索记录
    this.fetchDatasets();
    this.fetchRecommendedDatasets();
    document.addEventListener('click', this.handleClickOutside);
  },
  beforeUnmount() {
  document.removeEventListener('click', this.handleClickOutside);
  },
  computed: {
    // 当前页要显示的推荐数据集
    pagedRecommendedDatasets() {
      const start = this.recommendPageIndex * this.recommendPageSize;
      return this.recommendedDatasets.slice(start, start + this.recommendPageSize);
    },
    // 是否是最后一页推荐
    isLastRecommendPage() {
      return (this.recommendPageIndex + 1) * this.recommendPageSize >= this.recommendedDatasets.length;
    }
  },
  watch: {
    selectedSearchTagIds: {
      handler(newVal) {
        localStorage.setItem('selectedSearchTagIds', JSON.stringify(newVal));
        this.fetchDatasets();
      },
      deep: true,
      immediate: false
    }
  },
  methods: {
    async fetchTagTree() {
      try {
        const res = await axios.get('/tag/tree');
        this.tagTree = res.data;
        this.expandedFilterTags = this.getParentTagIdsFromSelected(this.selectedSearchTagIds);
      } catch (e) { console.error(e); }
    },
    toggleFilterExpand(parentId) {
      const idx = this.expandedFilterTags.indexOf(parentId);
      if (idx > -1) this.expandedFilterTags.splice(idx, 1);
      else this.expandedFilterTags.push(parentId);
    },
    getTagNameById(tagId) {
      for (const parent of this.tagTree) {
        const found = parent.children.find(child => child.id === tagId);
        if (found) return found.name;
      }
      return '未知标签';
    },
    getParentTagIdsFromSelected(selectedIds) {
      const expandedIds = new Set();
      for (const parent of this.tagTree) {
        for (const child of parent.children || []) {
          if (selectedIds.includes(child.id)) {
            expandedIds.add(parent.id);
            break; // 一个子标签命中即可展开该父标签
          }
        }
      }
      return [...expandedIds];
    },
    async fetchDatasets() {
      console.log('Fetching datasets with keyword:', this.searchKeyword);
      console.log("筛选标签 ID：", this.selectedSearchTagIds);
      try {
        const response = await axios.get('/dataset/approved', {
          params: {
            page: this.currentPage,
            pageSize: this.pageSize,
            type: this.selectedType,
            query: this.searchKeyword,
            permission: this.selectedPermission,
            dateRange: this.uploadDateRange,
            uploader: this.uploader,
            tagIds: this.selectedSearchTagIds  
          },
          paramsSerializer: params => qs.stringify(params, { arrayFormat: 'repeat' })
        });

        const datasetItems = response.data.items || [];

        if (datasetItems.length === 0) {
          // 没有数据集，直接赋空数组，不请求活动统计了
          this.datasets = [];
          this.totalPages = response.data.totalPages;
          this.saveRecentSearch();
          return;
        }
        // 获取数据集活动统计（浏览量+下载量）
        const activityResponse = await axios.get('/UserActivity/dataset-activity-stats', {
          params: {
            datasetIds: response.data.items.map(dataset => dataset.id).join(',')
          }
        });

        // 将浏览量和下载量添加到数据集对象中
        const datasetsWithStats = response.data.items.map(dataset => {

          const statsArray = Array.isArray(activityResponse.data) ? activityResponse.data : [];
          const activityStats = statsArray.find(stats => stats.id === dataset.id);
          return {
            ...dataset,
            views: activityStats ? activityStats.viewCount : 0,   // 浏览量
            downloads: activityStats ? activityStats.downloadCount : 0  // 下载量
          };
        });

        this.datasets = datasetsWithStats;
        // this.datasets = response.data.items;
        this.totalPages = response.data.totalPages;
        this.saveRecentSearch(); // 搜索完成后保存搜索记录
      } catch (err) {
        console.error('加载数据集失败:', err);
      }
    },
    async fetchRecommendedDatasets() {
      try {
        const token = localStorage.getItem('jwtToken');
        const response = await axios.get('/UserActivity/personalized-recommendations', {
          headers: {
            Authorization: `Bearer ${token}`
          }
        });
        console.log("Recommended Datasets Response:", response.data);
        this.recommendedDatasets = response.data || [];
      } catch (error) {
        console.error('加载推荐数据集失败:', error.response?.data?.message || error.message);
      }
    },
    // 推荐分页按钮对应逻辑
    goToNextRecommendPage() {
      if (!this.isLastRecommendPage) {
        this.recommendPageIndex++;
      }
    },
    goToPrevRecommendPage() {
      if (this.recommendPageIndex > 0) {
        this.recommendPageIndex--;
      }
    },
    hideRecommendations() {
      this.showRecommendations = false;
      localStorage.setItem('hideRecommendations', 'true');
    },
    handleSearch() {
      localStorage.setItem('searchState', JSON.stringify({
        selectedType: this.selectedType,
        selectedPermission: this.selectedPermission,
        uploadDateRange: this.uploadDateRange,
        uploader: this.uploader,
        searchKeyword: this.searchKeyword,
      }));
      this.currentPage = 1;
      this.saveRecentSearch();  // 保存搜索记录
      this.fetchDatasets(); // 执行数据集搜索
      // this.showRecentSearches = true; // 显示历史搜索
    },
    highlightKeyword(text, keyword) {
      if (!keyword) return text;
      const regex = new RegExp(`(${keyword})`, 'gi');
      return text.replace(regex, '<mark>$1</mark>');
    },
    changePage(page) {
      if (page < 1 || page > this.totalPages) return;
      this.currentPage = page;
      this.fetchDatasets();
    },
    async goToDetail(id) {
      try {
        const token = localStorage.getItem('jwtToken');
        // 先记录浏览记录
        await axios.post('/UserActivity/record-view', id, {
          headers: { 
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}` }
        });
        console.log('浏览记录成功');
      } catch (error) {
        console.error('记录浏览失败:', error.response?.data?.message || error.message);
        // 失败也不要阻止跳转
      }
      
      // 无论记录成功失败，都跳转到详情页
      this.$router.push({ name: 'DataSetDetail_2', query: { id } });
    },

    // 加载存储的最近搜索记录
    loadRecentSearches() {
      const recentSearches = JSON.parse(localStorage.getItem('recentSearches')) || [];
      this.recentSearches = recentSearches;
    },
    
    // 存储最近搜索记录
    saveRecentSearch() {
      const newSearch = this.searchKeyword.trim();
      if (newSearch && !this.recentSearches.includes(newSearch)) {
        this.recentSearches.unshift(newSearch);
        if (this.recentSearches.length > 5) {
          this.recentSearches.pop(); // 保证最多保存 5 条
        }
        localStorage.setItem('recentSearches', JSON.stringify(this.recentSearches));
      }
      localStorage.getItem('recentSearches')
    },
    selectSearch(searchTerm) {
      this.searchKeyword = searchTerm;
      this.showRecentSearches = false;  // 隐藏下拉框
      this.fetchDatasets();
    },
    //点击空白处消除历史记录下拉框
    handleClickOutside(event) {
      this.$nextTick(() => {
        const input = this.$el.querySelector('input[placeholder="请输入数据集名称"]');
        const dropdown = this.$el.querySelector('.recent-searches-dropdown');

        if (!input || !dropdown) return;

        if (!input.contains(event.target) && !dropdown.contains(event.target)) {
          this.showRecentSearches = false;
        }
      });
    }

  }
};
</script>

<style scoped>
.search-page {
  display: flex;
  padding: 20px;
  background-color: #f9fafb;
  min-height: 100vh;
  font-family: 'Segoe UI', sans-serif;
  margin-top: 50px;
}
.left-panel {
  width: 240px;
  border-right: 1px solid #ccc;
  padding: 16px;
  background-color: #f9f9f9;
  overflow-y: auto;
}

.right-panel {
  flex: 1;
  padding: 16px;
}

.search-bar {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  margin-bottom: 20px;
  background-color: white;
  padding: 15px;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.search-bar select,
.search-bar input {
  padding: 8px 10px;
  border: 1px solid #ccc;
  border-radius: 8px;
  font-size: 14px;
  min-width: 120px;
}

.search-bar button {
  padding: 8px 16px;
  background-color: #3b82f6;
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
}

.search-bar button:hover {
  background-color: #2563eb;
}

/* 最近搜索下拉框 */
.recent-searches-dropdown {
  position: absolute;
  left: 1220px;         /* 控制左边距 */
  top: 160px;          /* 控制上边距 */
  width: 200px;        /* 控制宽度 */
  background-color: white;
  border: 1px solid #ddd;
  border-radius: 8px;
  margin-top: 0px;
  z-index: 10;
  max-height: 180px;
  overflow-y: auto;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.08);
}

.recent-searches-dropdown ul {
  list-style: none;
  margin: 0;
  padding: 5px 0;
}

.recent-searches-dropdown li {
  padding: 8px 12px;
  cursor: pointer;
}

.recent-searches-dropdown li:hover {
  background-color: #f3f4f6;
}

/* 推荐区块 */
.recommendation-section {
  background-color: #fff;
  padding: 16px;
  border-radius: 12px;
  margin-bottom: 20px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.recommendation-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.recommendation-header h2 {
  margin: 0;
  font-size: 18px;
  color: #111827;
}

.hide-button {
  padding: 4px 10px;
  font-size: 12px;
  background-color: transparent;
  border: 1px solid #ccc;
  border-radius: 6px;
  cursor: pointer;
}

.recommendation-carousel {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-top: 10px;
}

.carousel-btn {
  background-color: #e5e7eb;
  border: none;
  border-radius: 8px;
  padding: 8px 12px;
  cursor: pointer;
}

.recommendation-row {
  display: flex;
  flex: 1;
  overflow-x: auto;
  gap: 12px;
}

.recommendation-card {
  width: 25%;                 /* 每行展示 4 个卡片 */
  min-width: 0;               /* 防止 min-width 撑破布局 */
  background-color: #f9fafb;
  padding: 10px;
  border-radius: 10px;
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.04);
  cursor: pointer;
  transition: transform 0.2s;
  box-sizing: border-box;     /* 确保 padding 不影响宽度计算 */
}

.recommendation-card:hover {
  transform: scale(1.03);
}

/* 数据集卡片 */
.dataset-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 16px;
}

.dataset-card {
  background-color: white;
  border-radius: 12px;
  padding: 16px;
  display: flex;
  justify-content: space-between;
  box-shadow: 0 1px 6px rgba(0, 0, 0, 0.05);
  cursor: pointer;
  transition: transform 0.2s;
}

.dataset-card:hover {
  transform: translateY(-3px);
}

.dataset-info-left h3 {
  font-size: 16px;
  margin: 0 0 8px;
  color: #111827;
}

.label {
  font-weight: bold;
  color: #6b7280;
}

.value {
  color: #111827;
}

/* 分页 */
.pagination {
  margin-top: 24px;
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 10px;
}

.pagination button {
  padding: 6px 14px;
  border: none;
  background-color: #dbeafe;
  color: #1e3a8a;
  border-radius: 6px;
  cursor: pointer;
}

.pagination button:disabled {
  background-color: #f3f4f6;
  color: #9ca3af;
  cursor: not-allowed;
}

.modal-overlay {
  position: fixed;
  top:0; left:0; right:0; bottom:0;
  background: rgba(0,0,0,0.4);
  display:flex; justify-content:center; align-items:center;
  z-index: 1000;
}
.modal-content {
  background: #fff;
  padding: 20px;
  border-radius: 8px;
  width: 400px;
  max-height: 80vh;
  overflow-y: auto;
}
.parent-title {
  font-weight: bold;
  cursor: pointer;
  margin: 8px 0;
  display: flex; justify-content: space-between;
}
.child-item {
  margin-left: 16px;
  padding: 4px 0;
}
.modal-actions {
  text-align: right;
  margin-top: 12px;
}
.tag-filter-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin: 12px 0;
}

.selected-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  font-size: 14px;
  color: #333;
}

.selected-tag {
  background-color: #e0f0ff;
  padding: 4px 8px;
  border-radius: 12px;
  border: 1px solid #90caff;
}
.filter-group button{
  padding: 8px 16px;
  background-color: #3b82f6;
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
}

/* 标签树整体容器 */
.tag-tree-list {
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0,0,0,0.1);
  padding: 16px;
  max-width: 280px;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
  user-select: none;
}

/* 每个父标签块 */
.parent-block {
  margin-bottom: 16px;
  border-radius: 8px;
  overflow: hidden;
  transition: background-color 0.3s ease;
}

/* 父标签标题 */
.parent-title {
  background: linear-gradient(90deg, #4a90e2, #357abd);
  color: #fff;
  font-weight: 600;
  font-size: 1.1rem;
  padding: 12px 16px;
  cursor: pointer;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(53,122,189,0.3);
  user-select: none;
  transition: background 0.3s ease;
}
.parent-title:hover {
  background: linear-gradient(90deg, #357abd, #2a5e9e);
  box-shadow: 0 4px 12px rgba(53,122,189,0.5);
}

/* 展开/收起箭头 */
.parent-title span {
  font-size: 1.2rem;
  transition: transform 0.3s ease;
}
.parent-title.expanded span {
  transform: rotate(90deg);
}

/* 子标签列表 */
.child-list {
  padding-left: 16px;
  margin-top: 8px;
  border-left: 3px solid #4a90e2;
  transition: max-height 0.3s ease;
  overflow: hidden;
}

/* 子标签单项 */
.child-item {
  padding: 8px 12px;
  margin-bottom: 6px;
  font-size: 0.95rem;
  color: #333;
  cursor: pointer;
  border-radius: 6px;
  transition: background-color 0.25s ease;
}
.child-item:hover {
  background-color: #e6f0fb;
}

/* 选中状态 */
.child-item.active {
  background-color: #357abd;
  color: #fff;
  font-weight: 600;
}


</style>
