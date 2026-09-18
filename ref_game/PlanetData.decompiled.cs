using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlanetData
{
	public GalaxyData galaxy;

	public StarData star;

	public int seed;

	public int infoSeed;

	public int id;

	public int index;

	public int orbitAround;

	public int number;

	public int orbitIndex;

	public string name = "";

	public string overrideName = "";

	public float orbitRadius = 1f;

	public float orbitInclination;

	public float orbitLongitude;

	public double orbitalPeriod = 3600.0;

	public float orbitPhase;

	public float obliquity;

	public double rotationPeriod = 480.0;

	public float rotationPhase;

	public float radius = 200f;

	public float scale = 1f;

	public float sunDistance;

	public float habitableBias;

	public float temperatureBias;

	public float ionHeight;

	public float windStrength;

	public float luminosity;

	public float landPercent;

	public double mod_x;

	public double mod_y;

	public float waterHeight;

	public int waterItemId;

	public bool levelized;

	public int iceFlag;

	public EPlanetType type;

	public EPlanetSingularity singularity;

	public int theme;

	public int algoId;

	public int style;

	public PlanetData orbitAroundPlanet;

	public VectorLF3 runtimePosition;

	public VectorLF3 runtimePositionNext;

	public Quaternion runtimeRotation;

	public Quaternion runtimeRotationNext;

	public Quaternion runtimeSystemRotation;

	public Quaternion runtimeOrbitRotation;

	public float runtimeOrbitPhase;

	public float runtimeRotationPhase;

	public VectorLF3 uPosition;

	public VectorLF3 uPositionNext;

	public Vector3 runtimeLocalSunDirection;

	public byte[] modData;

	public int precision;

	public int segment;

	public PlanetRawData data;

	public Mutex veinGroupsLock = new Mutex();

	public VeinGroup[] veinGroups;

	public Vector3 veinBiasVector;

	public const int kMaxMeshCnt = 100;

	public GameObject gameObject;

	public GameObject bodyObject;

	public Material terrainMaterial;

	public Material oceanMaterial;

	public Material atmosMaterial;

	public Material atmosMaterialLate;

	public Material nephogramMaterial;

	public Material cloudMaterial;

	public Material minimapMaterial;

	public Material reformMaterial0;

	public Material reformMaterial1;

	public RenderTexture heightmap;

	public AmbientDesc ambientDesc;

	public Color groundScreenColor;

	public AudioClip ambientSfx;

	public float ambientSfxVolume;

	public Mesh[] meshes = new Mesh[100];

	public MeshRenderer[] meshRenderers = new MeshRenderer[100];

	public MeshCollider[] meshColliders = new MeshCollider[100];

	public bool[] dirtyFlags = new bool[100];

	public bool landPercentDirtyFlag;

	public int factoryIndex = -1;

	public PlanetFactory factory;

	public PlanetPhysics physics;

	public PlanetAudio audio;

	public FactoryModel factoryModel;

	public FactoryAudio factoryAudio;

	public PlanetAuxData aux;

	public int[] gasItems;

	public float[] gasSpeeds;

	public float[] gasHeatValues;

	public double gasTotalHeat;

	public Vector3 birthPoint;

	public Vector3 birthResourcePoint0;

	public Vector3 birthResourcePoint1;

	public bool loaded;

	public bool wanted;

	public bool loading;

	public bool scanning;

	public bool scanned;

	public bool factoryLoaded;

	public bool factoryLoading;

	public int factingCompletedStage = -1;

	public const float kEnterAltitude = 1000f;

	public const float kBirthHeightShift = 1.45f;

	public string displayName
	{
		get
		{
			if (!string.IsNullOrEmpty(overrideName))
			{
				return overrideName;
			}
			return name;
		}
	}

	public string nonBreakDisplayName => displayName.Replace(' ', '\u00a0');

	public float realRadius => radius * scale;

	public VeinGroup[] runtimeVeinGroups
	{
		get
		{
			if (factory == null)
			{
				return veinGroups;
			}
			return factory.veinGroups;
		}
	}

	public VeinData[] runtimeVeinPool
	{
		get
		{
			if (factory != null)
			{
				return factory.veinPool;
			}
			if (data == null)
			{
				return null;
			}
			return data.veinPool;
		}
	}

	public int runtimeVeinCursor
	{
		get
		{
			if (factory != null)
			{
				return factory.veinCursor;
			}
			if (data == null)
			{
				return 0;
			}
			return data.veinCursor;
		}
	}

	public bool runtimeVeinGroupsScanned
	{
		get
		{
			if (factory == null || factory.veinGroups == null || factory.veinGroups.Length == 0)
			{
				if (factory == null && veinGroups != null)
				{
					return veinGroups.Length != 0;
				}
				return false;
			}
			return true;
		}
	}

	public int astroId => id;

	public string typeString
	{
		get
		{
			string result = "未知".Translate();
			ThemeProto themeProto = LDB.themes.Select(theme);
			if (themeProto != null)
			{
				result = themeProto.displayName;
			}
			return result;
		}
	}

	public string briefString
	{
		get
		{
			string result = "";
			ThemeProto themeProto = LDB.themes.Select(theme);
			if (themeProto != null)
			{
				result = themeProto.BriefIntroduction;
			}
			return result;
		}
	}

	public string singularityString
	{
		get
		{
			string text = "";
			if (orbitAround > 0)
			{
				text += "卫星".Translate();
			}
			if ((singularity & EPlanetSingularity.TidalLocked) != EPlanetSingularity.None)
			{
				text += "潮汐锁定永昼永夜".Translate();
			}
			if ((singularity & EPlanetSingularity.TidalLocked2) != EPlanetSingularity.None)
			{
				text += "潮汐锁定1:2".Translate();
			}
			if ((singularity & EPlanetSingularity.TidalLocked4) != EPlanetSingularity.None)
			{
				text += "潮汐锁定1:4".Translate();
			}
			if ((singularity & EPlanetSingularity.LaySide) != EPlanetSingularity.None)
			{
				text += "横躺自转".Translate();
			}
			if ((singularity & EPlanetSingularity.ClockwiseRotate) != EPlanetSingularity.None)
			{
				text += "反向自转".Translate();
			}
			if ((singularity & EPlanetSingularity.MultipleSatellites) != EPlanetSingularity.None)
			{
				text += "多卫星".Translate();
			}
			return text;
		}
	}

	public float atmosphereHeight
	{
		get
		{
			if (!(atmosMaterial == null))
			{
				return atmosMaterial.GetVector("_PlanetRadius").z - realRadius;
			}
			return 0f;
		}
	}

	public event Action<PlanetData> onLoaded;

	public event Action<PlanetData> onFactoryLoaded;

	public static PlanetData GetUnloadedCopy(PlanetData p)
	{
		PlanetData obj = p.MemberwiseClone() as PlanetData;
		obj.data = null;
		obj.modData = null;
		obj.veinGroupsLock = new Mutex();
		obj.veinGroups = null;
		obj.veinBiasVector = Vector3.zero;
		obj.gameObject = null;
		obj.bodyObject = null;
		obj.terrainMaterial = null;
		obj.oceanMaterial = null;
		obj.atmosMaterial = null;
		obj.atmosMaterialLate = null;
		obj.nephogramMaterial = null;
		obj.cloudMaterial = null;
		obj.minimapMaterial = null;
		obj.reformMaterial0 = null;
		obj.reformMaterial1 = null;
		obj.heightmap = null;
		obj.ambientDesc = null;
		obj.groundScreenColor = Color.white;
		obj.ambientSfx = null;
		obj.ambientSfxVolume = 0f;
		obj.meshes = null;
		obj.meshRenderers = null;
		obj.meshColliders = null;
		obj.dirtyFlags = null;
		obj.landPercentDirtyFlag = false;
		obj.factory = null;
		obj.physics = null;
		obj.audio = null;
		obj.factoryModel = null;
		obj.factoryAudio = null;
		obj.aux = null;
		obj.loaded = false;
		obj.wanted = false;
		obj.loading = false;
		obj.scanning = false;
		obj.scanned = false;
		obj.factoryLoaded = false;
		obj.factoryLoading = false;
		obj.factingCompletedStage = -1;
		obj.onLoaded = null;
		obj.onFactoryLoaded = null;
		return obj;
	}

	public static void ReleaseCopy(PlanetData copy)
	{
		if (copy != null)
		{
			Assert.Null(copy.factory);
			copy.UnloadData();
			copy.modData = null;
			copy.veinGroups = null;
			copy.gasItems = null;
			copy.gasSpeeds = null;
			copy.gasHeatValues = null;
			if (copy.aux != null)
			{
				copy.aux.Free();
				copy.aux = null;
			}
		}
	}

	public bool SummarizeVeinAmountsByFilter(ref long[] veinAmounts, HashSet<int> hashes, int filterType)
	{
		if (veinAmounts == null)
		{
			veinAmounts = new long[64];
		}
		Array.Clear(veinAmounts, 0, veinAmounts.Length);
		hashes.Clear();
		VeinGroup[] array = runtimeVeinGroups;
		switch (filterType)
		{
		case 1:
		{
			if (factory == null)
			{
				break;
			}
			MinerComponent[] minerPool = factory.factorySystem.minerPool;
			int minerCursor = factory.factorySystem.minerCursor;
			PowerConsumerComponent[] consumerPool = factory.powerSystem.consumerPool;
			VeinData[] veinPool = factory.veinPool;
			hashes.Clear();
			for (int j = 0; j < minerCursor; j++)
			{
				ref MinerComponent reference = ref minerPool[j];
				if (reference.id != j || reference.type == EMinerType.Water || consumerPool[reference.pcId].networkId <= 0)
				{
					continue;
				}
				for (int k = 0; k < reference.veinCount; k++)
				{
					int num = reference.veins[k];
					if (num > 0 && veinPool[num].id == num && !hashes.Contains(num))
					{
						veinAmounts[(uint)veinPool[num].type] += veinPool[num].amount;
						hashes.Add(num);
					}
				}
			}
			break;
		}
		case 2:
		{
			if (array != null)
			{
				lock (veinGroupsLock)
				{
					for (int l = 1; l < array.Length; l++)
					{
						veinAmounts[(uint)array[l].type] += array[l].amount;
					}
				}
			}
			if (factory == null)
			{
				break;
			}
			MinerComponent[] minerPool2 = factory.factorySystem.minerPool;
			int minerCursor2 = factory.factorySystem.minerCursor;
			PowerConsumerComponent[] consumerPool2 = factory.powerSystem.consumerPool;
			VeinData[] veinPool2 = factory.veinPool;
			hashes.Clear();
			for (int m = 0; m < minerCursor2; m++)
			{
				ref MinerComponent reference2 = ref minerPool2[m];
				if (reference2.id != m || reference2.type == EMinerType.Water || consumerPool2[reference2.pcId].networkId <= 0)
				{
					continue;
				}
				for (int n = 0; n < reference2.veinCount; n++)
				{
					int num2 = reference2.veins[n];
					if (num2 > 0 && veinPool2[num2].id == num2 && !hashes.Contains(num2))
					{
						veinAmounts[(uint)veinPool2[num2].type] -= veinPool2[num2].amount;
						hashes.Add(num2);
					}
				}
			}
			break;
		}
		default:
			if (array == null)
			{
				break;
			}
			lock (veinGroupsLock)
			{
				for (int i = 1; i < array.Length; i++)
				{
					veinAmounts[(uint)array[i].type] += array[i].amount;
				}
			}
			break;
		}
		veinAmounts[0] = 0L;
		return array != null;
	}

	public bool SummarizeVeinCountsByFilter(ref int[] veinCounts, HashSet<int> hashes, int filterType)
	{
		if (veinCounts == null)
		{
			veinCounts = new int[64];
		}
		Array.Clear(veinCounts, 0, veinCounts.Length);
		hashes.Clear();
		VeinGroup[] array = runtimeVeinGroups;
		switch (filterType)
		{
		case 1:
		{
			if (factory == null)
			{
				break;
			}
			MinerComponent[] minerPool = factory.factorySystem.minerPool;
			int minerCursor = factory.factorySystem.minerCursor;
			PowerConsumerComponent[] consumerPool = factory.powerSystem.consumerPool;
			VeinData[] veinPool = factory.veinPool;
			hashes.Clear();
			for (int j = 0; j < minerCursor; j++)
			{
				ref MinerComponent reference = ref minerPool[j];
				if (reference.id != j || reference.type == EMinerType.Water || consumerPool[reference.pcId].networkId <= 0)
				{
					continue;
				}
				for (int k = 0; k < reference.veinCount; k++)
				{
					int num = reference.veins[k];
					if (num > 0 && veinPool[num].id == num && !hashes.Contains(num))
					{
						veinCounts[(uint)veinPool[num].type]++;
						hashes.Add(num);
					}
				}
			}
			break;
		}
		case 2:
		{
			if (array != null)
			{
				lock (veinGroupsLock)
				{
					for (int l = 1; l < array.Length; l++)
					{
						veinCounts[(uint)array[l].type] += array[l].count;
					}
				}
			}
			if (factory == null)
			{
				break;
			}
			MinerComponent[] minerPool2 = factory.factorySystem.minerPool;
			int minerCursor2 = factory.factorySystem.minerCursor;
			PowerConsumerComponent[] consumerPool2 = factory.powerSystem.consumerPool;
			VeinData[] veinPool2 = factory.veinPool;
			hashes.Clear();
			for (int m = 0; m < minerCursor2; m++)
			{
				ref MinerComponent reference2 = ref minerPool2[m];
				if (reference2.id != m || reference2.type == EMinerType.Water || consumerPool2[reference2.pcId].networkId <= 0)
				{
					continue;
				}
				for (int n = 0; n < reference2.veinCount; n++)
				{
					int num2 = reference2.veins[n];
					if (num2 > 0 && veinPool2[num2].id == num2 && !hashes.Contains(num2))
					{
						veinCounts[(uint)veinPool2[num2].type]--;
						hashes.Add(num2);
					}
				}
			}
			break;
		}
		default:
			if (array == null)
			{
				break;
			}
			lock (veinGroupsLock)
			{
				for (int i = 1; i < array.Length; i++)
				{
					veinCounts[(uint)array[i].type] += array[i].count;
				}
			}
			break;
		}
		veinCounts[0] = 0;
		return array != null;
	}

	public bool ValidateScanningStatus(out bool error)
	{
		error = false;
		if (scanned)
		{
			if (veinGroups != null && veinGroups.Length >= 1 && (double)birthPoint.sqrMagnitude > 1E-08)
			{
				return true;
			}
			error = true;
			return false;
		}
		return false;
	}

	public void Load()
	{
		PlanetModelingManager.RequestLoadPlanet(this);
	}

	public void LoadFactory()
	{
		bodyObject.SetActive(value: true);
		PlanetModelingManager.RequestLoadPlanetFactory(this);
	}

	public void Unload()
	{
		if (!loading)
		{
			loaded = false;
			factoryLoaded = false;
			wanted = false;
			loading = false;
			factoryLoading = false;
			UnloadFactory();
			UnloadData();
			UnloadMeshes();
		}
	}

	public void RunScanThread()
	{
		PlanetModelingManager.RequestScanPlanet(this);
	}

	public void UnloadFactory()
	{
		factoryLoading = false;
		factoryLoaded = false;
		if (physics != null)
		{
			physics.Free();
			physics = null;
		}
		if (audio != null)
		{
			audio.Free();
			audio = null;
		}
		if (factoryModel != null)
		{
			factoryModel.Free();
			UnityEngine.Object.Destroy(factoryModel.gameObject);
			factoryModel = null;
		}
		if (factoryAudio != null)
		{
			factoryAudio.Free();
			UnityEngine.Object.Destroy(factoryAudio.gameObject);
			factoryAudio = null;
		}
		if (factory != null)
		{
			factory.UnloadDisplay();
			factory.FlushPools();
		}
	}

	public void Free()
	{
		loaded = false;
		factoryLoaded = false;
		wanted = false;
		loading = false;
		factoryLoading = false;
		UnloadFactory();
		UnloadData();
		UnloadMeshes();
		modData = null;
		veinGroups = null;
		gasItems = null;
		gasSpeeds = null;
		gasHeatValues = null;
		if (aux != null)
		{
			aux.Free();
			aux = null;
		}
	}

	public void NotifyLoaded()
	{
		loaded = true;
		loading = false;
		wanted = true;
		if (onLoaded != null)
		{
			onLoaded(this);
		}
	}

	public void NotifyFactoryLoaded()
	{
		factoryLoaded = true;
		factoryLoading = false;
		factingCompletedStage = -1;
		wanted = true;
		if (onFactoryLoaded != null)
		{
			onFactoryLoaded(this);
		}
	}

	public void NotifyFactingStageComplete(int stage)
	{
		factingCompletedStage = stage;
	}

	public void NotifyScanEnded()
	{
		scanned = true;
		scanning = false;
	}

	public void RegenerateName(bool notifychange)
	{
		string text = null;
		name = string.Concat(str2: (star.planetCount > 20) ? (index + 1).ToString() : NameGen.roman[index + 1], str0: star.displayName, str1: " ", str3: "号星".Translate());
		if (notifychange && string.IsNullOrEmpty(overrideName))
		{
			galaxy.NotifyAstroNameChange(astroId);
		}
	}

	public void UnloadData()
	{
		if (data != null)
		{
			data.Free();
			data = null;
		}
	}

	private void UnloadMeshes()
	{
		for (int i = 0; i < meshes.Length; i++)
		{
			if (meshes[i] != null)
			{
				UnityEngine.Object.Destroy(meshes[i]);
				meshes[i] = null;
			}
		}
		if (gameObject != null)
		{
			UnityEngine.Object.Destroy(gameObject);
			gameObject = null;
		}
		if (terrainMaterial != null)
		{
			UnityEngine.Object.Destroy(terrainMaterial);
			terrainMaterial = null;
		}
		if (oceanMaterial != null)
		{
			UnityEngine.Object.Destroy(oceanMaterial);
			oceanMaterial = null;
		}
		if (atmosMaterial != null)
		{
			UnityEngine.Object.Destroy(atmosMaterial);
			atmosMaterial = null;
		}
		if (atmosMaterialLate != null)
		{
			UnityEngine.Object.Destroy(atmosMaterialLate);
			atmosMaterialLate = null;
		}
		if (nephogramMaterial != null)
		{
			UnityEngine.Object.Destroy(nephogramMaterial);
			nephogramMaterial = null;
		}
		if (cloudMaterial != null)
		{
			UnityEngine.Object.Destroy(cloudMaterial);
			cloudMaterial = null;
		}
		if (minimapMaterial != null)
		{
			UnityEngine.Object.Destroy(minimapMaterial);
			minimapMaterial = null;
		}
		if (reformMaterial0 != null)
		{
			UnityEngine.Object.Destroy(reformMaterial0);
			reformMaterial0 = null;
		}
		if (reformMaterial1 != null)
		{
			UnityEngine.Object.Destroy(reformMaterial1);
			reformMaterial1 = null;
		}
		if (heightmap != null)
		{
			heightmap.Release();
			UnityEngine.Object.Destroy(heightmap);
			heightmap = null;
		}
	}

	public void SummarizeVeinGroups()
	{
		if (data == null)
		{
			return;
		}
		VeinData[] veinPool = data.veinPool;
		int veinCursor = data.veinCursor;
		int num = 0;
		for (int i = 1; i < veinCursor; i++)
		{
			int groupIndex = veinPool[i].groupIndex;
			if (groupIndex > num)
			{
				num = groupIndex;
			}
		}
		lock (veinGroupsLock)
		{
			if (veinGroups == null || veinGroups.Length != num + 1)
			{
				veinGroups = new VeinGroup[num + 1];
			}
			Array.Clear(veinGroups, 0, veinGroups.Length);
			veinGroups[0].SetNull();
			for (int j = 1; j < veinCursor; j++)
			{
				if (veinPool[j].id == j)
				{
					int groupIndex2 = veinPool[j].groupIndex;
					veinGroups[groupIndex2].type = veinPool[j].type;
					veinGroups[groupIndex2].pos += veinPool[j].pos;
					veinGroups[groupIndex2].count++;
					veinGroups[groupIndex2].amount += veinPool[j].amount;
				}
			}
			veinGroups[0].type = EVeinType.None;
			for (int k = 0; k < veinGroups.Length; k++)
			{
				veinGroups[k].pos.Normalize();
			}
		}
	}

	public void GenVeinBiasVector()
	{
		if (veinBiasVector.sqrMagnitude != 0f)
		{
			return;
		}
		DotNet35Random dotNet35Random = new DotNet35Random(seed);
		dotNet35Random.Next();
		dotNet35Random.Next();
		dotNet35Random.Next();
		dotNet35Random.Next();
		dotNet35Random.Next();
		if (galaxy != null && galaxy.birthPlanetId == id)
		{
			if (!loaded)
			{
				PlanetData unloadedCopy = GetUnloadedCopy(this);
				unloadedCopy.RegenerateRawDataImmediately();
				birthPoint = unloadedCopy.birthPoint;
				ReleaseCopy(unloadedCopy);
			}
			veinBiasVector = birthPoint;
			veinBiasVector.Normalize();
			veinBiasVector *= 0.75f;
		}
		else
		{
			DotNet35Random dotNet35Random2 = new DotNet35Random(dotNet35Random.Next());
			Vector3 vector = default(Vector3);
			vector.x = (float)dotNet35Random2.NextDouble() * 2f - 1f;
			vector.y = (float)dotNet35Random2.NextDouble() - 0.5f;
			vector.z = (float)dotNet35Random2.NextDouble() * 2f - 1f;
			vector.Normalize();
			vector *= (float)(dotNet35Random2.NextDouble() * 0.4 + 0.2);
			veinBiasVector = vector;
		}
	}

	public void GenBirthPoints()
	{
		if (type == EPlanetType.Gas)
		{
			birthPoint = new Vector3(0f, 0f, 0f - realRadius - 25f);
			birthResourcePoint0 = Vector3.up;
			birthResourcePoint1 = Vector3.down;
			return;
		}
		DotNet35Random dotNet35Random = new DotNet35Random(seed);
		dotNet35Random.Next();
		dotNet35Random.Next();
		dotNet35Random.Next();
		dotNet35Random.Next();
		int birthSeed = dotNet35Random.Next();
		GenBirthPoints(data, birthSeed);
	}

	public void GenBirthPoints(PlanetRawData rawData, int _birthSeed)
	{
		DotNet35Random dotNet35Random = new DotNet35Random(_birthSeed);
		Pose pose = PredictPose(85.0);
		Vector3 vector = Maths.QInvRotateLF(pose.rotation, star.uPosition - (VectorLF3)pose.position * 40000.0);
		vector.Normalize();
		Vector3 normalized = Vector3.Cross(vector, Vector3.up).normalized;
		Vector3 normalized2 = Vector3.Cross(normalized, vector).normalized;
		int i = 0;
		int num;
		for (num = 256; i < num; i++)
		{
			float num2 = (float)(dotNet35Random.NextDouble() * 2.0 - 1.0) * 0.5f;
			float num3 = (float)(dotNet35Random.NextDouble() * 2.0 - 1.0) * 0.5f;
			Vector3 vector2 = vector + num2 * normalized + num3 * normalized2;
			vector2.Normalize();
			birthPoint = vector2 * (realRadius + 0.2f + 1.45f);
			normalized = Vector3.Cross(vector2, Vector3.up).normalized;
			normalized2 = Vector3.Cross(normalized, vector2).normalized;
			bool flag = false;
			for (int j = 0; j < 10; j++)
			{
				float x = (float)(dotNet35Random.NextDouble() * 2.0 - 1.0);
				float y = (float)(dotNet35Random.NextDouble() * 2.0 - 1.0);
				Vector2 vector3 = new Vector2(x, y).normalized * 0.1f;
				Vector2 vector4 = -vector3;
				float num4 = (float)(dotNet35Random.NextDouble() * 2.0 - 1.0) * 0.06f;
				float num5 = (float)(dotNet35Random.NextDouble() * 2.0 - 1.0) * 0.06f;
				vector4.x += num4;
				vector4.y += num5;
				Vector3 normalized3 = (vector2 + vector3.x * normalized + vector3.y * normalized2).normalized;
				Vector3 normalized4 = (vector2 + vector4.x * normalized + vector4.y * normalized2).normalized;
				birthResourcePoint0 = normalized3.normalized;
				birthResourcePoint1 = normalized4.normalized;
				float num6 = realRadius + 0.2f;
				if (rawData.QueryHeight(vector2) > num6 && rawData.QueryHeight(normalized3) > num6 && rawData.QueryHeight(normalized4) > num6)
				{
					Vector3 vpos = normalized3 + normalized * 0.03f;
					Vector3 vpos2 = normalized3 - normalized * 0.03f;
					Vector3 vpos3 = normalized3 + normalized2 * 0.03f;
					Vector3 vpos4 = normalized3 - normalized2 * 0.03f;
					Vector3 vpos5 = normalized4 + normalized * 0.03f;
					Vector3 vpos6 = normalized4 - normalized * 0.03f;
					Vector3 vpos7 = normalized4 + normalized2 * 0.03f;
					Vector3 vpos8 = normalized4 - normalized2 * 0.03f;
					if (rawData.QueryHeight(vpos) > num6 && rawData.QueryHeight(vpos2) > num6 && rawData.QueryHeight(vpos3) > num6 && rawData.QueryHeight(vpos4) > num6 && rawData.QueryHeight(vpos5) > num6 && rawData.QueryHeight(vpos6) > num6 && rawData.QueryHeight(vpos7) > num6 && rawData.QueryHeight(vpos8) > num6)
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				break;
			}
		}
		if (i >= num)
		{
			birthPoint = new Vector3(0f, realRadius + 5f, 0f);
		}
	}

	public void UpdateRuntimePose(double time)
	{
		double num = time / orbitalPeriod + (double)orbitPhase / 360.0;
		int num2 = (int)(num + 0.1);
		num -= (double)num2;
		runtimeOrbitPhase = (float)num * 360f;
		num *= Math.PI * 2.0;
		double num3 = time / rotationPeriod + (double)rotationPhase / 360.0;
		int num4 = (int)(num3 + 0.1);
		num3 = (num3 - (double)num4) * 360.0;
		runtimeRotationPhase = (float)num3;
		VectorLF3 vectorLF = Maths.QRotateLF(runtimeOrbitRotation, new VectorLF3(Math.Cos(num) * (double)orbitRadius, 0.0, Math.Sin(num) * (double)orbitRadius));
		if (orbitAroundPlanet != null)
		{
			vectorLF.x += orbitAroundPlanet.runtimePosition.x;
			vectorLF.y += orbitAroundPlanet.runtimePosition.y;
			vectorLF.z += orbitAroundPlanet.runtimePosition.z;
		}
		runtimePosition = vectorLF;
		runtimeRotation = runtimeSystemRotation * Quaternion.AngleAxis((float)num3, Vector3.down);
		uPosition.x = star.uPosition.x + vectorLF.x * 40000.0;
		uPosition.y = star.uPosition.y + vectorLF.y * 40000.0;
		uPosition.z = star.uPosition.z + vectorLF.z * 40000.0;
		runtimeLocalSunDirection = Maths.QInvRotate(runtimeRotation, -vectorLF);
		double num5 = time + 1.0 / 60.0;
		double num6 = num5 / orbitalPeriod + (double)orbitPhase / 360.0;
		int num7 = (int)(num6 + 0.1);
		num6 -= (double)num7;
		num6 *= Math.PI * 2.0;
		double num8 = num5 / rotationPeriod + (double)rotationPhase / 360.0;
		int num9 = (int)(num8 + 0.1);
		num8 = (num8 - (double)num9) * 360.0;
		VectorLF3 vectorLF2 = Maths.QRotateLF(runtimeOrbitRotation, new VectorLF3(Math.Cos(num6) * (double)orbitRadius, 0.0, Math.Sin(num6) * (double)orbitRadius));
		if (orbitAroundPlanet != null)
		{
			vectorLF2.x += orbitAroundPlanet.runtimePositionNext.x;
			vectorLF2.y += orbitAroundPlanet.runtimePositionNext.y;
			vectorLF2.z += orbitAroundPlanet.runtimePositionNext.z;
		}
		runtimePositionNext = vectorLF2;
		runtimeRotationNext = runtimeSystemRotation * Quaternion.AngleAxis((float)num8, Vector3.down);
		uPositionNext.x = star.uPosition.x + vectorLF2.x * 40000.0;
		uPositionNext.y = star.uPosition.y + vectorLF2.y * 40000.0;
		uPositionNext.z = star.uPosition.z + vectorLF2.z * 40000.0;
		galaxy.astrosData[id].uPos = uPosition;
		galaxy.astrosData[id].uRot = runtimeRotation;
		galaxy.astrosData[id].uPosNext = uPositionNext;
		galaxy.astrosData[id].uRotNext = runtimeRotationNext;
		galaxy.astrosFactory[id] = factory;
	}

	public Pose PredictPose(double time)
	{
		double num = time / orbitalPeriod + (double)orbitPhase / 360.0;
		int num2 = (int)(num + 0.1);
		num -= (double)num2;
		num *= Math.PI * 2.0;
		double num3 = time / rotationPeriod + (double)rotationPhase / 360.0;
		int num4 = (int)(num3 + 0.1);
		num3 = (num3 - (double)num4) * 360.0;
		Vector3 position = Maths.QRotate(v: new Vector3((float)Math.Cos(num) * orbitRadius, 0f, (float)Math.Sin(num) * orbitRadius), q: runtimeOrbitRotation);
		if (orbitAroundPlanet != null)
		{
			Pose pose = orbitAroundPlanet.PredictPose(time);
			position.x += pose.position.x;
			position.y += pose.position.y;
			position.z += pose.position.z;
		}
		return new Pose(position, runtimeSystemRotation * Quaternion.AngleAxis((float)num3, Vector3.down));
	}

	public void PredictPose(double time, out VectorLF3 pos, out Quaternion rot)
	{
		double num = time / orbitalPeriod + (double)orbitPhase / 360.0;
		int num2 = (int)(num + 0.1);
		num -= (double)num2;
		num *= Math.PI * 2.0;
		double num3 = time / rotationPeriod + (double)rotationPhase / 360.0;
		int num4 = (int)(num3 + 0.1);
		num3 = (num3 - (double)num4) * 360.0;
		pos = new VectorLF3(Math.Cos(num) * (double)orbitRadius, 0.0, Math.Sin(num) * (double)orbitRadius);
		pos = Maths.QRotateLF(runtimeOrbitRotation, pos);
		if (orbitAroundPlanet != null)
		{
			orbitAroundPlanet.PredictPose(time, out var pos2, out var _);
			pos.x += pos2.x;
			pos.y += pos2.y;
			pos.z += pos2.z;
		}
		rot = runtimeSystemRotation * Quaternion.AngleAxis((float)num3, Vector3.down);
	}

	public void PredictUPose(double time, out VectorLF3 uPos, out Quaternion uRot)
	{
		PredictPose(time, out var pos, out var rot);
		uPos.x = pos.x * 40000.0 + star.uPosition.x;
		uPos.y = pos.y * 40000.0 + star.uPosition.y;
		uPos.z = pos.z * 40000.0 + star.uPosition.z;
		uRot = rot;
	}

	public VectorLF3 GetUniversalVelocityAtLocalPoint(double time, Vector3 lpoint)
	{
		double time2 = time + 1.0 / 60.0;
		PredictUPose(time, out var uPos, out var uRot);
		PredictUPose(time2, out var uPos2, out var uRot2);
		VectorLF3 vectorLF = uPos + (VectorLF3)(uRot * lpoint);
		return (uPos2 + (VectorLF3)(uRot2 * lpoint) - vectorLF) / (1.0 / 60.0);
	}

	private void PredictLocalGeography(Vector3 local, double time, out float sunMaxAngle, out float sunMinAngle, out float sunAngle)
	{
		Pose pose = PredictPose(time);
		float num = 90f - Vector3.Angle(pose.rotation * Vector3.up, -pose.position);
		float num2 = 90f - Vector3.Angle(Vector3.up, local);
		sunMaxAngle = 90f - Mathf.Abs(num - num2);
		sunMinAngle = 90f - Mathf.Abs(num - (180f - num2));
		if (sunMinAngle < -180f)
		{
			sunMinAngle += 360f;
		}
		if (sunMinAngle > 90f)
		{
			sunMinAngle = 180f - sunMinAngle;
		}
		if (sunMinAngle < -90f)
		{
			sunMinAngle = -180f - sunMinAngle;
		}
		sunAngle = 90f - Vector3.Angle(pose.rotation * local, -pose.position);
	}

	public void GetLocalGeography(Vector3 local, double time, out int summerWinter, out bool tropical, out bool polar, out float sunMaxAngle, out float sunMinAngle, out int dayNight, out float remainTime)
	{
		float num = 90f - Vector3.Angle(runtimeRotation * Vector3.up, -runtimePosition);
		float num2 = 90f - Vector3.Angle(Vector3.up, local);
		sunMaxAngle = 90f - Mathf.Abs(num - num2);
		sunMinAngle = 90f - Mathf.Abs(num - (180f - num2));
		if (sunMinAngle < -180f)
		{
			sunMinAngle += 360f;
		}
		if (sunMinAngle > 90f)
		{
			sunMinAngle = 180f - sunMinAngle;
		}
		if (sunMinAngle < -90f)
		{
			sunMinAngle = -180f - sunMinAngle;
		}
		float num3 = 90f - Vector3.Angle(runtimeRotation * local, -runtimePosition);
		dayNight = ((num3 >= 0f) ? 1 : (-1));
		remainTime = 10000000f;
		float num4 = Vector3.Angle(runtimeRotation * Vector3.up, Vector3.up);
		summerWinter = ((num * num2 >= 0f) ? 1 : (-1));
		tropical = (Mathf.Abs(num2) < num4 && sunMaxAngle > 70f) || sunMaxAngle > 85f;
		polar = sunMaxAngle < 0f || sunMinAngle > 0f;
		bool flag = false;
		int num5 = 0;
		double num6 = ((orbitAround == 0) ? orbitalPeriod : orbitAroundPlanet.orbitalPeriod);
		double value = rotationPeriod;
		bool flag2 = Math.Abs(rotationPeriod - num6) < 1.0;
		if (!flag2)
		{
			value = 1.0 / (1.0 / rotationPeriod - 1.0 / num6);
		}
		value = Math.Abs(value);
		if (dayNight >= 0)
		{
			float sunMaxAngle2 = sunMaxAngle;
			float sunMinAngle2 = sunMinAngle;
			float sunAngle = num3;
			double num7 = time;
			double num8 = time + value * 0.8;
			double num9 = time + value * 0.4;
			while (true)
			{
				if (sunMinAngle2 > 0f)
				{
					double num10 = ((orbitAroundPlanet != null) ? orbitAroundPlanet.orbitalPeriod : orbitalPeriod);
					num7 = time;
					num8 = time + num10 * 0.8;
					do
					{
						num9 = (num7 + num8) * 0.5;
						PredictLocalGeography(local, num9, out sunMaxAngle2, out sunMinAngle2, out sunAngle);
						if (sunMinAngle2 > 0f)
						{
							num7 = num9;
						}
						else
						{
							num8 = num9;
						}
						if (num8 - num7 < 1.0)
						{
							num7 = num9;
							num8 = num9 + value * 0.8;
							break;
						}
					}
					while (num5++ < 100);
				}
				while (true)
				{
					num9 = (num7 + num8) * 0.5;
					PredictLocalGeography(local, num9, out sunMaxAngle2, out sunMinAngle2, out sunAngle);
					if (sunAngle > 0f)
					{
						num7 = num9;
					}
					else
					{
						num8 = num9;
					}
					if (num8 - num7 < 0.10000000149011612)
					{
						break;
					}
					if (num5++ >= 100)
					{
						goto end_IL_0204;
					}
				}
				double num11 = num7 - time;
				if (num11 > value * 0.8 - 0.10999999940395355 && !flag)
				{
					flag = true;
					if (num5++ < 100)
					{
						continue;
					}
				}
				remainTime = (float)num11;
				break;
				continue;
				end_IL_0204:
				break;
			}
		}
		else
		{
			float sunMaxAngle3 = sunMaxAngle;
			float sunMinAngle3 = sunMinAngle;
			float sunAngle2 = num3;
			double num12 = time;
			double num13 = time + value * 0.8;
			double num14 = time + value * 0.4;
			while (true)
			{
				if (sunMaxAngle3 < 0f)
				{
					double num15 = ((orbitAroundPlanet != null) ? orbitAroundPlanet.orbitalPeriod : orbitalPeriod);
					num12 = time;
					num13 = time + num15 * 0.8;
					do
					{
						num14 = (num12 + num13) * 0.5;
						PredictLocalGeography(local, num14, out sunMaxAngle3, out sunMinAngle3, out sunAngle2);
						if (sunMaxAngle3 < 0f)
						{
							num12 = num14;
						}
						else
						{
							num13 = num14;
						}
						if (num13 - num12 < 1.0)
						{
							num12 = num14;
							num13 = num14 + value * 0.8;
							break;
						}
					}
					while (num5++ < 100);
				}
				while (true)
				{
					num14 = (num12 + num13) * 0.5;
					PredictLocalGeography(local, num14, out sunMaxAngle3, out sunMinAngle3, out sunAngle2);
					if (sunAngle2 < 0f)
					{
						num12 = num14;
					}
					else
					{
						num13 = num14;
					}
					if (num13 - num12 < 0.10000000149011612)
					{
						break;
					}
					if (num5++ >= 100)
					{
						goto end_IL_036b;
					}
				}
				double num16 = num12 - time;
				if (num16 > value * 0.8 - 0.10999999940395355 && !flag)
				{
					flag = true;
					if (num5++ < 100)
					{
						continue;
					}
				}
				remainTime = (float)num16;
				break;
				continue;
				end_IL_036b:
				break;
			}
		}
		if (flag2)
		{
			remainTime = 10000000f;
		}
	}

	public void AddHeightMapModLevel(int index, int level)
	{
		if (data.AddModLevel(index, level))
		{
			int num = precision / segment;
			int num2 = index % data.stride;
			int num3 = index / data.stride;
			int num4 = ((num2 >= data.substride) ? 1 : 0) + ((num3 >= data.substride) ? 2 : 0);
			int num5 = num2 % data.substride;
			int num6 = num3 % data.substride;
			int num7 = (num5 - 1) / num;
			int num8 = (num6 - 1) / num;
			int num9 = num5 / num;
			int num10 = num6 / num;
			if (num9 >= segment)
			{
				num9 = segment - 1;
			}
			if (num10 >= segment)
			{
				num10 = segment - 1;
			}
			int num11 = num4 * segment * segment;
			int num12 = num7 + num8 * segment + num11;
			int num13 = num9 + num8 * segment + num11;
			int num14 = num7 + num10 * segment + num11;
			int num15 = num9 + num10 * segment + num11;
			dirtyFlags[num12] = true;
			dirtyFlags[num13] = true;
			dirtyFlags[num14] = true;
			dirtyFlags[num15] = true;
		}
	}

	public bool UpdateDirtyMeshes()
	{
		bool result = false;
		for (int i = 0; i < dirtyFlags.Length; i++)
		{
			if (UpdateDirtyMesh(i))
			{
				result = true;
			}
		}
		return result;
	}

	public bool UpdateDirtyMesh(int dirtyIdx)
	{
		if (dirtyFlags[dirtyIdx])
		{
			dirtyFlags[dirtyIdx] = false;
			int num = precision / segment;
			int num2 = segment * segment;
			int num3 = dirtyIdx / num2;
			int num4 = num3 % 2;
			int num5 = num3 / 2;
			int num6 = dirtyIdx % num2;
			int num7 = num6 % segment * num + num4 * data.substride;
			int num8 = num6 / segment * num + num5 * data.substride;
			int stride = data.stride;
			float num9 = radius * scale + 0.2f;
			Mesh mesh = meshes[dirtyIdx];
			Vector3[] vertices = mesh.vertices;
			Vector3[] normals = mesh.normals;
			int num10 = 0;
			for (int i = num8; i <= num8 + num; i++)
			{
				for (int j = num7; j <= num7 + num; j++)
				{
					int num11 = j + i * stride;
					float num12 = (float)(int)data.heightData[num11] * 0.01f * scale;
					float num13 = (float)data.GetModLevel(num11) * 0.3333333f;
					float num14 = num9;
					if (num13 > 0f)
					{
						num14 = (float)data.GetModPlane(num11) * 0.01f * scale;
					}
					float num15 = num12 * (1f - num13) + num14 * num13;
					vertices[num10].x = data.vertices[num11].x * num15;
					vertices[num10].y = data.vertices[num11].y * num15;
					vertices[num10].z = data.vertices[num11].z * num15;
					normals[num10].x = data.normals[num11].x * (1f - num13) + data.vertices[num11].x * num13;
					normals[num10].y = data.normals[num11].y * (1f - num13) + data.vertices[num11].y * num13;
					normals[num10].z = data.normals[num11].z * (1f - num13) + data.vertices[num11].z * num13;
					normals[num10].Normalize();
					num10++;
				}
			}
			mesh.vertices = vertices;
			mesh.normals = normals;
			meshColliders[dirtyIdx].sharedMesh = null;
			meshColliders[dirtyIdx].sharedMesh = mesh;
			return true;
		}
		return false;
	}

	public void RegenerateRawDataTerrainOnlyImmediately()
	{
		PlanetAlgorithm planetAlgorithm = PlanetModelingManager.Algorithm(this);
		data = new PlanetRawData(precision);
		modData = data.InitModData(modData);
		data.CalcVerts();
		aux = new PlanetAuxData(this);
		planetAlgorithm.GenerateTerrain(mod_x, mod_y);
		planetAlgorithm.CalcWaterPercent();
		GenBirthPoints();
	}

	public void RegenerateRawDataImmediately()
	{
		PlanetAlgorithm planetAlgorithm = PlanetModelingManager.Algorithm(this);
		data = new PlanetRawData(precision);
		modData = data.InitModData(modData);
		data.CalcVerts();
		aux = new PlanetAuxData(this);
		planetAlgorithm.GenerateTerrain(mod_x, mod_y);
		planetAlgorithm.CalcWaterPercent();
		data.vegeCursor = 1;
		if (type != EPlanetType.Gas)
		{
			planetAlgorithm.GenerateVegetables();
		}
		data.veinCursor = 1;
		if (type != EPlanetType.Gas)
		{
			planetAlgorithm.GenerateVeins();
		}
		SummarizeVeinGroups();
		GenBirthPoints();
	}

	public void RegenerateVegetationImmediately()
	{
		data.vegeCursor = 1;
		PlanetModelingManager.Algorithm(this).GenerateVegetables();
	}

	public void ExportRuntime(BinaryWriter w)
	{
		w.Write(modData.Length);
		w.Write(modData);
		w.Write(0);
		w.Write(0);
	}

	public void ImportRuntime(BinaryReader r)
	{
		int count = r.ReadInt32();
		modData = r.ReadBytes(count);
		int num = r.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			r.ReadInt64();
		}
		int num2 = r.ReadInt32();
		for (int j = 0; j < num2; j++)
		{
			r.ReadInt32();
			r.ReadSingle();
			r.ReadSingle();
			r.ReadSingle();
			r.ReadInt32();
			r.ReadInt64();
		}
	}

	public void ExportScannedData(BinaryWriter w)
	{
		w.Write(0);
		w.Write(landPercent);
		w.Write(veinBiasVector.x);
		w.Write(veinBiasVector.y);
		w.Write(veinBiasVector.z);
		w.Write(birthPoint.x);
		w.Write(birthPoint.y);
		w.Write(birthPoint.z);
		w.Write(birthResourcePoint0.x);
		w.Write(birthResourcePoint0.y);
		w.Write(birthResourcePoint0.z);
		w.Write(birthResourcePoint1.x);
		w.Write(birthResourcePoint1.y);
		w.Write(birthResourcePoint1.z);
		lock (veinGroupsLock)
		{
			Assert.NotNull(veinGroups);
			if (veinGroups != null)
			{
				int num = veinGroups.Length;
				Assert.Positive(num);
				w.Write(num);
				for (int i = 1; i < num; i++)
				{
					veinGroups[i].Export(w);
				}
			}
		}
	}

	public void ImportScannedData(BinaryReader r)
	{
		r.ReadInt32();
		Assert.False(scanning);
		scanned = true;
		scanning = false;
		landPercent = r.ReadSingle();
		veinBiasVector.x = r.ReadSingle();
		veinBiasVector.y = r.ReadSingle();
		veinBiasVector.z = r.ReadSingle();
		birthPoint.x = r.ReadSingle();
		birthPoint.y = r.ReadSingle();
		birthPoint.z = r.ReadSingle();
		birthResourcePoint0.x = r.ReadSingle();
		birthResourcePoint0.y = r.ReadSingle();
		birthResourcePoint0.z = r.ReadSingle();
		birthResourcePoint1.x = r.ReadSingle();
		birthResourcePoint1.y = r.ReadSingle();
		birthResourcePoint1.z = r.ReadSingle();
		lock (veinGroupsLock)
		{
			int num = r.ReadInt32();
			Assert.Positive(num);
			if (num > 0)
			{
				veinGroups = new VeinGroup[num];
				veinGroups[0].SetNull();
				for (int i = 1; i < num; i++)
				{
					veinGroups[i].Import(r);
				}
			}
		}
	}

	public void CopyScannedDataFrom(PlanetData copy)
	{
		landPercent = copy.landPercent;
		veinBiasVector = copy.veinBiasVector;
		birthPoint = copy.birthPoint;
		birthResourcePoint0 = copy.birthResourcePoint0;
		birthResourcePoint1 = copy.birthResourcePoint1;
		veinGroups = new VeinGroup[copy.veinGroups.Length];
		veinGroups[0].SetNull();
		for (int i = 1; i < copy.veinGroups.Length; i++)
		{
			veinGroups[i] = copy.veinGroups[i];
		}
	}

	public override string ToString()
	{
		return "Planet " + displayName;
	}
}
