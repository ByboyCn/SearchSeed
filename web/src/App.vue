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
            <el-input-number v-model="starNumFrom" :min="32" :max="64" :controls="false" size="small" style="width:64px" />
            <span class="lbl">~</span>
            <el-input-number v-model="starNumTo" :min="32" :max="64" :controls="false" size="small" style="width:64px" />
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
          <el-table :data="matches" height="calc(100vh - 190px)" size="small">
            <el-table-column label="种子" width="86">
              <template #default="{ row }">
                <el-link type="primary" @click="viewSeed(row.seed, row.starNum)">{{ row.seed }}</el-link>
              </template>
            </el-table-column>
            <el-table-column prop="starNum" label="恒星" width="56" />
            <el-table-column label="命中恒星">
              <template #default="{ row }">
                <div v-for="s in row.stars" :key="s.index" class="hit">
                  {{ s.name }} <span class="dim">{{ s.type }} · {{ f6(s.distance) }}ly · L{{ s.dysonLumino }}</span>
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
        <el-input-number v-model="starNum" :min="32" :max="64" :controls="false" style="width:90px" />
        <span class="lbl">恒星</span>
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

      <el-table v-if="galaxy" :data="galaxy.stars" height="calc(100vh - 320px)" @row-click="selectStar" highlight-current-row>
        <el-table-column prop="index" label="#" width="50" />
        <el-table-column prop="name" label="恒星" width="180" />
        <el-table-column prop="type" label="类型" width="110" column-key="type"
          :filters="starTypeNames.map(t => ({ text: t, value: t }))"
          :filter-method="(v, row) => row.type === v" />
        <el-table-column label="距离(ly)" width="90"><template #default="{ row }">{{ f6(row.distance) }}</template></el-table-column>
        <el-table-column label="光度L" width="80"><template #default="{ row }">{{ row.dysonLumino }}</template></el-table-column>
        <el-table-column label="太阳能L" width="90"><template #default="{ row }">{{ row.luminosity }}</template></el-table-column>
        <el-table-column prop="planetCount" label="行星" width="60" />
        <el-table-column label="安全度" width="90">
          <template #default="{ row }">
            <el-tag size="small" :type="row.hivePatternLevel === 0 ? 'success' : row.hivePatternLevel === 1 ? 'warning' : 'danger'">
              {{ Math.round(row.safetyFactor * 100) }}%
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="矿脉">
          <template #default="{ row }">
            <el-tag v-for="(v, name) in row.veinsPoint" :key="name" size="small" class="tag" type="info">{{ name }} {{ formatNum(v) }}</el-tag>
          </template>
        </el-table-column>
      </el-table>

      <el-drawer v-model="drawer" :title="selected?.name" size="72%">
        <div v-if="selected">
          <el-descriptions :column="7" border size="small" class="star-desc">
            <el-descriptions-item label="类型">{{ selected.type }}</el-descriptions-item>
            <el-descriptions-item label="光谱">{{ selected.spectr }} 型</el-descriptions-item>
            <el-descriptions-item label="质量">{{ f6(selected.starMass) }} M☉</el-descriptions-item>
            <el-descriptions-item label="半径">{{ f6(selected.starRadius) }} R☉</el-descriptions-item>
            <el-descriptions-item label="光度">{{ selected.dysonLumino }} L</el-descriptions-item>
            <el-descriptions-item label="太阳能光度">{{ selected.luminosity }} L</el-descriptions-item>
            <el-descriptions-item label="表面温度">{{ Math.round(selected.temperature) }} K</el-descriptions-item>
            <el-descriptions-item label="年龄">{{ f6(selected.age * 100) }}%</el-descriptions-item>
          </el-descriptions>
          <el-table :data="flatPlanets(selected)" row-key="name" default-expand-all height="calc(100vh - 230px)">
            <el-table-column prop="name" label="行星" width="170" />
            <el-table-column prop="type" label="类型" width="95" column-key="type"
              :filters="planetTypeNames.map(t => ({ text: t, value: t }))"
              :filter-method="(v, row) => row.type === v" />
            <el-table-column label="特性" width="140">
              <template #default="{ row }">
                <el-tag v-for="s in row.singularity" :key="s" size="small" class="tag" type="warning">{{ s }}</el-tag>
              </template>
            </el-table-column>
            <el-table-column label="海洋" width="70">
              <template #default="{ row }"><span class="dim">{{ row.liquid || '—' }}</span></template>
            </el-table-column>
            <el-table-column label="适建区域" width="80">
              <template #default="{ row }"><span class="dim">{{ row.landPercent < 0 ? '—' : (row.landPercent * 100).toFixed(6) + '%' }}</span></template>
            </el-table-column>
            <el-table-column label="轨道半径" width="80">
              <template #default="{ row }"><span class="dim">{{ f6(row.orbitRadius) }} AU</span></template>
            </el-table-column>
            <el-table-column label="公转周期" width="90">
              <template #default="{ row }"><span class="dim">{{ fmtPeriod(row.orbitalPeriodSec) }}</span></template>
            </el-table-column>
            <el-table-column label="自转周期" width="90">
              <template #default="{ row }"><span class="dim">{{ fmtPeriod(row.rotationPeriodSec) }}</span></template>
            </el-table-column>
            <el-table-column label="轨道倾角" width="80">
              <template #default="{ row }"><span class="dim">{{ f6(row.orbitInclination) }}°</span></template>
            </el-table-column>
            <el-table-column label="升交点经度" width="90">
              <template #default="{ row }"><span class="dim">{{ f6(row.orbitLongitude) }}°</span></template>
            </el-table-column>
            <el-table-column label="地轴倾角" width="80">
              <template #default="{ row }"><span class="dim">{{ f6(row.obliquity) }}°</span></template>
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
const starTypeNames = ['红巨星','黄巨星','蓝巨星','白巨星','白矮星','中子星','黑洞','A型恒星','B型恒星','F型恒星','G型恒星','K型恒星','M型恒星','O型恒星']
const planetTypeNames = ['地中海','气态巨星','冰巨星','高产气巨','干旱荒漠','灰烬冻土','海洋丛林','熔岩','冰原冻土','贫瘠荒漠','戈壁','火山灰','红石','草原','水世界','黑石盐滩','樱林海','飓风石林','猩红冰湖','热带草原','橙晶荒漠','极寒冻土','潘多拉沼泽']

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
function viewSeed(seed, sn) { seedId.value = seed; if (sn) starNum.value = sn; tab.value = 'viewer'; load() }
function flatPlanets(star) {
  const out = []
  for (const p of star.planets) {
    out.push(p)
    for (const m of (p.moons || [])) out.push({ ...m, name: '　└ ' + m.name })
  }
  return out
}

function f6(v) { return v == null ? '—' : v.toFixed(6) }
function fmtPeriod(sec) {
  if (sec == null) return '—'
  const abs = Math.abs(sec)
  const sign = sec < 0 ? '-' : ''
  if (abs >= 86400 * 30) return sign + (abs / 86400).toFixed(0) + '天'
  if (abs >= 86400) return sign + (abs / 86400).toFixed(1) + '天'
  if (abs >= 3600) return sign + (abs / 3600).toFixed(1) + '时'
  if (abs >= 60) return sign + (abs / 60).toFixed(1) + '分'
  return sign + abs.toFixed(0) + '秒'
}
function formatNum(n) {
  if (n >= 1e8) return (n / 1e8).toFixed(2) + '亿'
  if (n >= 1e4) return (n / 1e4).toFixed(1) + '万'
  return n
}

// ===== 搜索 =====
const fromSeed = ref(0)
const starNumFrom = ref(32)
const starNumTo = ref(64)
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
    fromSeed: fromSeed.value, starNumFrom: starNumFrom.value, starNumTo: starNumTo.value,
    resourceIndex: resourceIndex.value,
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
.header { display: flex; justify-content: space-between; align-items: center; flex-wrap: wrap; gap: 8px; margin-bottom: 12px;
  position: sticky; top: 0; z-index: 20; background: #f5f7fa; padding: 8px 0; }
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
.star-desc { margin-bottom: 10px; }
.tag { margin: 1px; }
</style>