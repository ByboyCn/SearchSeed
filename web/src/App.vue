<template>
  <div class="page">
    <header class="header">
      <h1>戴森球计划 · 种子工具</h1>
      <el-radio-group v-model="tab">
        <el-radio-button value="search">种子搜索</el-radio-button>
        <el-radio-button value="viewer">种子查看</el-radio-button>
      </el-radio-group>
    </header>

    <!-- ===== 搜索器 ===== -->
    <el-row v-show="tab === 'search'" :gutter="12">
      <el-col :span="13">
        <el-card shadow="never">
          <template #header>
            <div class="card-head">
              <span>搜索条件</span>
              <div class="head-actions">
                <el-button size="small" @click="resetCond">清空条件</el-button>
              </div>
            </div>
          </template>

          <!-- 星区条件 -->
          <div class="galaxy-block">
            <div class="gb-title">星区条件</div>
            <div class="vein-block">
              <div class="vb-title">全星系矿脉数量 ≥</div>
              <div class="vein-cond">
                <div v-for="v in veins" :key="v" class="vc">
                  <span class="vn">{{ v }}</span>
                  <el-input-number v-model="cond.veins.point[v]" :min="0" :step="100" :controls="false" size="small" style="width:100%" />
                </div>
              </div>
              <div class="vb-title">全星系矿脉储量 ≥</div>
              <div class="vein-cond">
                <div v-for="v in veins" :key="v" class="vc">
                  <span class="vn">{{ v }}</span>
                  <el-input-number v-model="cond.veins.amount[v]" :min="0" :step="100000" :controls="false" size="small" style="width:100%" />
                </div>
              </div>
            </div>
          </div>

          <ConditionCard v-for="(s, i) in cond.stars" :key="'s' + i" :node="s" kind="star" @remove="cond.stars.splice(i, 1)" />
          <div class="add-row">
            <el-button type="primary" plain size="small" @click="addStar">+ 添加恒星系条件</el-button>
          </div>

          <ConditionCard v-for="(p, i) in cond.planets" :key="'p' + i" :node="p" kind="planet" @remove="cond.planets.splice(i, 1)" />
          <div class="add-row">
            <el-button type="primary" plain size="small" @click="addPlanet('planets')">+ 添加全星系行星条件</el-button>
          </div>

          <el-divider />
          <div class="run-row">
            <span class="lbl">起始种子</span>
            <el-input-number v-model="fromSeed" :min="0" :controls="false" size="small" style="width:120px" />
            <span class="lbl">恒星数</span>
            <el-select v-model="starNum" size="small" style="width:88px">
              <el-option v-for="n in [32,48,64,128,256]" :key="n" :label="n" :value="n" />
            </el-select>
            <span class="lbl">资源</span>
            <el-select v-model="resourceIndex" size="small" style="width:80px">
              <el-option v-for="(r, i) in resourceNames" :key="i" :label="r" :value="i" />
            </el-select>
            <el-select v-model="searchMode" size="small" style="width:96px">
              <el-option label="快速模式" value="fast" />
              <el-option label="标准模式" value="standard" />
            </el-select>
            <el-button type="primary" size="small" :disabled="!!jobId" @click="startSearch">开始搜索</el-button>
            <el-button type="danger" size="small" :disabled="!jobId" @click="stopSearch">停止</el-button>
          </div>
        </el-card>
      </el-col>

      <el-col :span="11">
        <el-card shadow="never">
          <template #header>
            搜索结果
            <span v-if="job" class="stat">
              已扫 {{ job.scanned }} 个 · 命中 {{ matches.length }} · 缓存命中 {{ job.skipped }}
              <el-tag v-if="job.finished" size="small" type="success">已完成</el-tag>
              <el-tag v-else-if="jobId" size="small" type="warning">搜索中…</el-tag>
            </span>
          </template>
          <el-table :data="matches" height="620" size="small">
            <el-table-column label="种子" width="90">
              <template #default="{ row }">
                <el-link type="primary" @click="viewSeed(row.seed)">{{ row.seed }}</el-link>
              </template>
            </el-table-column>
            <el-table-column label="命中恒星">
              <template #default="{ row }">
                <div v-for="s in row.stars" :key="s.index" class="hit">
                  {{ s.name }} <span class="dim">{{ s.type }} · {{ s.distance.toFixed(1) }}ly · L{{ s.dysonLumino }}</span>
                </div>
              </template>
            </el-table-column>
          </el-table>
        </el-card>
      </el-col>
    </el-row>

    <!-- ===== 查看器 ===== -->
    <div v-show="tab === 'viewer'">
      <div class="toolbar">
        <el-input-number v-model="seedId" :min="0" :max="999999999" :controls="false" style="width:150px" placeholder="种子" />
        <el-select v-model="starNum" style="width:100px">
          <el-option v-for="n in [32,48,64,128,256]" :key="n" :label="n + ' 恒星'" :value="n" />
        </el-select>
        <el-select v-model="resourceIndex" style="width:100px">
          <el-option v-for="(r, i) in resourceNames" :key="i" :label="r" :value="i" />
        </el-select>
        <el-radio-group v-model="mode">
          <el-radio-button value="standard">标准</el-radio-button>
          <el-radio-button value="fast">快速</el-radio-button>
        </el-radio-group>
        <el-button type="primary" :loading="loading" @click="load">查看</el-button>
      </div>

      <el-row v-if="galaxy" :gutter="12" class="summary">
        <el-col :span="12">
          <el-card shadow="never"><template #header>全星系矿脉数量</template>
            <div class="vein-grid">
              <div v-for="(v, name) in galaxy.veinsPoint" :key="name" class="vein-cell"><span class="vn">{{ name }}</span><span>{{ formatNum(v) }}</span></div>
            </div>
          </el-card>
        </el-col>
        <el-col :span="12">
          <el-card shadow="never"><template #header>全星系矿脉储量</template>
            <div class="vein-grid">
              <div v-for="(v, name) in galaxy.veinsAmount" :key="name" class="vein-cell"><span class="vn">{{ name }}</span><span>{{ formatNum(v) }}</span></div>
            </div>
          </el-card>
        </el-col>
      </el-row>

      <el-table v-if="galaxy" :data="galaxy.stars" height="520" @row-click="selectStar" highlight-current-row>
        <el-table-column prop="index" label="#" width="50" />
        <el-table-column prop="name" label="恒星" width="180" />
        <el-table-column prop="type" label="类型" width="110" />
        <el-table-column label="距离(ly)" width="90"><template #default="{ row }">{{ row.distance.toFixed(2) }}</template></el-table-column>
        <el-table-column label="光度L" width="80"><template #default="{ row }">{{ row.luminosity }}</template></el-table-column>
        <el-table-column label="戴森圈L" width="90"><template #default="{ row }">{{ row.dysonLumino }}</template></el-table-column>
        <el-table-column prop="planetCount" label="行星" width="60" />
        <el-table-column label="矿脉">
          <template #default="{ row }">
            <el-tag v-for="(v, name) in row.veinsPoint" :key="name" size="small" class="tag" type="info">{{ name }} {{ formatNum(v) }}</el-tag>
          </template>
        </el-table-column>
      </el-table>

      <el-drawer v-model="drawer" :title="selected?.name" size="72%">
        <div v-if="selected">
          <el-table :data="flatPlanets(selected)" row-key="name" default-expand-all>
            <el-table-column prop="name" label="行星" width="170" />
            <el-table-column prop="type" label="类型" width="95" />
            <el-table-column label="特性" width="140">
              <template #default="{ row }">
                <el-tag v-for="s in row.singularity" :key="s" size="small" class="tag" type="warning">{{ s }}</el-tag>
              </template>
            </el-table-column>
            <el-table-column label="大气成分" width="170">
              <template #default="{ row }">
                <div v-for="g in row.gas" :key="g" class="gas">{{ g }}</div>
              </template>
            </el-table-column>
            <el-table-column label="光度" width="65"><template #default="{ row }">{{ row.luminosity }}</template></el-table-column>
            <el-table-column label="风能" width="65"><template #default="{ row }">{{ row.wind }}</template></el-table-column>
            <el-table-column label="矿脉">
              <template #default="{ row }">
                <div v-for="(v, name) in row.veinsPoint" :key="name" class="pv">{{ name }}: <b>{{ formatNum(v) }}</b> 个 / <b>{{ formatNum(row.veinsAmount[name]) }}</b></div>
              </template>
            </el-table-column>
          </el-table>
        </div>
      </el-drawer>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onUnmounted } from 'vue'
import { ElMessage } from 'element-plus'
import ConditionCard from './components/ConditionCard.vue'

const tab = ref('search')
const seedId = ref(1234)
const starNum = ref(64)
const resourceIndex = ref(4)
const mode = ref('standard')
const loading = ref(false)
const galaxy = ref(null)
const drawer = ref(false)
const selected = ref(null)
const resourceNames = ['0.1x','0.3x','0.5x','0.8x','1x','1.5x','2x','3x','5x','8x','无限']
const veins = ['铁','铜','硅','钛','石','煤','油','可燃冰','金伯利矿石','分形硅石','有机晶体','光栅石','刺笋结晶','单极磁石']

function emptyGalaxy() { return { veins: { point: {}, amount: {} }, stars: [], planets: [] } }
const cond = reactive(emptyGalaxy())

function resetCond() {
  const e = emptyGalaxy()
  cond.veins = e.veins
  cond.stars = e.stars
  cond.planets = e.planets
}
function addStar() {
  cond.stars.push({ name: '恒星系条件', checked: true, types: [], minLumino: 0, maxDistance: -1, satisfyNum: 1, veins: { point: {}, amount: {} }, planets: [] })
}
function addPlanet(list) {
  cond[list].push({ name: '行星条件', checked: true, types: [], singularity: [], liquid: '', dspLevelMin: 0, satisfyNum: 1, veins: { point: {}, amount: {} }, moons: [] })
}

async function load() {
  loading.value = true
  galaxy.value = null
  try {
    const res = await fetch(`/api/seed/${seedId.value}?starNum=${starNum.value}&resourceIndex=${resourceIndex.value}&mode=${mode.value}`)
    if (!res.ok) throw new Error(await res.text())
    galaxy.value = await res.json()
  } catch (e) { ElMessage.error('加载失败: ' + e.message) }
  finally { loading.value = false }
}
function selectStar(row) { selected.value = row; drawer.value = true }
function viewSeed(seed) { seedId.value = seed; tab.value = 'viewer'; load() }
function flatPlanets(star) {
  const out = []
  for (const p of star.planets) {
    out.push(p)
    for (const m of (p.moons || [])) out.push({ ...m, name: '　└ ' + m.name })
  }
  return out
}
function formatNum(n) {
  if (n >= 1e8) return (n / 1e8).toFixed(2) + '亿'
  if (n >= 1e4) return (n / 1e4).toFixed(1) + '万'
  return n
}

// ===== 搜索 =====
const fromSeed = ref(0)
const searchMode = ref('fast')
const jobId = ref(null)
const job = ref(null)
const matches = ref([])
let pollTimer = null

function cleanVeins(v) {
  const point = {}, amount = {}
  for (const [k, val] of Object.entries(v.point || {})) if (val > 0) point[k] = val
  for (const [k, val] of Object.entries(v.amount || {})) if (val > 0) amount[k] = val
  return { point, amount }
}
function cleanTree(node) {
  const o = { ...node, veins: cleanVeins(node.veins) }
  if (node.planets) o.planets = node.planets.map(cleanTree)
  if (node.moons) o.moons = node.moons.map(cleanTree)
  delete o.uuid
  return o
}

async function startSearch() {
  const body = {
    fromSeed: fromSeed.value, starNum: starNum.value, resourceIndex: resourceIndex.value,
    fastMode: searchMode.value === 'fast', maxResults: 200,
    conditions: {
      veins: cleanVeins(cond.veins),
      stars: cond.stars.map(cleanTree),
      planets: cond.planets.map(cleanTree),
    },
  }
  const res = await fetch('/api/search/start', {
    method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body),
  })
  if (!res.ok) { ElMessage.warning(await res.text()); return }
  const { jobId: id } = await res.json()
  jobId.value = id
  matches.value = []
  job.value = null
  pollTimer = setInterval(poll, 1500)
  ElMessage.success('搜索已开始')
}
async function poll() {
  const res = await fetch(`/api/search/${jobId.value}`)
  if (!res.ok) return
  job.value = await res.json()
  matches.value = job.value.matches
  if (job.value.finished || job.value.stopped || matches.value.length >= job.value.maxResults) {
    clearInterval(pollTimer)
    jobId.value = null
  }
}
async function stopSearch() {
  if (jobId.value) await fetch(`/api/search/${jobId.value}/stop`, { method: 'POST' })
}
onUnmounted(() => clearInterval(pollTimer))

load()
</script>

<style>
body { margin: 0; background: #f5f7fa; color: #303133; }
.page { max-width: 1500px; margin: 0 auto; padding: 16px; }
.header { display: flex; justify-content: space-between; align-items: center; flex-wrap: wrap; gap: 8px; margin-bottom: 12px; }
.header h1 { font-size: 20px; margin: 0; }
.toolbar { display: flex; gap: 8px; align-items: center; flex-wrap: wrap; margin-bottom: 12px; }
.summary { margin-bottom: 12px; }
.card-head { display: flex; justify-content: space-between; align-items: center; }
.galaxy-block { border: 1px solid #f0c78a; background: #fffaf2; border-radius: 6px; padding: 8px; margin-bottom: 8px; }
.gb-title { font-weight: 600; font-size: 13px; margin-bottom: 6px; }
.vein-block { margin-top: 4px; }
.vb-title { font-size: 12px; color: #909399; margin: 4px 0 2px; }
.vein-cond { display: grid; grid-template-columns: repeat(7, 1fr); gap: 3px; }
.vc { display: flex; flex-direction: column; align-items: center; font-size: 11px; }
.vn { color: #606266; white-space: nowrap; }
.add-row { margin: 6px 0 10px; }
.run-row { display: flex; align-items: center; gap: 8px; flex-wrap: wrap; }
.lbl { font-size: 12px; color: #606266; }
.stat { font-size: 12px; color: #909399; margin-left: 8px; }
.hit { font-size: 12px; }
.dim { color: #a8abb2; }
.vein-grid { display: grid; grid-template-columns: repeat(7, 1fr); gap: 4px; }
.vein-cell { display: flex; justify-content: space-between; background: #f0f2f5; border-radius: 4px; padding: 4px 8px; }
.pv { font-size: 12px; }
.gas { font-size: 12px; color: #67c23a; }
.tag { margin: 1px; }
</style>
