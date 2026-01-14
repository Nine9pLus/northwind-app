<!-- 建立員工清單頁（/employees） -->
<template>
  <div style="padding: 16px; max-width: 1000px; margin: 0 auto">
    <h2>員工管理</h2>

    <div style="display: flex; gap: 8px; margin: 12px 0">
      <input v-model="searchword" placeholder="搜尋姓名/職稱..." />
      <button @click="load" :disabled="loading">{{ loading ? "載入中..." : "搜尋" }}</button>

      <NuxtLink to="/employees/new">
        <button>新增</button>
      </NuxtLink>
    </div>

    <table border="1" cellpadding="8" cellspacing="0" style="width: 100%">
      <thead>
        <tr>
          <th style="width: 90px">照片</th>
          <th>姓名</th>
          <th>職稱</th>
          <th style="width: 140px">入職日期</th>
          <th style="width: 220px">操作</th>
        </tr>
      </thead>

      <tbody>
        <tr v-for="e in employees" :key="e.employeeId">
          <td>
            <img
              v-if="e.photoUrl"
              :src="e.photoUrl"
              style="width: 70px; height: 70px; object-fit: cover; border-radius: 10px"
            />
          </td>
          <td>{{ e.firstName }} {{ e.lastName }}</td>
          <td>{{ e.title }}</td>
          <td>{{ formatDate(e.hireDate) }}</td>
          <td>
            <NuxtLink :to="`/employees/${e.employeeId}`">
              <button>編輯</button>
            </NuxtLink>
            <button @click="onDelete(e.employeeId)">刪除</button>
          </td>
        </tr>
      </tbody>
    </table>

    <p v-if="error" style="color: red; margin-top: 12px">{{ error }}</p>
  </div>
</template>

<script setup>
import { useEmployeesApi } from "@/composables/useEmployeesApi";

const { getEmployees, deleteEmployee } = useEmployeesApi();

const employees = ref([]);
const searchword = ref("");
const loading = ref(false);
const error = ref("");

const load = async () => {
  error.value = "";
  loading.value = true;
  try {
    employees.value = await getEmployees(searchword.value || undefined);
  } catch (err) {
    error.value = err?.data?.message || err?.message || "載入失敗";
  } finally {
    loading.value = false;
  }
};

const onDelete = async (id) => {
  if (!confirm(`確定刪除 EmployeeId=${id} ?`)) return;
  error.value = "";
  try {
    await deleteEmployee(id);
    await load();
  } catch (err) {
    error.value = err?.data?.message || err?.message || "刪除失敗";
  }
};

const formatDate = (s) => (s ? String(s).slice(0, 10) : "");

onMounted(load);
</script>
