import HomePage from "../pages/HomePage.vue";
import LoginPage from "../pages/LoginPage.vue";
import ReleasePage from "../pages/ReleasePage.vue";
import {createRouter, createWebHistory} from "vue-router";
import Cookies from "js-cookie";

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
        meta: { requiresAuth: true },
    },
]

const router = createRouter({
    history: createWebHistory(),
    routes,
})

router.beforeEach((to, _from, next) => {
    if (to.meta.requiresAuth) {
        const token = Cookies.get('jwt_token')
        if (!token) {
            next({ name: 'Login' })
        } else {
            next()
        }
    } else {
        next()
    }
})

export default router;
