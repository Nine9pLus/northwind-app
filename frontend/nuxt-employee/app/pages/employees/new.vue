<!-- 新增頁（/employees/new）— 上傳照片 FormData -->
<template>
  <div style="padding: 16px; max-width: 720px; margin: 0 auto">
    <h2>新增員工</h2>

    <form @submit.prevent="submit" style="display: grid; gap: 10px">
      <label>名 (FirstName) <input v-model="form.firstName" required /></label>
      <label>姓 (LastName) <input v-model="form.lastName" required /></label>
      <label>職稱 (Title) <input v-model="form.title" /></label>
      <label>入職日期 (HireDate) <input type="date" v-model="form.hireDate" /></label>

      <label>
        個人照 (Photo)
        <input type="file" accept="image/*" @change="onFile" />
      </label>

      <img
        v-if="previewUrl"
        :src="previewUrl"
        style="width: 160px; height: 160px; object-fit: cover; border-radius: 12px"
      />

      <div style="display: flex; gap: 8px">
        <button type="submit" :disabled="loading">{{ loading ? "儲存中..." : "儲存" }}</button>
        <NuxtLink to="/employees"><button type="button">返回</button></NuxtLink>
      </div>

      <p v-if="error" style="color: red">{{ error }}</p>
    </form>
  </div>
</template>

<script setup>
import { useEmployeesApi } from "@/composables/useEmployeesApi";

const { createEmployee } = useEmployeesApi();
const router = useRouter();

const form = reactive({
  firstName: "",
  lastName: "",
  title: "",
  hireDate: "", // yyyy-MM-dd
  photo: null,
});

const previewUrl = ref("");
const loading = ref(false);
const error = ref("");

const onFile = (ev) => {
  const file = ev.target.files?.[0];
  form.photo = file || null;
  previewUrl.value = file ? URL.createObjectURL(file) : "";
};

const submit = async () => {
  error.value = "";
  loading.value = true;
  try {
    await createEmployee(form);
    router.push("/employees");
  } catch (err) {
    error.value = err?.data?.message || err?.message || "新增失敗";
  } finally {
    loading.value = false;
  }
};
</script>
