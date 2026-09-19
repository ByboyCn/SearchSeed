<template>
  <div ref="container" class="bp3d"></div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, watch } from 'vue'
import * as THREE from 'three'
import { OrbitControls } from 'three/addons/controls/OrbitControls.js'

const props = defineProps({ buildings: { type: Array, default: () => [] }, height: { type: String, default: '420px' } })
const emit = defineEmits(['select'])
const container = ref(null)

// 建筑类别 → 颜色
const CAT_COLORS = {
  belt: 0x4a9eff, inserter: 0x9d7bff, sorter: 0x9d7bff,
  smelter: 0xff8c42, assembler: 0x67c23a, lab: 0xe6a23c,
  station: 0xf56c6c, power: 0xf7ba2a, miner: 0x8b949e, other: 0xb0b8c4,
}
function catOf(id) {
  if (id >= 2001 && id <= 2020) return 'belt'
  if (id >= 2011 && id <= 2019) return 'inserter'
  if (id >= 2306 && id <= 2309) return 'smelter'
  if (id >= 2301 && id <= 2305 || id === 2317) return 'assembler'
  if (id >= 2103 && id <= 2104) return 'station'
  if (id >= 2201 && id <= 2210) return 'power'
  if (id === 2301 || id === 2316) return 'miner'
  if (id >= 2320 && id <= 2322) return 'lab'
  return 'other'
}
const BOX_SIZE = { belt: [1, 0.15, 1], inserter: [1, 0.3, 1], default: [2, 1.6, 2] }

let scene, camera, renderer, controls, group, raycaster, pointer, animId
const boxByMesh = new Map()

function build() {
  if (!group) return
  scene.remove(group)
  group.traverse(o => { if (o.geometry) o.geometry.dispose() })
  group = new THREE.Group()
  boxByMesh.clear()

  const geoCache = new Map()
  for (const b of props.buildings) {
    const cat = catOf(b.itemId)
    const size = BOX_SIZE[cat] || BOX_SIZE.default
    const key = size.join()
    if (!geoCache.has(key)) geoCache.set(key, new THREE.BoxGeometry(...size))
    const mesh = new THREE.Mesh(geoCache.get(key), new THREE.MeshLambertMaterial({ color: CAT_COLORS[cat] || CAT_COLORS.other }))
    mesh.position.set(b.x, (size[1] / 2), -b.y) // 蓝图 Y 向上 → 3D Z
    group.add(mesh)
    boxByMesh.set(mesh, b)
  }
  scene.add(group)
  fitView()
}

function fitView() {
  if (!props.buildings.length) return
  const box = new THREE.Box3().setFromObject(group)
  const center = box.getCenter(new THREE.Vector3())
  const size = box.getSize(new THREE.Vector3())
  const maxDim = Math.max(size.x, size.z, 10)
  controls.target.copy(center)
  camera.position.set(center.x + maxDim * 0.7, maxDim * 0.8, center.z + maxDim * 0.7)
  camera.far = maxDim * 20
  camera.updateProjectionMatrix()
}

function onPointerDown(ev) {
  if (ev.button !== 0) return
  const rect = renderer.domElement.getBoundingClientRect()
  pointer.x = ((ev.clientX - rect.left) / rect.width) * 2 - 1
  pointer.y = -((ev.clientY - rect.top) / rect.height) * 2 + 1
  raycaster.setFromCamera(pointer, camera)
  const hits = raycaster.intersectObjects(group?.children ?? [])
  if (hits.length) {
    const b = boxByMesh.get(hits[0].object)
    if (b) {
      // 高亮
      hits[0].object.material.emissive?.setHex(0x333333)
      emit('select', b)
    }
  }
}

function onDblClick(ev) {
  const rect = renderer.domElement.getBoundingClientRect()
  pointer.x = ((ev.clientX - rect.left) / rect.width) * 2 - 1
  pointer.y = -((ev.clientY - rect.top) / rect.height) * 2 + 1
  raycaster.setFromCamera(pointer, camera)
  const hits = raycaster.intersectObjects(group?.children ?? [])
  if (hits.length) {
    controls.target.copy(hits[0].object.position)
    const d = camera.position.distanceTo(hits[0].object.position)
    camera.position.copy(hits[0].object.position).add(new THREE.Vector3(d * 0.2, d * 0.2, d * 0.2))
  }
}

onMounted(() => {
  const el = container.value
  scene = new THREE.Scene()
  scene.background = new THREE.Color(0x1a2030)
  scene.fog = new THREE.Fog(0x1a2030, 100, 800)
  camera = new THREE.PerspectiveCamera(50, el.clientWidth / el.clientHeight, 0.1, 2000)
  renderer = new THREE.WebGLRenderer({ antialias: true })
  renderer.setSize(el.clientWidth, el.clientHeight)
  el.appendChild(renderer.domElement)
  controls = new OrbitControls(camera, renderer.domElement)
  controls.enableDamping = true
  raycaster = new THREE.Raycaster()
  pointer = new THREE.Vector2()

  scene.add(new THREE.AmbientLight(0xffffff, 0.7))
  const dir = new THREE.DirectionalLight(0xffffff, 1.2)
  dir.position.set(50, 100, 30)
  scene.add(dir)
  // 网格地面
  const grid = new THREE.GridHelper(400, 200, 0x2a3550, 0x222c40)
  scene.add(grid)

  group = new THREE.Group()
  build()

  renderer.domElement.addEventListener('pointerdown', onPointerDown)
  renderer.domElement.addEventListener('dblclick', onDblClick)
  new ResizeObserver(() => {
    if (!el.clientWidth) return
    camera.aspect = el.clientWidth / el.clientHeight
    camera.updateProjectionMatrix()
    renderer.setSize(el.clientWidth, el.clientHeight)
  }).observe(el)

  const animate = () => {
    animId = requestAnimationFrame(animate)
    controls.update()
    renderer.render(scene, camera)
  }
  animate()
})
watch(() => props.buildings, build, { deep: false })
onUnmounted(() => { if (animId) cancelAnimationFrame(animId); renderer?.dispose() })
</script>

<style scoped>
.bp3d { width: 100%; height: v-bind(height); border-radius: 6px; overflow: hidden; }
</style>
