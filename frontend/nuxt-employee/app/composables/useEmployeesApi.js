// app\composables\useEmployeesApi.js
export const useEmployeesApi = () => {
  const { apiFetch } = useApi();

  const getEmployees = (searchword) => apiFetch("/api/Employees", { query: searchword ? { searchword } : undefined });

  const getEmployee = (id) => apiFetch(`/api/Employees/${id}`);

  const createEmployee = (form) => {
    const fd = new FormData();
    fd.append("firstName", form.firstName);
    fd.append("lastName", form.lastName);
    if (form.title) fd.append("title", form.title);
    if (form.hireDate) fd.append("hireDate", form.hireDate);
    if (form.photo) fd.append("photo", form.photo);
    return apiFetch("/api/Employees", { method: "POST", body: fd });
  };

  const updateEmployee = (id, form) => {
    const fd = new FormData();
    fd.append("firstName", form.firstName);
    fd.append("lastName", form.lastName);
    if (form.title) fd.append("title", form.title);
    if (form.hireDate) fd.append("hireDate", form.hireDate);
    if (form.photo) fd.append("photo", form.photo);
    return apiFetch(`/api/Employees/${id}`, { method: "PUT", body: fd });
  };

  const deleteEmployee = (id) => apiFetch(`/api/Employees/${id}`, { method: "DELETE" });

  return { getEmployees, getEmployee, createEmployee, updateEmployee, deleteEmployee };
};

// useEmployeesApi：把 Employees 的 CRUD + 上傳「包成一組專用函式」

// useEmployeesApi.js 是在 useApi 的基礎上，針對你的後端路由 /api/Employees 做了「功能封裝」，通常包含：

// getEmployees(searchword)：查清單（可帶搜尋條件）

// getEmployee(id)：查單筆

// createEmployee(form)：新增（用 FormData 上傳文字 + 圖片）

// updateEmployee(id, form)：修改（同樣用 FormData）

// deleteEmployee(id)：刪除

// 重點是：
// 上傳圖片一定要用 FormData（multipart/form-data），所以在 create/update 裡面你看到 new FormData() 與 fd.append('photo', form.photo)。
