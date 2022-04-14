import { createApp } from "vue";
import { createPinia } from "pinia";
import axios from "axios";
import VueAxios from "vue-axios";

import router from "./router";
import "./index.css";
import App from "./App.vue";

const axiosInstance = axios.create({
  baseURL: "https://pkmntracker.ensmann.de",
});

const app = createApp(App);
app
  .use(createPinia())
  .use(VueAxios, axiosInstance)
  .provide("axios", app.config.globalProperties.axiosInstance)
  .use(router)
  .mount("#app");
