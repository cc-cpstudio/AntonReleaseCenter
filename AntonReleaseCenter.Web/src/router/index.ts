import HomePage from "../pages/HomePage.vue";
import LoginPage from "../pages/LoginPage.vue";
import ReleasePage from "../pages/ReleasePage.vue";
import {createRouter, createWebHistory} from "vue-router";

const routes = [
    {
        path: '/',
        name: "Home", 
        component: HomePage,
    },
    {
        path: '/login',
        name: "Login",
        component: LoginPage,
    },
    {
        path: '/release',
        name: "Release",
        component: ReleasePage,
    },
]

const router = createRouter({
    history: createWebHistory(),
    routes,
})

export default router;