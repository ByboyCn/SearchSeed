using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlanetAlgorithm
{
	protected int seed;

	protected PlanetData planet;

	private Vector3[] veinVectors = new Vector3[512];

	private EVeinType[] veinVectorTypes = new EVeinType[512];

	private int veinVectorCount;

	private List<Vector2> tmp_vecs = new List<Vector2>(100);

	public void Reset(int _seed, PlanetData _planet)
	{
		seed = _seed;
		planet = _planet;
	}

	public abstract void GenerateTerrain(double modX, double modY);

	public abstract void GenerateVegetables();

	public virtual void CalcWaterPercent()
	{
		if (planet.type == EPlanetType.Gas)
		{
			planet.windStrength = 0f;
		}
		CalcLandPercent(planet);
	}

	public virtual void GenerateVeins()
	{
		lock (planet)
		{
			ThemeProto themeProto = LDB.themes.Select(planet.theme);
			if (themeProto == null)
			{
				return;
			}
			DotNet35Random dotNet35Random = new DotNet35Random(planet.seed);
			dotNet35Random.Next();
			dotNet35Random.Next();
			dotNet35Random.Next();
			dotNet35Random.Next();
			int birthSeed = dotNet35Random.Next();
			DotNet35Random dotNet35Random2 = new DotNet35Random(dotNet35Random.Next());
			PlanetRawData data = planet.data;
			float num = 2.1f / planet.radius;
			VeinProto[] veinProtos = PlanetModelingManager.veinProtos;
			int[] veinModelIndexs = PlanetModelingManager.veinModelIndexs;
			int[] veinModelCounts = PlanetModelingManager.veinModelCounts;
			int[] veinProducts = PlanetModelingManager.veinProducts;
			int[] array = new int[veinProtos.Length];
			float[] array2 = new float[veinProtos.Length];
			float[] array3 = new float[veinProtos.Length];
			if (themeProto.VeinSpot != null)
			{
				Array.Copy(themeProto.VeinSpot, 0, array, 1, Math.Min(themeProto.VeinSpot.Length, array.Length - 1));
			}
			if (themeProto.VeinCount != null)
			{
				Array.Copy(themeProto.VeinCount, 0, array2, 1, Math.Min(themeProto.VeinCount.Length, array2.Length - 1));
			}
			if (themeProto.VeinOpacity != null)
			{
				Array.Copy(themeProto.VeinOpacity, 0, array3, 1, Math.Min(themeProto.VeinOpacity.Length, array3.Length - 1));
			}
			float p = 1f;
			ESpectrType spectr = planet.star.spectr;
			switch (planet.star.type)
			{
			case EStarType.MainSeqStar:
				switch (spectr)
				{
				case ESpectrType.M:
					p = 2.5f;
					break;
				case ESpectrType.K:
					p = 1f;
					break;
				case ESpectrType.G:
					p = 0.7f;
					break;
				case ESpectrType.F:
					p = 0.6f;
					break;
				case ESpectrType.A:
					p = 1f;
					break;
				case ESpectrType.B:
					p = 0.4f;
					break;
				case ESpectrType.O:
					p = 1.6f;
					break;
				}
				break;
			case EStarType.GiantStar:
				p = 2.5f;
				break;
			case EStarType.WhiteDwarf:
			{
				p = 3.5f;
				array[9]++;
				array[9]++;
				for (int j = 1; j < 12; j++)
				{
					if (dotNet35Random.NextDouble() >= 0.44999998807907104)
					{
						break;
					}
					array[9]++;
				}
				array2[9] = 0.7f;
				array3[9] = 1f;
				array[10]++;
				array[10]++;
				for (int k = 1; k < 12; k++)
				{
					if (dotNet35Random.NextDouble() >= 0.44999998807907104)
					{
						break;
					}
					array[10]++;
				}
				array2[10] = 0.7f;
				array3[10] = 1f;
				array[12]++;
				for (int l = 1; l < 12; l++)
				{
					if (dotNet35Random.NextDouble() >= 0.5)
					{
						break;
					}
					array[12]++;
				}
				array2[12] = 0.7f;
				array3[12] = 0.3f;
				break;
			}
			case EStarType.NeutronStar:
			{
				p = 4.5f;
				array[14]++;
				for (int m = 1; m < 12; m++)
				{
					if (dotNet35Random.NextDouble() >= 0.6499999761581421)
					{
						break;
					}
					array[14]++;
				}
				array2[14] = 0.7f;
				array3[14] = 0.3f;
				break;
			}
			case EStarType.BlackHole:
			{
				p = 5f;
				array[14]++;
				for (int i = 1; i < 12; i++)
				{
					if (dotNet35Random.NextDouble() >= 0.6499999761581421)
					{
						break;
					}
					array[14]++;
				}
				array2[14] = 0.7f;
				array3[14] = 0.3f;
				break;
			}
			}
			for (int n = 0; n < themeProto.RareVeins.Length; n++)
			{
				int num2 = themeProto.RareVeins[n];
				float num3 = ((planet.star.index == 0) ? themeProto.RareSettings[n * 4] : themeProto.RareSettings[n * 4 + 1]);
				float num4 = themeProto.RareSettings[n * 4 + 2];
				float num5 = themeProto.RareSettings[n * 4 + 3];
				float num6 = num5;
				num3 = 1f - Mathf.Pow(1f - num3, p);
				num5 = 1f - Mathf.Pow(1f - num5, p);
				num6 = 1f - Mathf.Pow(1f - num6, p);
				if (!(dotNet35Random.NextDouble() < (double)num3))
				{
					continue;
				}
				array[num2]++;
				array2[num2] = num5;
				array3[num2] = num5;
				for (int num7 = 1; num7 < 12; num7++)
				{
					if (dotNet35Random.NextDouble() >= (double)num4)
					{
						break;
					}
					array[num2]++;
				}
			}
			bool flag = planet.galaxy.birthPlanetId == planet.id;
			if (flag)
			{
				planet.GenBirthPoints(data, birthSeed);
			}
			float num8 = planet.star.resourceCoef;
			bool isInfiniteResource = GameMain.data.gameDesc.isInfiniteResource;
			bool isRareResource = GameMain.data.gameDesc.isRareResource;
			if (flag)
			{
				num8 *= 2f / 3f;
			}
			else if (isRareResource)
			{
				if (num8 > 1f)
				{
					num8 = Mathf.Pow(num8, 0.8f);
				}
				num8 *= 0.7f;
			}
			float num9 = 1f;
			num9 *= 1.1f;
			Array.Clear(veinVectors, 0, veinVectors.Length);
			Array.Clear(veinVectorTypes, 0, veinVectorTypes.Length);
			veinVectorCount = 0;
			Vector3 birthPoint;
			if (flag)
			{
				birthPoint = planet.birthPoint;
				birthPoint.Normalize();
				birthPoint *= 0.75f;
			}
			else
			{
				birthPoint.x = (float)dotNet35Random2.NextDouble() * 2f - 1f;
				birthPoint.y = (float)dotNet35Random2.NextDouble() - 0.5f;
				birthPoint.z = (float)dotNet35Random2.NextDouble() * 2f - 1f;
				birthPoint.Normalize();
				birthPoint *= (float)(dotNet35Random2.NextDouble() * 0.4 + 0.2);
			}
			planet.veinBiasVector = birthPoint;
			if (flag)
			{
				veinVectorTypes[0] = EVeinType.Iron;
				veinVectors[0] = planet.birthResourcePoint0;
				veinVectorTypes[1] = EVeinType.Copper;
				veinVectors[1] = planet.birthResourcePoint1;
				veinVectorCount = 2;
			}
			for (int num10 = 1; num10 < 15; num10++)
			{
				if (veinVectorCount >= veinVectors.Length)
				{
					break;
				}
				EVeinType eVeinType = (EVeinType)num10;
				int num11 = array[num10];
				if (num11 > 1)
				{
					num11 += dotNet35Random2.Next(-1, 2);
				}
				for (int num12 = 0; num12 < num11; num12++)
				{
					int num13 = 0;
					Vector3 zero = Vector3.zero;
					bool flag2 = false;
					while (num13++ < 200)
					{
						zero.x = (float)dotNet35Random2.NextDouble() * 2f - 1f;
						zero.y = (float)dotNet35Random2.NextDouble() * 2f - 1f;
						zero.z = (float)dotNet35Random2.NextDouble() * 2f - 1f;
						if (eVeinType != EVeinType.Oil)
						{
							zero += birthPoint;
						}
						zero.Normalize();
						float num14 = data.QueryHeight(zero);
						if (num14 < planet.radius || (eVeinType == EVeinType.Oil && num14 < planet.radius + 0.5f))
						{
							continue;
						}
						bool flag3 = false;
						float num15 = ((eVeinType == EVeinType.Oil) ? 100f : 196f);
						for (int num16 = 0; num16 < veinVectorCount; num16++)
						{
							if ((veinVectors[num16] - zero).sqrMagnitude < num * num * num15)
							{
								flag3 = true;
								break;
							}
						}
						if (!flag3)
						{
							flag2 = true;
							break;
						}
					}
					if (flag2)
					{
						veinVectors[veinVectorCount] = zero;
						veinVectorTypes[veinVectorCount] = eVeinType;
						veinVectorCount++;
						if (veinVectorCount == veinVectors.Length)
						{
							break;
						}
					}
				}
			}
			data.veinCursor = 1;
			tmp_vecs.Clear();
			VeinData vein = default(VeinData);
			for (int num17 = 0; num17 < veinVectorCount; num17++)
			{
				tmp_vecs.Clear();
				Vector3 normalized = veinVectors[num17].normalized;
				EVeinType eVeinType2 = veinVectorTypes[num17];
				int num18 = (int)eVeinType2;
				Quaternion quaternion = Quaternion.FromToRotation(Vector3.up, normalized);
				Vector3 vector = quaternion * Vector3.right;
				Vector3 vector2 = quaternion * Vector3.forward;
				tmp_vecs.Add(Vector2.zero);
				int num19 = Mathf.RoundToInt(array2[num18] * (float)dotNet35Random2.Next(20, 25));
				if (eVeinType2 == EVeinType.Oil)
				{
					num19 = 1;
				}
				float num20 = array3[num18];
				if (flag && num17 < 2)
				{
					num19 = 6;
					num20 = 0.2f;
				}
				int num21 = 0;
				while (num21++ < 20)
				{
					int count = tmp_vecs.Count;
					for (int num22 = 0; num22 < count; num22++)
					{
						if (tmp_vecs.Count >= num19)
						{
							break;
						}
						if (tmp_vecs[num22].sqrMagnitude > 36f)
						{
							continue;
						}
						double num23 = dotNet35Random2.NextDouble() * Math.PI * 2.0;
						Vector2 vector3 = new Vector2((float)Math.Cos(num23), (float)Math.Sin(num23));
						vector3 += tmp_vecs[num22] * 0.2f;
						vector3.Normalize();
						Vector2 vector4 = tmp_vecs[num22] + vector3;
						bool flag4 = false;
						for (int num24 = 0; num24 < tmp_vecs.Count; num24++)
						{
							if ((tmp_vecs[num24] - vector4).sqrMagnitude < 0.85f)
							{
								flag4 = true;
								break;
							}
						}
						if (!flag4)
						{
							tmp_vecs.Add(vector4);
						}
					}
					if (tmp_vecs.Count >= num19)
					{
						break;
					}
				}
				float num25 = num8;
				if (eVeinType2 == EVeinType.Oil)
				{
					num25 = Mathf.Pow(num8, 0.5f);
				}
				int num26 = Mathf.RoundToInt(num20 * 100000f * num25);
				if (num26 < 20)
				{
					num26 = 20;
				}
				int num27 = ((num26 < 16000) ? Mathf.FloorToInt((float)num26 * 0.9375f) : 15000);
				int minValue = num26 - num27;
				int maxValue = num26 + num27 + 1;
				for (int num28 = 0; num28 < tmp_vecs.Count; num28++)
				{
					Vector3 vector5 = (tmp_vecs[num28].x * vector + tmp_vecs[num28].y * vector2) * num;
					vein.type = eVeinType2;
					vein.groupIndex = (short)(num17 + 1);
					vein.modelIndex = (short)dotNet35Random2.Next(veinModelIndexs[num18], veinModelIndexs[num18] + veinModelCounts[num18]);
					vein.amount = Mathf.RoundToInt((float)dotNet35Random2.Next(minValue, maxValue) * num9);
					if (eVeinType2 != EVeinType.Oil)
					{
						vein.amount = Mathf.RoundToInt((float)vein.amount * DSPGame.GameDesc.resourceMultiplier);
					}
					else
					{
						vein.amount = Mathf.RoundToInt((float)vein.amount * DSPGame.GameDesc.oilAmountMultiplier);
					}
					if (vein.amount < 1)
					{
						vein.amount = 1;
					}
					if (isInfiniteResource && vein.type != EVeinType.Oil)
					{
						vein.amount = 1000000000;
					}
					vein.productId = veinProducts[num18];
					vein.pos = normalized + vector5;
					if (vein.type == EVeinType.Oil)
					{
						vein.pos = planet.aux.RawSnap(vein.pos);
					}
					vein.minerCount = 0;
					float num29 = data.QueryHeight(vein.pos);
					data.EraseVegetableAtPoint(vein.pos);
					vein.pos = vein.pos.normalized * num29;
					if (planet.waterItemId == 0 || !(num29 < planet.radius))
					{
						data.AddVeinData(vein);
					}
				}
			}
			tmp_vecs.Clear();
		}
	}

	public static void CalcLandPercent(PlanetData _planet)
	{
		if (_planet == null)
		{
			return;
		}
		if (_planet.type == EPlanetType.Gas)
		{
			_planet.landPercent = 0f;
			return;
		}
		if (_planet.waterItemId == -2)
		{
			_planet.landPercent = 1f;
			return;
		}
		PlanetRawData data = _planet.data;
		if (data == null)
		{
			return;
		}
		int stride = data.stride;
		int num = stride / 2;
		int dataLength = data.dataLength;
		ushort[] heightData = data.heightData;
		if (heightData == null)
		{
			return;
		}
		float num2 = _planet.radius * 100f - 20f;
		int num3 = 0;
		int num4 = 0;
		for (int i = 0; i < dataLength; i++)
		{
			int num5 = i % stride;
			int num6 = i / stride;
			if (num5 > num)
			{
				num5--;
			}
			if (num6 > num)
			{
				num6--;
			}
			if ((num5 & 1) == 1 && (num6 & 1) == 1)
			{
				if ((float)(int)heightData[i] >= num2)
				{
					num4++;
				}
				else if (data.GetModLevel(i) == 3)
				{
					num4++;
				}
				num3++;
			}
		}
		_planet.landPercent = ((num3 > 0) ? ((float)num4 / (float)num3) : 0f);
		_planet.landPercentDirtyFlag = false;
	}
}
