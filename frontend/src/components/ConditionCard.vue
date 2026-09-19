<template>
  <div class="cond-card" :class="{ star: kind === 'star', planet: kind === 'planet', galaxy: kind === 'galaxy' }">
    <div class="cc-head">
      <el-checkbox v-model="node.checked" />
      <el-input v-model="node.name" size="small" class="name-input" />
      <el-button v-if="kind !== 'galaxy'" size="small" text type="danger" @click="$emit('remove')">删除</el-button>
    </div>

    <!-- 恒星系条件 -->
    <template v-if="kind === 'star'">
      <div class="row">
        <span class="lbl">恒星类型</span>
        <el-select v-model="node.types" multiple collapse-tags size="small" placeholder="不限" class="grow">
          <el-option v-for="t in starTypes" :key="t" :label="t" :value="t" />
        </el-select>
      </div>
      <div class="row">
        <span class="lbl">最低光照</span>
        <el-input-number v-model="node.minLumino" :min="0" :step="0.1" :precision="2" :controls="false" size="small" style="width:80px" />
        <span class="lbl">最远距离</span>
        <el-input-number v-model="node.maxDistance" :min="-1" :step="5" :controls="false" size="small" style="width:80px" />
        <span class="hint">ly，-1=不限，0=初始恒星系</span>
      </div>
      <div class="row">
        <span class="lbl">符合数量</span>
        <el-input-number v-model="node.satisfyNum" :min="1" :controls="false" size="small" style="width:70px" />
        <span class="hint">颗恒星需满足本条件</span>
      </div>
    </template>

    <!-- 行星条件 -->
    <template v-if="kind === 'planet'">
      <div class="row">
        <span class="lbl">星球类型</span>
        <el-select v-model="node.types" multiple collapse-tags filterable size="small" placeholder="不限" class="grow">
          <el-option v-for="t in planetTypes" :key="t" :label="t" :value="t" />
        </el-select>
      </div>
      <div class="row">
        <span class="lbl">特殊条件</span>
        <el-select v-model="node.singularity" multiple collapse-tags size="small" placeholder="不限" class="grow">
          <el-option v-for="t in singularities" :key="t" :label="t" :value="t" />
        </el-select>
      </div>
      <div class="row">
        <span class="lbl">液体类型</span>
        <el-select v-model="node.liquid" size="small" placeholder="不限" style="width:110px">
          <el-option label="不限" value="" />
          <el-option v-for="t in liquids" :key="t" :label="t" :value="t" />
        </el-select>
        <span class="lbl">戴森球层≥</span>
        <el-input-number v-model="node.dspLevelMin" :min="0" :max="90" :step="5" :controls="false" size="small" style="width:70px" />
        <span class="hint">度</span>
      </div>
      <div class="row">
        <span class="lbl">符合数量</span>
        <el-input-number v-model="node.satisfyNum" :min="1" :controls="false" size="small" style="width:70px" />
        <span class="hint">颗行星需满足（在所属恒星系内计数）</span>
      </div>
    </template>

    <!-- 矿脉条件：数量 / 储量 -->
    <div class="vein-block">
      <div class="vb-title">矿脉数量 ≥</div>
      <div class="vein-cond">
        <div v-for="v in veins" :key="v" class="vc">
          <span class="vn">{{ v }}</span>
          <el-input-number v-model="node.veins.point[v]" :min="0" :step="10" :controls="false" size="small" style="width:100%" />
        </div>
      </div>
      <div class="vb-title">矿脉储量 ≥</div>
      <div class="vein-cond">
        <div v-for="v in veins" :key="v" class="vc">
          <span class="vn">{{ v }}</span>
          <el-input-number v-model="node.veins.amount[v]" :min="0" :step="10000" :controls="false" size="small" style="width:100%" />
        </div>
      </div>
    </div>

    <!-- 子条件 -->
    <div v-if="kind === 'star'" class="children">
      <div v-for="(child, i) in node.planets" :key="i">
        <ConditionCard :node="child" kind="planet" @remove="node.planets.splice(i, 1)" />
      </div>
      <el-button size="small" type="primary" plain @click="addPlanet(node, 'planets')">+ 行星条件</el-button>
    </div>

    <div v-if="kind === 'planet'" class="children">
      <div v-for="(child, i) in node.moons" :key="i">
        <ConditionCard :node="child" kind="planet" @remove="node.moons.splice(i, 1)" />
      </div>
      <el-button size="small" type="primary" plain @click="addPlanet(node, 'moons')">+ 卫星条件</el-button>
    </div>

    <div v-if="kind === 'galaxy'" class="children">
      <div v-for="(child, i) in node.stars" :key="i">
        <ConditionCard :node="child" kind="star" @remove="node.stars.splice(i, 1)" />
      </div>
      <el-button size="small" type="primary" plain @click="addStar(node)">+ 恒星系条件</el-button>
      <div v-for="(child, i) in node.planets" :key="'p' + i">
        <ConditionCard :node="child" kind="planet" @remove="node.planets.splice(i, 1)" />
      </div>
      <el-button size="small" type="primary" plain @click="addPlanet(node, 'planets')">+ 全星系行星条件</el-button>
    </div>
  </div>
</template>

<script setup>
import { ElMessage } from 'element-plus'

const props = defineProps({ node: Object, kind: String })
defineEmits(['remove'])

const starTypes = ['红巨星','黄巨星','蓝巨星','白巨星','白矮星','中子星','黑洞','A型恒星','B型恒星','F型恒星','G型恒星','K型恒星','M型恒星','O型恒星']
const planetTypes = ['地中海','气态巨星','冰巨星','高产气巨','干旱荒漠','灰烬冻土','海洋丛林','熔岩','冰原冻土','贫瘠荒漠','戈壁','火山灰','红石','草原','水世界','黑石盐滩','樱林海','飓风石林','猩红冰湖','热带草原','橙晶荒漠','极寒冻土','潘多拉沼泽']
const singularities = ['潮汐锁定永昼永夜','轨道共振1:2','轨道共振1:4','横躺自转','反向自转','多卫星','卫星']
const liquids = ['水','硫酸','熔岩']
const veins = ['铁','铜','硅','钛','石','煤','油','可燃冰','金伯利矿石','分形硅石','有机晶体','光栅石','刺笋结晶','单极磁石']

function addPlanet(node, list) {
  node[list].push({
    name: list === 'moons' ? '卫星条件' : '行星条件', checked: true,
    types: [], singularity: [], liquid: '', dspLevelMin: 0, satisfyNum: 1,
    veins: { point: {}, amount: {} }, moons: [],
  })
}
function addStar(node) {
  node.stars.push({
    name: '恒星系条件', checked: true, types: [], minLumino: 0, maxDistance: -1,
    satisfyNum: 1, veins: { point: {}, amount: {} }, planets: [],
  })
}
</script>

<style scoped>
.cond-card { border: 1px solid #dcdfe6; border-radius: 6px; padding: 8px; margin-bottom: 8px; background: #fff; }
.cond-card.star { border-color: #b3d8ff; background: #f4f9ff; }
.cond-card.planet { border-color: #c2e7b0; background: #f6fdf3; }
.cc-head { display: flex; align-items: center; gap: 6px; margin-bottom: 6px; }
.name-input { width: 150px; }
.row { display: flex; align-items: center; gap: 6px; margin-bottom: 5px; flex-wrap: wrap; }
.lbl { font-size: 12px; color: #606266; white-space: nowrap; }
.hint { font-size: 11px; color: #a8abb2; }
.grow { flex: 1; min-width: 200px; }
.vein-block { margin-top: 6px; }
.vb-title { font-size: 12px; color: #909399; margin: 4px 0 2px; }
.vein-cond { display: grid; grid-template-columns: repeat(7, 1fr); gap: 3px; }
.vc { display: flex; flex-direction: column; align-items: center; font-size: 11px; }
.vn { color: #606266; white-space: nowrap; }
.children { margin-top: 8px; padding-left: 10px; border-left: 2px dashed #dcdfe6; }
</style>