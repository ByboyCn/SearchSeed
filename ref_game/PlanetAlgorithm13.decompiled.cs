using System;
using System.Collections.Generic;
using UnityEngine;

public class PlanetAlgorithm13 : PlanetAlgorithm
{
	private Vector3[] veinVectors = new Vector3[512];

	private EVeinType[] veinVectorTypes = new EVeinType[512];

	private int veinVectorCount;

	private List<Vector2> tmp_vecs = new List<Vector2>(100);

	private float[] notClampedBiomo;

	public override void GenerateTerrain(double modX, double modY)
	{
		double num = 0.007 * modX;
		double num2 = 0.007 * modX;
		double num3 = 0.007 * modX;
		SimplexNoise simplexNoise = new SimplexNoise(new DotNet35Random(planet.seed).Next());
		PlanetRawData data = planet.data;
		for (int i = 0; i < data.dataLength; i++)
		{
			double num4 = data.vertices[i].x * planet.radius;
			double num5 = data.vertices[i].y * planet.radius;
			double num6 = data.vertices[i].z * planet.radius;
			double num7 = 0.0;
			double num8 = 0.0;
			double x = Remap(-1.0, 1.0, 0.0, 1.0, simplexNoise.Noise3DFBM(num4 * num, num5 * num2, num6 * num3, 6));
			x = Math.Pow(x, modY) * 3.0625;
			x = Remap(0.0, 2.0, 0.0, 4.0, x);
			if (x < 1.0)
			{
				x = Math.Pow(x, 2.0);
			}
			x -= 0.2;
			num8 = Math.Min(x, 4.0);
			Math.Max(1.0 - Math.Abs(1.0 - num8), 0.0);
			if (num8 > 2.0)
			{
				num8 = ((!(num8 > 3.0)) ? (2.0 - 1.0 * (num8 - 2.0)) : ((!(num8 > 3.5)) ? 1.0 : (1.0 + 2.0 * (num8 - 3.5))));
			}
			num7 = num8;
			data.heightData[i] = (ushort)(((double)planet.radius + num7 + 0.1) * 100.0);
			data.biomoData[i] = (byte)Mathf.Clamp((float)(num8 * 100.0), 0f, 200f);
		}
	}

	private double Remap(double sourceMin, double sourceMax, double targetMin, double targetMax, double x)
	{
		return (x - sourceMin) / (sourceMax - sourceMin) * (targetMax - targetMin) + targetMin;
	}

	private static float diff(float a, float b)
	{
		if (!(a > b))
		{
			return b - a;
		}
		return a - b;
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
		int[] vegetables4 = themeProto.Vegetables3;
		int[] vegetables5 = themeProto.Vegetables4;
		int[] vegetables6 = themeProto.Vegetables5;
		double num = 0.005;
		double num2 = 0.02;
		double num3 = 0.005;
		float num4 = 0.18f;
		float num5 = -0.34f;
		float num6 = 2.5f;
		float num7 = 2f;
		float num8 = -0.2f;
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
		ComputeRawBiomo();
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
			float b = (float)(int)data.heightData[i + 1 + stride] * 0.01f;
			float b2 = (float)(int)data.heightData[i - 1 + stride] * 0.01f;
			float b3 = (float)(int)data.heightData[i + 1 - stride] * 0.01f;
			float b4 = (float)(int)data.heightData[i - 1 - stride] * 0.01f;
			_ = data.heightData[i + 1];
			_ = data.heightData[i - 1];
			_ = data.heightData[i + stride];
			_ = data.heightData[i - stride];
			float num18 = (float)(int)data.biomoData[i] * 0.01f;
			float num19 = notClampedBiomo[i];
			float num20 = planet.radius + 0.15f;
			bool flag = false;
			bool flag2 = true;
			if (diff(num17, b) > 0.2f)
			{
				flag2 = false;
			}
			if (diff(num17, b2) > 0.2f)
			{
				flag2 = false;
			}
			if (diff(num17, b3) > 0.2f)
			{
				flag2 = false;
			}
			if (diff(num17, b4) > 0.2f)
			{
				flag2 = false;
			}
			double num21 = dotNet35Random2.NextDouble();
			num21 *= num21;
			double num22 = dotNet35Random2.NextDouble();
			float num23 = (float)dotNet35Random2.NextDouble() - 0.5f;
			float num24 = (float)dotNet35Random2.NextDouble() - 0.5f;
			float num25 = (float)Math.Sqrt(dotNet35Random2.NextDouble());
			float angle = (float)dotNet35Random2.NextDouble() * 360f;
			float num26 = (float)dotNet35Random2.NextDouble();
			float num27 = (float)dotNet35Random2.NextDouble();
			float num28 = 1f;
			float num29 = 0.5f;
			float num30 = 1f;
			int[] array;
			if (!flag)
			{
				if (num19 < 0.3f)
				{
					array = vegetables;
					num28 = num4;
					num29 = num5;
					num30 = num6;
					num = 0.005;
					num2 = 0.02;
					num3 = 0.005;
				}
				else if (num19 >= 2.5f)
				{
					array = vegetables4;
					num28 = num4;
					num29 = num5 + 0.1f;
					num30 = num6;
					num = 0.005;
					num2 = 0.005;
					num3 = 0.005;
				}
				else if (num19 < 0.7f)
				{
					array = vegetables5;
					num28 = num4;
					num29 = num5 + 0.05f;
					num30 = num6;
					num = 0.005;
					num2 = 0.005;
					num3 = 0.005;
				}
				else
				{
					array = vegetables2;
					num28 = num4;
					num29 = num5 + 0.05f;
					num30 = num6;
					num = 0.005;
					num2 = 0.005;
					num3 = 0.005;
				}
			}
			else
			{
				array = null;
			}
			double num31 = simplexNoise.Noise(num14 * num, num15 * num2, num16 * num3) * (double)num28 + (double)num29 + 0.5;
			double num32 = simplexNoise2.Noise(num14 * 0.4, num15 * 0.4, num16 * 0.4) * (double)num7 + (double)num8 + 0.5;
			double num33 = num32 - 0.55;
			double num34 = num32 - 1.1;
			int[] array2;
			double num35;
			int num36;
			if (!flag)
			{
				if (num18 > 1f)
				{
					array2 = vegetables3;
					num35 = num32;
					num36 = ((vegetables6 == null || vegetables6.Length == 0) ? 4 : 2);
				}
				else if (num18 > 0.5f)
				{
					array2 = null;
					num35 = num33;
					num36 = 1;
				}
				else if (num18 > 0f)
				{
					array2 = null;
					num35 = num33;
					num36 = 1;
				}
				else
				{
					array2 = null;
					num35 = num32;
					num36 = 1;
				}
			}
			else
			{
				if (!(num17 < num20 - 1f) || !(num17 > num20 - 2.2f))
				{
					continue;
				}
				array2 = vegetables6;
				num35 = num34;
				num36 = 1;
			}
			if (flag2 && num22 < num31 && array != null && array.Length != 0)
			{
				vege.protoId = (short)array[(int)(num21 * (double)array.Length)];
				Quaternion quaternion = Quaternion.FromToRotation(Vector3.up, vector);
				Vector3 vector2 = quaternion * Vector3.forward;
				Vector3 vector3 = quaternion * Vector3.right;
				Vector4 vector4 = vegeScaleRanges[vege.protoId];
				Vector3 vector5 = vector * num17;
				Vector3 normalized = (vector3 * num23 + vector2 * num24).normalized;
				float num37 = num25 * num30;
				Vector3 vector6 = normalized * (num37 * num11);
				float num38 = num27 * (vector4.x + vector4.y) + (1f - vector4.x);
				float num39 = (num26 * (vector4.z + vector4.w) + (1f - vector4.z)) * num38;
				vege.pos = (vector5 + vector6).normalized;
				num17 = data.QueryHeight(vege.pos);
				vege.pos *= num17;
				vege.rot = Quaternion.FromToRotation(Vector3.up, vege.pos.normalized) * Quaternion.AngleAxis(angle, Vector3.up);
				vege.scl = new Vector3(num39, num38, num39);
				vege.modelIndex = (short)vegeProtos[vege.protoId].ModelIndex;
				vege.hash.InitHashBits(vege.pos.x, vege.pos.y, vege.pos.z);
				int num40 = data.AddVegeData(vege);
				data.vegeIds[i] = (ushort)num40;
			}
			if (num22 < num35 && array2 != null && array2.Length != 0)
			{
				vege.protoId = (short)array2[(int)(num21 * (double)array2.Length)];
				Quaternion quaternion2 = Quaternion.FromToRotation(Vector3.up, vector);
				Vector3 vector7 = quaternion2 * Vector3.forward;
				Vector3 vector8 = quaternion2 * Vector3.right;
				Vector4 vector9 = vegeScaleRanges[vege.protoId];
				for (int j = 0; j < num36; j++)
				{
					float num41 = (float)dotNet35Random2.NextDouble() - 0.5f;
					float num42 = (float)dotNet35Random2.NextDouble() - 0.5f;
					float num43 = (float)Math.Sqrt(dotNet35Random2.NextDouble());
					float angle2 = (float)dotNet35Random2.NextDouble() * 360f;
					float num44 = (float)dotNet35Random2.NextDouble();
					float num45 = (float)dotNet35Random2.NextDouble();
					Vector3 vector10 = vector * num17;
					Vector3 normalized2 = (vector8 * num41 + vector7 * num42).normalized;
					float num46 = num43 * num9;
					Vector3 vector11 = normalized2 * (num46 * num11);
					float num47 = num45 * (vector9.x + vector9.y) + (1f - vector9.x);
					float num48 = (num44 * (vector9.z + vector9.w) + (1f - vector9.z)) * num47;
					vege.pos = (vector10 + vector11).normalized;
					num17 = (flag ? num20 : data.QueryHeight(vege.pos));
					vege.pos *= num17;
					vege.rot = Quaternion.FromToRotation(Vector3.up, vege.pos.normalized) * Quaternion.AngleAxis(angle2, Vector3.up);
					vege.scl = new Vector3(num48, num47, num48);
					vege.modelIndex = (short)vegeProtos[vege.protoId].ModelIndex;
					vege.hash.InitHashBits(vege.pos.x, vege.pos.y, vege.pos.z);
					int num49 = data.AddVegeData(vege);
					data.vegeIds[i] = (ushort)num49;
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
						if (num14 < planet.radius || (eVeinType == EVeinType.Oil && num14 < planet.radius + 0.5f) || ((int)eVeinType <= 4 && num14 > planet.radius + 0.7f))
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

	public void ComputeRawBiomo()
	{
		double num = 0.007 * planet.mod_x;
		double num2 = 0.007 * planet.mod_x;
		double num3 = 0.007 * planet.mod_x;
		SimplexNoise simplexNoise = new SimplexNoise(new DotNet35Random(planet.seed).Next());
		notClampedBiomo = new float[planet.data.dataLength];
		PlanetRawData data = planet.data;
		for (int i = 0; i < data.dataLength; i++)
		{
			double num4 = data.vertices[i].x * planet.radius;
			double num5 = data.vertices[i].y * planet.radius;
			double num6 = data.vertices[i].z * planet.radius;
			double num7 = 0.0;
			double x = Remap(-1.0, 1.0, 0.0, 1.0, simplexNoise.Noise3DFBM(num4 * num, num5 * num2, num6 * num3, 6));
			x = Math.Pow(x, planet.mod_y) * 3.0625;
			x = Remap(0.0, 2.0, 0.0, 4.0, x);
			if (x < 1.0)
			{
				x = Math.Pow(x, 2.0);
			}
			x -= 0.2;
			num7 = Math.Min(x, 4.0);
			notClampedBiomo[i] = (float)num7;
		}
	}
}
