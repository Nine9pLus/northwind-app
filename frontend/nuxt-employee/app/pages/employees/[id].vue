<!-- 編輯頁（/employees/:id）— 可換照片 -->
<template>
  <div style="padding: 16px; max-width: 720px; margin: 0 auto">
    <h2>編輯員工 #{{ id }}</h2>

    <form v-if="loaded" @submit.prevent="submit" style="display: grid; gap: 10px">
      <label>名 <input v-model="form.firstName" required /></label>
      <label>姓 <input v-model="form.lastName" required /></label>
      <label>職稱 <input v-model="form.title" /></label>
      <label>入職日期 <input type="date" v-model="form.hireDate" /></label>

      <div>
        <div style="margin-bottom: 6px">目前照片</div>
        <img
          v-if="currentPhotoUrl"
          :src="currentPhotoUrl"
          style="width: 160px; height: 160px; object-fit: cover; border-radius: 12px"
        />
      </div>

      <label>
        更換照片（可不選）
        <input type="file" accept="image/*" @change="onFile" />
      </label>

      <img
        v-if="previewUrl"
        :src="previewUrl"
        style="width: 160px; height: 160px; object-fit: cover; border-radius: 12px"
      />

      <div style="display: flex; gap: 8px">
        <button type="submit" :disabled="loading">{{ loading ? "更新中..." : "更新" }}</button>
        <NuxtLink to="/employees"><button type="button">返回</button></NuxtLink>
      </div>

      <p v-if="error" style="color: red">{{ error }}</p>
    </form>

    <p v-else>Loading...</p>
  </div>
</template>

<script setup>
import { useEmployeesApi } from "@/composables/useEmployeesApi";

const route = useRoute();
const id = Number(route.params.id);

const { getEmployee, updateEmployee } = useEmployeesApi();

const loaded = ref(false);
const loading = ref(false);
const error = ref("");

const currentPhotoUrl = ref("");
const previewUrl = ref("");

const form = reactive({
  firstName: "",
  lastName: "",
  title: "",
  hireDate: "",
  photo: null,
});

const onFile = (ev) => {
  const file = ev.target.files?.[0];
  form.photo = file || null;
  previewUrl.value = file ? URL.createObjectURL(file) : "";
};

onMounted(async () => {
  error.value = "";
  try {
    const dto = await getEmployee(id);
    form.firstName = dto.firstName;
    form.lastName = dto.lastName;
    form.title = dto.title || "";
    form.hireDate = dto.hireDate ? String(dto.hireDate).slice(0, 10) : "";
    currentPhotoUrl.value = dto.photoUrl || "";
    loaded.value = true;
  } catch (err) {
    error.value = err?.data?.message || err?.message || "載入失敗";
  }
});

const submit = async () => {
  error.value = "";
  loading.value = true;
  try {
    await updateEmployee(id, form);
    navigateTo("/employees");
  } catch (err) {
    error.value = err?.data?.message || err?.message || "更新失敗";
  } finally {
    loading.value = false;
  }
};
</script>
