using System;
using System.Collections.Generic;
using UnityEngine;

public class PlanetAlgorithm11 : PlanetAlgorithm
{
	private Vector3[] veinVectors = new Vector3[512];

	private EVeinType[] veinVectorTypes = new EVeinType[512];

	private int veinVectorCount;

	private List<Vector2> tmp_vecs = new List<Vector2>(100);

	public override void GenerateTerrain(double modX, double modY)
	{
		double num = 0.007;
		double num2 = 0.007;
		double num3 = 0.007;
		double num4 = 0.002 * modX;
		double num5 = 0.002 * modX * 4.0;
		double num6 = 0.002 * modX;
		DotNet35Random dotNet35Random = new DotNet35Random(planet.seed);
		int num7 = dotNet35Random.Next();
		int num8 = dotNet35Random.Next();
		int num9 = dotNet35Random.Next();
		SimplexNoise simplexNoise = new SimplexNoise(num7);
		SimplexNoise simplexNoise2 = new SimplexNoise(num8);
		SimplexNoise simplexNoise3 = new SimplexNoise(num9);
		PlanetRawData data = planet.data;
		for (int i = 0; i < data.dataLength; i++)
		{
			double num10 = data.vertices[i].x * planet.radius;
			double num11 = data.vertices[i].y * planet.radius;
			double num12 = data.vertices[i].z * planet.radius;
			double num13 = 0.0;
			double num14 = 0.0;
			double num15 = simplexNoise2.Noise3DFBM(num10 * num * 4.0, num11 * num2 * 8.0, num12 * num3 * 4.0, 3);
			double num16 = 0.6;
			double x = simplexNoise.Noise3DFBM(num10 * num * num16, num11 * num * 1.5 * 2.5, num12 * num * num16, 6, 0.45, 1.8) * 0.95 + num15 * 0.05;
			x = Remap(-1.0, 1.0, 0.0, 1.0, x);
			x = Math.Pow(x, modY);
			x++;
			x = Maths.Levelize2(x);
			double x2 = simplexNoise3.Noise3DFBM(num10 * num4, num11 * num5, num12 * num6, 5, 0.55);
			x2 = Remap(-1.0, 1.0, 0.0, 1.0, x2);
			x2 = Math.Pow(x2, 0.65);
			num14 = Maths.Levelize3(x2) * x;
			num13 = (num14 - 0.4) * 0.9;
			num13 = Math.Max(-0.3, num13);
			data.heightData[i] = (ushort)(((double)planet.radius + num13) * 100.0);
			data.biomoData[i] = (byte)Mathf.Clamp((float)(num14 * 100.0), 0f, 200f);
		}
	}

	private double Remap(double sourceMin, double sourceMax, double targetMin, double targetMax, double x)
	{
		return (x - sourceMin) / (sourceMax - sourceMin) * (targetMax - targetMin) + targetMin;
	}

	public override void GenerateVegetables()
	{
		ThemeProto themeProto = LDB.themes.Select(planet.theme);
		if (themeProto == null)
		{
			return;
		}
		int[] vegetables = themeProto.Vegetables0;
		int[] vegetables2 = themeProto.Vegetables1;
		int[] vegetables3 = themeProto.Vegetables2;
		double num = 0.005;
		double num2 = 0.02;
		double num3 = 0.005;
		float num4 = 0.25f;
		float num5 = -0.5f;
		float num6 = 1f;
		float num7 = 2f;
		float num8 = -0.5f;
		float num9 = 1.4f;
		DotNet35Random dotNet35Random = new DotNet35Random(planet.seed);
		dotNet35Random.Next();
		dotNet35Random.Next();
		dotNet35Random.Next();
		DotNet35Random dotNet35Random2 = new DotNet35Random(dotNet35Random.Next());
		SimplexNoise simplexNoise = new SimplexNoise(dotNet35Random2.Next());
		SimplexNoise simplexNoise2 = new SimplexNoise(dotNet35Random2.Next());
		PlanetRawData data = planet.data;
		int stride = data.stride;
		int num10 = stride / 2;
		float num11 = planet.radius * 3.14159f * 2f / ((float)data.precision * 4f);
		VegeData vege = default(VegeData);
		VegeProto[] vegeProtos = PlanetModelingManager.vegeProtos;
		Vector4[] vegeScaleRanges = PlanetModelingManager.vegeScaleRanges;
		_ = PlanetModelingManager.vegeHps;
		for (int i = 0; i < data.dataLength; i++)
		{
			int num12 = i % stride;
			int num13 = i / stride;
			if (num12 > num10)
			{
				num12--;
			}
			if (num13 > num10)
			{
				num13--;
			}
			if (num12 % 2 != 1 || num13 % 2 != 1)
			{
				continue;
			}
			Vector3 vector = data.vertices[i];
			double num14 = data.vertices[i].x * planet.radius;
			double num15 = data.vertices[i].y * planet.radius;
			double num16 = data.vertices[i].z * planet.radius;
			float num17 = (float)(int)data.heightData[i] * 0.01f;
			float num18 = (float)(int)data.biomoData[i] * 0.01f;
			double num19 = dotNet35Random2.NextDouble();
			double num20 = dotNet35Random2.NextDouble();
			float num21 = (float)dotNet35Random2.NextDouble() - 0.5f;
			float num22 = (float)dotNet35Random2.NextDouble() - 0.5f;
			float num23 = (float)Math.Sqrt(dotNet35Random2.NextDouble());
			float angle = (float)dotNet35Random2.NextDouble() * 360f;
			float num24 = (float)dotNet35Random2.NextDouble();
			float num25 = (float)dotNet35Random2.NextDouble();
			float num26 = 1f;
			float num27 = 0.5f;
			float num28 = 1f;
			int[] array = null;
			int[] array2 = null;
			if (num18 < 0.5f)
			{
				array = null;
			}
			else if (num18 < 1.5f && num18 > 0.95f)
			{
				num = 0.5;
				num2 = 0.5;
				num3 = 0.5;
				num19 *= num19;
				array = vegetables2;
				num26 = num4;
				num27 = num5 + 0.1f;
				num28 = num6;
			}
			else if (num18 >= 1.5f)
			{
				num = 0.015;
				num2 = 0.03;
				num3 = 0.015;
				num19 *= num19;
				array = vegetables3;
				num26 = num4 * 2f;
				num27 = num5 + 0.05f;
				num28 = num6;
			}
			else
			{
				array = null;
			}
			double num29 = simplexNoise.Noise3DFBM(num14 * num, num15 * num2, num16 * num3, 2) * (double)num26 + (double)num27 + 0.5;
			double num30 = simplexNoise2.Noise(num14 * 0.4, num15 * 0.4, num16 * 0.4) * (double)num7 + (double)num8 + 0.5;
			double num31;
			int num32;
			if (num18 < 0.6f)
			{
				array2 = vegetables;
				num31 = num30;
				num32 = 2;
			}
			else
			{
				array2 = null;
				num31 = 0.0;
				num32 = 0;
			}
			if (num20 < num29 && array != null && array.Length != 0)
			{
				vege.protoId = (short)array[(int)(num19 * (double)array.Length)];
				Quaternion quaternion = Quaternion.FromToRotation(Vector3.up, vector);
				Vector3 vector2 = quaternion * Vector3.forward;
				Vector3 vector3 = quaternion * Vector3.right;
				Vector3 vector4 = vector * num17;
				Vector3 normalized = (vector3 * num21 + vector2 * num22).normalized;
				float num33 = num23 * num28;
				Vector3 vector5 = normalized * (num33 * num11);
				Vector4 vector6 = vegeScaleRanges[vege.protoId];
				float num34 = num25 * (vector6.x + vector6.y) + (1f - vector6.x);
				float num35 = (num24 * (vector6.z + vector6.w) + (1f - vector6.z)) * num34;
				vege.pos = (vector4 + vector5).normalized;
				num17 = data.QueryHeight(vege.pos);
				vege.pos *= num17;
				vege.rot = Quaternion.FromToRotation(Vector3.up, vege.pos.normalized) * Quaternion.AngleAxis(angle, Vector3.up);
				vege.scl = new Vector3(num35, num34, num35);
				vege.modelIndex = (short)vegeProtos[vege.protoId].ModelIndex;
				vege.hash.InitHashBits(vege.pos.x, vege.pos.y, vege.pos.z);
				int num36 = data.AddVegeData(vege);
				data.vegeIds[i] = (ushort)num36;
			}
			if (num20 < num31 && array2 != null && array2.Length != 0)
			{
				vege.protoId = (short)array2[(int)(num19 * (double)array2.Length)];
				Quaternion quaternion2 = Quaternion.FromToRotation(Vector3.up, vector);
				Vector3 vector7 = quaternion2 * Vector3.forward;
				Vector3 vector8 = quaternion2 * Vector3.right;
				Vector4 vector9 = vegeScaleRanges[vege.protoId];
				for (int j = 0; j < num32; j++)
				{
					float num37 = (float)dotNet35Random2.NextDouble() - 0.5f;
					float num38 = (float)dotNet35Random2.NextDouble() - 0.5f;
					float num39 = (float)Math.Sqrt(dotNet35Random2.NextDouble());
					float angle2 = (float)dotNet35Random2.NextDouble() * 360f;
					float num40 = (float)dotNet35Random2.NextDouble();
					float num41 = (float)dotNet35Random2.NextDouble();
					Vector3 vector10 = vector * num17;
					Vector3 normalized2 = (vector8 * num37 + vector7 * num38).normalized;
					float num42 = num39 * num9;
					Vector3 vector11 = normalized2 * (num42 * num11);
					float num43 = num41 * (vector9.x + vector9.y) + (1f - vector9.x);
					float num44 = (num40 * (vector9.z + vector9.w) + (1f - vector9.z)) * num43;
					vege.pos = (vector10 + vector11).normalized;
					num17 = data.QueryHeight(vege.pos);
					vege.pos *= num17;
					vege.rot = Quaternion.FromToRotation(Vector3.up, vege.pos.normalized) * Quaternion.AngleAxis(angle2, Vector3.up);
					vege.scl = new Vector3(num44, num43, num44);
					vege.modelIndex = (short)vegeProtos[vege.protoId].ModelIndex;
					vege.hash.InitHashBits(vege.pos.x, vege.pos.y, vege.pos.z);
					int num45 = data.AddVegeData(vege);
					data.vegeIds[i] = (ushort)num45;
				}
			}
		}
	}

	public override void GenerateVeins()
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
						if (num14 < planet.radius || (eVeinType == EVeinType.Oil && num14 < planet.radius + 0.5f) || ((int)eVeinType <= 2 && num14 > planet.radius + 0.7f) || ((eVeinType == EVeinType.Silicium || eVeinType == EVeinType.Titanium) && num14 <= planet.radius + 0.7f))
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
}
