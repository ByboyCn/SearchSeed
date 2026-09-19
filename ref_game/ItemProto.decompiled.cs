using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public class ItemProto : Proto
{
	public EItemType Type;

	public int SubID;

	public string MiningFrom;

	public string ProduceFrom;

	public int StackSize;

	public int Grade;

	public int[] Upgrades;

	public bool IsFluid;

	public bool IsEntity;

	public bool CanBuild;

	public bool BuildInGas;

	public string IconPath;

	public string IconTag;

	public int ModelIndex;

	public int ModelCount;

	public int HpMax;

	public int Ability;

	public long HeatValue;

	public long Potential;

	public float ReactorInc;

	public int FuelType;

	public EAmmoType AmmoType;

	public EBombType BombType;

	public int CraftType;

	public int BuildIndex;

	public int BuildMode;

	public int GridIndex;

	public int UnlockKey;

	public int PreTechOverride;

	public bool Productive;

	public int MechaMaterialID;

	public float DropRate;

	public int EnemyDropLevel;

	public Vector2 EnemyDropRange;

	public float EnemyDropCount;

	public int EnemyDropMask;

	public float EnemyDropMaskRatio;

	public int[] DescFields;

	public string Description;

	public string iconTagString;

	public string iconNameTagString;

	[NonSerialized]
	public bool isRaw;

	[NonSerialized]
	public RecipeProto handcraft;

	[NonSerialized]
	public RecipeProto maincraft;

	[NonSerialized]
	public int handcraftProductCount;

	[NonSerialized]
	public int maincraftProductCount;

	[NonSerialized]
	public List<RecipeProto> handcrafts;

	[NonSerialized]
	public List<RecipeProto> recipes;

	[NonSerialized]
	public List<RecipeProto> makes;

	[NonSerialized]
	public List<IDCNTINC> rawMats;

	[NonSerialized]
	public TechProto preTech;

	[NonSerialized]
	public bool missingTech;

	[NonSerialized]
	public int productionMask;

	[NonSerialized]
	public int consumptionMask;

	public const int kMaxProtoId = 12000;

	public const int kMaxSubId = 256;

	public const int kMaxProtoIndex = 9999;

	public const int kWaterId = 1000;

	public const int kDroneId = 5001;

	public const int kShipId = 5002;

	public const int kCourierId = 5003;

	public const int kConstructionDroneModelIndex = 454;

	public const int kRocketId = 1503;

	public const int kWarperId = 1210;

	public const int kVeinMinerId = 2301;

	public const int kOilMinerId = 2307;

	public const int kSpraycoaterId = 2313;

	public const int kAccumulator = 2206;

	public const int kAccumulatorFull = 2207;

	public const int kSoilPileId = 1099;

	public const int kTerrainId = 1131;

	public const int kMarkerId = 2401;

	public const int kAmmoPlasma = 1607;

	public const int kAmmoAntimatter = 1608;

	public const int kDFMemoryUnitId = 5201;

	public const int kDFSiliconNeuronId = 5202;

	public const int kDFReassemblerId = 5203;

	public const int kDFSingularityId = 5204;

	public const int kDFVirtualParticleId = 5205;

	public const int kDFEnergyFragment = 5206;

	public static int stationCollectorId;

	public static int[] kFuelAutoReplenishIds = new int[17]
	{
		1804, 1803, 1802, 1801, 1130, 1129, 1128, 1121, 1120, 1109,
		1011, 1114, 1007, 1006, 1117, 1030, 1031
	};

	private Sprite _iconSprite;

	private Sprite _propertyIconSprite;

	private Sprite _propertyIconSpriteSmall;

	[NonSerialized]
	public PrefabDesc prefabDesc;

	public static int[][] fuelNeeds = new int[64][];

	public static int[][] turretNeeds = new int[16][];

	public static int[] fluids;

	public static int[] turrets;

	public static int[] enemyDropRangeTable;

	public static int[] enemyDropLevelTable;

	public static float[] enemyDropCountTable;

	public static int[] enemyDropMaskTable;

	public static float[] enemyDropMaskRatioTable;

	public static int constructableCount;

	public static HashSet<int> constructableIdHash;

	public static int[] constructableIds;

	public static int[] constructableIndiceById;

	public static ItemProto[] itemProtoById;

	public static int[] itemIds;

	public static int[] itemIndices;

	public static int[] mechaMaterials;

	public const int kFighterIdRangeMin = 5100;

	public const int kFighterIdRangeMax = 5199;

	public static int[] kFighterIds;

	public static int[] kFighterGroundIds;

	public static int[] kFighterSpaceIds;

	public static int[] kFighterSpaceSmallIds;

	public static int[] kFighterSpaceLargeIds;

	public static int[] powerGenIndex2Id;

	public static int[] powerConIndex2Id;

	public static int[] powerGenId2Index;

	public static int[] powerConId2Index;

	public string miningFrom { get; set; }

	public string produceFrom { get; set; }

	public string description { get; set; }

	public int index { get; private set; }

	public Sprite iconSprite => _iconSprite;

	public Sprite propertyIconSprite => _propertyIconSprite;

	public Sprite propertyIconSpriteSmall => _propertyIconSpriteSmall;

	public string propertyName { get; private set; }

	public bool canUpgrade
	{
		get
		{
			if (Grade > 0)
			{
				return Upgrades.Length != 0;
			}
			return false;
		}
	}

	public bool isAmmo => (int)AmmoType > 0;

	public bool isBomb => BombType > EBombType.None;

	public bool isCraft => CraftType > 0;

	public bool isDynamicCraft => (CraftType & 1) > 0;

	public bool isSpaceCraft => (CraftType & 2) > 0;

	public bool isLargeCraft => (CraftType & 4) > 0;

	public bool isFighter
	{
		get
		{
			if (5100 <= ID)
			{
				return ID <= 5199;
			}
			return false;
		}
	}

	public bool isGroundFighter
	{
		get
		{
			if (isFighter)
			{
				return !isSpaceCraft;
			}
			return false;
		}
	}

	public bool isSpaceFighter
	{
		get
		{
			if (isFighter)
			{
				return isSpaceCraft;
			}
			return false;
		}
	}

	public bool isSmallSpaceFighter
	{
		get
		{
			if (isFighter && isSpaceCraft)
			{
				return !isLargeCraft;
			}
			return false;
		}
	}

	public bool isLargeSpaceFighter
	{
		get
		{
			if (isFighter && isSpaceCraft)
			{
				return isLargeCraft;
			}
			return false;
		}
	}

	public string typeString
	{
		get
		{
			switch (Type)
			{
			case EItemType.Unknown:
				return "未知分类".Translate();
			case EItemType.Resource:
				return "自然资源".Translate();
			case EItemType.Material:
				return "材料".Translate();
			case EItemType.Component:
				return "组件".Translate();
			case EItemType.Product:
				return "成品".Translate();
			case EItemType.Logistics:
				if (prefabDesc.isAccumulator)
				{
					return "电力储存".Translate();
				}
				if (prefabDesc.isPowerNode)
				{
					return "电力运输".Translate();
				}
				if (prefabDesc.isPowerExchanger)
				{
					return "电力交换".Translate();
				}
				return "物流运输".Translate();
			case EItemType.Production:
				if (prefabDesc.isPowerGen)
				{
					return "电力设备".Translate();
				}
				if (prefabDesc.isLab)
				{
					return "科研设备".Translate();
				}
				return prefabDesc.assemblerRecipeType switch
				{
					ERecipeType.Smelt => "冶炼设备".Translate(), 
					ERecipeType.Chemical => "化工设备".Translate(), 
					ERecipeType.Refine => "精炼设备".Translate(), 
					ERecipeType.Assemble => "制造台".Translate(), 
					ERecipeType.Particle => "粒子对撞机".Translate(), 
					ERecipeType.Exchange => "能量交换器".Translate(), 
					ERecipeType.PhotonStore => "射线接收站".Translate(), 
					ERecipeType.Fractionate => "分馏设备".Translate(), 
					ERecipeType.Research => "科研设备".Translate(), 
					_ => prefabDesc.minerType switch
					{
						EMinerType.Vein => "采矿设备".Translate(), 
						EMinerType.Water => "抽水设备".Translate(), 
						EMinerType.Oil => "抽油设备".Translate(), 
						_ => "生产设备".Translate(), 
					}, 
				};
			case EItemType.Decoration:
				return "装饰物".Translate();
			case EItemType.Turret:
				return "武器".Translate();
			case EItemType.Defense:
				return "防御设施".Translate();
			case EItemType.DarkFog:
				return "黑雾物品".Translate();
			case EItemType.Matrix:
				return "科学矩阵".Translate();
			default:
				return "其他分类".Translate();
			}
		}
	}

	public string fuelTypeString
	{
		get
		{
			string text = "";
			if ((FuelType & 1) == 1)
			{
				text += "化学".Translate();
			}
			if ((FuelType & 2) == 2)
			{
				if (!string.IsNullOrEmpty(text))
				{
					text += " / ";
				}
				text += "核能".Translate();
			}
			if ((FuelType & 4) == 4)
			{
				if (!string.IsNullOrEmpty(text))
				{
					text += " / ";
				}
				text += "质能".Translate();
			}
			if ((FuelType & 8) == 8)
			{
				if (!string.IsNullOrEmpty(text))
				{
					text += " / ";
				}
				text += "储存".Translate();
			}
			if ((FuelType & 0x10) == 16)
			{
				if (!string.IsNullOrEmpty(text))
				{
					text += " / ";
				}
				text += "X";
			}
			if (!string.IsNullOrEmpty(text))
			{
				return text;
			}
			return "-";
		}
	}

	public void RefreshTranslation()
	{
		base.name = Name.Translate();
		miningFrom = MiningFrom.Translate();
		produceFrom = ProduceFrom.Translate();
		description = Description.Translate();
		if (ID <= 6000 || ID >= 6100)
		{
			return;
		}
		for (int i = 0; i < PropertySystem.itemIds.Length; i++)
		{
			if (PropertySystem.itemIds[i] == ID)
			{
				propertyName = PropertySystem.itemNames[i];
				propertyName = propertyName.Translate();
				break;
			}
		}
	}

	public void Preload(int _index)
	{
		index = _index;
		if (ID >= 12000)
		{
			Debug.LogError("物品ID不能大于 " + 12000);
		}
		if (SubID >= 256)
		{
			Debug.LogError("物品SubID不能大于 " + 256);
		}
		base.name = Name.Translate();
		miningFrom = MiningFrom.Translate();
		produceFrom = ProduceFrom.Translate();
		description = Description.Translate();
		iconTagString = "\\" + IconTag + ";";
		iconNameTagString = "\\" + IconTag + "-;";
		if (!string.IsNullOrEmpty(IconPath))
		{
			_iconSprite = Resources.Load<Sprite>(IconPath);
		}
		int num = ModelIndex + ModelCount;
		for (int i = ModelIndex; i < num; i++)
		{
			ModelProto modelProto = LDB.models.modelArray[i];
			if (modelProto != null && modelProto.iconSprite == null && iconSprite != null)
			{
				modelProto.OverrideIconSprite(iconSprite);
			}
		}
		if (ID > 6000 && ID < 6100)
		{
			_propertyIconSprite = Resources.Load<Sprite>($"Icons/Property/property-icon-{ID}");
			_propertyIconSpriteSmall = Resources.Load<Sprite>($"Icons/Property/property-icon-40px-{ID}");
			propertyName = "";
			for (int j = 0; j < PropertySystem.itemIds.Length; j++)
			{
				if (PropertySystem.itemIds[j] == ID)
				{
					propertyName = PropertySystem.itemNames[j];
					propertyName = propertyName.Translate();
					break;
				}
			}
		}
		ModelProto modelProto2 = LDB.models.modelArray[ModelIndex];
		if (modelProto2 != null)
		{
			prefabDesc = modelProto2.prefabDesc;
		}
		else
		{
			prefabDesc = PrefabDesc.none;
		}
		if (prefabDesc.isCollectStation)
		{
			stationCollectorId = ID;
		}
		itemProtoById[ID] = this;
		FindRecipes();
		ComputeRawMats();
		FindPreTech();
	}

	public int GetUpgradeID(int upgrade)
	{
		if (Grade == 0 || Upgrades.Length == 0)
		{
			return ID;
		}
		int num = Grade + upgrade;
		if (num < 1)
		{
			num = 1;
		}
		else if (num > Upgrades.Length)
		{
			num = Upgrades.Length;
		}
		return Upgrades[num - 1];
	}

	public int GetGradeID(int grade)
	{
		if (Grade == 0 || Upgrades.Length == 0)
		{
			return ID;
		}
		if (grade < 1)
		{
			grade = 1;
		}
		else if (grade > Upgrades.Length)
		{
			grade = Upgrades.Length;
		}
		return Upgrades[grade - 1];
	}

	public ItemProto GetUpgradeItem(int upgrade)
	{
		if (Grade == 0 || Upgrades.Length == 0)
		{
			return this;
		}
		int num = Grade + upgrade;
		if (num < 1)
		{
			num = 1;
		}
		else if (num > Upgrades.Length)
		{
			num = Upgrades.Length;
		}
		return LDB.items.Select(Upgrades[num - 1]);
	}

	public ItemProto GetUpgradeItem(GameHistoryData history, int upgrade)
	{
		if (Grade == 0 || Upgrades.Length == 0)
		{
			return this;
		}
		int num = Grade + upgrade;
		if (num < 1)
		{
			num = 1;
		}
		else if (num > Upgrades.Length)
		{
			num = Upgrades.Length;
		}
		ItemProto itemProto = LDB.items.Select(Upgrades[num - 1]);
		if (itemProto != null && itemProto != this && (itemProto.ID == 2318 || itemProto.ID == 2319 || itemProto.ID == 2902) && !history.ItemUnlocked(itemProto.ID))
		{
			itemProto = LDB.items.Select(Upgrades[num - 2]);
		}
		if (itemProto != null)
		{
			return itemProto;
		}
		return this;
	}

	public ItemProto GetGradeItem(int grade)
	{
		if (Grade == 0 || Upgrades.Length == 0)
		{
			return this;
		}
		if (grade < 1)
		{
			grade = 1;
		}
		else if (grade > Upgrades.Length)
		{
			grade = Upgrades.Length;
		}
		return LDB.items.Select(Upgrades[grade - 1]);
	}

	public bool IsUpgradeOf(ItemProto other)
	{
		if (other == null)
		{
			return false;
		}
		if (ID == other.ID)
		{
			return true;
		}
		for (int i = 0; i < Upgrades.Length; i++)
		{
			if (Upgrades[i] == other.ID)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsUpgradeOf(int other)
	{
		if (other == 0)
		{
			return false;
		}
		if (ID == other)
		{
			return true;
		}
		for (int i = 0; i < Upgrades.Length; i++)
		{
			if (Upgrades[i] == other)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsUpgradeOf(int itemProtoIdA, int itemProtoIdB)
	{
		ItemProto itemProto = LDB.items.Select(itemProtoIdA);
		ItemProto other = LDB.items.Select(itemProtoIdB);
		return itemProto?.IsUpgradeOf(other) ?? false;
	}

	public bool IsSimilar(ItemProto other)
	{
		if (other == null)
		{
			return false;
		}
		if (ID == other.ID)
		{
			return true;
		}
		for (int i = 0; i < Upgrades.Length; i++)
		{
			if (Upgrades[i] == other.ID)
			{
				return true;
			}
		}
		return false;
	}

	private void FindRecipes()
	{
		if (recipes != null)
		{
			return;
		}
		isRaw = true;
		recipes = new List<RecipeProto>(4);
		handcrafts = new List<RecipeProto>(4);
		makes = new List<RecipeProto>(4);
		handcraft = null;
		maincraft = null;
		RecipeProto[] dataArray = LDB.recipes.dataArray;
		int num = dataArray.Length;
		int num2 = 100;
		int num3 = 100;
		for (int i = 0; i < num; i++)
		{
			RecipeProto recipeProto = dataArray[i];
			int[] results = recipeProto.Results;
			for (int j = 0; j < results.Length; j++)
			{
				if (results[j] != ID)
				{
					continue;
				}
				recipes.Add(recipeProto);
				if (j < num2)
				{
					num2 = j;
					maincraft = recipeProto;
					maincraftProductCount = recipeProto.ResultCounts[j];
				}
				if (recipeProto.Handcraft)
				{
					handcrafts.Add(recipeProto);
					isRaw = false;
					if (j < num3)
					{
						num3 = j;
						handcraft = recipeProto;
						handcraftProductCount = recipeProto.ResultCounts[j];
					}
				}
				break;
			}
			int[] items = recipeProto.Items;
			for (int k = 0; k < items.Length; k++)
			{
				if (items[k] == ID)
				{
					makes.Add(recipeProto);
					break;
				}
			}
		}
		ComputeRawMats();
	}

	private void ComputeRawMats()
	{
		if (rawMats != null)
		{
			return;
		}
		rawMats = new List<IDCNTINC>();
		int num = 0;
		while (maincraft != null && num < maincraft.Items.Length)
		{
			ItemProto itemProto = LDB.items.Select(maincraft.Items[num]);
			if (itemProto != null)
			{
				if (itemProto.recipes == null)
				{
					itemProto.FindRecipes();
				}
				if (itemProto.isRaw)
				{
					_add_raw_mat(maincraft.Items[num], maincraft.ItemCounts[num]);
				}
				else
				{
					List<IDCNTINC> list = itemProto.rawMats;
					for (int i = 0; i < list.Count; i++)
					{
						_add_raw_mat(list[i].id, list[i].count * maincraft.ItemCounts[num]);
					}
				}
			}
			num++;
		}
	}

	private void _add_raw_mat(int id, int cnt)
	{
		for (int i = 0; i < rawMats.Count; i++)
		{
			if (id == rawMats[i].id)
			{
				rawMats[i] = new IDCNTINC(id, cnt + rawMats[i].count, 0);
				return;
			}
		}
		rawMats.Add(new IDCNTINC(id, cnt, 0));
	}

	public static void InitFuelNeeds()
	{
		ItemProto[] dataArray = LDB.items.dataArray;
		List<int> list = new List<int>();
		for (int i = 0; i < fuelNeeds.Length; i++)
		{
			list.Clear();
			ItemProto[] array = dataArray;
			foreach (ItemProto itemProto in array)
			{
				if ((i & itemProto.FuelType) != 0)
				{
					list.Add(itemProto.ID);
				}
			}
			fuelNeeds[i] = list.ToArray();
		}
		list.Clear();
	}

	public static void InitTurretNeeds()
	{
		ItemProto[] dataArray = LDB.items.dataArray;
		List<int> list = new List<int>();
		for (int i = 1; i < turretNeeds.Length; i++)
		{
			list.Clear();
			ItemProto[] array = dataArray;
			foreach (ItemProto itemProto in array)
			{
				if (i == (int)itemProto.AmmoType)
				{
					list.Add(itemProto.ID);
				}
			}
			if (list.Count > 0)
			{
				for (int k = list.Count; k < 6; k++)
				{
					list.Add(0);
				}
			}
			turretNeeds[i] = list.ToArray();
		}
		list.Clear();
	}

	public static bool isFluid(int itemId)
	{
		int num = fluids.Length;
		for (int i = 0; i < num; i++)
		{
			if (fluids[i] == itemId)
			{
				return true;
			}
		}
		return false;
	}

	public static void InitFluids()
	{
		ItemProto[] dataArray = LDB.items.dataArray;
		List<int> list = new List<int>();
		for (int i = 0; i < dataArray.Length; i++)
		{
			if (dataArray[i].IsFluid)
			{
				list.Add(dataArray[i].ID);
			}
		}
		fluids = list.ToArray();
	}

	public static void InitTurrets()
	{
		ItemProto[] dataArray = LDB.items.dataArray;
		List<int> list = new List<int>();
		for (int i = 0; i < dataArray.Length; i++)
		{
			if (dataArray[i].Type == EItemType.Turret)
			{
				list.Add(dataArray[i].ID);
			}
		}
		turrets = list.ToArray();
	}

	public static void InitEnemyDropTables()
	{
		enemyDropRangeTable = new int[10001];
		enemyDropLevelTable = new int[12000];
		enemyDropCountTable = new float[12000];
		enemyDropMaskTable = new int[12000];
		enemyDropMaskRatioTable = new float[12000];
		for (int i = 0; i < 12000; i++)
		{
			enemyDropLevelTable[i] = -1;
			enemyDropCountTable[i] = 0f;
			enemyDropMaskTable[i] = 0;
			enemyDropMaskRatioTable[i] = 1f;
		}
		ItemProto[] dataArray = LDB.items.dataArray;
		for (int j = 0; j < dataArray.Length; j++)
		{
			int iD = dataArray[j].ID;
			if (dataArray[j].EnemyDropRange.y > 5E-05f)
			{
				enemyDropLevelTable[iD] = dataArray[j].EnemyDropLevel;
				enemyDropCountTable[iD] = dataArray[j].EnemyDropCount;
				enemyDropMaskTable[iD] = dataArray[j].EnemyDropMask;
				enemyDropMaskRatioTable[iD] = dataArray[j].EnemyDropMaskRatio;
				int num = (int)(dataArray[j].EnemyDropRange.x * 10000f + 0.5f);
				int num2 = (int)((dataArray[j].EnemyDropRange.x + dataArray[j].EnemyDropRange.y) * 10000f + 0.5f);
				for (int k = num; k < num2; k++)
				{
					enemyDropRangeTable[k] = iD;
				}
			}
		}
	}

	public static void InitConstructableItems()
	{
		ItemProto[] dataArray = LDB.items.dataArray;
		constructableIdHash = new HashSet<int>();
		constructableIndiceById = new int[12000];
		for (int i = 0; i < 12000; i++)
		{
			constructableIndiceById[i] = -1;
		}
		int num = 0;
		List<int> list = new List<int>();
		for (int j = 0; j < dataArray.Length; j++)
		{
			if (dataArray[j].CanBuild && dataArray[j].ID != 1131)
			{
				list.Add(dataArray[j].ID);
				constructableIdHash.Add(dataArray[j].ID);
				constructableIndiceById[dataArray[j].ID] = num++;
			}
		}
		constructableCount = num;
		constructableIds = list.ToArray();
	}

	public static void InitItemIds()
	{
		ItemProto[] dataArray = LDB.items.dataArray;
		List<int> list = new List<int>();
		for (int i = 0; i < dataArray.Length; i++)
		{
			list.Add(dataArray[i].ID);
		}
		list.Add(11901);
		list.Add(11902);
		list.Add(11903);
		itemIds = list.ToArray();
	}

	public static void InitItemIndices()
	{
		ItemProto[] dataArray = LDB.items.dataArray;
		itemIndices = new int[12000];
		for (int i = 0; i < 12000; i++)
		{
			itemIndices[i] = 9999;
		}
		for (int j = 0; j < dataArray.Length; j++)
		{
			itemIndices[dataArray[j].ID] = dataArray[j].index;
		}
	}

	public static void InitMechaMaterials()
	{
		int[] array = new int[128];
		int num = 0;
		ItemProto[] dataArray = LDB.items.dataArray;
		for (int i = 0; i < dataArray.Length; i++)
		{
			if (dataArray[i].ID > 0 && dataArray[i].MechaMaterialID > 0)
			{
				array[dataArray[i].MechaMaterialID] = dataArray[i].ID;
				num++;
			}
		}
		mechaMaterials = new int[num];
		int num2 = 0;
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j] > 0)
			{
				mechaMaterials[num2++] = array[j];
			}
		}
	}

	public static void InitFighterIndices()
	{
		ItemProto[] dataArray = LDB.items.dataArray;
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		List<int> list3 = new List<int>();
		List<int> list4 = new List<int>();
		List<int> list5 = new List<int>();
		for (int i = 0; i < dataArray.Length; i++)
		{
			PrefabDesc obj = dataArray[i].prefabDesc;
			int iD = dataArray[i].ID;
			if (obj.isCraftUnit)
			{
				list.Add(iD);
				if (dataArray[i].isSpaceCraft)
				{
					list3.Add(iD);
				}
				else
				{
					list2.Add(iD);
				}
				if (dataArray[i].isSmallSpaceFighter)
				{
					list4.Add(iD);
				}
				else if (dataArray[i].isLargeSpaceFighter)
				{
					list5.Add(iD);
				}
			}
		}
		kFighterIds = list.ToArray();
		kFighterGroundIds = list2.ToArray();
		kFighterSpaceIds = list3.ToArray();
		kFighterSpaceSmallIds = list4.ToArray();
		kFighterSpaceLargeIds = list5.ToArray();
	}

	public static void InitPowerFacilityIndices()
	{
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		list.Add(0);
		list2.Add(0);
		powerGenId2Index = new int[12000];
		powerConId2Index = new int[12000];
		ItemProto[] dataArray = LDB.items.dataArray;
		foreach (ItemProto itemProto in dataArray)
		{
			if (itemProto == null)
			{
				continue;
			}
			PrefabDesc obj = itemProto.prefabDesc;
			if (obj.isPowerGen && !list.Contains(itemProto.ID))
			{
				list.Add(itemProto.ID);
				powerGenId2Index[itemProto.ID] = list.Count - 1;
			}
			if (obj.isPowerConsumer && !list2.Contains(itemProto.ID))
			{
				list2.Add(itemProto.ID);
				powerConId2Index[itemProto.ID] = list2.Count - 1;
			}
			if (obj.isPowerCharger && !list2.Contains(itemProto.ID))
			{
				list2.Add(itemProto.ID);
				powerConId2Index[itemProto.ID] = list2.Count - 1;
			}
			if (obj.isAccumulator)
			{
				if (!list.Contains(itemProto.ID))
				{
					list.Add(itemProto.ID);
					powerGenId2Index[itemProto.ID] = list.Count - 1;
				}
				if (!list2.Contains(itemProto.ID))
				{
					list2.Add(itemProto.ID);
					powerConId2Index[itemProto.ID] = list2.Count - 1;
				}
			}
			if (obj.isPowerExchanger)
			{
				if (!list.Contains(itemProto.ID))
				{
					list.Add(itemProto.ID);
					powerGenId2Index[itemProto.ID] = list.Count - 1;
				}
				if (!list2.Contains(itemProto.ID))
				{
					list2.Add(itemProto.ID);
					powerConId2Index[itemProto.ID] = list2.Count - 1;
				}
			}
		}
		powerGenIndex2Id = list.ToArray();
		powerConIndex2Id = list2.ToArray();
		list = null;
		list2 = null;
	}

	public void FindPreTech()
	{
		if (PreTechOverride > 0)
		{
			missingTech = false;
			preTech = LDB.techs.Select(PreTechOverride);
		}
		else if (UnlockKey == -2)
		{
			missingTech = false;
			preTech = null;
		}
		else if (maincraft != null)
		{
			for (int i = 0; i < LDB.techs.Length; i++)
			{
				TechProto techProto = LDB.techs.dataArray[i];
				for (int j = 0; j < techProto.UnlockRecipes.Length; j++)
				{
					if (techProto.UnlockRecipes[j] == maincraft.ID)
					{
						missingTech = false;
						preTech = techProto;
						return;
					}
				}
			}
			missingTech = true;
			preTech = null;
		}
		else if (Type == EItemType.Resource)
		{
			missingTech = false;
			preTech = null;
		}
		else
		{
			missingTech = true;
			preTech = null;
		}
	}

	public static void InitProductionMask()
	{
		RecipeProto[] dataArray = LDB.recipes.dataArray;
		foreach (RecipeProto recipeProto in dataArray)
		{
			if (recipeProto.Type == ERecipeType.None)
			{
				continue;
			}
			int[] items = recipeProto.Items;
			foreach (int id in items)
			{
				ItemProto itemProto = LDB.items.Select(id);
				if (itemProto != null)
				{
					if ((int)recipeProto.Type <= 5)
					{
						itemProto.consumptionMask |= 1;
					}
					else if (recipeProto.Type == ERecipeType.Fractionate)
					{
						itemProto.consumptionMask |= 8;
					}
					else if (recipeProto.Type == ERecipeType.Research)
					{
						itemProto.consumptionMask |= 2;
					}
				}
			}
			int[] results = recipeProto.Results;
			foreach (int id2 in results)
			{
				ItemProto itemProto2 = LDB.items.Select(id2);
				if (itemProto2 != null)
				{
					if ((int)recipeProto.Type <= 5)
					{
						itemProto2.productionMask |= 1;
					}
					else if (recipeProto.Type == ERecipeType.Fractionate)
					{
						itemProto2.productionMask |= 8;
					}
					else if (recipeProto.Type == ERecipeType.Research)
					{
						itemProto2.productionMask |= 2;
					}
				}
			}
		}
		for (int l = 0; l < LabComponent.matrixIds.Length; l++)
		{
			int id3 = LabComponent.matrixIds[l];
			ItemProto itemProto3 = LDB.items.Select(id3);
			if (itemProto3 != null)
			{
				itemProto3.consumptionMask |= 2;
			}
		}
		VeinProto[] dataArray2 = LDB.veins.dataArray;
		for (int m = 0; m < dataArray2.Length; m++)
		{
			int miningItem = dataArray2[m].MiningItem;
			ItemProto itemProto4 = LDB.items.Select(miningItem);
			if (itemProto4 != null)
			{
				itemProto4.productionMask |= 4;
			}
		}
		ItemProto[] dataArray3 = LDB.items.dataArray;
		for (int n = 0; n < dataArray3.Length; n++)
		{
			PrefabDesc prefabDesc = dataArray3[n].prefabDesc;
			if (prefabDesc.isEjector)
			{
				int ejectorBulletId = prefabDesc.ejectorBulletId;
				ItemProto itemProto5 = LDB.items.Select(ejectorBulletId);
				if (itemProto5 != null)
				{
					itemProto5.consumptionMask |= 16;
				}
			}
			if (prefabDesc.isSilo)
			{
				int siloBulletId = prefabDesc.siloBulletId;
				ItemProto itemProto6 = LDB.items.Select(siloBulletId);
				if (itemProto6 != null)
				{
					itemProto6.consumptionMask |= 32;
				}
			}
			if (prefabDesc.isPowerGen)
			{
				if (prefabDesc.fuelMask > 0)
				{
					int[] array = fuelNeeds[prefabDesc.fuelMask];
					foreach (int id4 in array)
					{
						ItemProto itemProto7 = LDB.items.Select(id4);
						if (itemProto7 != null)
						{
							itemProto7.consumptionMask |= 64;
						}
					}
				}
				if (prefabDesc.powerProductId > 0)
				{
					int powerProductId = prefabDesc.powerProductId;
					ItemProto itemProto8 = LDB.items.Select(powerProductId);
					if (itemProto8 != null)
					{
						itemProto8.productionMask |= 64;
					}
				}
				if (prefabDesc.powerCatalystId > 0)
				{
					int powerCatalystId = prefabDesc.powerCatalystId;
					ItemProto itemProto9 = LDB.items.Select(powerCatalystId);
					if (itemProto9 != null)
					{
						itemProto9.consumptionMask |= 64;
					}
				}
			}
			if (!prefabDesc.isSpraycoster)
			{
				continue;
			}
			int[] incItemId = prefabDesc.incItemId;
			foreach (int id5 in incItemId)
			{
				ItemProto itemProto10 = LDB.items.Select(id5);
				if (itemProto10 != null)
				{
					itemProto10.consumptionMask |= 256;
				}
			}
		}
		ThemeProto[] dataArray4 = LDB.themes.dataArray;
		for (int num3 = 0; num3 < dataArray4.Length; num3++)
		{
			if (dataArray4[num3].PlanetType == EPlanetType.Gas)
			{
				int[] gasItems = dataArray4[num3].GasItems;
				foreach (int id6 in gasItems)
				{
					ItemProto itemProto11 = LDB.items.Select(id6);
					if (itemProto11 != null)
					{
						itemProto11.productionMask |= 128;
					}
				}
			}
			if (dataArray4[num3].WaterItemId > 0)
			{
				int waterItemId = dataArray4[num3].WaterItemId;
				ItemProto itemProto12 = LDB.items.Select(waterItemId);
				if (itemProto12 != null)
				{
					itemProto12.productionMask |= 4;
				}
			}
		}
	}

	public string GetPropName(int index)
	{
		if ((uint)index < DescFields.Length)
		{
			switch (DescFields[index])
			{
			default:
				return "??";
			case 0:
				return "采集自".Translate();
			case 1:
				return "制造于".Translate();
			case 2:
				return "燃料类型".Translate();
			case 3:
				return "能量".Translate();
			case 4:
				return "发电类型".Translate();
			case 5:
				return "发电功率".Translate();
			case 6:
				return "热效率".Translate();
			case 7:
				return "流体消耗".Translate();
			case 8:
				return "输入功率".Translate();
			case 9:
				return "输出功率".Translate();
			case 10:
				return "蓄电量".Translate();
			case 11:
				return "工作功率".Translate();
			case 12:
				return "待机功率".Translate();
			case 13:
				return "连接长度".Translate();
			case 14:
				return "覆盖范围".Translate();
			case 15:
				return "运载速度".Translate();
			case 16:
				return "接口数量".Translate();
			case 17:
				return "仓储空间".Translate();
			case 18:
				return "开采对象".Translate();
			case 19:
				return "开采速度".Translate();
			case 20:
				if (prefabDesc.inserterGrade != 4)
				{
					return "运送速度".Translate();
				}
				if (!GameMain.history.inserterBidirectional)
				{
					return "单程耗时".Translate();
				}
				return "传输速度".Translate();
			case 21:
				return "单次拾取货物".Translate();
			case 22:
				return "制造速度".Translate();
			case 23:
				return "血量".Translate();
			case 24:
				return "仓储物品".Translate();
			case 25:
				return "船运载量".Translate();
			case 26:
				return "船运载量".Translate();
			case 27:
				return "飞行速度".Translate();
			case 28:
				return "制造加速".Translate();
			case 29:
				return "喷涂次数".Translate();
			case 30:
				return "流体容量".Translate();
			case 31:
				return "机甲功率提升".Translate();
			case 32:
				return "采集速度".Translate();
			case 33:
				return "研究速度".Translate();
			case 34:
				return "弹射速度".Translate();
			case 35:
				return "发射速度".Translate();
			case 36:
				return "使用寿命".Translate();
			case 37:
				return "潜在能量".Translate();
			case 38:
				return "最大充能功率".Translate();
			case 39:
				return "基础发电功率".Translate();
			case 40:
				return "增产剂效果".Translate();
			case 41:
				return "喷涂增产效果".Translate();
			case 42:
				return "喷涂加速效果".Translate();
			case 43:
				return "额外电力消耗".Translate();
			case 44:
				return "配送范围".Translate();
			case 45:
				return "船运载量".Translate();
			case 46:
				return "飞行速度".Translate();
			case 47:
				return "弹药数量".Translate();
			case 48:
				return "伤害".Translate();
			case 49:
				return "射击速度".Translate();
			case 50:
				return "每秒伤害".Translate();
			case 51:
				return "防御塔类型".Translate();
			case 52:
				return "弹药类型".Translate();
			case 53:
				return "最大耐久度".Translate();
			case 54:
				return "手动制造速度".Translate();
			case 55:
				return "伤害类型".Translate();
			case 56:
				return "目标类型".Translate();
			case 57:
				return "对地防御范围".Translate();
			case 58:
				return "对天防御范围".Translate();
			case 59:
				return "飞行速度".Translate();
			case 60:
				return "射击距离".Translate();
			case 61:
				return "爆炸半径".Translate();
			case 62:
				return "主炮伤害".Translate();
			case 63:
				return "主炮射速".Translate();
			case 64:
				return "主炮射程".Translate();
			case 65:
				return "舰炮伤害".Translate();
			case 66:
				return "舰炮射速".Translate();
			case 67:
				return "舰炮射程".Translate();
			case 68:
				return "干扰单位".Translate();
			case 69:
				return "干扰效果".Translate();
			case 70:
				return "干扰时间".Translate();
			case 71:
				return "可投掷".Translate();
			case 72:
				return "单次拾取货物".Translate();
			case 73:
				return "输出堆叠层数".Translate();
			}
		}
		return "";
	}

	public string GetPropValue(int index, StringBuilder sb, int incLevel)
	{
		incLevel = ((incLevel > 0) ? ((incLevel > 10) ? 10 : incLevel) : 0);
		double num = Cargo.incTableMilli[incLevel] + 1.0;
		double num2 = Cargo.accTableMilli[incLevel] + 1.0;
		if ((uint)index < DescFields.Length)
		{
			switch (DescFields[index])
			{
			default:
				return "-";
			case 0:
				if (!string.IsNullOrEmpty(miningFrom))
				{
					return miningFrom;
				}
				return "-";
			case 1:
				if (maincraft != null)
				{
					return maincraft.madeFromString;
				}
				if (!string.IsNullOrEmpty(produceFrom))
				{
					return produceFrom;
				}
				return "-";
			case 2:
				return fuelTypeString;
			case 3:
			{
				long valuel = (long)((double)HeatValue * (Productive ? num : 1.0) + 0.1);
				StringBuilderUtility.WriteKMG(sb, 8, valuel, blank: true);
				if (Productive && incLevel > 0)
				{
					return "<color=#61D8FFB8>" + sb.ToString().TrimStart() + "J</color>";
				}
				return sb.ToString().TrimStart() + "J";
			}
			case 4:
				if (prefabDesc.isPowerGen)
				{
					if (prefabDesc.windForcedPower)
					{
						return "风能".Translate();
					}
					if (prefabDesc.photovoltaic)
					{
						return "光伏".Translate();
					}
					if (prefabDesc.gammaRayReceiver)
					{
						return "离子流".Translate();
					}
					if (prefabDesc.fuelMask <= 1 && prefabDesc.fuelMask > 0)
					{
						return "火力".Translate();
					}
					if (prefabDesc.geothermal)
					{
						return "地热".Translate();
					}
					return "离子流".Translate();
				}
				return "-";
			case 5:
			case 39:
				StringBuilderUtility.WriteKMG(sb, 8, prefabDesc.genEnergyPerTick * 60, blank: true);
				return sb.ToString().TrimStart() + "W";
			case 6:
				if (prefabDesc.useFuelPerTick > 0)
				{
					return ((float)prefabDesc.genEnergyPerTick / (float)prefabDesc.useFuelPerTick).ToString("0.# %");
				}
				return "0";
			case 7:
				return "-";
			case 8:
				if (prefabDesc.exchangeEnergyPerTick > 0)
				{
					StringBuilderUtility.WriteKMG(sb, 8, prefabDesc.exchangeEnergyPerTick * 60, blank: true);
				}
				else
				{
					StringBuilderUtility.WriteKMG(sb, 8, prefabDesc.inputEnergyPerTick * 60, blank: true);
				}
				return sb.ToString().TrimStart() + "W";
			case 9:
				if (prefabDesc.exchangeEnergyPerTick > 0)
				{
					StringBuilderUtility.WriteKMG(sb, 8, prefabDesc.exchangeEnergyPerTick * 60, blank: true);
				}
				else
				{
					StringBuilderUtility.WriteKMG(sb, 8, prefabDesc.outputEnergyPerTick * 60, blank: true);
				}
				return sb.ToString().TrimStart() + "W";
			case 10:
				StringBuilderUtility.WriteKMG(sb, 8, prefabDesc.maxAcuEnergy + prefabDesc.stationMaxEnergyAcc + prefabDesc.dispenserMaxEnergyAcc, blank: true);
				return sb.ToString().TrimStart() + "J";
			case 11:
				StringBuilderUtility.WriteKMG(sb, 8, prefabDesc.workEnergyPerTick * 60, blank: true);
				return sb.ToString().TrimStart() + "W";
			case 12:
				StringBuilderUtility.WriteKMG(sb, 8, prefabDesc.idleEnergyPerTick * 60, blank: true);
				return sb.ToString().TrimStart() + "W";
			case 13:
				if (prefabDesc.powerConnectDistance > 0f)
				{
					return (prefabDesc.powerConnectDistance - 0.5f).ToString("0.##") + " m";
				}
				return "0";
			case 14:
				if (prefabDesc.powerCoverRadius > 0f && prefabDesc.beaconSignalRadius > 0f)
				{
					return "电力".Translate() + " " + (prefabDesc.powerCoverRadius - 0.5f).ToString("0.##") + " m  /  " + "信号".Translate() + " " + prefabDesc.beaconSignalRadius.ToString("0.##") + " m";
				}
				if (prefabDesc.powerCoverRadius > 0f)
				{
					return (prefabDesc.powerCoverRadius - 0.5f).ToString("0.##") + " m";
				}
				if (prefabDesc.battleBasePickRange > 0f && prefabDesc.constructionRange > 0f)
				{
					return "建造".Translate() + " " + prefabDesc.constructionRange.ToString("0.##") + " m  /  " + "拾取".Translate() + " " + prefabDesc.battleBasePickRange.ToString("0.##") + " m";
				}
				return "0";
			case 15:
				if (prefabDesc.isBelt)
				{
					return ((double)prefabDesc.beltSpeed * 60.0 / 10.0).ToString("0.##") + "/s";
				}
				return "0";
			case 16:
				if (prefabDesc.portPoses.Length != 0)
				{
					return prefabDesc.portPoses.Length.ToString("0");
				}
				return "0";
			case 17:
				if (prefabDesc.isStorage)
				{
					return prefabDesc.storageRow * prefabDesc.storageCol + "仓储空间的后缀".Translate();
				}
				return "0";
			case 18:
				if (prefabDesc.minerType == EMinerType.Water)
				{
					return "水源".Translate();
				}
				if (prefabDesc.minerType == EMinerType.Vein)
				{
					return "矿脉".Translate();
				}
				if (prefabDesc.minerType == EMinerType.Oil)
				{
					return "油田".Translate();
				}
				if (prefabDesc.isCollectStation)
				{
					return "气态行星".Translate();
				}
				return "-";
			case 19:
				if (prefabDesc.minerType == EMinerType.Vein)
				{
					return (60.0 / ((double)prefabDesc.minerPeriod / 600000.0) * (double)GameMain.history.miningSpeedScale).ToString("0.#") + "每分每矿脉".Translate();
				}
				if (prefabDesc.minerType == EMinerType.Oil)
				{
					return GameMain.history.miningSpeedScale.ToString("0.##") + "x";
				}
				if (prefabDesc.minerType == EMinerType.Water)
				{
					return (60.0 / ((double)prefabDesc.minerPeriod / 600000.0) * (double)GameMain.history.miningSpeedScale).ToString("0.#") + "/min";
				}
				return "-";
			case 20:
				if (prefabDesc.isInserter)
				{
					if (prefabDesc.inserterGrade != 4)
					{
						return (300000.0 / (double)prefabDesc.inserterSTT).ToString("0.0") + "往返每秒每格".Translate();
					}
					if (!GameMain.history.inserterBidirectional)
					{
						return ((double)prefabDesc.inserterSTT / 600000.0).ToString("0.00") + "单程每格耗时".Translate();
					}
					return "<color=#61D8FFB8>120" + "每秒运送货物".Translate() + "</color>";
				}
				return "";
			case 21:
			{
				int num23 = ((prefabDesc.inserterGrade != 3) ? 1 : GameMain.history.inserterStackCountObsolete);
				if (num23 <= 1)
				{
					return "不支持".Translate();
				}
				return num23.ToString();
			}
			case 22:
			case 34:
			case 35:
				if (prefabDesc.isAssembler)
				{
					return ((double)prefabDesc.assemblerSpeed / 10000.0).ToString("0.###") + "x";
				}
				if (prefabDesc.isLab)
				{
					return ((double)prefabDesc.labAssembleSpeed / 10000.0).ToString("0.###") + "x";
				}
				if (prefabDesc.isEjector)
				{
					return (3600.0 / (double)(prefabDesc.ejectorChargeFrame + prefabDesc.ejectorColdFrame)).ToString("0.##") + "/min";
				}
				if (prefabDesc.isSilo)
				{
					return (3600.0 / (double)(prefabDesc.siloChargeFrame + prefabDesc.siloColdFrame)).ToString("0.##") + "/min";
				}
				return "-";
			case 23:
				return HpMax.ToString("0");
			case 24:
				return prefabDesc.stationMaxItemKinds + "仓储物品种类后缀".Translate();
			case 25:
				if (GameMain.history == null)
				{
					return "-";
				}
				return GameMain.history.logisticDroneCarries.ToString();
			case 26:
				if (GameMain.history == null)
				{
					return "-";
				}
				return GameMain.history.logisticShipCarries.ToString();
			case 27:
				if (GameMain.history == null)
				{
					return "-";
				}
				return GameMain.history.logisticDroneSpeedModified.ToString("0.###") + " m/s";
			case 28:
				return Ability + " %";
			case 29:
			{
				int num4 = (int)((double)HpMax * num + 0.1);
				if (incLevel > 0)
				{
					return "<color=#61D8FFB8>" + num4 + "</color>" + "喷涂次数的后缀".Translate();
				}
				return HpMax + "喷涂次数的后缀".Translate();
			}
			case 30:
				return prefabDesc.fluidStorageCount.ToString("#,##0");
			case 31:
			{
				float num18 = ReactorInc + 1f;
				num18 *= (float)(Productive ? num : num2);
				num18--;
				if (incLevel > 0)
				{
					if (Productive)
					{
						return "<color=#61D8FFB8>" + ((num18 > 0f) ? num18.ToString("+0.###%") : num18.ToString("0.###%")) + "</color>";
					}
					return "<color=#FD965EB8>" + ((num18 > 0f) ? num18.ToString("+0.###%") : num18.ToString("0.###%")) + "</color>";
				}
				if (!(num18 > 0f))
				{
					return num18.ToString("0%");
				}
				return num18.ToString("+0%");
			}
			case 32:
				return ((float)prefabDesc.stationCollectSpeed * GameMain.history.miningSpeedScale).ToString("0.0###") + "x";
			case 33:
				return prefabDesc.labResearchSpeed * 60f * (float)GameMain.history.techSpeed + " Hash/s";
			case 36:
				return GameMain.history.solarSailLife + "空格秒".Translate();
			case 37:
				StringBuilderUtility.WriteKMG(sb, 8, Potential, blank: true);
				return sb.ToString().TrimStart() + "J";
			case 38:
				StringBuilderUtility.WriteKMG(sb, 8, prefabDesc.workEnergyPerTick * 60 * 5, blank: true);
				return sb.ToString().TrimStart() + "W";
			case 40:
				if (Productive)
				{
					return "额外产出".Translate();
				}
				return "加速生产".Translate();
			case 41:
				return "+" + ((float)Cargo.incTable[Ability] * 0.1f).ToString("0.0") + "%";
			case 42:
				return "+" + ((float)Cargo.accTable[Ability] * 0.1f).ToString("0.0") + "%";
			case 43:
				return "+" + ((float)Cargo.powerTable[Ability] * 0.1f).ToString("0.0") + "%";
			case 44:
				if (GameMain.history == null)
				{
					return "-";
				}
				return GameMain.history.dispenserDeliveryMaxAngle.ToString("0.##") + "°";
			case 45:
				if (GameMain.history == null)
				{
					return "-";
				}
				return GameMain.history.logisticCourierCarries.ToString();
			case 46:
				if (GameMain.history == null)
				{
					return "-";
				}
				return GameMain.history.logisticCourierSpeedModified.ToString("0.0#") + " m/s";
			case 47:
			{
				int num7 = (int)((double)HpMax * num + ((HpMax < 12) ? 0.51 : 0.1));
				string text = (((int)AmmoType > 0) ? "弹药数量单位".Translate() : "");
				if (incLevel > 0)
				{
					return "<color=#61D8FFB8>" + num7 + "</color>" + text;
				}
				return HpMax + text;
			}
			case 48:
				if (isAmmo)
				{
					int ability4 = Ability;
					double num21 = 0.0;
					GameHistoryData history9 = GameMain.history;
					if (history9 != null)
					{
						num21 = AmmoType switch
						{
							EAmmoType.Bullet => (float)ability4 * history9.kineticDamageScale - (float)ability4, 
							EAmmoType.Cannon => (float)ability4 * (history9.kineticDamageScale * 0.5f + history9.blastDamageScale * 0.5f) - (float)ability4, 
							EAmmoType.Plasma => (float)ability4 * history9.energyDamageScale - (float)ability4, 
							EAmmoType.Missile => (float)ability4 * history9.blastDamageScale - (float)ability4, 
							_ => 0.0, 
						};
						ability4 /= 100;
						num21 /= 100.0;
						if (num21 != 0.0)
						{
							return ability4.ToString("0.0") + "<color=#61D8FFB8> + " + num21.ToString("0.0#") + "</color> hp";
						}
						return ability4.ToString("0.0") + " hp";
					}
					return (ability4 / 100).ToString("0.0") + " hp";
				}
				if (prefabDesc.isCraftUnit)
				{
					int craftUnitAttackDamage2 = prefabDesc.craftUnitAttackDamage0;
					float num22 = 0f;
					GameHistoryData history10 = GameMain.history;
					if (history10 != null)
					{
						num22 = (float)craftUnitAttackDamage2 * history10.energyDamageScale;
						num22 = (isSpaceCraft ? (num22 * history10.combatShipDamageRatio - (float)craftUnitAttackDamage2) : (num22 * history10.combatDroneDamageRatio - (float)craftUnitAttackDamage2));
						craftUnitAttackDamage2 /= 100;
						num22 /= 100f;
						if (num22 != 0f)
						{
							return craftUnitAttackDamage2.ToString("0.0") + "<color=#61D8FFB8> + " + num22.ToString("0.0#") + "</color> hp";
						}
						return craftUnitAttackDamage2.ToString("0.0") + " hp";
					}
					return (craftUnitAttackDamage2 / 100).ToString("0.0") + " hp";
				}
				return "";
			case 49:
				if (prefabDesc.isTurret)
				{
					if (prefabDesc.turretMuzzleCount <= 1)
					{
						return ((float)prefabDesc.turretROF * 60f / (float)prefabDesc.turretRoundInterval).ToString("0.##") + "发每秒".Translate();
					}
					return ((float)prefabDesc.turretROF * 60f * (float)(int)prefabDesc.turretMuzzleCount / (float)(prefabDesc.turretRoundInterval + prefabDesc.turretMuzzleInterval * (prefabDesc.turretMuzzleCount - 1))).ToString("0.##") + "发每秒".Translate();
				}
				if (prefabDesc.isCraftUnit)
				{
					float num12 = ((prefabDesc.craftUnitMuzzleCount0 > 1) ? ((float)prefabDesc.craftUnitROF0 * 60f * (float)prefabDesc.craftUnitMuzzleCount0 / (float)(prefabDesc.craftUnitRoundInterval0 + prefabDesc.craftUnitMuzzleInterval0 * (prefabDesc.craftUnitMuzzleCount0 - 1))) : ((float)prefabDesc.craftUnitROF0 * 60f / (float)prefabDesc.craftUnitRoundInterval0));
					GameHistoryData history5 = GameMain.history;
					if (history5 != null)
					{
						float num13 = (isSpaceCraft ? (num12 * history5.combatShipROFRatio - num12) : (num12 * history5.combatDroneROFRatio - num12));
						if (num13 != 0f)
						{
							return num12.ToString("0.##") + "<color=#61D8FFB8> + " + num13.ToString("0.##") + "</color>" + "发每秒".Translate();
						}
						return num12.ToString("0.##") + "发每秒".Translate();
					}
					return num12.ToString("0.##") + "发每秒".Translate();
				}
				return "";
			case 50:
				return (100f * prefabDesc.turretDamageScale * GameMain.history.energyDamageScale * 0.6f).ToString("0.0#") + " hp";
			case 51:
				return prefabDesc.turretType switch
				{
					ETurretType.Gauss => "动能武器".Translate(), 
					ETurretType.Laser => "能量武器".Translate(), 
					ETurretType.Cannon => "动能加爆破武器".Translate(), 
					ETurretType.Plasma => "能量武器".Translate(), 
					ETurretType.Missile => "爆破武器".Translate(), 
					ETurretType.LocalPlasma => "能量武器".Translate(), 
					ETurretType.Disturb => "电磁武器".Translate(), 
					_ => "-", 
				};
			case 52:
				return (prefabDesc.isTurret ? prefabDesc.turretAmmoType : AmmoType) switch
				{
					EAmmoType.Bullet => "子弹".Translate(), 
					EAmmoType.Cannon => "炮弹".Translate(), 
					EAmmoType.Plasma => "能量胶囊".Translate(), 
					EAmmoType.Missile => "导弹".Translate(), 
					EAmmoType.EMCapsule => "电磁胶囊".Translate(), 
					_ => "-", 
				};
			case 53:
			{
				ModelProto modelProto = LDB.models.modelArray[ModelIndex];
				if (modelProto != null)
				{
					int hpMax = modelProto.HpMax;
					double num25 = 0.0;
					GameHistoryData history12 = GameMain.history;
					if (history12 != null)
					{
						double num26 = 1.0 + (double)history12.globalHpEnhancement;
						num26 = (double)(int)(num26 * 1000.0 + 0.5) / 1000.0;
						if (prefabDesc.isCraftUnit)
						{
							double num27 = (isSpaceCraft ? ((double)history12.combatShipDurabilityRatio * num26) : ((double)history12.combatDroneDurabilityRatio * num26));
							num25 = (isSpaceCraft ? ((double)hpMax * num27 - (double)hpMax) : ((double)hpMax * num27 - (double)hpMax));
						}
						else
						{
							num25 = (double)hpMax * num26 - (double)hpMax;
						}
						hpMax /= 100;
						num25 /= 100.0;
						if (num25 != 0.0)
						{
							return hpMax.ToString("0.#") + "<color=#61D8FFB8> + " + num25.ToString("0.##") + "</color> hp";
						}
						return hpMax.ToString("0.#") + " hp";
					}
					return hpMax / 100 + " hp";
				}
				return "-";
			}
			case 54:
				if (incLevel == 0)
				{
					return "+" + Ability + "%";
				}
				return "<color=#FD965EB8>+" + (double)Ability * num2 + "%</color>";
			case 55:
				if (isAmmo)
				{
					return AmmoType switch
					{
						EAmmoType.Bullet => "子弹伤害类型".Translate(), 
						EAmmoType.Cannon => "炮弹伤害类型".Translate(), 
						EAmmoType.Plasma => "能量胶囊伤害类型".Translate(), 
						EAmmoType.Missile => "导弹伤害类型".Translate(), 
						_ => "-", 
					};
				}
				if (isBomb)
				{
					if (BombType == EBombType.ExplosiveUnit)
					{
						return "炸药伤害类型".Translate();
					}
					return "-";
				}
				if (prefabDesc.isCraftUnit)
				{
					return "战斗无人机伤害类型".Translate();
				}
				return "-";
			case 56:
				if ((prefabDesc.turretVSCaps & VSLayerMask.GroundAndAirAndSpace) != VSLayerMask.GroundAndAirAndSpace)
				{
					if ((int)(prefabDesc.turretVSCaps & VSLayerMask.SpaceHigh) <= 0)
					{
						return "对地".Translate();
					}
					return "对天".Translate();
				}
				return "对地且对天".Translate();
			case 57:
				if (prefabDesc.turretMinAttackRange != 0f)
				{
					return prefabDesc.turretMinAttackRange + " ~ " + prefabDesc.turretMaxAttackRange + " m";
				}
				return prefabDesc.turretMaxAttackRange + " m";
			case 58:
				return prefabDesc.turretSpaceAttackRange + " m";
			case 59:
				return prefabDesc.craftUnitMaxMovementSpeed + " m/s";
			case 60:
				return prefabDesc.craftUnitAttackRange0 + " m";
			case 61:
				if (AmmoType == EAmmoType.Cannon)
				{
					return prefabDesc.AmmoBlastRadius1 + " m";
				}
				if (AmmoType == EAmmoType.Plasma)
				{
					if (ID == 1607)
					{
						return SkillSystem.plasmaDamageRadius1 + " m";
					}
					if (ID == 1608)
					{
						return SkillSystem.antimatterDamageRadius1 + " m";
					}
					return "-";
				}
				if (AmmoType == EAmmoType.Missile)
				{
					return prefabDesc.AmmoBlastRadius1 + " m";
				}
				if (BombType == EBombType.ExplosiveUnit)
				{
					return prefabDesc.AmmoBlastRadius1 + " m";
				}
				return "-";
			case 62:
				if (prefabDesc.isCraftUnit)
				{
					int craftUnitAttackDamage = prefabDesc.craftUnitAttackDamage1;
					double num9 = 0.0;
					GameHistoryData history2 = GameMain.history;
					if (history2 != null)
					{
						num9 = (float)craftUnitAttackDamage * history2.energyDamageScale;
						num9 = (isSpaceCraft ? (num9 * (double)history2.combatShipDamageRatio - (double)craftUnitAttackDamage) : (num9 * (double)history2.combatDroneDamageRatio - (double)craftUnitAttackDamage));
						craftUnitAttackDamage /= 100;
						num9 /= 100.0;
						if (num9 != 0.0)
						{
							return craftUnitAttackDamage.ToString("0.0") + "<color=#61D8FFB8> + " + num9.ToString("0.0#") + "</color> hp";
						}
						return craftUnitAttackDamage.ToString("0.0") + " hp";
					}
					return (craftUnitAttackDamage / 100).ToString("0.0") + " hp";
				}
				return "";
			case 63:
				if (prefabDesc.isCraftUnit)
				{
					float num5 = ((prefabDesc.craftUnitMuzzleCount1 > 1) ? ((float)prefabDesc.craftUnitROF1 * 60f * (float)prefabDesc.craftUnitMuzzleCount1 / (float)(prefabDesc.craftUnitRoundInterval1 + prefabDesc.craftUnitMuzzleInterval1 * (prefabDesc.craftUnitMuzzleCount1 - 1))) : ((float)prefabDesc.craftUnitROF1 * 60f / (float)prefabDesc.craftUnitRoundInterval1));
					GameHistoryData history = GameMain.history;
					if (history != null)
					{
						float num6 = (isSpaceCraft ? (num5 * history.combatShipROFRatio - num5) : (num5 * history.combatDroneROFRatio - num5));
						if (num6 != 0f)
						{
							return num5.ToString("0.##") + "<color=#61D8FFB8> + " + num6.ToString("0.##") + "</color>" + "发每秒".Translate();
						}
						return num5.ToString("0.##") + "发每秒".Translate();
					}
					return num5.ToString("0.##") + "发每秒".Translate();
				}
				return "";
			case 64:
				return prefabDesc.craftUnitAttackRange1 + " m";
			case 65:
				if (prefabDesc.isCraftUnit)
				{
					int craftUnitAttackDamage3 = prefabDesc.craftUnitAttackDamage0;
					double num24 = 0.0;
					GameHistoryData history11 = GameMain.history;
					if (history11 != null)
					{
						num24 = (float)craftUnitAttackDamage3 * history11.energyDamageScale;
						num24 = (isSpaceCraft ? (num24 * (double)history11.combatShipDamageRatio - (double)craftUnitAttackDamage3) : (num24 * (double)history11.combatDroneDamageRatio - (double)craftUnitAttackDamage3));
						craftUnitAttackDamage3 /= 100;
						num24 /= 100.0;
						if (num24 != 0.0)
						{
							return craftUnitAttackDamage3.ToString("0.0") + "<color=#61D8FFB8> + " + num24.ToString("0.0#") + "</color> hp";
						}
						return craftUnitAttackDamage3.ToString("0.0") + " hp";
					}
					return (craftUnitAttackDamage3 / 100).ToString("0.0") + " hp";
				}
				return "";
			case 66:
				if (prefabDesc.isCraftUnit)
				{
					float num19 = ((prefabDesc.craftUnitMuzzleCount0 > 1) ? ((float)prefabDesc.craftUnitROF0 * 60f * (float)prefabDesc.craftUnitMuzzleCount0 / (float)(prefabDesc.craftUnitRoundInterval0 + prefabDesc.craftUnitMuzzleInterval0 * (prefabDesc.craftUnitMuzzleCount0 - 1))) : ((float)prefabDesc.craftUnitROF0 * 60f / (float)prefabDesc.craftUnitRoundInterval0));
					GameHistoryData history8 = GameMain.history;
					if (history8 != null)
					{
						float num20 = (isSpaceCraft ? (num19 * history8.combatShipROFRatio - num19) : (num19 * history8.combatDroneROFRatio - num19));
						if (num20 != 0f)
						{
							return num19.ToString("0.##") + "<color=#61D8FFB8> + " + num20.ToString("0.##") + "</color>" + "发每秒".Translate();
						}
						return num19.ToString("0.##") + "发每秒".Translate();
					}
					return num19.ToString("0.##") + "发每秒".Translate();
				}
				return "";
			case 67:
				return prefabDesc.craftUnitAttackRange0 + " m";
			case 68:
			{
				int num17 = (int)((double)HpMax * num + 0.1);
				string text2 = (((int)AmmoType > 0) ? "干扰胶囊数量单位".Translate() : "");
				if (incLevel > 0)
				{
					return "<color=#61D8FFB8>" + num17 + "</color>" + text2;
				}
				return HpMax + text2;
			}
			case 69:
			{
				int ability3 = Ability;
				double num16 = 0.0;
				GameHistoryData history7 = GameMain.history;
				if (history7 != null)
				{
					num16 = (float)ability3 * history7.magneticDamageScale - (float)ability3;
					if (num16 != 0.0)
					{
						return "干扰胶囊效果".Translate() + " " + ability3 + "%<color=#61D8FFB8> + " + num16.ToString("0.##") + "%</color>";
					}
					return "干扰胶囊效果".Translate() + " " + ability3 + "%";
				}
				return "干扰胶囊效果".Translate() + " " + ability3 + "%";
			}
			case 70:
			{
				double num14 = Ability;
				double num15 = 0.0;
				GameHistoryData history6 = GameMain.history;
				if (history6 != null)
				{
					num15 = num14 * (double)history6.magneticDamageScale - num14;
					num14 /= 50.0;
					num15 /= 50.0;
					if (num15 != 0.0)
					{
						return "干扰胶囊持续时间".Translate() + " " + num14.ToString("0.##") + "s<color=#61D8FFB8> + " + num15.ToString("0.##") + "s</color>";
					}
					return "干扰胶囊持续时间".Translate() + " " + num14.ToString("0.##") + "s";
				}
				num14 /= 100.0;
				return "干扰胶囊持续时间".Translate() + " " + num14.ToString("0.##") + "s";
			}
			case 71:
				if (BombType == EBombType.EMCapusle)
				{
					int ability = Ability;
					double num10 = 0.0;
					GameHistoryData history3 = GameMain.history;
					if (history3 != null)
					{
						num10 = (float)ability * history3.magneticDamageScale - (float)ability;
						if (num10 != 0.0)
						{
							return "干扰胶囊效果".Translate() + " " + ability + "%<color=#61D8FFB8> + " + num10.ToString("0.##") + "%</color>";
						}
						return "干扰胶囊效果".Translate() + " " + ability + "%";
					}
					return "干扰胶囊效果".Translate() + " " + ability + "%";
				}
				if (BombType == EBombType.ExplosiveUnit)
				{
					int ability2 = Ability;
					double num11 = 0.0;
					GameHistoryData history4 = GameMain.history;
					if (history4 != null)
					{
						num11 = (float)ability2 * history4.blastDamageScale - (float)ability2;
						ability2 /= 100;
						num11 /= 100.0;
						if (num11 != 0.0)
						{
							return "伤害".Translate() + " " + ability2.ToString("0.0") + "<color=#61D8FFB8> + " + num11.ToString("0.0#") + "</color> hp";
						}
						return "伤害".Translate() + " " + ability2.ToString("0.0") + " hp";
					}
					ability2 /= 100;
					return "伤害".Translate() + " " + ability2.ToString("0.0") + " hp";
				}
				if (BombType == EBombType.Liquid)
				{
					return "投掷无效果".Translate();
				}
				return "-";
			case 72:
			{
				int num8 = ((prefabDesc.inserterGrade != 4) ? 1 : GameMain.history.inserterStackInput);
				if (num8 <= 1)
				{
					return "不支持".Translate();
				}
				if (num8 >= 4)
				{
					return "<color=#61D8FFB8>" + num8 + "</color>";
				}
				return "<color=#61D8FFB8>" + num8 + "</color>" + "可升级".Translate();
			}
			case 73:
			{
				int num3 = ((prefabDesc.inserterGrade == 4) ? GameMain.history.inserterStackOutput : 0);
				if (num3 <= 0)
				{
					return "不支持".Translate();
				}
				if (num3 >= 4)
				{
					return "<color=#61D8FFB8>" + num3 + "</color>";
				}
				return "<color=#61D8FFB8>" + num3 + "</color>" + "可升级".Translate();
			}
			}
		}
		return "";
	}
}
