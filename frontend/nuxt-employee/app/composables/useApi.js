// app\composables\useApi.js
export const useApi = () => {
  const config = useRuntimeConfig();
  const baseURL = config.public.apiBase;

  const apiFetch = (url, options = {}) => {
    return $fetch(url, {
      baseURL,
      ...options,
    });
  };

  return { apiFetch, baseURL };
};

// useApi：統一管理「API 基底網址」與「送出 HTTP 請求」

// 從 runtimeConfig.public.apiBase 取出後端網址（例如 https://localhost:7047）

// 封裝 $fetch，讓你之後呼叫 API 不用每次都手動寫 baseURL

// 概念上等於先做一個「共用的 API 客戶端」：

// 輸入：/api/Employees、/api/Employees/1

// 輸出：自動變成 https://localhost:7047/api/Employees…並送出請求

// 這樣的好處是：以後你換後端網址，只要改 .env 或 nuxt.config，前端所有 API 呼叫都不用改。
