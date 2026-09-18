using System;
using System.Collections.Generic;
using UnityEngine;

public class StarData
{
	public GalaxyData galaxy;

	public int seed;

	public int index;

	public int id;

	public string name = "";

	public string overrideName = "";

	public VectorLF3 position = VectorLF3.zero;

	public VectorLF3 uPosition;

	public float mass = 1f;

	public float lifetime = 50f;

	public float age;

	public EStarType type;

	public float temperature = 8500f;

	public ESpectrType spectr;

	public float classFactor;

	public float color;

	public float luminosity = 1f;

	public float radius = 1f;

	public float acdiskRadius;

	public float habitableRadius = 1f;

	public float lightBalanceRadius = 1f;

	public float dysonRadius = 10f;

	public float orbitScaler = 1f;

	public float asterBelt1OrbitIndex;

	public float asterBelt2OrbitIndex;

	public float asterBelt1Radius;

	public float asterBelt2Radius;

	public int planetCount;

	public float level;

	public float resourceCoef = 1f;

	public PlanetData[] planets;

	public float safetyFactor = 1f;

	public int hivePatternLevel;

	public int initialHiveCount;

	public int maxHiveCount;

	public AstroOrbitData[] hiveAstroOrbits;

	public const double kEnterDistance = 3600000.0;

	public const float kPhysicsRadiusRatio = 1200f;

	public const float kViewRadiusRatio = 800f;

	public const int kMaxDFHiveOrbit = 8;

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

	public float expSharingFactor => (1f - safetyFactor) * (1f - safetyFactor) * 2.6f + 0.4f;

	public int astroId => id * 100;

	public float dysonLumino => Mathf.Round((float)Math.Pow(luminosity, 0.33000001311302185) * 1000f) / 1000f;

	public float systemRadius
	{
		get
		{
			float sunDistance = dysonRadius;
			if (planetCount > 0)
			{
				sunDistance = planets[planetCount - 1].sunDistance;
			}
			return sunDistance;
		}
	}

	public float physicsRadius => radius * 1200f;

	public float viewRadius => radius * 800f;

	public string typeString
	{
		get
		{
			string text = "";
			if (type == EStarType.GiantStar)
			{
				text = ((spectr <= ESpectrType.K) ? (text + "红巨星".Translate()) : ((spectr <= ESpectrType.F) ? (text + "黄巨星".Translate()) : ((spectr != ESpectrType.A) ? (text + "蓝巨星".Translate()) : (text + "白巨星".Translate()))));
			}
			else if (type == EStarType.WhiteDwarf)
			{
				text += "白矮星".Translate();
			}
			else if (type == EStarType.NeutronStar)
			{
				text += "中子星".Translate();
			}
			else if (type == EStarType.BlackHole)
			{
				text += "黑洞".Translate();
			}
			else if (type == EStarType.MainSeqStar)
			{
				text += string.Format("型恒星".Translate(), spectr);
			}
			return text;
		}
	}

	public bool epicHive
	{
		get
		{
			if (type != EStarType.NeutronStar)
			{
				return type == EStarType.BlackHole;
			}
			return true;
		}
	}

	public bool loaded
	{
		get
		{
			if (planets == null)
			{
				return false;
			}
			for (int i = 0; i < planetCount; i++)
			{
				if (!planets[i].loaded)
				{
					return false;
				}
			}
			return true;
		}
	}

	public bool scanned
	{
		get
		{
			if (planets == null)
			{
				return false;
			}
			for (int i = 0; i < planetCount; i++)
			{
				if (!planets[i].scanned)
				{
					return false;
				}
			}
			return true;
		}
	}

	public bool runtimeVeinGroupsScanned
	{
		get
		{
			if (planets == null)
			{
				return false;
			}
			for (int i = 0; i < planetCount; i++)
			{
				if (!planets[i].runtimeVeinGroupsScanned)
				{
					return false;
				}
			}
			return true;
		}
	}

	public bool hasAnyFactory
	{
		get
		{
			if (planets == null)
			{
				return false;
			}
			for (int i = 0; i < planetCount; i++)
			{
				if (planets[i].factory != null)
				{
					return true;
				}
			}
			return false;
		}
	}

	public event Action<StarData> onLoaded;

	public bool CalcVeinAmounts(ref long[] veinAmounts, HashSet<int> hashes, int filterType)
	{
		if (veinAmounts == null)
		{
			veinAmounts = new long[64];
		}
		Array.Clear(veinAmounts, 0, veinAmounts.Length);
		hashes.Clear();
		bool result = true;
		for (int i = 0; i < planetCount; i++)
		{
			PlanetData planetData = planets[i];
			VeinGroup[] runtimeVeinGroups = planetData.runtimeVeinGroups;
			if (runtimeVeinGroups != null)
			{
				switch (filterType)
				{
				case 1:
				{
					PlanetFactory factory = planetData.factory;
					if (factory == null)
					{
						break;
					}
					MinerComponent[] minerPool = factory.factorySystem.minerPool;
					int minerCursor = factory.factorySystem.minerCursor;
					PowerConsumerComponent[] consumerPool = factory.powerSystem.consumerPool;
					VeinData[] veinPool = factory.veinPool;
					hashes.Clear();
					for (int k = 0; k < minerCursor; k++)
					{
						ref MinerComponent reference = ref minerPool[k];
						if (reference.id != k || reference.type == EMinerType.Water || consumerPool[reference.pcId].networkId <= 0)
						{
							continue;
						}
						for (int l = 0; l < reference.veinCount; l++)
						{
							int num = reference.veins[l];
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
					for (int m = 1; m < runtimeVeinGroups.Length; m++)
					{
						veinAmounts[(uint)runtimeVeinGroups[m].type] += runtimeVeinGroups[m].amount;
					}
					PlanetFactory factory2 = planetData.factory;
					if (factory2 == null)
					{
						break;
					}
					MinerComponent[] minerPool2 = factory2.factorySystem.minerPool;
					int minerCursor2 = factory2.factorySystem.minerCursor;
					PowerConsumerComponent[] consumerPool2 = factory2.powerSystem.consumerPool;
					VeinData[] veinPool2 = factory2.veinPool;
					hashes.Clear();
					for (int n = 0; n < minerCursor2; n++)
					{
						ref MinerComponent reference2 = ref minerPool2[n];
						if (reference2.id != n || reference2.type == EMinerType.Water || consumerPool2[reference2.pcId].networkId <= 0)
						{
							continue;
						}
						for (int num2 = 0; num2 < reference2.veinCount; num2++)
						{
							int num3 = reference2.veins[num2];
							if (num3 > 0 && veinPool2[num3].id == num3 && !hashes.Contains(num3))
							{
								veinAmounts[(uint)veinPool2[num3].type] -= veinPool2[num3].amount;
								hashes.Add(num3);
							}
						}
					}
					break;
				}
				default:
				{
					for (int j = 1; j < runtimeVeinGroups.Length; j++)
					{
						veinAmounts[(uint)runtimeVeinGroups[j].type] += runtimeVeinGroups[j].amount;
					}
					break;
				}
				}
			}
			else
			{
				result = false;
			}
		}
		veinAmounts[0] = 0L;
		return result;
	}

	public bool CalcVeinCounts(ref int[] veinCounts, HashSet<int> hashes, int filterType)
	{
		if (veinCounts == null)
		{
			veinCounts = new int[64];
		}
		Array.Clear(veinCounts, 0, veinCounts.Length);
		hashes.Clear();
		bool result = true;
		for (int i = 0; i < planetCount; i++)
		{
			PlanetData planetData = planets[i];
			VeinGroup[] runtimeVeinGroups = planetData.runtimeVeinGroups;
			if (runtimeVeinGroups != null)
			{
				switch (filterType)
				{
				case 1:
				{
					PlanetFactory factory = planetData.factory;
					if (factory == null)
					{
						break;
					}
					MinerComponent[] minerPool = factory.factorySystem.minerPool;
					int minerCursor = factory.factorySystem.minerCursor;
					PowerConsumerComponent[] consumerPool = factory.powerSystem.consumerPool;
					VeinData[] veinPool = factory.veinPool;
					hashes.Clear();
					for (int k = 0; k < minerCursor; k++)
					{
						ref MinerComponent reference = ref minerPool[k];
						if (reference.id != k || reference.type == EMinerType.Water || consumerPool[reference.pcId].networkId <= 0)
						{
							continue;
						}
						for (int l = 0; l < reference.veinCount; l++)
						{
							int num = reference.veins[l];
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
					for (int m = 1; m < runtimeVeinGroups.Length; m++)
					{
						veinCounts[(uint)runtimeVeinGroups[m].type] += runtimeVeinGroups[m].count;
					}
					PlanetFactory factory2 = planetData.factory;
					if (factory2 == null)
					{
						break;
					}
					MinerComponent[] minerPool2 = factory2.factorySystem.minerPool;
					int minerCursor2 = factory2.factorySystem.minerCursor;
					PowerConsumerComponent[] consumerPool2 = factory2.powerSystem.consumerPool;
					VeinData[] veinPool2 = factory2.veinPool;
					hashes.Clear();
					for (int n = 0; n < minerCursor2; n++)
					{
						ref MinerComponent reference2 = ref minerPool2[n];
						if (reference2.id != n || reference2.type == EMinerType.Water || consumerPool2[reference2.pcId].networkId <= 0)
						{
							continue;
						}
						for (int num2 = 0; num2 < reference2.veinCount; num2++)
						{
							int num3 = reference2.veins[num2];
							if (num3 > 0 && veinPool2[num3].id == num3 && !hashes.Contains(num3))
							{
								veinCounts[(uint)veinPool2[num3].type]--;
								hashes.Add(num3);
							}
						}
					}
					break;
				}
				default:
				{
					for (int j = 1; j < runtimeVeinGroups.Length; j++)
					{
						veinCounts[(uint)runtimeVeinGroups[j].type] += runtimeVeinGroups[j].count;
					}
					break;
				}
				}
			}
			else
			{
				result = false;
			}
		}
		veinCounts[0] = 0;
		return result;
	}

	public void Load()
	{
		PlanetModelingManager.RequestLoadStar(this);
		if (GameMain.universeSimulator != null)
		{
			GameMain.universeSimulator.SetLocalStar(this);
		}
	}

	public void Unload()
	{
		for (int i = 0; i < planetCount; i++)
		{
			if (planets == null)
			{
				Debug.LogWarning("planet == null\r\nstar = " + id + " planetCount = " + planetCount);
			}
			if (planets[i] == null)
			{
				Debug.LogWarning("planet[" + i + "] == null\r\nstar = " + id + " planetCount = " + planetCount);
			}
			planets[i].Unload();
		}
		if (GameMain.universeSimulator != null)
		{
			GameMain.universeSimulator.SetLocalStar(null);
		}
	}

	public void RunScanningThread()
	{
		PlanetModelingManager.RequestScanStar(this);
	}

	public void Free()
	{
		for (int i = 0; i < planetCount; i++)
		{
			planets[i].Free();
		}
		planets = null;
	}

	public void NotifyLoaded()
	{
		if (onLoaded != null)
		{
			onLoaded(this);
		}
	}

	public void RegeneratePlanetNames(bool notifyChange)
	{
		PlanetData[] array = planets;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].RegenerateName(notifyChange);
		}
	}

	public string OrbitsDescString()
	{
		string text = "";
		for (int i = 1; i <= 12; i++)
		{
			int num = 0;
			for (int j = 0; j < planetCount; j++)
			{
				if (planets[j].orbitAround == 0 && planets[j].orbitIndex == i)
				{
					num = planets[j].number;
					break;
				}
			}
			text = ((asterBelt1OrbitIndex != (float)i) ? ((asterBelt2OrbitIndex != (float)i) ? (text + num) : (text + "b")) : (text + "a"));
		}
		return text;
	}

	public override string ToString()
	{
		return "Star " + displayName;
	}
}
