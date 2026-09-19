<template>
  <div class="page">
    <header class="header">
      <h1>戴森球计划 · 种子工具</h1>
      <el-radio-group v-model="tab">
        <el-radio-button value="search">种子搜索</el-radio-button>
        <el-radio-button value="blueprint">蓝图解析</el-radio-button>
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

    <!-- 物流塔编辑 -->
    <el-dialog v-model="stationDlg" :title="'编辑 ' + (stationRow?.name || '') + ' #' + (stationRow?.index ?? '')" width="780px">
      <el-form v-if="stationData" label-width="110px" size="small">
        <el-divider content-position="left">存储槽</el-divider>
        <div v-for="(s2, i) in stationData.storage" :key="i" class="st-row">
          <span class="dim" style="width:44px">槽{{ i + 1 }}</span>
          <el-select v-model="s2.itemId" filterable style="width:130px" placeholder="空">
            <el-option :value="0" label="（空）" />
            <el-option v-for="(n, id) in itemOptions" :key="id" :label="n" :value="Number(id)" />
          </el-select>
          <el-select v-model="s2.localRole" style="width:100px">
            <el-option :value="0" label="本地仓储" /><el-option :value="1" label="本地供应" /><el-option :value="2" label="本地需求" />
          </el-select>
          <el-select v-model="s2.remoteRole" style="width:100px">
            <el-option :value="0" label="星际仓储" /><el-option :value="1" label="星际供应" /><el-option :value="2" label="星际需求" />
          </el-select>
          <el-input-number v-model="s2.max" :min="0" :step="100" :controls="false" style="width:90px" placeholder="上限" />
          <el-select v-model="s2.lockAmount" style="width:86px">
            <el-option :value="0" label="不锁定" /><el-option :value="1" label="锁满仓" /><el-option :value="2" label="锁半仓" />
          </el-select>
        </div>
        <el-divider content-position="left">运输设置</el-divider>
        <div class="st-grid">
          <el-form-item label="运输机起送"><el-input-number v-model="stationData.deliveryDronesPct" :min="1" :max="100" style="width:110px" /> %</el-form-item>
          <el-form-item label="运输船起送"><el-input-number v-model="stationData.deliveryShipsPct" :min="1" :max="100" style="width:110px" /> %</el-form-item>
          <el-form-item label="最大充能"><el-input-number v-model="stationData.workEnergyMW" :min="30" :max="300" style="width:110px" /> MW</el-form-item>
          <el-form-item label="运输机航程"><el-input-number v-model="stationData.tripRangeDronesDeg" :min="20" :max="180" style="width:110px" /> °</el-form-item>
          <el-form-item label="运输船航程"><el-input-number v-model="stationData.tripRangeShipsLy" :min="1" :max="10000" style="width:110px" /> ly</el-form-item>
          <el-form-item label="曲速启用"><el-input-number v-model="stationData.warpEnableAu" :min="0.5" :max="60" :step="0.5" style="width:110px" /> AU</el-form-item>
          <el-form-item label="集装数量"><el-input-number v-model="stationData.pilerCount" :min="0" :max="4" style="width:110px" />（0=科技上限）</el-form-item>
          <el-form-item label="翘曲必备"><el-switch v-model="stationData.warperNecessary" /></el-form-item>
          <el-form-item label="取轨道采集器"><el-switch v-model="stationData.includeOrbitCollector" /></el-form-item>
          <el-form-item label="自动补无人机"><el-switch v-model="stationData.droneAutoReplenish" /></el-form-item>
          <el-form-item label="自动补运输船"><el-switch v-model="stationData.shipAutoReplenish" /></el-form-item>
        </div>
      </el-form>
      <template #footer>
        <el-button @click="stationDlg = false">取消</el-button>
        <el-button type="primary" :loading="bpLoading" @click="saveStation">保存并生成新蓝图</el-button>
      </template>
    </el-dialog>

    <!-- ===== 蓝图解析 ===== -->
    <div v-show="tab === 'blueprint'">
      <el-row :gutter="12">
        <el-col :span="8">
          <el-card shadow="never">
            <template #header>上传蓝图</template>
            <el-upload drag :auto-upload="false" :limit="1" :on-change="f => (bpFile = f.raw)" accept=".blueprint,.blueprint-zip,.txt">
              <div class="el-upload__text">拖入 .blueprint 文件<br />或 <em>点击选择</em></div>
            </el-upload>
            <el-divider>或粘贴游戏内复制的蓝图文本</el-divider>
            <el-input v-model="bpText" type="textarea" :rows="6" placeholder="BLUEPRINT:0,&quot;9&quot;,&quot;蓝图名&quot;,&quot;AAAA...&quot;" />
            <div style="margin-top:10px">
              <el-button type="primary" :loading="bpLoading" @click="parseBlueprint">解析蓝图</el-button>
            </div>
          </el-card>
        </el-col>
        <el-col :span="16">
          <el-card shadow="never" v-if="bp">
            <template #header>
              {{ bp.name || '未命名蓝图' }}
              <span class="stat">{{ bp.buildingCount }} 建筑 · {{ bp.beltCount }} 传送带<template v-if="bp.area"> · {{ bp.area.width }}×{{ bp.area.height }}</template></span>
            </template>
            <el-tabs>
              <el-tab-pane label="3D 预览">
                <Blueprint3D :buildings="bp3dBuildings" height="460px" @select="b3dSelect = $event" @dblselect="on3dDblSelect" />
                <div v-if="b3dSelect" class="b3d-info">
                  <b>{{ b3dSelect.name }}</b> #{{ b3dSelect.index }}
                  坐标 X={{ b3dSelect.x.toFixed(1) }} Y={{ b3dSelect.y.toFixed(1) }}
                  <template v-if="b3dSelect.recipeId"> · 配方 #{{ b3dSelect.recipeId }}</template>
                </div>
                <div class="b3d-legend">
                  <span style="color:#4a9eff">■ 传送带</span> <span style="color:#9d7bff">■ 分拣器</span>
                  <span style="color:#ff8c42">■ 熔炉</span> <span style="color:#67c23a">■ 制造台</span>
                  <span style="color:#f56c6c">■ 物流塔</span> <span style="color:#f7ba2a">■ 电力</span>
                  <span class="dim">（拖动旋转 · 滚轮缩放 · 单击选中 · 双击聚焦）</span>
                </div>
              </el-tab-pane>
              <el-tab-pane label="建筑清单">
                <el-table :data="bpBuildings" height="480" size="small">
                  <el-table-column type="index" width="50" />
                  <el-table-column prop="name" label="建筑" />
                  <el-table-column prop="count" label="数量" width="90" sortable />
                </el-table>
              </el-tab-pane>
              <el-tab-pane label="物流塔">
                <el-table :data="bpStations" height="480" size="small">
                  <el-table-column prop="index" label="#" width="60" />
                  <el-table-column prop="name" label="类型" width="150" />
                  <el-table-column label="存储槽">
                    <template #default="{ row }">
                      <el-tag v-for="(s2, i) in row.station.storage.filter(x => x.itemId)" :key="i" size="small" class="tag" :type="s2.localRole === 1 || s2.remoteRole === 1 ? 'success' : 'warning'">
                        {{ itemName(s2.itemId) }} {{ roleText(s2) }}
                      </el-tag>
                    </template>
                  </el-table-column>
                  <el-table-column label="操作" width="180">
                    <template #default="{ row }">
                      <el-button size="small" type="primary" plain @click="openStation(row)">编辑</el-button>
                      <el-button v-if="row.itemId === 2103 || row.itemId === 2104" size="small" @click="swapStation(row)">转{{ row.itemId === 2103 ? '星际' : '行星' }}塔</el-button>
                    </template>
                  </el-table-column>
                </el-table>
              </el-tab-pane>
              <el-tab-pane :label="'流水线 (' + bp.recipes.length + ')'">
                <div v-for="r in bp.recipes" :key="r.name" class="recipe">
                  <div class="r-head">
                    <b>{{ r.name }}</b>
                    <el-tag size="small" type="info">{{ r.building }} × {{ r.buildings }}</el-tag>
                  </div>
                  <div class="r-flow">
                    <div class="r-io">
                      <div v-for="(i, idx) in r.inputRates" :key="'i'+idx" class="io-line in">{{ i.name }} <b>{{ i.perSec }}</b>/s</div>
                    </div>
                    <div class="arrow">→</div>
                    <div class="r-mid">{{ r.building }}<br /><span class="dim">×{{ r.buildings }}</span></div>
                    <div class="arrow">→</div>
                    <div class="r-io">
                      <div v-for="(o, idx) in r.outputRates" :key="'o'+idx" class="io-line out">{{ o.name }} <b>{{ o.perSec }}</b>/s</div>
                    </div>
                  </div>
                </div>
                <el-empty v-if="!bp.recipes.length" description="蓝图内没有生产配方建筑" />
              </el-tab-pane>
            </el-tabs>
          </el-card>
        </el-col>
      </el-row>

      <!-- 蓝图编辑工具 -->
      <el-card v-if="bp" shadow="never" class="bp-tools">
        <template #header>蓝图工具（变换后生成新蓝图，复制回游戏即可使用）</template>
        <el-form :inline="true" size="small">
          <el-form-item label="平移">
            X <el-input-number v-model="bpDx" :step="5" style="width:90px" />
            Y <el-input-number v-model="bpDy" :step="5" style="width:90px" />
            <el-button size="small" type="primary" plain @click="bpEdit('translate')">应用</el-button>
          </el-form-item>
          <el-form-item>
            <el-button size="small" @click="bpEdit('mirrorH')">左右镜像</el-button>
            <el-button size="small" @click="bpEdit('mirrorV')">上下镜像</el-button>
            <el-button size="small" @click="bpEdit('rotate', 1)">旋转90°</el-button>
            <el-button size="small" @click="bpEdit('rotate', 2)">旋转180°</el-button>
            <el-button size="small" type="warning" plain @click="bpNoBelt">无带流</el-button>
            <el-button size="small" plain @click="bpEdit('snap')">格点吸附（修复不可放置）</el-button>
          </el-form-item>
          <el-form-item label="线性变换">
            X缩放 <el-input-number v-model="bpZoomX" :step="0.5" :precision="1" style="width:95px" />
            Y缩放 <el-input-number v-model="bpZoomY" :step="0.5" :precision="1" style="width:95px" />
            旋转 <el-input-number v-model="bpRotate" :step="90" style="width:95px" /> °
            <el-button size="small" type="primary" plain @click="bpEdit('linear')">应用</el-button>
            <span class="dim">（负数=翻转；非±1缩放会自动吸附整格）</span>
          </el-form-item>
          <el-form-item label="垂直叠加">
            层数 <el-input-number v-model="bpFloors" :min="1" :max="30" style="width:80px" />
            Z间隔 <el-input-number v-model="bpSpacing" :min="0.5" :step="0.5" style="width:90px" />
            <el-radio-group v-model="bpPile" size="small">
              <el-radio-button :value="true">堆叠</el-radio-button>
              <el-radio-button :value="false">独立</el-radio-button>
            </el-radio-group>
            <el-button size="small" type="primary" plain @click="bpEdit('vstack')">应用</el-button>
            <span class="dim">（Z 轴多层，堆叠=箱体互通；悬空自动补地基）</span>
          </el-form-item>
          <el-form-item label="垂直偏移">
            Z <el-input-number v-model="bpDz" :step="1" style="width:85px" />
            <el-button size="small" type="primary" plain @click="bpEdit('voffset')">应用</el-button>
          </el-form-item>
          <el-form-item label="改名">
            <el-input v-model="bpName" style="width:180px" placeholder="留空不改" />
            <el-button size="small" type="primary" plain @click="bpEdit(null)">生成新蓝图</el-button>
          </el-form-item>
        </el-form>
        <el-divider>拼接第二份蓝图</el-divider>
        <el-input v-model="bpSecond" type="textarea" :rows="3" placeholder="粘贴第二份蓝图文本" />
        <el-form :inline="true" size="small" style="margin-top:6px">
          <el-form-item label="水平对齐">
            <el-radio-group v-model="bpAlign" size="small">
              <el-radio-button value="top">顶对齐</el-radio-button>
              <el-radio-button value="center">居中</el-radio-button>
              <el-radio-button value="bottom">底对齐</el-radio-button>
            </el-radio-group>
            <el-button size="small" type="primary" plain @click="bpEdit('hstack')">左右拼接</el-button>
            <el-button size="small" type="primary" plain @click="bpEdit('stack')">上下拼接</el-button>
          </el-form-item>
        </el-form>

        <el-divider content-position="left">无中生有 · 无褶皱垂直传送带</el-divider>
        <el-form :inline="true" size="small">
          <el-form-item label="起点Z"><el-input-number v-model="bpVz1" :step="0.5" style="width:90px" /></el-form-item>
          <el-form-item label="终点Z"><el-input-number v-model="bpVz2" :step="0.5" style="width:90px" /></el-form-item>
          <el-form-item><el-button size="small" type="primary" plain @click="makeVbelt">生成</el-button>
          <span class="dim">（生成新蓝图，不基于当前蓝图）</span></el-form-item>
        </el-form>

        <el-divider content-position="left">结果蓝图（游戏内 Ctrl+V 粘贴）</el-divider>
        <el-input v-if="bpExported" v-model="bpExported" type="textarea" :rows="4" readonly />
        <div v-if="bpExported" style="margin-top:6px">
          <el-button size="small" type="success" @click="copyBp">复制蓝图</el-button>
          <span class="dim" style="margin-left:8px">{{ bpExportInfo }}</span>
        </div>
      </el-card>
    </div>

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

      <div v-if="galaxy" class="sort-bar">
        <span class="lbl">排序：</span>
        <div v-for="(r, i) in sortRules" :key="i" class="sort-rule">
          <span v-if="i > 0" class="lbl">→</span>
          <el-select v-model="r.key" size="small" style="width:150px">
            <el-option label="（不排序）" value="" />
            <el-option label="距离" value="distance" />
            <el-option label="光度 L" value="dysonLumino" />
            <el-option label="太阳能 L" value="luminosity" />
            <el-option label="安全度" value="safetyFactor" />
            <el-option label="行星数" value="planetCount" />
            <el-option label="总矿脉数" value="__veinTotal" />
            <el-option v-for="v in veins" :key="v" :label="'星系 ' + v + ' 数量'" :value="'vp:' + v" />
            <el-option v-for="v in veins" :key="'a' + v" :label="'星系 ' + v + ' 储量'" :value="'va:' + v" />
            <el-option label="海洋·有水（行星数）" value="ocean:水" />
            <el-option label="海洋·有硫酸（行星数）" value="ocean:硫酸" />
            <el-option label="海洋·有熔岩（行星数）" value="ocean:熔岩" />
            <el-option label="行星·最大适建区域" value="p:maxLand" />
            <el-option label="行星·最大轨道半径" value="p:maxOrbit" />
            <el-option label="行星·最高光度" value="p:maxLum" />
            <el-option label="行星·最大风能" value="p:maxWind" />
            <el-option v-for="v in veins" :key="'pv' + v" :label="'行星·最富 ' + v + '（数量）'" :value="'pvp:' + v" />
            <el-option v-for="v in veins" :key="'pa' + v" :label="'行星·最富 ' + v + '（储量）'" :value="'pva:' + v" />
          </el-select>
          <el-button size="small" :type="r.dir === 'asc' ? 'success' : 'primary'" plain @click="r.dir = r.dir === 'asc' ? 'desc' : 'asc'">
            {{ r.dir === 'asc' ? '↑ 升' : '↓ 降' }}
          </el-button>
          <el-button v-if="sortRules.length > 1" size="small" text type="danger" @click="sortRules.splice(i, 1)">✕</el-button>
        </div>
        <el-button v-if="sortRules.length < 5" size="small" plain @click="sortRules.push({ key: '', dir: 'desc' })">+ 新增条件</el-button>
        <el-input v-model="starFilter" size="small" placeholder="按名称/类型筛选恒星" style="width:170px;margin-left:8px" clearable />
      </div>
      <el-table v-if="galaxy" :data="sortedStars" height="calc(100vh - 360px)" @row-click="selectStar" highlight-current-row>
        <el-table-column prop="index" label="#" width="50" />
        <el-table-column prop="name" label="恒星" width="180" />
        <el-table-column prop="type" label="类型" width="110" column-key="type"
          :filters="starTypeNames.map(t => ({ text: t, value: t }))"
          :filter-method="(v, row) => row.type === v" />
        <el-table-column label="距离(ly)" width="90" sortable prop="distance"><template #default="{ row }">{{ f6(row.distance) }}</template></el-table-column>
        <el-table-column label="光度L" width="80" sortable prop="dysonLumino"><template #default="{ row }">{{ row.dysonLumino }}</template></el-table-column>
        <el-table-column label="太阳能L" width="90" sortable prop="luminosity"><template #default="{ row }">{{ row.luminosity }}</template></el-table-column>
        <el-table-column prop="planetCount" label="行星" width="60" sortable />
        <el-table-column label="安全度" width="90">
          <template #default="{ row }">
            <el-tag size="small" :type="row.hivePatternLevel === 0 ? 'success' : row.hivePatternLevel === 1 ? 'warning' : 'danger'">
              {{ Math.round(row.safetyFactor * 100) }}%
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="海洋" width="130">
          <template #default="{ row }">
            <el-tag v-for="l in row.liquids" :key="l" size="small" class="tag" type="primary" effect="plain">{{ l }}</el-tag>
            <span v-if="!row.liquids?.length" class="dim">—</span>
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
            <el-table-column label="适建区域" width="80" sortable prop="landPercent">
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
            <el-table-column label="光度" width="65" sortable prop="luminosity"><template #default="{ row }">{{ row.luminosity }}</template></el-table-column>
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
import { ref, reactive, computed, onUnmounted } from 'vue'
import { ElMessage } from 'element-plus'
import ConditionCard from './components/ConditionCard.vue'
import Blueprint3D from './components/Blueprint3D.vue'

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
const bpModel = ref('')
const bpZoomX = ref(1)
const bpZoomY = ref(1)
const bpRotate = ref(0)
const bpFloors = ref(2)
const bpSpacing = ref(3)
const bpPile = ref(true)
const bpDz = ref(0)
const bpAlign = ref('center')
const bpVz1 = ref(0)
const bpVz2 = ref(10)
const bpDx = ref(0)
const bpDy = ref(0)
const bpName = ref('')
const bpSecond = ref('')
const bpExported = ref('')
const bpExportInfo = ref('')

const sortRules = ref([{ key: '', dir: 'desc' }])
const starFilter = ref('')

function sortVal(star, key) {
  const planets = star.planets || []
  if (key === '__veinTotal') return Object.values(star.veinsPoint || {}).reduce((a, v) => a + v, 0)
  if (key.startsWith('vp:')) return star.veinsPoint?.[key.slice(3)] || 0
  if (key.startsWith('va:')) return Number(star.veinsAmount?.[key.slice(3)] || 0)
  if (key === 'p:maxLand') return planets.reduce((m, p) => Math.max(m, p.landPercent ?? -1), -1)
  if (key === 'p:maxOrbit') return planets.reduce((m, p) => Math.max(m, p.orbitRadius || 0), 0)
  if (key === 'p:maxLum') return planets.reduce((m, p) => Math.max(m, p.luminosity || 0), 0)
  if (key === 'p:maxWind') return planets.reduce((m, p) => Math.max(m, p.wind || 0), 0)
  if (key.startsWith('ocean:')) { const n = key.slice(6); return (star.planets || []).filter(p2 => p2.liquid === n).length }
  if (key.startsWith('pvp:')) { const n = key.slice(4); return planets.reduce((m, p) => Math.max(m, p.veinsPoint?.[n] || 0), 0) }
  if (key.startsWith('pva:')) { const n = key.slice(4); return planets.reduce((m, p) => Math.max(m, Number(p.veinsAmount?.[n] || 0)), 0) }
  return star[key] ?? 0
}

const sortedStars = computed(() => {
  if (!galaxy.value) return []
  let arr = [...galaxy.value.stars]
  const f = starFilter.value.trim().toLowerCase()
  if (f) arr = arr.filter(x => x.name.toLowerCase().includes(f) || x.type.includes(f))
  const rules = sortRules.value.filter(r => r.key)
  if (!rules.length) return arr
  arr.sort((a, b) => {
    for (const r of rules) {
      const d = sortVal(b, r.key) - sortVal(a, r.key)
      if (d !== 0) return r.dir === 'asc' ? -d : d
    }
    return 0
  })
  return arr
})

const b3dSelect = ref(null)
const bp3dBuildings = computed(() => {
  if (!bpModel.value) return []
  try { return JSON.parse(bpModel.value).buildings.map(b => ({ ...b, x: b.x ?? 0, y: b.y ?? 0 })) } catch { return [] }
})

const stationDlg = ref(false)
const stationRow = ref(null)
const stationData = ref(null)
const bpStations = computed(() => {
  if (!bpModel.value) return []
  try { return JSON.parse(bpModel.value).buildings.filter(b => b.station) } catch { return [] }
})
const bpBuildings = computed(() => bp.value?.buildings || [])
const itemOptions = computed(() => ITEM_NAMES)

const ITEM_NAMES = {
  1001:'铁矿',1002:'铜矿',1003:'硅石',1004:'钛石',1005:'石矿',1006:'煤矿',1007:'可燃冰',1008:'金伯利矿石',1009:'分形硅石',1010:'有机晶体',
  1011:'刺笋结晶',1110:'玻璃',1108:'石材',1101:'铁块',1102:'铜块',1103:'钢材',1104:'高纯硅块',1105:'钛块',1106:'磁铁',1107:'电磁涡轮',
  1111:'金刚石',1112:'反物质',1120:'氢',1121:'重氢',1122:'反物质',1208:'光子',1210:'引力透镜',1401:'齿轮',1402:'电动机',1501:'电路板',
  1502:'处理器',1503:'量子芯片',1601:'石墨烯',1602:'碳纳米管',1603:'粒子宽带',1701:'硫酸',1801:'混凝土',1802:'钛化玻璃',1901:'戴森球组件',
  1902:'太阳帆',2001:'太阳帆',2011:'翘曲器',2201:'电力感应塔',2306:'电弧熔炉',
}
function itemName(id) { return ITEM_NAMES[id] || '物品#' + id }
function roleText(s2) {
  const parts = []
  if (s2.localRole === 1) parts.push('本地供')
  if (s2.localRole === 2) parts.push('本地需')
  if (s2.remoteRole === 1) parts.push('星际供')
  if (s2.remoteRole === 2) parts.push('星际需')
  return parts.join('·') || '仓储'
}
function on3dDblSelect(b) {
  if (b.station) openStation(b)
}
function openStation(row) {
  stationRow.value = row
  stationData.value = JSON.parse(JSON.stringify(row.station))
  stationDlg.value = true
}
async function saveStation() {
  bpLoading.value = true
  try {
    const payload = { Json: bpModel.value, Op: 'station', BuildingIndex: stationRow.value.index, StationData: stationData.value,
      NewName: bp.value.name, NewDesc: bp.value.desc || '' }
    const res = await fetch('/api/blueprint/edit', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) })
    if (!res.ok) throw new Error(await res.text())
    const d = await res.json()
    bpModel.value = d.json
    bpExported.value = d.blueprint
    bpExportInfo.value = '物流塔已修改'
    stationDlg.value = false
    ElMessage.success('已生成新蓝图，请在下方复制')
  } catch (e) { ElMessage.error(e.message) }
  finally { bpLoading.value = false }
}
async function swapStation(row) {
  bpLoading.value = true
  try {
    const payload = { Json: bpModel.value, Op: 'swapStation', BuildingIndex: row.index, ToInterstellar: row.itemId === 2103,
      NewName: bp.value.name, NewDesc: bp.value.desc || '' }
    const res = await fetch('/api/blueprint/edit', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) })
    if (!res.ok) throw new Error(await res.text())
    const d = await res.json()
    bpModel.value = d.json
    bpExported.value = d.blueprint
    bpExportInfo.value = '塔类型已互换'
    ElMessage.success('已生成新蓝图')
  } catch (e) { ElMessage.error(e.message) }
  finally { bpLoading.value = false }
}

async function bpEdit(op, quarter) {
  bpLoading.value = true
  try {
    const payload = { Json: bpModel.value, Op: op, Quarter: quarter ?? 0, Dx: bpDx.value, Dy: bpDy.value,
      ZoomX: bpZoomX.value, ZoomY: bpZoomY.value, RotateDeg: bpRotate.value,
      Floors: bpFloors.value, Spacing: bpSpacing.value, Pile: bpPile.value,
      Dz: bpDz.value, Align: bpAlign.value }
    if (op === 'stack') payload.RawText = bpSecond.value.trim()
    if (bpName.value.trim()) { payload.NewName = bpName.value.trim(); payload.NewDesc = bp.desc || '' }
    const res = await fetch('/api/blueprint/edit', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) })
    if (!res.ok) throw new Error(await res.text())
    const d = await res.json()
    bpModel.value = d.json
    bp.value = d.summary
    bpExported.value = d.blueprint
    bpExportInfo.value = d.summary.buildingCount + ' 建筑 · ' + d.summary.area.width + '×' + d.summary.area.height
    ElMessage.success('已生成新蓝图')
  } catch (e) { ElMessage.error('编辑失败: ' + e.message) }
  finally { bpLoading.value = false }
}
async function makeVbelt() {
  bpLoading.value = true
  try {
    const payload = { Json: bpModel.value || '{}', Op: 'vbelt', StartZ: bpVz1.value, EndZ: bpVz2.value }
    const res = await fetch('/api/blueprint/edit', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) })
    if (!res.ok) throw new Error(await res.text())
    const d = await res.json()
    bpModel.value = d.json
    bp.value = d.summary
    bpExported.value = d.blueprint
    bpExportInfo.value = '垂直传送带已生成'
    ElMessage.success('已生成垂直传送带蓝图')
  } catch (e) { ElMessage.error(e.message) }
  finally { bpLoading.value = false }
}

async function bpNoBelt() {
  bpLoading.value = true
  try {
    const payload = { Json: bpModel.value, Op: 'removeItems', ItemIds: [2001, 2002, 2003, 2011, 2012, 2013, 2014] }
    if (bpName.value.trim()) { payload.NewName = bpName.value.trim(); payload.NewDesc = bp.desc || '' }
    const res = await fetch('/api/blueprint/edit', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) })
    if (!res.ok) throw new Error(await res.text())
    const d = await res.json()
    bpModel.value = d.json
    bp.value = d.summary
    bpExported.value = d.blueprint
    bpExportInfo.value = d.summary.buildingCount + ' 建筑（已删带）'
    ElMessage.success('已生成无带流蓝图')
  } catch (e) { ElMessage.error(e.message) }
  finally { bpLoading.value = false }
}
function copyBp() {
  navigator.clipboard.writeText(bpExported.value).then(() => ElMessage.success('已复制，到游戏里 Ctrl+V 粘贴'))
}

const bpFile = ref(null)
const bpText = ref('')
const bpLoading = ref(false)
const bp = ref(null)
async function parseBlueprint() {
  bpLoading.value = true
  bp.value = null
  try {
    let res
    if (bpFile.value) {
      const fd = new FormData()
      fd.append('file', bpFile.value)
      res = await fetch('/api/blueprint', { method: 'POST', body: fd })
    } else if (bpText.value.trim()) {
      res = await fetch('/api/blueprint', { method: 'POST', body: bpText.value.trim() })
    } else { ElMessage.warning('请选择文件或粘贴蓝图文本'); bpLoading.value = false; return }
    if (!res.ok) throw new Error(await res.text())
    const d = await res.json()
    bp.value = d.summary
    bpModel.value = d.json
    bpExported.value = ''
  } catch (e) { ElMessage.error('解析失败: ' + e.message) }
  finally { bpLoading.value = false }
}

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
.sort-bar { display: flex; align-items: center; margin-bottom: 8px; flex-wrap: wrap; gap: 4px; }
.sort-rule { display: flex; align-items: center; gap: 2px; }
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
.bp-tools { margin-top: 12px; }
.st-row { display: flex; gap: 4px; align-items: center; margin-bottom: 4px; flex-wrap: wrap; }
.st-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 0 12px; }
.b3d-info { margin-top: 6px; font-size: 13px; }
.b3d-legend { margin-top: 4px; font-size: 12px; display: flex; gap: 10px; flex-wrap: wrap; }
.recipe { border: 1px solid #ebeef5; border-radius: 6px; padding: 8px 12px; margin-bottom: 8px; }
.r-head { display: flex; align-items: center; gap: 8px; margin-bottom: 6px; }
.r-flow { display: flex; align-items: center; gap: 12px; }
.r-io { min-width: 160px; }
.io-line { font-size: 12px; }
.io-line.in { color: #e6a23c; }
.io-line.out { color: #67c23a; }
.arrow { color: #c0c4cc; font-size: 18px; }
.r-mid { text-align: center; font-size: 12px; background: #f0f7ff; border-radius: 4px; padding: 6px 10px; }
.tag { margin: 1px; }
</style>