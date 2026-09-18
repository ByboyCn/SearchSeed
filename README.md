# SearchSeed · 戴森球计划种子查看器 & 搜索器（Web 版）

用 C# 重写的《戴森球计划》种子工具：输入一个种子号，离线算出整个宇宙——恒星、行星、卫星、矿脉数量与储量、气态巨星大气成分、戴森球建造数据；也可以按条件树自动扫描种子号，找出满足要求的"神种"。

**在线体验：<https://dsp.byboy.cc>**

---

## 特性

### 种子查看器
- 输入 `种子号 + 恒星数量 + 资源倍率`，秒出完整星系数据（无需游戏、无需存档）
- 恒星：名称 / 类型（红巨星…黑洞/O 型）/ 距离 / 光度 L / 戴森球光度 / 行星数量 / 该星系矿脉汇总
- 行星：主题类型 / 特殊特性（潮汐锁定、轨道共振、横躺自转、反向自转…）/ 光度 / 风能 / 离子层高度
- **气态巨星与冰巨星的大气成分**（氢、重氢、可燃冰的每秒产量）
- **矿脉精确到"个"**：14 种矿产的数量与储量（铁/铜/硅/钛/石/煤/油/可燃冰/金伯利矿石/分形硅石/有机晶体/光栅石/刺笋结晶/单极磁石）
- 卫星按层级挂在母星下展示

### 种子搜索器
- 四层**嵌套条件树**，与前辈工具的条件组织方式一致：
  ```
  星区条件（全星系矿脉数量/储量）
   └─ 恒星系条件（恒星类型、最低光照、最远距离、符合数量、矿脉数量/储量）
       ├─ 行星条件（星球类型、特殊条件、液体类型、戴森球层、符合数量、矿脉数量/储量）
       │   └─ 卫星条件（可继续嵌套）
       └─ …
  ```
- 每个条件卡可**独立勾选/停用、自定义名称、随时增删**，层层嵌套
- 后台多线程异步扫描，实时显示"已扫种子数 / 命中数 / 缓存命中数"，可随时停止
- 结果可直接点击种子号跳转查看器

### 计算精度
- **标准模式**：完整复刻游戏的地形生成（Simplex 噪声 + 14 套地形算法）后按游戏规则撒矿点，结果与游戏近乎一致
- **快速模式**：用理论矿簇上限估算，速度快约 100 倍，实际值约为其 0.8 倍
- 已验证：恒星名称 / 类型 / 随机种子链与参考实现**逐位一致**（64 星 × 多种子全对）

### 预热与缓存
- 服务启动后自动**后台预热**常见组合（64 星 × 各资源倍率优先），结果 gzip 落盘
- 用户搜索/查看过的种子自动记录，**下次直接命中缓存秒回**，预热与搜索互相跳过已算过的种子
- 进度带游标持久化，容器重启后续算不丢

---

## 技术架构

```
SearchSeed.Core/          计算核心（.NET 类库，无外部依赖）
├── Gen/                 游戏算法移植
│   ├── DotNet35Random    .NET 3.5 System.Random 精确移植（随机链的根）
│   ├── SimplexNoise      Simplex 噪声（含 FBM / Ridged）
│   ├── GalaxyGen         星系生成：恒星位置/类型/物理参数、行星轨道与主题分配
│   ├── PlanetAlgorithms  14 套星球地形算法（algo 0–13）
│   ├── NativeMath        Unity 原生数学的逐位移植（ucrt sin/cos/acos、Slerp）
│   ├── StandardVeins     标准模式矿脉生成（地形采样 + 矿簇扩散）
│   └── QuickVeins        快速模式矿脉估算
├── Data/                 游戏静态数据（25 种星球主题表、矿产、星名库）
└── SeedService           种子 → 结果 DTO，含并行地形计算

SearchSeed.Api/           ASP.NET Core Web API
├── Controllers           /api/seed、/api/search
└── Services
    ├── GalaxyStore       结果落盘（gzip）+ 已计算索引 + 游标
    ├── PrecomputeService 后台预热计算
    └── SearchService     条件搜索（并行扫描）

web/                      Vue 3 + Element Plus 前端（构建产物由 API 静态托管）
deploy/                   nginx 反向代理配置
```

---

## 快速开始

### 本地运行

```bash
# 后端（默认监听 http://localhost:5073）
dotnet run --project SearchSeed.Api

# 前端（开发模式，已配置 /api 代理）
cd web && npm install && npm run dev
```

### Docker 部署

```bash
docker compose up -d --build
# 容器内监听 8080，宿主映射 127.0.0.1:18090
```

nginx 反向代理示例见 `deploy/dsp.byboy.cc.conf`。

### 配置项（appsettings.json / 环境变量）

| 配置 | 默认 | 说明 |
| --- | --- | --- |
| `Precompute:Enabled` | `false` | 是否启动后台预热计算 |
| `Search:MaxConcurrency` | `4` | 搜索并行度 |

> 预热会持续占用 CPU，生产环境建议留出至少一半核心给用户请求。

---

## API

### 查看种子

```
GET /api/seed/{seedId}?starNum=64&resourceIndex=4&mode=standard
```

| 参数 | 说明 |
| --- | --- |
| `seedId` | 种子号 |
| `starNum` | 恒星数量：32 / 48 / 64 / 128 / 256 |
| `resourceIndex` | 资源倍率下标：0=0.1x, 1=0.3x, 2=0.5x, 3=0.8x, 4=1x, 5=1.5x, 6=2x, 7=3x, 8=5x, 9=8x, 10=无限 |
| `mode` | `standard`（精确）或 `fast`（快速） |

### 搜索种子

```
POST /api/search/start      # 提交条件，返回 { jobId }
GET  /api/search/{jobId}    # 轮询进度与结果
POST /api/search/{jobId}/stop
```

请求体示例——找一颗带"贫瘠荒漠 + 单极磁石 ≥ 10"行星的中子星：

```json
{
  "fromSeed": 0,
  "starNum": 64,
  "resourceIndex": 4,
  "fastMode": true,
  "maxResults": 20,
  "conditions": {
    "veins": { "point": {}, "amount": {} },
    "stars": [{
      "name": "中子星条件", "checked": true,
      "types": ["中子星"], "minLumino": 0, "maxDistance": -1, "satisfyNum": 1,
      "veins": { "point": {}, "amount": {} },
      "planets": [{
        "name": "行星条件", "checked": true,
        "types": ["贫瘠荒漠"], "singularity": [], "liquid": "", "dspLevelMin": 0,
        "satisfyNum": 1,
        "veins": { "point": { "单极磁石": 10 }, "amount": {} },
        "moons": []
      }]
    }],
    "planets": []
  }
}
```

---

## 精度说明与已知差异

种子在游戏里是**永久确定**的：同一个种子 + 同样的恒星数量与资源倍率，任何机器、任何时间算出来的宇宙都逐位一致（随机数链固定）。本项目的做法就是把这套生成代码原样重演，因此不需要存档、不需要联网、也不依赖游戏版本数据库。

已知差异（均为浮点舍入级别，全部集中在"矿脉落在海陆边界"的临界点上）：

| 对照对象 | 差异 | 原因 |
| --- | --- | --- |
| 参考实现（AVX2/SIMD 编译版） | 矿脉总数差 0.17% ~ 0.30% | 参考实现的 SIMD 路径在两个 Levelize 之间多一次 float 舍入，本项目采用**标量路径**（即游戏本体 C# 代码的算法） |
| 参考实现（单星球地形吻合度） | 121 颗星球中 118 颗一致，3 颗有差 | 同上，3 颗均为同一套地形算法（algo 10） |

`DotNet35Random`、`SimplexNoise`、`Levelize` 等基础函数已用独立编译的 C++ 标量基准验证**逐位一致**。

---

## 鸣谢

本项目的算法与数据移植自以下开源项目，没有它们就没有这个工具：

- **[botany233/dsp_search_seed](https://github.com/botany233/dsp_search_seed)** — 戴森球计划种子搜索&查看器（C++ / PyQt）。
  本项目的**主要参考**：星系生成、地形算法、矿脉生成、星名生成、游戏静态数据表、条件搜索语义，以及验证用的编译基准 `CApi/search_seed.pyd` 均来自该项目。GPL-3.0。
- **[crazyyao0](https://github.com/crazyyao0)** — 上述项目中星系生成、随机数、星名、静态数据等核心 C++ 代码的原始作者（源码头部保留其版权声明）。
- **[soarqin/DSPSeedCalc](https://github.com/soarqin/DSPSeedCalc)** — Unity 原生数学（ucrt `sinf`/`cosf`/`acosf` 多项式近似、`Vector3.Slerp`、`CalcVerts` 顶点表）的逐位精确实现来源。
- **[Xinyuell/DspFindSeed](https://github.com/Xinyuell/DspFindSeed)** — 种子列表 CSV 格式（`种子id, 恒星数量`）与之兼容。

同时感谢：

- **柚子猫工作室（Youthcat Studio）** 与 **Gamera Game** 开发的《戴森球计划》（Dyson Sphere Program）——所有算法与数据最终都源于这款游戏。
- [.NET](https://dotnet.microsoft.com/) / ASP.NET Core、[Vue 3](https://vuejs.org/)、[Element Plus](https://element-plus.org/)、[Vite](https://vitejs.dev/)、[Docker](https://www.docker.com/)、[nginx](https://nginx.org/)

---

## 许可

本项目为上述 GPL-3.0 项目的衍生移植作品，因此同样以 **GPL-3.0** 发布，详见 [LICENSE](LICENSE)。

## 免责声明

本项目为粉丝向工具，与《戴森球计划》开发方及发行方无任何关联，不包含游戏本体资源。计算结果是生成算法的离线重演，仅供参考；请以游戏内实际表现为准。
