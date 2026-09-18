using System;
using UnityEngine;

public static class StarGen
{
	public static float[] orbitRadius = new float[17]
	{
		0f, 0.4f, 0.7f, 1f, 1.4f, 1.9f, 2.5f, 3.3f, 4.3f, 5.5f,
		6.9f, 8.4f, 10f, 11.7f, 13.5f, 15.4f, 17.5f
	};

	public static float[] hiveOrbitRadius = new float[18]
	{
		0.4f, 0.55f, 0.7f, 0.83f, 1f, 1.2f, 1.4f, 1.58f, 1.72f, 1.9f,
		2.11f, 2.29f, 2.5f, 2.78f, 3.02f, 3.3f, 3.6f, 3.9f
	};

	public static int[] planet2HiveOrbitTable = new int[8] { 0, 0, 2, 4, 6, 9, 12, 15 };

	public static bool[] hiveOrbitCondition = new bool[hiveOrbitRadius.Length];

	public const double GRAVITY = 1.3538551990520382E-06;

	public const float E = MathF.E;

	public static float specifyBirthStarMass = 0f;

	public static float specifyBirthStarAge = 0f;

	private static double[] pGas = new double[10];

	private const double PI = Math.PI;

	public static StarData CreateStar(GalaxyData galaxy, VectorLF3 pos, GameDesc gameDesc, int id, int seed, EStarType needtype, ESpectrType needSpectr = ESpectrType.X)
	{
		StarData starData = new StarData();
		starData.galaxy = galaxy;
		starData.index = id - 1;
		if (galaxy.starCount > 1)
		{
			starData.level = (float)starData.index / (float)(galaxy.starCount - 1);
		}
		else
		{
			starData.level = 0f;
		}
		starData.id = id;
		starData.seed = seed;
		DotNet35Random dotNet35Random = new DotNet35Random(seed);
		int seed2 = dotNet35Random.Next();
		int seed3 = dotNet35Random.Next();
		starData.position = pos;
		float num = (float)pos.magnitude;
		float num2 = num / 32f;
		if (num2 > 1f)
		{
			num2 = Mathf.Log(num2) + 1f;
			num2 = Mathf.Log(num2) + 1f;
			num2 = Mathf.Log(num2) + 1f;
			num2 = Mathf.Log(num2) + 1f;
			num2 = Mathf.Log(num2) + 1f;
		}
		starData.resourceCoef = Mathf.Pow(7f, num2) * 0.6f;
		DotNet35Random dotNet35Random2 = new DotNet35Random(seed3);
		double num3 = dotNet35Random2.NextDouble();
		double num4 = dotNet35Random2.NextDouble();
		double num5 = dotNet35Random2.NextDouble();
		double rn = dotNet35Random2.NextDouble();
		double rt = dotNet35Random2.NextDouble();
		double num6 = (dotNet35Random2.NextDouble() - 0.5) * 0.2;
		double num7 = dotNet35Random2.NextDouble() * 0.2 + 0.9;
		double num8 = dotNet35Random2.NextDouble() * 0.4 - 0.2;
		double num9 = Math.Pow(2.0, num8);
		DotNet35Random dotNet35Random3 = new DotNet35Random(dotNet35Random2.Next());
		double num10 = dotNet35Random3.NextDouble();
		float num11 = Mathf.Lerp(-0.98f, 0.88f, starData.level);
		num11 = ((!(num11 < 0f)) ? (num11 + 0.65f) : (num11 - 0.65f));
		float standardDeviation = 0.33f;
		if (needtype == EStarType.GiantStar)
		{
			num11 = ((num8 > -0.08) ? (-1.5f) : 1.6f);
			standardDeviation = 0.3f;
		}
		float num12 = RandNormal(num11, standardDeviation, num3, num4);
		switch (needSpectr)
		{
		case ESpectrType.M:
			num12 = -3f;
			break;
		case ESpectrType.O:
			num12 = 3f;
			break;
		}
		num12 = ((!(num12 > 0f)) ? (num12 * 1f) : (num12 * 2f));
		num12 = Mathf.Clamp(num12, -2.4f, 4.65f) + (float)num6 + 1f;
		switch (needtype)
		{
		case EStarType.BlackHole:
			starData.mass = 18f + (float)(num3 * num4) * 30f;
			break;
		case EStarType.NeutronStar:
			starData.mass = 7f + (float)num3 * 11f;
			break;
		case EStarType.WhiteDwarf:
			starData.mass = 1f + (float)num4 * 5f;
			break;
		default:
			starData.mass = Mathf.Pow(2f, num12);
			break;
		}
		double d = 5.0;
		if (starData.mass < 2f)
		{
			d = 2.0 + 0.4 * (1.0 - (double)starData.mass);
		}
		starData.lifetime = (float)(10000.0 * Math.Pow(0.1, Math.Log10((double)starData.mass * 0.5) / Math.Log10(d) + 1.0) * num7);
		switch (needtype)
		{
		case EStarType.GiantStar:
			starData.lifetime = (float)(10000.0 * Math.Pow(0.1, Math.Log10((double)starData.mass * 0.58) / Math.Log10(d) + 1.0) * num7);
			starData.age = (float)num5 * 0.04f + 0.96f;
			break;
		case EStarType.WhiteDwarf:
		case EStarType.NeutronStar:
		case EStarType.BlackHole:
			starData.age = (float)num5 * 0.4f + 1f;
			switch (needtype)
			{
			case EStarType.WhiteDwarf:
				starData.lifetime += 10000f;
				break;
			case EStarType.NeutronStar:
				starData.lifetime += 1000f;
				break;
			}
			break;
		default:
			if ((double)starData.mass < 0.5)
			{
				starData.age = (float)num5 * 0.12f + 0.02f;
			}
			else if ((double)starData.mass < 0.8)
			{
				starData.age = (float)num5 * 0.4f + 0.1f;
			}
			else
			{
				starData.age = (float)num5 * 0.7f + 0.2f;
			}
			break;
		}
		float num13 = starData.lifetime * starData.age;
		if (num13 > 5000f)
		{
			num13 = (Mathf.Log(num13 / 5000f) + 1f) * 5000f;
		}
		if (num13 > 8000f)
		{
			num13 = (Mathf.Log(Mathf.Log(Mathf.Log(num13 / 8000f) + 1f) + 1f) + 1f) * 8000f;
		}
		starData.lifetime = num13 / starData.age;
		float num14 = (1f - Mathf.Pow(Mathf.Clamp01(starData.age), 20f) * 0.5f) * starData.mass;
		starData.temperature = (float)(Math.Pow(num14, 0.56 + 0.14 / (Math.Log10(num14 + 4f) / Math.Log10(5.0))) * 4450.0 + 1300.0);
		double num15 = Math.Log10(((double)starData.temperature - 1300.0) / 4500.0) / Math.Log10(2.6) - 0.5;
		if (num15 < 0.0)
		{
			num15 *= 4.0;
		}
		if (num15 > 2.0)
		{
			num15 = 2.0;
		}
		else if (num15 < -4.0)
		{
			num15 = -4.0;
		}
		starData.spectr = (ESpectrType)Mathf.RoundToInt((float)num15 + 4f);
		starData.color = Mathf.Clamp01(((float)num15 + 3.5f) * 0.2f);
		starData.classFactor = (float)num15;
		starData.luminosity = Mathf.Pow(num14, 0.7f);
		starData.radius = (float)(Math.Pow(starData.mass, 0.4) * num9);
		starData.acdiskRadius = 0f;
		float p = (float)num15 + 2f;
		starData.habitableRadius = Mathf.Pow(1.7f, p) + 0.25f * Mathf.Min(1f, starData.orbitScaler);
		starData.lightBalanceRadius = Mathf.Pow(1.7f, p);
		starData.orbitScaler = Mathf.Pow(1.35f, p);
		if (starData.orbitScaler < 1f)
		{
			starData.orbitScaler = Mathf.Lerp(starData.orbitScaler, 1f, 0.6f);
		}
		SetStarAge(starData, starData.age, rn, rt);
		starData.dysonRadius = starData.orbitScaler * 0.28f;
		if ((double)starData.dysonRadius * 40000.0 < (double)(starData.physicsRadius * 1.5f))
		{
			starData.dysonRadius = (float)((double)(starData.physicsRadius * 1.5f) / 40000.0);
		}
		starData.uPosition = starData.position * 2400000.0;
		starData.name = NameGen.RandomStarName(seed2, starData, galaxy);
		starData.overrideName = "";
		float num16 = Mathf.Pow(starData.color, 1.3f);
		float num17 = Mathf.Clamp((num - 2f) / 20f, 0f, 2.5f);
		if (num17 > 1f)
		{
			num17 = Mathf.Log(num17) + 1f;
			num17 = Mathf.Log(num17) + 1f;
		}
		num17 /= 1.4f;
		if (starData.type == EStarType.BlackHole)
		{
			num16 = 5f;
		}
		else if (starData.type == EStarType.NeutronStar)
		{
			num16 = 1.7f;
		}
		else if (starData.type == EStarType.WhiteDwarf)
		{
			num16 = 1.2f;
		}
		else if (starData.type == EStarType.GiantStar)
		{
			num16 = Mathf.Max(0.6f, num16);
		}
		else if (starData.spectr == ESpectrType.O)
		{
			num16 += 0.05f;
		}
		num16 *= 0.9f;
		num16 += 0.07f;
		float num18 = Mathf.Clamp01(1f - Mathf.Pow(num16, 0.73f) * Mathf.Pow(num17, 0.27f) + (float)num10 * 0.08f - 0.04f);
		if (num18 >= 0.7f)
		{
			starData.hivePatternLevel = 0;
		}
		else if (num18 >= 0.3f)
		{
			starData.hivePatternLevel = 1;
		}
		else
		{
			starData.hivePatternLevel = 2;
		}
		starData.safetyFactor = num18;
		int num19 = dotNet35Random3.Next(0, 1000);
		int num20 = ((!starData.epicHive) ? 1 : 2);
		starData.maxHiveCount = (int)(gameDesc.combatSettings.maxDensity * (float)num20 * 1000f + (float)num19 + 0.5f) / 1000;
		float initialColonize = gameDesc.combatSettings.initialColonize;
		if (initialColonize < 0.015f)
		{
			starData.initialHiveCount = 0;
		}
		else
		{
			float num21 = Mathf.Pow(Mathf.Clamp01(starData.safetyFactor * 1.05f - 0.15f), 0.82f);
			float num22 = Mathf.Clamp01(1f - num21 - (float)(starData.maxHiveCount - 1) * 0.05f) * (1.1f - (float)starData.maxHiveCount * 0.1f);
			num22 = ((!(initialColonize <= 1f)) ? Mathf.Lerp(num22, 1f + (initialColonize - 1f) * 0.2f, (initialColonize - 1f) * 0.5f) : (num22 * initialColonize));
			if (starData.type == EStarType.GiantStar)
			{
				num22 *= 1.2f;
			}
			else if (starData.type == EStarType.WhiteDwarf)
			{
				num22 *= 1.4f;
			}
			else if (starData.type == EStarType.NeutronStar)
			{
				num22 *= 1.6f;
			}
			else if (starData.type == EStarType.BlackHole)
			{
				num22 *= 1.8f;
			}
			else if (starData.spectr == ESpectrType.O)
			{
				num22 *= 1.1f;
			}
			float num23 = num22 * (float)starData.maxHiveCount;
			if (num23 > (float)starData.maxHiveCount + 0.75f)
			{
				num23 = (float)starData.maxHiveCount + 0.75f;
			}
			float standardDeviation2 = 0.5f;
			if ((double)num23 <= 0.01)
			{
				standardDeviation2 = 0f;
			}
			else if (num23 < 1f)
			{
				standardDeviation2 = Mathf.Sqrt(num23) * 0.29f + 0.21f;
			}
			else if (num23 > 1f)
			{
				standardDeviation2 = 0.3f + 0.2f * num23;
			}
			int num24 = 64;
			do
			{
				double r = dotNet35Random3.NextDouble();
				double r2 = dotNet35Random3.NextDouble();
				starData.initialHiveCount = (int)((double)RandNormal(num23, standardDeviation2, r, r2) + 0.5);
			}
			while (num24-- > 0 && (starData.initialHiveCount < 0 || starData.initialHiveCount > starData.maxHiveCount));
			if (starData.initialHiveCount < 0)
			{
				starData.initialHiveCount = 0;
			}
			else if (starData.initialHiveCount > starData.maxHiveCount)
			{
				starData.initialHiveCount = starData.maxHiveCount;
			}
		}
		if (starData.type == EStarType.BlackHole)
		{
			int num25 = (int)(gameDesc.combatSettings.maxDensity * 1000f + (float)num19 + 0.5f) / 1000;
			if (starData.initialHiveCount < num25)
			{
				starData.initialHiveCount = num25;
			}
			if (starData.initialHiveCount < 1)
			{
				starData.initialHiveCount = 1;
			}
		}
		return starData;
	}

	public static StarData CreateBirthStar(GalaxyData galaxy, GameDesc gameDesc, int seed)
	{
		StarData starData = new StarData();
		starData.galaxy = galaxy;
		starData.index = 0;
		starData.level = 0f;
		starData.id = 1;
		starData.seed = seed;
		starData.resourceCoef = 0.6f;
		DotNet35Random dotNet35Random = new DotNet35Random(seed);
		int seed2 = dotNet35Random.Next();
		int seed3 = dotNet35Random.Next();
		starData.name = NameGen.RandomName(seed2);
		starData.overrideName = "";
		starData.position = VectorLF3.zero;
		DotNet35Random dotNet35Random2 = new DotNet35Random(seed3);
		double r = dotNet35Random2.NextDouble();
		double r2 = dotNet35Random2.NextDouble();
		double num = dotNet35Random2.NextDouble();
		double rn = dotNet35Random2.NextDouble();
		double rt = dotNet35Random2.NextDouble();
		double num2 = dotNet35Random2.NextDouble() * 0.2 + 0.9;
		double y = dotNet35Random2.NextDouble() * 0.4 - 0.2;
		double num3 = Math.Pow(2.0, y);
		DotNet35Random dotNet35Random3 = new DotNet35Random(dotNet35Random2.Next());
		double num4 = dotNet35Random3.NextDouble();
		float value = RandNormal(0f, 0.08f, r, r2);
		value = Mathf.Clamp(value, -0.2f, 0.2f);
		starData.mass = Mathf.Pow(2f, value);
		if (specifyBirthStarMass > 0.1f)
		{
			starData.mass = specifyBirthStarMass;
		}
		if (specifyBirthStarAge > 1E-05f)
		{
			starData.age = specifyBirthStarAge;
		}
		double num5 = 5.0;
		num5 = 2.0 + 0.4 * (1.0 - (double)starData.mass);
		starData.lifetime = (float)(10000.0 * Math.Pow(0.1, Math.Log10((double)starData.mass * 0.5) / Math.Log10(num5) + 1.0) * num2);
		starData.age = (float)(num * 0.4 + 0.3);
		if (specifyBirthStarAge > 1E-05f)
		{
			starData.age = specifyBirthStarAge;
		}
		float num6 = (1f - Mathf.Pow(Mathf.Clamp01(starData.age), 20f) * 0.5f) * starData.mass;
		starData.temperature = (float)(Math.Pow(num6, 0.56 + 0.14 / (Math.Log10(num6 + 4f) / Math.Log10(5.0))) * 4450.0 + 1300.0);
		double num7 = Math.Log10(((double)starData.temperature - 1300.0) / 4500.0) / Math.Log10(2.6) - 0.5;
		if (num7 < 0.0)
		{
			num7 *= 4.0;
		}
		if (num7 > 2.0)
		{
			num7 = 2.0;
		}
		else if (num7 < -4.0)
		{
			num7 = -4.0;
		}
		starData.spectr = (ESpectrType)Mathf.RoundToInt((float)num7 + 4f);
		starData.color = Mathf.Clamp01(((float)num7 + 3.5f) * 0.2f);
		starData.classFactor = (float)num7;
		starData.luminosity = Mathf.Pow(num6, 0.7f);
		starData.radius = (float)(Math.Pow(starData.mass, 0.4) * num3);
		starData.acdiskRadius = 0f;
		float p = (float)num7 + 2f;
		starData.habitableRadius = Mathf.Pow(1.7f, p) + 0.2f * Mathf.Min(1f, starData.orbitScaler);
		starData.lightBalanceRadius = Mathf.Pow(1.7f, p);
		starData.orbitScaler = Mathf.Pow(1.35f, p);
		if (starData.orbitScaler < 1f)
		{
			starData.orbitScaler = Mathf.Lerp(starData.orbitScaler, 1f, 0.6f);
		}
		SetStarAge(starData, starData.age, rn, rt);
		starData.dysonRadius = starData.orbitScaler * 0.28f;
		if ((double)starData.dysonRadius * 40000.0 < (double)(starData.physicsRadius * 1.5f))
		{
			starData.dysonRadius = (float)((double)(starData.physicsRadius * 1.5f) / 40000.0);
		}
		starData.uPosition = VectorLF3.zero;
		starData.name = NameGen.RandomStarName(seed2, starData, galaxy);
		starData.overrideName = "";
		starData.hivePatternLevel = 0;
		starData.safetyFactor = 0.847f + (float)num4 * 0.026f;
		int num8 = dotNet35Random3.Next(0, 1000);
		starData.maxHiveCount = (int)(gameDesc.combatSettings.maxDensity * 1000f + (float)num8 + 0.5f) / 1000;
		float initialColonize = gameDesc.combatSettings.initialColonize;
		int num9 = ((!(initialColonize * (float)starData.maxHiveCount < 0.7f)) ? 1 : 0);
		if (initialColonize < 0.015f)
		{
			starData.initialHiveCount = 0;
		}
		else
		{
			float num10 = 0.6f * initialColonize * (float)starData.maxHiveCount;
			float standardDeviation = 0.5f;
			if (num10 < 1f)
			{
				standardDeviation = Mathf.Sqrt(num10) * 0.29f + 0.21f;
			}
			else if (num10 > (float)starData.maxHiveCount)
			{
				num10 = starData.maxHiveCount;
			}
			int num11 = 16;
			do
			{
				double r3 = dotNet35Random3.NextDouble();
				double r4 = dotNet35Random3.NextDouble();
				starData.initialHiveCount = (int)((double)RandNormal(num10, standardDeviation, r3, r4) + 0.5);
			}
			while (num11-- > 0 && (starData.initialHiveCount < 0 || starData.initialHiveCount > starData.maxHiveCount));
			if (starData.initialHiveCount < num9)
			{
				starData.initialHiveCount = num9;
			}
			else if (starData.initialHiveCount > starData.maxHiveCount)
			{
				starData.initialHiveCount = starData.maxHiveCount;
			}
		}
		return starData;
	}

	private static double _signpow(double x, double pow)
	{
		double num = ((x > 0.0) ? 1.0 : (-1.0));
		return Math.Abs(Math.Pow(x, pow)) * num;
	}

	public static void CreateStarPlanets(GalaxyData galaxy, StarData star, GameDesc gameDesc)
	{
		DotNet35Random dotNet35Random = new DotNet35Random(star.seed);
		dotNet35Random.Next();
		dotNet35Random.Next();
		dotNet35Random.Next();
		DotNet35Random dotNet35Random2 = new DotNet35Random(dotNet35Random.Next());
		double num = dotNet35Random2.NextDouble();
		double num2 = dotNet35Random2.NextDouble();
		double num3 = dotNet35Random2.NextDouble();
		double num4 = dotNet35Random2.NextDouble();
		double num5 = dotNet35Random2.NextDouble();
		double num6 = dotNet35Random2.NextDouble() * 0.2 + 0.9;
		double num7 = dotNet35Random2.NextDouble() * 0.2 + 0.9;
		DotNet35Random dotNet35Random3 = new DotNet35Random(dotNet35Random.Next());
		SetHiveOrbitsConditionsTrue();
		if (star.type == EStarType.BlackHole)
		{
			star.planetCount = 1;
			star.planets = new PlanetData[star.planetCount];
			int info_seed = dotNet35Random2.Next();
			int gen_seed = dotNet35Random2.Next();
			star.planets[0] = PlanetGen.CreatePlanet(galaxy, star, gameDesc.savedThemeIds, 0, 0, 3, 1, gasGiant: false, info_seed, gen_seed);
		}
		else if (star.type == EStarType.NeutronStar)
		{
			star.planetCount = 1;
			star.planets = new PlanetData[star.planetCount];
			int info_seed2 = dotNet35Random2.Next();
			int gen_seed2 = dotNet35Random2.Next();
			star.planets[0] = PlanetGen.CreatePlanet(galaxy, star, gameDesc.savedThemeIds, 0, 0, 3, 1, gasGiant: false, info_seed2, gen_seed2);
		}
		else if (star.type == EStarType.WhiteDwarf)
		{
			if (num < 0.699999988079071)
			{
				star.planetCount = 1;
				star.planets = new PlanetData[star.planetCount];
				int info_seed3 = dotNet35Random2.Next();
				int gen_seed3 = dotNet35Random2.Next();
				star.planets[0] = PlanetGen.CreatePlanet(galaxy, star, gameDesc.savedThemeIds, 0, 0, 3, 1, gasGiant: false, info_seed3, gen_seed3);
			}
			else
			{
				star.planetCount = 2;
				star.planets = new PlanetData[star.planetCount];
				int num8 = 0;
				int num9 = 0;
				if (num2 < 0.30000001192092896)
				{
					num8 = dotNet35Random2.Next();
					num9 = dotNet35Random2.Next();
					star.planets[0] = PlanetGen.CreatePlanet(galaxy, star, gameDesc.savedThemeIds, 0, 0, 3, 1, gasGiant: false, num8, num9);
					num8 = dotNet35Random2.Next();
					num9 = dotNet35Random2.Next();
					star.planets[1] = PlanetGen.CreatePlanet(galaxy, star, gameDesc.savedThemeIds, 1, 0, 4, 2, gasGiant: false, num8, num9);
				}
				else
				{
					num8 = dotNet35Random2.Next();
					num9 = dotNet35Random2.Next();
					star.planets[0] = PlanetGen.CreatePlanet(galaxy, star, gameDesc.savedThemeIds, 0, 0, 4, 1, gasGiant: true, num8, num9);
					num8 = dotNet35Random2.Next();
					num9 = dotNet35Random2.Next();
					star.planets[1] = PlanetGen.CreatePlanet(galaxy, star, gameDesc.savedThemeIds, 1, 1, 1, 1, gasGiant: false, num8, num9);
				}
			}
		}
		else if (star.type == EStarType.GiantStar)
		{
			if (num < 0.30000001192092896)
			{
				star.planetCount = 1;
				star.planets = new PlanetData[star.planetCount];
				int info_seed4 = dotNet35Random2.Next();
				int gen_seed4 = dotNet35Random2.Next();
				int orbitIndex = ((num3 > 0.5) ? 3 : 2);
				star.planets[0] = PlanetGen.CreatePlanet(galaxy, star, gameDesc.savedThemeIds, 0, 0, orbitIndex, 1, gasGiant: false, info_seed4, gen_seed4);
			}
			else if (num < 0.800000011920929)
			{
				star.planetCount = 2;
				star.planets = new PlanetData[star.planetCount];
				int num10 = 0;
				int num11 = 0;
				if (num2 < 0.25)
				{
					num10 = dotNet35Random2.Next();
					num11 = dotNet35Random2.Next();
					int orbitIndex2 = ((num3 > 0.5) ? 3 : 2);
					star.planets[0] = PlanetGen.CreatePlanet(galaxy, star, gameDesc.savedThemeIds, 0, 0, orbitIndex2, 1, gasGiant: false, num10, num11);
					num10 = dotNet35Random2.Next();
					num11 = dotNet35Random2.Next();
					orbitIndex2 = ((num3 > 0.5) ? 4 : 3);
					star.planets[1] = PlanetGen.CreatePlanet(galaxy, star, gameDesc.savedThemeIds, 1, 0, orbitIndex2, 2, gasGiant: false, num10, num11);
				}
				else
				{
					num10 = dotNet35Random2.Next();
					num11 = dotNet35Random2.Next();
					star.planets[0] = PlanetGen.CreatePlanet(galaxy, star, gameDesc.savedThemeIds, 0, 0, 3, 1, gasGiant: true, num10, num11);
					num10 = dotNet35Random2.Next();
					num11 = dotNet35Random2.Next();
					star.planets[1] = PlanetGen.CreatePlanet(galaxy, star, gameDesc.savedThemeIds, 1, 1, 1, 1, gasGiant: false, num10, num11);
				}
			}
			else
			{
				star.planetCount = 3;
				star.planets = new PlanetData[star.planetCount];
				int num12 = 0;
				int num13 = 0;
				if (num2 < 0.15000000596046448)
				{
					num12 = dotNet35Random2.Next();
					num13 = dotNet35Random2.Next();
					int orbitIndex3 = ((num3 > 0.5) ? 3 : 2);
					star.planets[0] = PlanetGen.CreatePlanet(galaxy, star, gameDesc.savedThemeIds, 0, 0, orbitIndex3, 1, gasGiant: false, num12, num13);
					num12 = dotNet35Random2.Next();
					num13 = dotNet35Random2.Next();
					orbitIndex3 = ((num3 > 0.5) ? 4 : 3);
					star.planets[1] = PlanetGen.CreatePlanet(galaxy, star, gameDesc.savedThemeIds, 1, 0, orbitIndex3, 2, gasGiant: false, num12, num13);
					num12 = dotNet35Random2.Next();
					num13 = dotNet35Random2.Next();
					orbitIndex3 = ((num3 > 0.5) ? 5 : 4);
					star.planets[2] = PlanetGen.CreatePlanet(galaxy, star, gameDesc.savedThemeIds, 2, 0, orbitIndex3, 3, gasGiant: false, num12, num13);
				}
				else if (num2 < 0.75)
				{
					num12 = dotNet35Random2.Next();
					num13 = dotNet35Random2.Next();
					int orbitIndex4 = ((num3 > 0.5) ? 3 : 2);
					star.planets[0] = PlanetGen.CreatePlanet(galaxy, star, gameDesc.savedThemeIds, 0, 0, orbitIndex4, 1, gasGiant: false, num12, num13);
					num12 = dotNet35Random2.Next();
					num13 = dotNet35Random2.Next();
					star.planets[1] = PlanetGen.CreatePlanet(galaxy, star, gameDesc.savedThemeIds, 1, 0, 4, 2, gasGiant: true, num12, num13);
					num12 = dotNet35Random2.Next();
					num13 = dotNet35Random2.Next();
					star.planets[2] = PlanetGen.CreatePlanet(galaxy, star, gameDesc.savedThemeIds, 2, 2, 1, 1, gasGiant: false, num12, num13);
				}
				else
				{
					num12 = dotNet35Random2.Next();
					num13 = dotNet35Random2.Next();
					int orbitIndex5 = ((num3 > 0.5) ? 4 : 3);
					star.planets[0] = PlanetGen.CreatePlanet(galaxy, star, gameDesc.savedThemeIds, 0, 0, orbitIndex5, 1, gasGiant: true, num12, num13);
					num12 = dotNet35Random2.Next();
					num13 = dotNet35Random2.Next();
					star.planets[1] = PlanetGen.CreatePlanet(galaxy, star, gameDesc.savedThemeIds, 1, 1, 1, 1, gasGiant: false, num12, num13);
					num12 = dotNet35Random2.Next();
					num13 = dotNet35Random2.Next();
					star.planets[2] = PlanetGen.CreatePlanet(galaxy, star, gameDesc.savedThemeIds, 2, 1, 2, 2, gasGiant: false, num12, num13);
				}
			}
		}
		else
		{
			Array.Clear(pGas, 0, pGas.Length);
			if (star.index == 0)
			{
				star.planetCount = 4;
				pGas[0] = 0.0;
				pGas[1] = 0.0;
				pGas[2] = 0.0;
			}
			else if (star.spectr == ESpectrType.M)
			{
				if (num < 0.1)
				{
					star.planetCount = 1;
				}
				else if (num < 0.3)
				{
					star.planetCount = 2;
				}
				else if (num < 0.8)
				{
					star.planetCount = 3;
				}
				else
				{
					star.planetCount = 4;
				}
				if (star.planetCount <= 3)
				{
					pGas[0] = 0.2;
					pGas[1] = 0.2;
				}
				else
				{
					pGas[0] = 0.0;
					pGas[1] = 0.2;
					pGas[2] = 0.3;
				}
			}
			else if (star.spectr == ESpectrType.K)
			{
				if (num < 0.1)
				{
					star.planetCount = 1;
				}
				else if (num < 0.2)
				{
					star.planetCount = 2;
				}
				else if (num < 0.7)
				{
					star.planetCount = 3;
				}
				else if (num < 0.95)
				{
					star.planetCount = 4;
				}
				else
				{
					star.planetCount = 5;
				}
				if (star.planetCount <= 3)
				{
					pGas[0] = 0.18;
					pGas[1] = 0.18;
				}
				else
				{
					pGas[0] = 0.0;
					pGas[1] = 0.18;
					pGas[2] = 0.28;
					pGas[3] = 0.28;
				}
			}
			else if (star.spectr == ESpectrType.G)
			{
				if (num < 0.4)
				{
					star.planetCount = 3;
				}
				else if (num < 0.9)
				{
					star.planetCount = 4;
				}
				else
				{
					star.planetCount = 5;
				}
				if (star.planetCount <= 3)
				{
					pGas[0] = 0.18;
					pGas[1] = 0.18;
				}
				else
				{
					pGas[0] = 0.0;
					pGas[1] = 0.2;
					pGas[2] = 0.3;
					pGas[3] = 0.3;
				}
			}
			else if (star.spectr == ESpectrType.F)
			{
				if (num < 0.35)
				{
					star.planetCount = 3;
				}
				else if (num < 0.8)
				{
					star.planetCount = 4;
				}
				else
				{
					star.planetCount = 5;
				}
				if (star.planetCount <= 3)
				{
					pGas[0] = 0.2;
					pGas[1] = 0.2;
				}
				else
				{
					pGas[0] = 0.0;
					pGas[1] = 0.22;
					pGas[2] = 0.31;
					pGas[3] = 0.31;
				}
			}
			else if (star.spectr == ESpectrType.A)
			{
				if (num < 0.3)
				{
					star.planetCount = 3;
				}
				else if (num < 0.75)
				{
					star.planetCount = 4;
				}
				else
				{
					star.planetCount = 5;
				}
				if (star.planetCount <= 3)
				{
					pGas[0] = 0.2;
					pGas[1] = 0.2;
				}
				else
				{
					pGas[0] = 0.1;
					pGas[1] = 0.28;
					pGas[2] = 0.3;
					pGas[3] = 0.35;
				}
			}
			else if (star.spectr == ESpectrType.B)
			{
				if (num < 0.3)
				{
					star.planetCount = 4;
				}
				else if (num < 0.75)
				{
					star.planetCount = 5;
				}
				else
				{
					star.planetCount = 6;
				}
				if (star.planetCount <= 3)
				{
					pGas[0] = 0.2;
					pGas[1] = 0.2;
				}
				else
				{
					pGas[0] = 0.1;
					pGas[1] = 0.22;
					pGas[2] = 0.28;
					pGas[3] = 0.35;
					pGas[4] = 0.35;
				}
			}
			else if (star.spectr == ESpectrType.O)
			{
				if (num < 0.5)
				{
					star.planetCount = 5;
				}
				else
				{
					star.planetCount = 6;
				}
				pGas[0] = 0.1;
				pGas[1] = 0.2;
				pGas[2] = 0.25;
				pGas[3] = 0.3;
				pGas[4] = 0.32;
				pGas[5] = 0.35;
			}
			else
			{
				star.planetCount = 1;
			}
			star.planets = new PlanetData[star.planetCount];
			int num14 = 0;
			int num15 = 0;
			int num16 = 0;
			int num17 = 1;
			for (int i = 0; i < star.planetCount; i++)
			{
				int info_seed5 = dotNet35Random2.Next();
				int gen_seed5 = dotNet35Random2.Next();
				double num18 = dotNet35Random2.NextDouble();
				double num19 = dotNet35Random2.NextDouble();
				bool flag = false;
				if (num16 == 0)
				{
					num14++;
					if (i < star.planetCount - 1 && num18 < pGas[i])
					{
						flag = true;
						if (num17 < 3)
						{
							num17 = 3;
						}
					}
					while (true)
					{
						if (star.index == 0 && num17 == 3)
						{
							flag = true;
							break;
						}
						int num20 = star.planetCount - i;
						int num21 = 9 - num17;
						if (num21 <= num20)
						{
							break;
						}
						float a = (float)num20 / (float)num21;
						a = ((num17 <= 3) ? (Mathf.Lerp(a, 1f, 0.15f) + 0.01f) : (Mathf.Lerp(a, 1f, 0.45f) + 0.01f));
						if (dotNet35Random2.NextDouble() < (double)a)
						{
							break;
						}
						num17++;
					}
				}
				else
				{
					num15++;
					flag = false;
				}
				star.planets[i] = PlanetGen.CreatePlanet(galaxy, star, gameDesc.savedThemeIds, i, num16, (num16 == 0) ? num17 : num15, (num16 == 0) ? num14 : num15, flag, info_seed5, gen_seed5);
				num17++;
				if (flag)
				{
					num16 = num14;
					num15 = 0;
				}
				if (num15 >= 1 && num19 < 0.8)
				{
					num16 = 0;
					num15 = 0;
				}
			}
		}
		int num22 = 0;
		int num23 = 0;
		int num24 = 0;
		int num25 = 0;
		for (int j = 0; j < star.planetCount; j++)
		{
			if (star.planets[j].type == EPlanetType.Gas)
			{
				num22 = star.planets[j].orbitIndex;
				break;
			}
		}
		for (int k = 0; k < star.planetCount; k++)
		{
			if (star.planets[k].orbitAround == 0)
			{
				num23 = star.planets[k].orbitIndex;
			}
		}
		if (num22 > 0)
		{
			int num26 = num22 - 1;
			bool flag2 = true;
			for (int l = 0; l < star.planetCount; l++)
			{
				if (star.planets[l].orbitAround == 0 && star.planets[l].orbitIndex == num22 - 1)
				{
					flag2 = false;
					break;
				}
			}
			if (flag2 && num4 < 0.2 + (double)num26 * 0.2)
			{
				num24 = num26;
			}
		}
		num25 = ((num5 < 0.2) ? (num23 + 3) : ((num5 < 0.4) ? (num23 + 2) : ((num5 < 0.8) ? (num23 + 1) : 0)));
		if (num25 != 0 && num25 < 5)
		{
			num25 = 5;
		}
		star.asterBelt1OrbitIndex = num24;
		star.asterBelt2OrbitIndex = num25;
		if (num24 > 0)
		{
			star.asterBelt1Radius = orbitRadius[num24] * (float)num6 * star.orbitScaler;
		}
		if (num25 > 0)
		{
			star.asterBelt2Radius = orbitRadius[num25] * (float)num7 * star.orbitScaler;
		}
		for (int m = 0; m < star.planetCount; m++)
		{
			PlanetData planetData = star.planets[m];
			int orbitIndex6 = planetData.orbitIndex;
			int orbitAroundOrbitIndex = ((planetData.orbitAroundPlanet != null) ? planetData.orbitAroundPlanet.orbitIndex : 0);
			SetHiveOrbitConditionFalse(orbitIndex6, orbitAroundOrbitIndex, planetData.sunDistance / star.orbitScaler, star.index);
		}
		star.hiveAstroOrbits = new AstroOrbitData[8];
		AstroOrbitData[] hiveAstroOrbits = star.hiveAstroOrbits;
		int num27 = 0;
		for (int n = 0; n < hiveOrbitCondition.Length; n++)
		{
			if (hiveOrbitCondition[n])
			{
				num27++;
			}
		}
		for (int num28 = 0; num28 < 8; num28++)
		{
			double value = dotNet35Random3.NextDouble() * 2.0 - 1.0;
			double num29 = dotNet35Random3.NextDouble();
			double num30 = dotNet35Random3.NextDouble();
			value = (double)Math.Sign(value) * Math.Pow(Math.Abs(value), 0.7) * 90.0;
			num29 *= 360.0;
			num30 *= 360.0;
			float num31 = 0.3f;
			Assert.Positive(num27);
			if (num27 > 0)
			{
				int num32 = ((star.index != 0) ? 5 : 2);
				num32 = ((num27 > num32) ? num32 : num27);
				int num33 = num32 * 100;
				int num34 = num33 * 100;
				int num35 = dotNet35Random3.Next(num33);
				int num36 = num35 * num35 / num34;
				for (int num37 = 0; num37 < hiveOrbitCondition.Length; num37++)
				{
					if (hiveOrbitCondition[num37])
					{
						if (num36 == 0)
						{
							num31 = hiveOrbitRadius[num37];
							hiveOrbitCondition[num37] = false;
							num27--;
							break;
						}
						num36--;
					}
				}
			}
			float num38 = num31 * star.orbitScaler;
			hiveAstroOrbits[num28] = new AstroOrbitData();
			hiveAstroOrbits[num28].orbitRadius = num38;
			hiveAstroOrbits[num28].orbitInclination = (float)value;
			hiveAstroOrbits[num28].orbitLongitude = (float)num29;
			hiveAstroOrbits[num28].orbitPhase = (float)num30;
			if (gameDesc.creationVersion.Build < 20700)
			{
				hiveAstroOrbits[num28].orbitalPeriod = Math.Sqrt(39.47841760435743 * (double)num31 * (double)num31 * (double)num31 / (1.3538551990520382E-06 * (double)star.mass));
			}
			else
			{
				hiveAstroOrbits[num28].orbitalPeriod = Math.Sqrt(39.47841760435743 * (double)num38 * (double)num38 * (double)num38 / (5.415420796208153E-06 * (double)star.mass));
			}
			hiveAstroOrbits[num28].orbitRotation = Quaternion.AngleAxis(hiveAstroOrbits[num28].orbitLongitude, Vector3.up) * Quaternion.AngleAxis(hiveAstroOrbits[num28].orbitInclination, Vector3.forward);
			hiveAstroOrbits[num28].orbitNormal = Maths.QRotateLF(hiveAstroOrbits[num28].orbitRotation, new VectorLF3(0f, 1f, 0f)).normalized;
		}
	}

	public static void SetStarAge(StarData star, float age, double rn, double rt)
	{
		float num = (float)(rn * 0.1 + 0.95);
		float num2 = (float)(rt * 0.4 + 0.8);
		float num3 = (float)(rt * 9.0 + 1.0);
		star.age = age;
		if (age >= 1f)
		{
			if (star.mass >= 18f)
			{
				star.type = EStarType.BlackHole;
				star.spectr = ESpectrType.X;
				star.mass *= 2.5f * num2;
				star.radius *= 1f;
				star.acdiskRadius = star.radius * 5f;
				star.temperature = 0f;
				star.luminosity *= 0.001f * num;
				star.habitableRadius = 0f;
				star.lightBalanceRadius *= 0.4f * num;
				star.color = 1f;
			}
			else if (star.mass >= 7f)
			{
				star.type = EStarType.NeutronStar;
				star.spectr = ESpectrType.X;
				star.mass *= 0.2f * num;
				star.radius *= 0.15f;
				star.acdiskRadius = star.radius * 9f;
				star.temperature = num3 * 10000000f;
				star.luminosity *= 0.1f * num;
				star.habitableRadius = 0f;
				star.lightBalanceRadius *= 3f * num;
				star.orbitScaler *= 1.5f * num;
				star.color = 1f;
			}
			else
			{
				star.type = EStarType.WhiteDwarf;
				star.spectr = ESpectrType.X;
				star.mass *= 0.2f * num;
				star.radius *= 0.2f;
				star.acdiskRadius = 0f;
				star.temperature = num2 * 150000f;
				star.luminosity *= 0.04f * num2;
				star.habitableRadius *= 0.15f * num2;
				star.lightBalanceRadius *= 0.2f * num;
				star.color = 0.7f;
			}
		}
		else if (age >= 0.96f)
		{
			float num4 = (float)(Math.Pow(5.0, Math.Abs(Math.Log10(star.mass) - 0.7)) * 5.0);
			if (num4 > 10f)
			{
				num4 = (Mathf.Log(num4 * 0.1f) + 1f) * 10f;
			}
			float num5 = 1f - Mathf.Pow(star.age, 30f) * 0.5f;
			star.type = EStarType.GiantStar;
			star.mass = num5 * star.mass;
			star.radius = num4 * num2;
			star.acdiskRadius = 0f;
			star.temperature = num5 * star.temperature;
			star.luminosity = 1.6f * star.luminosity;
			star.habitableRadius = 9f * star.habitableRadius;
			star.lightBalanceRadius = 3f * star.habitableRadius;
			star.orbitScaler = 3.3f * star.orbitScaler;
		}
	}

	private static float RandNormal(float averageValue, float standardDeviation, double r1, double r2)
	{
		return averageValue + standardDeviation * (float)(Math.Sqrt(-2.0 * Math.Log(1.0 - r1)) * Math.Sin(Math.PI * 2.0 * r2));
	}

	private static void SetHiveOrbitsConditionsTrue()
	{
		for (int i = 0; i < hiveOrbitCondition.Length; i++)
		{
			hiveOrbitCondition[i] = true;
		}
	}

	private static void SetHiveOrbitConditionFalse(int planetOrbitIndex, int orbitAroundOrbitIndex, float sunDistance, int starIndex)
	{
		int num = ((orbitAroundOrbitIndex > 0) ? orbitAroundOrbitIndex : planetOrbitIndex);
		int num2 = ((orbitAroundOrbitIndex > 0) ? planetOrbitIndex : 0);
		if (num <= 0 || num >= planet2HiveOrbitTable.Length)
		{
			return;
		}
		int num3 = planet2HiveOrbitTable[num];
		hiveOrbitCondition[num3] = false;
		if (num2 > 0)
		{
			float num4 = 0f;
			num4 = ((starIndex != 0) ? (0.049f * (float)num2 + 0.026f + 0.13f) : (0.041f * (float)num2 + 0.026f + 0.12f));
			int num5 = num3 - 1;
			int num6 = num3 + 1;
			if (num5 >= 0 && sunDistance - hiveOrbitRadius[num5] < num4)
			{
				hiveOrbitCondition[num5] = false;
			}
			if (num6 < hiveOrbitCondition.Length && hiveOrbitRadius[num6] - sunDistance < num4)
			{
				hiveOrbitCondition[num6] = false;
			}
		}
	}
}
