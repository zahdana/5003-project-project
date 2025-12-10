<template>
  <div class="upload-container">
    <h2>Upload dataset</h2>
    <form @submit.prevent="submitForm">
      <div class="form-group">
        <label for="datasetName">dataset name</label>
        <input v-model="datasetName" type="text" id="datasetName" required placeholder="input name" />
      </div>

      <div class="form-group">
        <label for="description">description</label>
        <textarea v-model="description" id="description" required placeholder="input description"></textarea>
      </div>

      <div class="form-group">
        <label for="datasetType">type</label>
        <select v-model="datasetType" id="datasetType" required>
          <option value="">choose type</option>
          <option value="image">image</option>
          <option value="text">text</option>
          <option value="csv">CSV</option>
          <option value="audio">vedio</option>
        </select>
      </div>

      <div class="form-group">
        <label for="downloadPermission">download permission</label>
        <select v-model="downloadPermission" id="downloadPermission" required>
          <option value="">choose download permission</option>
          <option value="public">public</option>
          <option value="restricted">restricted</option>
          <option value="private">private</option>
        </select>
      </div>

      <!-- 标签选择 UI -->
      <div class="form-group">
        <label>choose labels</label>
        <div class="tag-select-row">
          <button type="button" @click="showTagModal = true">click to choose</button>
          <div class="selected-tags">
            <span v-for="tag in selectedTagNames" :key="tag" class="tag-chip">{{ tag }}</span>
          </div>
        </div>
      </div>

      <!-- 标签选择弹窗 -->
      <div v-if="showTagModal" class="modal-overlay">
        <div class="modal-content">
          <h3>choose labels</h3>
          <div class="tag-tree-list">
            <div v-for="parent in tagTree" :key="parent.id" class="parent-tag-block">
              <!-- 一级标签标题（不可选） -->
              <div class="parent-tag-title" @click="toggleExpanded(parent.id)">
                <span>{{ parent.name }}</span>
                <span class="expand-icon">{{ expandedTags.includes(parent.id) ? '▼' : '▶' }}</span>
              </div>

              <!-- 二级标签列表（可选） -->
              <div v-if="expandedTags.includes(parent.id)" class="child-tag-list">
                <div
                  class="child-tag-item"
                  v-for="child in parent.children"
                  :key="child.id"
                >
                  <span class="child-name">{{ child.name }}</span>
                  <input
                    type="checkbox"
                    :value="child.id"
                    v-model="tempSelectedTagIds"
                    class="checkbox-right"
                  />
                </div>
              </div>
            </div>
          </div>

          <div class="modal-actions">
            <button @click="confirmTagSelection">confirm</button>
            <button @click="showTagModal = false">cancel</button>
          </div>
        </div>
      </div>

      <div class="form-group">
        <label for="fileUpload">upload the file</label>
        <input type="file" @change="handleFileUpload" id="fileUpload" required />
      </div>

      <button type="submit">submit</button>
    </form>

    <div v-if="uploadStatus" :class="uploadStatusClass">
      {{ uploadStatus }}
    </div>
  </div>
</template>

<script>
import axios from '@/services/axios';
import { jwtDecode } from 'jwt-decode'; 

export default {
  data() {
    return {
      datasetName: '',
      description: '',
      datasetType: '',
      downloadPermission: '',
      file: null,
      uploadStatus: '',
      uploadStatusClass: '',
      tagTree: [],           // 标签树数据
      selectedTagIds: [] ,    // 用户勾选的标签 ID
      tempSelectedTagIds: [], // 弹窗中临时选择值
      showTagModal: false,
      expandedTags: [] // 存储展开的一级标签ID
    };
  },
  created() {
    this.fetchTagTree();
  },
  computed: {
    selectedTagNames() {
      const flat = this.flattenTags(this.tagTree);
      return flat
        .filter(tag => this.selectedTagIds.includes(tag.id))
        .map(tag => tag.name);
    }
  },
  methods: {
    async fetchTagTree() {
      try {
        const response = await axios.get('/tag/tree');
        this.tagTree = response.data;
      } catch (error) {
        console.error('标签加载失败:', error);
      }
    },
    flattenTags(tree, prefix = '') {
      let result = [];
      for (const node of tree) {
        const name = prefix ? `${prefix} / ${node.name}` : node.name;
        result.push({ id: node.id, name });
        if (node.children) {
          result = result.concat(this.flattenTags(node.children, name));
        }
      }
      return result;
    },
    // 展开/收缩一级标签
    toggleExpanded(parentId) {
      const index = this.expandedTags.indexOf(parentId);
      if (index > -1) {
        this.expandedTags.splice(index, 1);
      } else {
        this.expandedTags.push(parentId);
      }
    },
    confirmTagSelection() {
      this.selectedTagIds = [...this.tempSelectedTagIds];
      this.showTagModal = false;
    },  

    handleFileUpload(event) {
      this.file = event.target.files[0];
    },
    async submitForm() {
      if (!this.file) {
        this.uploadStatus = '请选择文件进行上传';
        this.uploadStatusClass = 'error';
        return;
      }

      // 获取存储的 JWT Token
      const token = localStorage.getItem('jwtToken');

      if (!token) {
        this.uploadStatus = '未找到有效的JWT Token';
        this.uploadStatusClass = 'error';
        return;
      }

      try {
        // 解码 JWT Token 获取 UserId
        const decodedToken = jwtDecode(token);
        console.log('解码Token：');
        console.log(decodedToken)
        const userId = decodedToken.userId;  // 假设 token 中存储的是 UserId 字段

        if (!userId) {
          this.uploadStatus = '未找到有效的用户信息';
          this.uploadStatusClass = 'error';
          return;
        }

        // 创建 FormData
        const formData = new FormData();
        formData.append('Name', this.datasetName);
        formData.append('Description', this.description);
        formData.append('Type', this.datasetType);
        formData.append('DownloadPermission', this.downloadPermission);
        formData.append('File', this.file);
        formData.append('UserId', userId);  // 将 UserId 添加到表单数据中
        this.selectedTagIds.forEach(tagId => {
          formData.append('TagIds', tagId);
        });

        const response = await axios.post('/dataset/upload', formData, {
          headers: {
            'Content-Type': 'multipart/form-data',
            'Authorization': `Bearer ${token}`  // 将 JWT Token 加入请求头
          },
        });

        if (response.data && response.data.message) {
          this.uploadStatus = response.data.message;
          this.uploadStatusClass = 'success';
        } else {
          this.uploadStatus = '上传失败，请稍后再试';
          this.uploadStatusClass = 'error';
        }
      } catch (error) {
        console.error('上传失败:', error);

        if (error.response) {
          this.uploadStatus = `上传失败: ${error.response.data.message || '未知错误'}`;
        } else {
          this.uploadStatus = '上传失败，请检查网络或服务器设置';
        }

        this.uploadStatusClass = 'error';
      }
    }
  },
};
</script>

<style scoped>
.upload-container {
  max-width: 500px;
  margin: 0 auto;
  padding: 20px;
  border: 1px solid #ccc;
  border-radius: 8px;
  background-color: #f9f9f9;
}

.form-group {
  margin-bottom: 15px;
}

label {
  display: block;
  margin-bottom: 8px;
}

input,
textarea,
select {
  width: 100%;
  padding: 8px;
  margin-top: 5px;
  border-radius: 4px;
  border: 1px solid #ccc;
}

button {
  padding: 10px 20px;
  background-color: #4CAF50;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

button:hover {
  background-color: #45a049;
}

.error {
  color: red;
}

.success {
  color: green;
}

.parent-tag-block {
  border-bottom: 1px solid #ddd;
  padding: 6px 0;
}

.parent-tag-title {
  font-weight: bold;
  cursor: pointer;
  display: flex;
  justify-content: space-between;
  padding: 6px;
  background-color: #f5f5f5;
  border-radius: 6px;
}

.expand-icon {
  font-size: 14px;
  color: #666;
}

.child-tag-list {
  padding-left: 16px;
  margin-top: 6px;
}

.child-tag-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 4px 8px;
}

.child-name {
  flex-grow: 1;
  text-align: left;
}

.checkbox-right {
  margin-left: 10px;
}
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.4); /* 半透明背景 */
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 999;
}

.modal-content {
  background-color: #fff;
  padding: 20px;
  width: 400px;
  max-height: 80vh;
  overflow-y: auto;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  margin-top: 15px;
}

.modal-actions button {
  margin-left: 10px;
}

.tag-select-row {
  display: flex;
  flex-wrap: wrap;
  align-items: flex-start;
  gap: 10px;
  margin-top: 8px;
}

.selected-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  padding: 6px;
  border-radius: 6px;
  flex: 1;
  min-height: 38px;
  align-items: center;
}

.tag-chip {
  display: inline-block;
  background-color: #e0f7fa;
  color: #00792c;
  padding: 4px 10px;
  border-radius: 20px;
  font-size: 13px;
  line-height: 1.2;
  white-space: nowrap;
}
</style>
