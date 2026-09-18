using System;
using UnityEngine;

public class PlanetAlgorithm10 : PlanetAlgorithm
{
	private const int kCircleCount = 10;

	private Vector4[] ellipses = new Vector4[10];

	private double[] eccentricities = new double[10];

	private double[] heights = new double[10];

	public override void GenerateTerrain(double modX, double modY)
	{
		double num = 0.007;
		double num2 = 0.007;
		double num3 = 0.007;
		DotNet35Random dotNet35Random = new DotNet35Random(planet.seed);
		int num4 = dotNet35Random.Next();
		int num5 = dotNet35Random.Next();
		int num6 = dotNet35Random.Next();
		int num7 = dotNet35Random.Next();
		SimplexNoise simplexNoise = new SimplexNoise(num4);
		SimplexNoise simplexNoise2 = new SimplexNoise(num5);
		SimplexNoise simplexNoise3 = new SimplexNoise(num6);
		SimplexNoise simplexNoise4 = new SimplexNoise(num7);
		int num8 = dotNet35Random.Next();
		for (int i = 0; i < 10; i++)
		{
			VectorLF3 vectorLF = RandomTable.SphericNormal(ref num8, 1.0);
			Vector4 vector = new Vector4((float)vectorLF.x, (float)vectorLF.y, (float)vectorLF.z);
			vector.Normalize();
			vector *= planet.radius;
			vector.w = (float)(dotNet35Random.NextDouble() * 10.0 + 40.0);
			ellipses[i] = vector;
			if (dotNet35Random.NextDouble() > 0.5)
			{
				eccentricities[i] = Remap(0.0, 1.0, 3.0, 5.0, dotNet35Random.NextDouble());
			}
			else
			{
				eccentricities[i] = Remap(0.0, 1.0, 0.2, 1.0 / 3.0, dotNet35Random.NextDouble());
			}
			heights[i] = Remap(0.0, 1.0, 1.0, 2.0, dotNet35Random.NextDouble());
		}
		PlanetRawData data = planet.data;
		for (int j = 0; j < data.dataLength; j++)
		{
			double num9 = data.vertices[j].x * planet.radius;
			double num10 = data.vertices[j].y * planet.radius;
			double num11 = data.vertices[j].z * planet.radius;
			double num12 = Maths.Levelize(num9 * 0.007);
			double num13 = Maths.Levelize(num10 * 0.007);
			double num14 = Maths.Levelize(num11 * 0.007);
			num12 += simplexNoise3.Noise(num9 * 0.05, num10 * 0.05, num11 * 0.05) * 0.04;
			num13 += simplexNoise3.Noise(num10 * 0.05, num11 * 0.05, num9 * 0.05) * 0.04;
			num14 += simplexNoise3.Noise(num11 * 0.05, num9 * 0.05, num10 * 0.05) * 0.04;
			double num15 = Math.Abs(simplexNoise4.Noise(num12, num13, num14));
			double num16 = (0.16 - num15) * 10.0;
			num16 = ((!(num16 > 0.0)) ? 0.0 : ((num16 > 1.0) ? 1.0 : num16));
			num16 *= num16;
			double num17 = (simplexNoise3.Noise3DFBM(num10 * 0.005, num11 * 0.005, num9 * 0.005, 4) + 0.22) * 5.0;
			num17 = ((!(num17 > 0.0)) ? 0.0 : ((num17 > 1.0) ? 1.0 : num17));
			double num18 = Math.Abs(simplexNoise4.Noise3DFBM(num12 * 1.5, num13 * 1.5, num14 * 1.5, 2));
			double num19 = 0.0;
			double num20 = 0.0;
			double num21 = simplexNoise2.Noise3DFBM(num9 * num * 5.0, num10 * num2 * 5.0, num11 * num3 * 5.0, 4);
			double num22 = num21 * 0.2;
			double num23 = 0.0;
			for (int k = 0; k < 10; k++)
			{
				double num24 = (double)ellipses[k].x - num9;
				double num25 = (double)ellipses[k].y - num10;
				double num26 = (double)ellipses[k].z - num11;
				double num27 = eccentricities[k] * num24 * num24 + num25 * num25 + num26 * num26;
				num27 = Remap(-1.0, 1.0, 0.2, 5.0, num21) * num27;
				if (!(num27 >= (double)(ellipses[k].w * ellipses[k].w)))
				{
					double num28 = 1f - Mathf.Sqrt((float)(num27 / (double)(ellipses[k].w * ellipses[k].w)));
					double num29 = 1.0 - num28;
					double num30 = 1.0 - num29 * num29 * num29 * num29 + num22 * 2.0;
					if (num30 < 0.0)
					{
						num30 = 0.0;
					}
					num23 = Max(num23, heights[k] * num30);
				}
			}
			num9 += Math.Sin(num10 * 0.15) * 2.0;
			num10 += Math.Sin(num11 * 0.15) * 2.0;
			num11 += Math.Sin(num9 * 0.15) * 2.0;
			num9 *= num;
			num10 *= num2;
			num11 *= num3;
			double f = Mathf.Pow((float)((simplexNoise.Noise3DFBM(num9 * 0.6, num10 * 0.6, num11 * 0.6, 4, 0.5, 1.8) + 1.0) * 0.5), 1.3f);
			double x = simplexNoise2.Noise3DFBM(num9 * 6.0, num10 * 6.0, num11 * 6.0, 5);
			x = Remap(-1.0, 1.0, -0.1, 0.15, x);
			double num31 = simplexNoise2.Noise3DFBM(num9 * 5.0 * 3.0, num10 * 5.0, num11 * 5.0, 1);
			double num32 = simplexNoise2.Noise3DFBM(num9 * 5.0 * 3.0 + num31 * 0.3, num10 * 5.0 + num31 * 0.3, num11 * 5.0 + num31 * 0.3, 5) * 0.1;
			f = (float)Maths.Levelize(Maths.Levelize4(f));
			f = Math.Min(1.0, f);
			if (!(f > 0.8))
			{
				f = ((!(f > 0.4)) ? (f + x) : (f + num32));
			}
			double a = f * 2.5 - f * num23;
			num19 = Max(a, x * 2.0);
			double num33 = (2.0 - num19) / 2.0;
			num19 -= num16 * 1.2 * num17 * num33;
			if (num19 >= 0.0)
			{
				num19 += (num15 * 0.25 + num18 * 0.6) * num33;
			}
			num19 -= 0.1;
			num20 = num19;
			num20 = Max(num20, -1.0);
			num20 = Math.Abs(num20);
			double num34 = 100.0;
			if (f < 0.4)
			{
				num20 += Remap(-1.0, 1.0, -0.2, 0.2, simplexNoise.Noise3DFBM(num9 * 2.0 + num34, num10 * 2.0 + num34, num11 * 2.0 + num34, 5));
			}
			data.heightData[j] = (ushort)(((double)planet.radius + num19 + 0.1) * 100.0);
			data.biomoData[j] = (byte)Mathf.Clamp((float)(num20 * 100.0), 0f, 200f);
		}
	}

	private double Remap(double sourceMin, double sourceMax, double targetMin, double targetMax, double x)
	{
		return (x - sourceMin) / (sourceMax - sourceMin) * (targetMax - targetMin) + targetMin;
	}

	private double Max(double a, double b)
	{
		if (!(a > b))
		{
			return b;
		}
		return a;
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
		_ = themeProto.Vegetables4;
		int[] vegetables5 = themeProto.Vegetables5;
		float num = 1.3f;
		float num2 = -1f;
		float num3 = 2.5f;
		float num4 = 0.25f;
		float num5 = -0.45f;
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
			float num18 = (float)(int)data.heightData[i + 1 + stride] * 0.01f;
			float num19 = (float)(int)data.heightData[i - 1 + stride] * 0.01f;
			float num20 = (float)(int)data.heightData[i + 1 - stride] * 0.01f;
			float num21 = (float)(int)data.heightData[i - 1 - stride] * 0.01f;
			float num22 = (float)(int)data.heightData[i + 1] * 0.01f;
			float num23 = (float)(int)data.heightData[i - 1] * 0.01f;
			float num24 = (float)(int)data.heightData[i + stride] * 0.01f;
			float num25 = (float)(int)data.heightData[i - stride] * 0.01f;
			float num26 = (float)(int)data.biomoData[i] * 0.01f;
			float num27 = planet.radius - 0.1f;
			bool flag = false;
			if (num17 < num27)
			{
				flag = true;
			}
			else if (num18 < num27)
			{
				flag = true;
			}
			else if (num19 < num27)
			{
				flag = true;
			}
			else if (num20 < num27)
			{
				flag = true;
			}
			else if (num21 < num27)
			{
				flag = true;
			}
			else if (num22 < num27)
			{
				flag = true;
			}
			else if (num23 < num27)
			{
				flag = true;
			}
			else if (num24 < num27)
			{
				flag = true;
			}
			else if (num25 < num27)
			{
				flag = true;
			}
			if (flag && (vegetables5 == null || vegetables5.Length == 0))
			{
				continue;
			}
			bool flag2 = true;
			if (diff(num17, num18) > 0.2f)
			{
				flag2 = false;
			}
			if (diff(num17, num19) > 0.2f)
			{
				flag2 = false;
			}
			if (diff(num17, num20) > 0.2f)
			{
				flag2 = false;
			}
			if (diff(num17, num21) > 0.2f)
			{
				flag2 = false;
			}
			double num28 = dotNet35Random2.NextDouble();
			num28 *= num28;
			double num29 = dotNet35Random2.NextDouble();
			float num30 = (float)dotNet35Random2.NextDouble() - 0.5f;
			float num31 = (float)dotNet35Random2.NextDouble() - 0.5f;
			float num32 = (float)Math.Sqrt(dotNet35Random2.NextDouble());
			float angle = (float)dotNet35Random2.NextDouble() * 360f;
			float num33 = (float)dotNet35Random2.NextDouble();
			float num34 = (float)dotNet35Random2.NextDouble();
			float num35 = 1f;
			float num36 = 0.5f;
			float num37 = 1f;
			int[] array;
			if (!flag)
			{
				if (num26 < 0.8f)
				{
					array = vegetables;
					num35 = num;
					num36 = num2;
					num37 = num3;
				}
				else
				{
					array = vegetables2;
					num35 = num4;
					num36 = num5;
					num37 = num6;
				}
			}
			else
			{
				array = null;
			}
			double num38 = simplexNoise.Noise(num14 * 0.07, num15 * 0.07, num16 * 0.07) * (double)num35 + (double)num36 + 0.5;
			double num39 = simplexNoise2.Noise(num14 * 0.4, num15 * 0.4, num16 * 0.4) * (double)num7 + (double)num8 + 0.5;
			double num40 = num39 - 1.1;
			int[] array2;
			double num41;
			int num42;
			if (!flag)
			{
				if (num26 > 1f)
				{
					array2 = null;
					num41 = num39;
					num42 = ((vegetables5 == null || vegetables5.Length == 0) ? 4 : 2);
				}
				else if (num26 > 0.6f)
				{
					array2 = vegetables3;
					num41 = num39 - 0.55;
					num42 = 1;
				}
				else if (num26 > 0f)
				{
					array2 = vegetables4;
					num41 = num39;
					num42 = 2;
				}
				else
				{
					array2 = null;
					num41 = num39;
					num42 = 1;
				}
			}
			else
			{
				if (!(num17 < num27 - 1f) || !(num17 > num27 - 2.2f))
				{
					continue;
				}
				array2 = vegetables5;
				num41 = num40;
				num42 = 1;
			}
			if (flag2 && num29 < num38 && array != null && array.Length != 0)
			{
				vege.protoId = (short)array[(int)(num28 * (double)array.Length)];
				Quaternion quaternion = Quaternion.FromToRotation(Vector3.up, vector);
				Vector3 vector2 = quaternion * Vector3.forward;
				Vector3 vector3 = quaternion * Vector3.right;
				Vector4 vector4 = vegeScaleRanges[vege.protoId];
				Vector3 vector5 = vector * num17;
				Vector3 normalized = (vector3 * num30 + vector2 * num31).normalized;
				float num43 = num32 * num37;
				Vector3 vector6 = normalized * (num43 * num11);
				float num44 = num34 * (vector4.x + vector4.y) + (1f - vector4.x);
				float num45 = (num33 * (vector4.z + vector4.w) + (1f - vector4.z)) * num44;
				vege.pos = (vector5 + vector6).normalized;
				num17 = data.QueryHeight(vege.pos);
				vege.pos *= num17;
				vege.rot = Quaternion.FromToRotation(Vector3.up, vege.pos.normalized) * Quaternion.AngleAxis(angle, Vector3.up);
				vege.scl = new Vector3(num45, num44, num45);
				vege.modelIndex = (short)vegeProtos[vege.protoId].ModelIndex;
				vege.hash.InitHashBits(vege.pos.x, vege.pos.y, vege.pos.z);
				int num46 = data.AddVegeData(vege);
				data.vegeIds[i] = (ushort)num46;
			}
			if (num29 < num41 && array2 != null && array2.Length != 0)
			{
				vege.protoId = (short)array2[(int)(num28 * (double)array2.Length)];
				Quaternion quaternion2 = Quaternion.FromToRotation(Vector3.up, vector);
				Vector3 vector7 = quaternion2 * Vector3.forward;
				Vector3 vector8 = quaternion2 * Vector3.right;
				Vector4 vector9 = vegeScaleRanges[vege.protoId];
				for (int j = 0; j < num42; j++)
				{
					float num47 = (float)dotNet35Random2.NextDouble() - 0.5f;
					float num48 = (float)dotNet35Random2.NextDouble() - 0.5f;
					float num49 = (float)Math.Sqrt(dotNet35Random2.NextDouble());
					float angle2 = (float)dotNet35Random2.NextDouble() * 360f;
					float num50 = (float)dotNet35Random2.NextDouble();
					float num51 = (float)dotNet35Random2.NextDouble();
					Vector3 vector10 = vector * num17;
					Vector3 normalized2 = (vector8 * num47 + vector7 * num48).normalized;
					float num52 = num49 * num9;
					Vector3 vector11 = normalized2 * (num52 * num11);
					float num53 = num51 * (vector9.x + vector9.y) + (1f - vector9.x);
					float num54 = (num50 * (vector9.z + vector9.w) + (1f - vector9.z)) * num53;
					vege.pos = (vector10 + vector11).normalized;
					num17 = (flag ? num27 : data.QueryHeight(vege.pos));
					vege.pos *= num17;
					vege.rot = Quaternion.FromToRotation(Vector3.up, vege.pos.normalized) * Quaternion.AngleAxis(angle2, Vector3.up);
					vege.scl = new Vector3(num54, num53, num54);
					vege.modelIndex = (short)vegeProtos[vege.protoId].ModelIndex;
					vege.hash.InitHashBits(vege.pos.x, vege.pos.y, vege.pos.z);
					int num55 = data.AddVegeData(vege);
					data.vegeIds[i] = (ushort)num55;
				}
			}
		}
	}
}
