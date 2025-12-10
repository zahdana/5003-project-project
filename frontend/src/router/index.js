//index.js
import { createRouter, createWebHistory } from 'vue-router'
import Login from '../views/Login.vue'
import Register from '../views/Register.vue'
import Home from '../views/Home.vue' 
import Mainpage from '@/views/Mainpage.vue'
import UpLoadDataSet from '@/views/UpLoadDataSet.vue';
import SearchDataSet from '@/views/SearchDataSet.vue';
import PermiManage from '@/views/PermiManage.vue';
import PermiRequest from '@/views/PermiRequest.vue';
import PermissionManageRestricted from '@/views/PermissionManageRestricted.vue';
import PersonalCenter from '@/views/PersonalCenter.vue';
import AdminMainPage from '../views/AdminMainpage.vue'
import UploadChecking from '../views/UploadChecking.vue'
import DataSetDetail from '../views/DataSetDetail.vue';
import AuditRecord from '../views/AuditRecord.vue';
import DataSetDetail_1 from '../views/DataSetDetail_1.vue';
import DataSetDetail_2 from '../views/DataSetDetail_2.vue';
import DataSetDetail_3 from '../views/DataSetDetail_3.vue';
import DataSetDetail_4 from '../views/DataSetDetail_4.vue';
import DatasetViewRecord from '@/views/DatasetViewRecord.vue'
import DatasetDownloadRecord from '@/views/DatasetDownloadRecord.vue'
const routes = [
  {
    path: '/',
    name: 'Home',
    component: Home
    },
    {
    path: '/login',
    name: 'Login',
    component: Login
    },
    {
    path: '/register',
    name: 'Register',
    component: Register
    },
    {
    path: '/mainpage',
    name: 'Mainpage',
    component: Mainpage
    },
    {
    path: '/uploadDataSet',
    name: 'uploadDataSet',
    component: UpLoadDataSet
    },
    {
    path: '/searchDataSet',
    name: 'searchDataSet',
    component: SearchDataSet
    },
    {
    path: '/permiManage',
    name: 'permiManage',
    component: PermiManage
    },
    {
    path: '/personalCenter',
    name: 'personalCenter',
    component: PersonalCenter
    },
    { 
        path: '/adminMainpage',
        name: 'AdminMainPage', 
        component: AdminMainPage 
    },
    {
        path: '/uploadChecking',
        name: 'UploadChecking',
        component: UploadChecking
    },
    {
        path: '/dataSetDetail',
        name: 'DataSetDetail',
        component: DataSetDetail
    },
    {
        path: '/auditRecord',
        name: 'AuditRecord',
        component: AuditRecord
    },
    {
        path: '/dataSetDetail_1',
        name: 'DataSetDetail_1',
        component: DataSetDetail_1
    },
    {
        path: '/dataSetDetail_2',
        name: 'DataSetDetail_2',
        component: DataSetDetail_2
    },
    {
    path: '/permiRequest',
    name: 'permiRequest',
    component: PermiRequest
    },
    {
    path: '/permissionManageRestricted',
    name: 'permissionManageRestricted',
    component: PermissionManageRestricted
    },
  { path: '/dataSetDetail_3', name: 'DataSetDetail_3', component: DataSetDetail_3},
  { path: '/dataSetDetail_4', name: 'DataSetDetail_4', component: DataSetDetail_4},
  { path: '/datasetViewRecord', name: 'DatasetViewRecord', component: DatasetViewRecord},
  { path: '/datasetDownloadRecord', name: 'DatasetDownloadRecord', component: DatasetDownloadRecord}
]

const router = createRouter({
  history: createWebHistory(process.env.BASE_URL),
  routes
})

export default router
