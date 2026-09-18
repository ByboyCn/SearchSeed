using System;
using System.Collections.Generic;
using UnityEngine;

public class PlanetAlgorithm7 : PlanetAlgorithm
{
	private Vector3[] veinVectors = new Vector3[512];

	private EVeinType[] veinVectorTypes = new EVeinType[512];

	private int veinVectorCount;

	private List<Vector2> tmp_vecs = new List<Vector2>(100);

	public override void GenerateTerrain(double modX, double modY)
	{
		double num = 0.008;
		double num2 = 0.01;
		double num3 = 0.01;
		double num4 = 3.0;
		double num5 = -2.4;
		double num6 = 0.9;
		double num7 = 0.5;
		double num8 = 2.5;
		double num9 = 0.3;
		DotNet35Random dotNet35Random = new DotNet35Random(planet.seed);
		int num10 = dotNet35Random.Next();
		int num11 = dotNet35Random.Next();
		SimplexNoise simplexNoise = new SimplexNoise(num10);
		SimplexNoise simplexNoise2 = new SimplexNoise(num11);
		PlanetRawData data = planet.data;
		for (int i = 0; i < data.dataLength; i++)
		{
			double num12 = data.vertices[i].x * planet.radius;
			double num13 = data.vertices[i].y * planet.radius;
			double num14 = data.vertices[i].z * planet.radius;
			double num15 = 0.0;
			double num16 = 0.0;
			double num17 = simplexNoise.Noise3DFBM(num12 * num, num13 * num2, num14 * num3, 6) * num4 + num5;
			double num18 = simplexNoise2.Noise3DFBM(num12 * 0.0025, num13 * 0.0025, num14 * 0.0025, 3) * num4 * num6 + num7;
			double num19 = ((num18 > 0.0) ? (num18 * 0.5) : num18);
			double num20 = num17 + num19;
			double num21 = ((num20 > 0.0) ? (num20 * 0.5) : (num20 * 1.6));
			double num22 = ((num21 > 0.0) ? Maths.Levelize3(num21, 0.7) : Maths.Levelize2(num21, 0.5));
			double num23 = simplexNoise2.Noise3DFBM(num12 * num * 2.5, num13 * num2 * 8.0, num14 * num3 * 2.5, 2) * 0.6 - 0.3;
			double num24 = num21 * num8 + num23 + num9;
			double num25 = ((num24 < 1.0) ? num24 : ((num24 - 1.0) * 0.8 + 1.0));
			num15 = num22;
			num16 = num25;
			data.heightData[i] = (ushort)(((double)planet.radius + num15) * 100.0);
			data.biomoData[i] = (byte)Mathf.Clamp((float)(num16 * 100.0), 0f, 200f);
		}
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
		float num = 1.3f;
		float num2 = -0.5f;
		float num3 = 2.5f;
		float num4 = 4f;
		float num5 = 0.5f;
		float num6 = 1f;
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
			float num18 = (float)(int)data.biomoData[i] * 0.01f;
			bool flag = true;
			if (diff(num17, b) > 0.2f)
			{
				flag = false;
			}
			if (diff(num17, b2) > 0.2f)
			{
				flag = false;
			}
			if (diff(num17, b3) > 0.2f)
			{
				flag = false;
			}
			if (diff(num17, b4) > 0.2f)
			{
				flag = false;
			}
			double num19 = dotNet35Random2.NextDouble();
			num19 *= num19;
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
			int[] array;
			if (num18 < 0.8f)
			{
				array = vegetables;
				num26 = num;
				num27 = num2;
				num28 = num3;
			}
			else if (num18 < 2f)
			{
				array = vegetables2;
				num26 = num4;
				num27 = num5;
				num28 = num6;
			}
			else
			{
				array = vegetables6;
				num26 = num4;
				num27 = num5;
				num28 = num6;
			}
			double num29 = simplexNoise.Noise(num14 * 0.07, num15 * 0.07, num16 * 0.07) * (double)num26 + (double)num27 + 0.5;
			double num30 = simplexNoise2.Noise(num14 * 0.4, num15 * 0.4, num16 * 0.4) * (double)num7 + (double)num8 + 0.5;
			double num31 = num30 - 0.55;
			int[] array2;
			double num32;
			int num33;
			if (num18 > 1f)
			{
				array2 = vegetables3;
				num32 = num30;
				num33 = 4;
			}
			else if (num18 > 0.5f)
			{
				array2 = vegetables4;
				num32 = num31;
				num33 = 1;
			}
			else if (num18 > 0f)
			{
				array2 = vegetables5;
				num32 = num31;
				num33 = 1;
			}
			else
			{
				array2 = null;
				num32 = num30;
				num33 = 1;
			}
			if (flag && num20 < num29 && array != null && array.Length != 0)
			{
				vege.protoId = (short)array[(int)(num19 * (double)array.Length)];
				Quaternion quaternion = Quaternion.FromToRotation(Vector3.up, vector);
				Vector3 vector2 = quaternion * Vector3.forward;
				Vector3 vector3 = quaternion * Vector3.right;
				Vector4 vector4 = vegeScaleRanges[vege.protoId];
				Vector3 vector5 = vector * num17;
				Vector3 normalized = (vector3 * num21 + vector2 * num22).normalized;
				float num34 = num23 * num28;
				Vector3 vector6 = normalized * (num34 * num11);
				float num35 = num25 * (vector4.x + vector4.y) + (1f - vector4.x);
				float num36 = (num24 * (vector4.z + vector4.w) + (1f - vector4.z)) * num35;
				vege.pos = (vector5 + vector6).normalized;
				num17 = data.QueryHeight(vege.pos);
				vege.pos *= num17;
				vege.rot = Quaternion.FromToRotation(Vector3.up, vege.pos.normalized) * Quaternion.AngleAxis(angle, Vector3.up);
				vege.scl = new Vector3(num36, num35, num36);
				vege.modelIndex = (short)vegeProtos[vege.protoId].ModelIndex;
				vege.hash.InitHashBits(vege.pos.x, vege.pos.y, vege.pos.z);
				int num37 = data.AddVegeData(vege);
				data.vegeIds[i] = (ushort)num37;
			}
			if (num20 < num32 && array2 != null && array2.Length != 0)
			{
				vege.protoId = (short)array2[(int)(num19 * (double)array2.Length)];
				Quaternion quaternion2 = Quaternion.FromToRotation(Vector3.up, vector);
				Vector3 vector7 = quaternion2 * Vector3.forward;
				Vector3 vector8 = quaternion2 * Vector3.right;
				Vector4 vector9 = vegeScaleRanges[vege.protoId];
				for (int j = 0; j < num33; j++)
				{
					float num38 = (float)dotNet35Random2.NextDouble() - 0.5f;
					float num39 = (float)dotNet35Random2.NextDouble() - 0.5f;
					float num40 = (float)Math.Sqrt(dotNet35Random2.NextDouble());
					float angle2 = (float)dotNet35Random2.NextDouble() * 360f;
					float num41 = (float)dotNet35Random2.NextDouble();
					float num42 = (float)dotNet35Random2.NextDouble();
					Vector3 vector10 = vector * num17;
					Vector3 normalized2 = (vector8 * num38 + vector7 * num39).normalized;
					float num43 = num40 * num9;
					Vector3 vector11 = normalized2 * (num43 * num11);
					float num44 = num42 * (vector9.x + vector9.y) + (1f - vector9.x);
					float num45 = (num41 * (vector9.z + vector9.w) + (1f - vector9.z)) * num44;
					vege.pos = (vector10 + vector11).normalized;
					num17 = data.QueryHeight(vege.pos);
					vege.pos *= num17;
					vege.rot = Quaternion.FromToRotation(Vector3.up, vege.pos.normalized) * Quaternion.AngleAxis(angle2, Vector3.up);
					vege.scl = new Vector3(num45, num44, num45);
					vege.modelIndex = (short)vegeProtos[vege.protoId].ModelIndex;
					vege.hash.InitHashBits(vege.pos.x, vege.pos.y, vege.pos.z);
					int num46 = data.AddVegeData(vege);
					data.vegeIds[i] = (ushort)num46;
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
			dotNet35Random.Next();
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
			bool num8 = planet.galaxy.birthPlanetId == planet.id;
			float num9 = planet.star.resourceCoef;
			bool isInfiniteResource = GameMain.data.gameDesc.isInfiniteResource;
			bool isRareResource = GameMain.data.gameDesc.isRareResource;
			if (num8)
			{
				num9 *= 2f / 3f;
			}
			else if (isRareResource)
			{
				if (num9 > 1f)
				{
					num9 = Mathf.Pow(num9, 0.8f);
				}
				num9 *= 0.7f;
			}
			float num10 = 1f;
			num10 *= 1.1f;
			Array.Clear(veinVectors, 0, veinVectors.Length);
			Array.Clear(veinVectorTypes, 0, veinVectorTypes.Length);
			veinVectorCount = 0;
			Vector3 vector;
			if (planet.galaxy.birthPlanetId == planet.id)
			{
				Pose pose = planet.PredictPose(120.0);
				vector = Maths.QInvRotateLF(pose.rotation, planet.star.uPosition - (VectorLF3)pose.position * 40000.0);
				vector.Normalize();
				vector *= 0.75f;
			}
			else
			{
				vector.x = (float)dotNet35Random2.NextDouble() * 2f - 1f;
				vector.y = (float)dotNet35Random2.NextDouble() - 0.5f;
				vector.z = (float)dotNet35Random2.NextDouble() * 2f - 1f;
				vector.Normalize();
				vector *= (float)(dotNet35Random2.NextDouble() * 0.4 + 0.2);
			}
			planet.veinBiasVector = vector;
			for (int num11 = 1; num11 < 15; num11++)
			{
				if (veinVectorCount >= veinVectors.Length)
				{
					break;
				}
				EVeinType eVeinType = (EVeinType)num11;
				int num12 = array[num11];
				if (num12 > 1)
				{
					num12 += dotNet35Random2.Next(-1, 2);
				}
				for (int num13 = 0; num13 < num12; num13++)
				{
					int num14 = 0;
					Vector3 zero = Vector3.zero;
					bool flag = false;
					while (num14++ < 200)
					{
						zero.x = (float)dotNet35Random2.NextDouble() * 2f - 1f;
						zero.y = (float)dotNet35Random2.NextDouble() * 2f - 1f;
						zero.z = (float)dotNet35Random2.NextDouble() * 2f - 1f;
						if (eVeinType != EVeinType.Oil)
						{
							zero += vector;
						}
						zero.Normalize();
						if (eVeinType == EVeinType.Bamboo && data.QueryHeight(zero) > planet.realRadius - 4f)
						{
							continue;
						}
						bool flag2 = false;
						float num15 = ((eVeinType == EVeinType.Oil) ? 100f : 196f);
						for (int num16 = 0; num16 < veinVectorCount; num16++)
						{
							if ((veinVectors[num16] - zero).sqrMagnitude < num * num * num15)
							{
								flag2 = true;
								break;
							}
						}
						if (!flag2)
						{
							flag = true;
							break;
						}
					}
					if (flag)
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
				Vector3 vector2 = quaternion * Vector3.right;
				Vector3 vector3 = quaternion * Vector3.forward;
				tmp_vecs.Add(Vector2.zero);
				int num19 = Mathf.RoundToInt(array2[num18] * (float)dotNet35Random2.Next(20, 25));
				if (eVeinType2 == EVeinType.Oil)
				{
					num19 = 1;
				}
				int num20 = 0;
				while (num20++ < 20)
				{
					int count = tmp_vecs.Count;
					for (int num21 = 0; num21 < count; num21++)
					{
						if (tmp_vecs.Count >= num19)
						{
							break;
						}
						if (tmp_vecs[num21].sqrMagnitude > 36f)
						{
							continue;
						}
						double num22 = dotNet35Random2.NextDouble() * Math.PI * 2.0;
						Vector2 vector4 = new Vector2((float)Math.Cos(num22), (float)Math.Sin(num22));
						vector4 += tmp_vecs[num21] * 0.2f;
						vector4.Normalize();
						Vector2 vector5 = tmp_vecs[num21] + vector4;
						bool flag3 = false;
						for (int num23 = 0; num23 < tmp_vecs.Count; num23++)
						{
							if ((tmp_vecs[num23] - vector5).sqrMagnitude < 0.85f)
							{
								flag3 = true;
								break;
							}
						}
						if (!flag3)
						{
							tmp_vecs.Add(vector5);
						}
					}
					if (tmp_vecs.Count >= num19)
					{
						break;
					}
				}
				float num24 = num9;
				if (eVeinType2 == EVeinType.Oil)
				{
					num24 = Mathf.Pow(num9, 0.5f);
				}
				int num25 = Mathf.RoundToInt(array3[num18] * 100000f * num24);
				if (num25 < 20)
				{
					num25 = 20;
				}
				int num26 = ((num25 < 16000) ? Mathf.FloorToInt((float)num25 * 0.9375f) : 15000);
				int minValue = num25 - num26;
				int maxValue = num25 + num26 + 1;
				for (int num27 = 0; num27 < tmp_vecs.Count; num27++)
				{
					Vector3 vector6 = (tmp_vecs[num27].x * vector2 + tmp_vecs[num27].y * vector3) * num;
					vein.type = eVeinType2;
					vein.groupIndex = (short)(num17 + 1);
					vein.modelIndex = (short)dotNet35Random2.Next(veinModelIndexs[num18], veinModelIndexs[num18] + veinModelCounts[num18]);
					vein.amount = Mathf.RoundToInt((float)dotNet35Random2.Next(minValue, maxValue) * num10);
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
					vein.pos = normalized + vector6;
					if (vein.type == EVeinType.Oil)
					{
						vein.pos = planet.aux.RawSnap(vein.pos);
					}
					vein.minerCount = 0;
					float num28 = data.QueryHeight(vein.pos);
					data.EraseVegetableAtPoint(vein.pos);
					vein.pos = vein.pos.normalized * num28;
					data.AddVeinData(vein);
				}
			}
			tmp_vecs.Clear();
		}
	}

	private static float diff(float a, float b)
	{
		if (!(a > b))
		{
			return b - a;
		}
		return a - b;
	}
}
