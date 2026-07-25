# AntonReleaseCenter

**软件版本管理中心** — 提供软件版本发布、多渠道分发、客户端更新检查的一站式解决方案。

## 项目架构

```
┌─────────────────────────────────────────────────────┐
│               AntonReleaseCenter.Web                │
│              (Vue 3 + TypeScript)                   │
├─────────────────────────────────────────────────────┤
│             AntonReleaseCenter.Server               │
│        (ASP.NET Core Web API + PostgreSQL)          │
├─────────────────────────────────────────────────────┤
│             AntonReleaseCenter.Core                 │
│         (领域模型、DTO、服务接口)                       │
├─────────────────────────────────────────────────────┤
│          AntonReleaseCenter.SoftwareSDK             │
│           (.NET 客户端 SDK)                          │
└─────────────────────────────────────────────────────┘
```

## 技术栈

| 层 | 技术 |
|---|------|
| 后端框架 | .NET 10 + ASP.NET Core Web API |
| 数据库 | PostgreSQL + EF Core 10 |
| 认证 | JWT Bearer |
| API 文档 | OpenAPI + Scalar |
| 前端 | Vue 3 + TypeScript + Element Plus + Vite |
| 客户端 SDK | .NET 10 |
| 测试 | xUnit + Moq + EF Core InMemory |

## 前提条件

- .NET 10 SDK
- PostgreSQL（>= 14）
- Node.js（>= 20）
- pnpm（可选，推荐）

## 快速开始

### 0. 仓库

使用前，请先将本仓库 Fork 至您的 GitHub，再执行：

```bash
git clone https://github.com/your-account-id/AntonReleaseCenter
```

### 1. 数据库

```bash
# 修改 appsettings.Development.json 中的连接字符串
# 然后执行迁移
dotnet ef database update --project AntonReleaseCenter.Server
```

### 2. 启动后端

```bash
dotnet run --project AntonReleaseCenter.Server
```

默认监听 `http://localhost:5251`，API 文档访问 `http://localhost:5251/scalar`。

### 3. 启动前端

```bash
cd AntonReleaseCenter.Web
pnpm install
pnpm run dev
```

默认监听 `http://localhost:5173`。

### 4. 初始化管理员账号

启动后调用以下接口创建初始管理员：

```bash
POST /api/admin/register
Content-Type: application/json

{
  "username": "admin",
  "password": "your-password"
}
```

## 项目结构

```
AntonReleaseCenter/
├── AntonReleaseCenter.Core/          # 领域层
│   ├── Models/                       #   实体
│   ├── DTOs/                         #   数据传输对象
│   └── Services/                     #   服务接口
├── AntonReleaseCenter.Server/        # Web API 服务端
│   ├── Controllers/                  #   API 控制器
│   ├── Services/                     #   服务实现
│   ├── DbContexts/                   #   EF Core 上下文
│   └── Migrations/                   #   数据库迁移
├── AntonReleaseCenter.Web/           # 管理后台前端
│   └── src/
│       ├── pages/                    #   页面
│       └── components/               #   组件
├── AntonReleaseCenter.SoftwareSDK/   # .NET 客户端 SDK
└── AntonReleaseCenter.Tests/         # 单元测试
```

## API 概览

| 分组 | 路由 | 说明 |
|------|------|------|
| 公开接口 | `/api/public/*` | 客户端更新检查，无需认证 |
| 认证 | `/api/auth/*` | 管理员登录 |
| 管理 | `/api/admin/*` | 管理员管理 |
| 软件 | `/api/software/*` | 软件 CRUD |
| 发布 | `/api/releases/*` | 版本发布管理 |
| 渠道 | `/api/channels/*` | 分发渠道管理 |

## 客户端集成

### .NET SDK

```csharp
services.AddAntonReleaseCenter(options =>
{
    options.Url = "http://your-server:5251";
    options.SoftwareKey = "your-software-key";
    options.ChannelCode = 1;
    options.Platform = PlatformEnum.Windows;
});
```

### 检查更新

```csharp
var result = await updateCheckService.CheckUpdateAsync("1.0.0.0");
if (result?.HasUpdate == true)
{
    // 有新版本可用
    await fileDownloadService.DownloadRelease(result.Release, savePath, progress);
}
```

## 开发指引

- 后端使用 **Scalar** 浏览 API 文档（`/scalar`）
- 前端代理配置在 `vite.config.ts` 中调整
- 数据库迁移使用 `dotnet ef migrations add <名称>`
- 测试使用 InMemory 数据库，无需真实 PostgreSQL

## 协议

MIT License — 详见 [LICENSE](./LICENSE)
