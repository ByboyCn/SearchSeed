using System;
using UnityEngine;

public class PlanetAlgorithm6 : PlanetAlgorithm
{
	public override void GenerateTerrain(double modX, double modY)
	{
		DotNet35Random dotNet35Random = new DotNet35Random(planet.seed);
		int num = dotNet35Random.Next();
		int num2 = dotNet35Random.Next();
		SimplexNoise simplexNoise = new SimplexNoise(num);
		SimplexNoise simplexNoise2 = new SimplexNoise(num2);
		PlanetRawData data = planet.data;
		for (int i = 0; i < data.dataLength; i++)
		{
			double num3 = data.vertices[i].x * planet.radius;
			double num4 = data.vertices[i].y * planet.radius;
			double num5 = data.vertices[i].z * planet.radius;
			double num6 = 0.0;
			double num7 = 0.0;
			double num8 = Maths.Levelize(num3 * 0.007);
			double num9 = Maths.Levelize(num4 * 0.007);
			double num10 = Maths.Levelize(num5 * 0.007);
			num8 += simplexNoise.Noise(num3 * 0.05, num4 * 0.05, num5 * 0.05) * 0.04;
			num9 += simplexNoise.Noise(num4 * 0.05, num5 * 0.05, num3 * 0.05) * 0.04;
			num10 += simplexNoise.Noise(num5 * 0.05, num3 * 0.05, num4 * 0.05) * 0.04;
			double num11 = Math.Abs(simplexNoise2.Noise(num8, num9, num10));
			double num12 = (0.16 - num11) * 10.0;
			num12 = ((!(num12 > 0.0)) ? 0.0 : ((num12 > 1.0) ? 1.0 : num12));
			num12 *= num12;
			double num13 = (simplexNoise.Noise3DFBM(num4 * 0.005, num5 * 0.005, num3 * 0.005, 4) + 0.22) * 5.0;
			num13 = ((!(num13 > 0.0)) ? 0.0 : ((num13 > 1.0) ? 1.0 : num13));
			double num14 = Math.Abs(simplexNoise2.Noise3DFBM(num8 * 1.5, num9 * 1.5, num10 * 1.5, 2));
			num6 -= num12 * 1.2 * num13;
			if (num6 >= 0.0)
			{
				num6 += num11 * 0.25 + num14 * 0.6;
			}
			num6 -= 0.1;
			double num15 = -0.3 - num6;
			if (num15 > 0.0)
			{
				num15 = ((num15 > 1.0) ? 1.0 : num15);
				num15 = (3.0 - num15 - num15) * num15 * num15;
				num6 = -0.3 - num15 * 3.700000047683716;
			}
			double f = ((num12 > 0.30000001192092896) ? num12 : 0.30000001192092896);
			f = Maths.Levelize(f, 0.7);
			num6 = ((num6 > -0.800000011920929) ? num6 : ((0.0 - f - num11) * 0.8999999761581421));
			num6 = ((num6 > -1.2000000476837158) ? num6 : (-1.2000000476837158));
			num7 = num6 * num12;
			num7 += num11 * 2.1 + 0.800000011920929;
			if (num7 > 1.7000000476837158 && num7 < 2.0)
			{
				num7 = 2.0;
			}
			data.heightData[i] = (ushort)(((double)planet.radius + num6 + 0.2) * 100.0);
			data.biomoData[i] = (byte)Mathf.Clamp((float)(num7 * 100.0), 0f, 200f);
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
			float num18 = (float)(int)data.heightData[i + 1 + stride] * 0.01f;
			float num19 = (float)(int)data.heightData[i - 1 + stride] * 0.01f;
			float num20 = (float)(int)data.heightData[i + 1 - stride] * 0.01f;
			float num21 = (float)(int)data.heightData[i - 1 - stride] * 0.01f;
			float num22 = (float)(int)data.heightData[i + 1] * 0.01f;
			float num23 = (float)(int)data.heightData[i - 1] * 0.01f;
			float num24 = (float)(int)data.heightData[i + stride] * 0.01f;
			float num25 = (float)(int)data.heightData[i - stride] * 0.01f;
			float num26 = (float)(int)data.biomoData[i] * 0.01f;
			float num27 = planet.radius + 0.2f;
			if (num17 < num27 || num18 < num27 || num19 < num27 || num20 < num27 || num21 < num27 || num22 < num27 || num23 < num27 || num24 < num27 || num25 < num27)
			{
				continue;
			}
			bool flag = true;
			if (diff(num17, num18) > 0.2f)
			{
				flag = false;
			}
			if (diff(num17, num19) > 0.2f)
			{
				flag = false;
			}
			if (diff(num17, num20) > 0.2f)
			{
				flag = false;
			}
			if (diff(num17, num21) > 0.2f)
			{
				flag = false;
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
			if (num26 < 0.8f)
			{
				array = vegetables;
				num35 = num;
				num36 = num2;
				num37 = num3;
			}
			else if (num26 < 2f)
			{
				array = vegetables2;
				num35 = num4;
				num36 = num5;
				num37 = num6;
			}
			else
			{
				array = vegetables6;
				num35 = num4;
				num36 = num5;
				num37 = num6;
			}
			double num38 = simplexNoise.Noise(num14 * 0.07, num15 * 0.07, num16 * 0.07) * (double)num35 + (double)num36 + 0.5;
			double num39 = simplexNoise2.Noise(num14 * 0.4, num15 * 0.4, num16 * 0.4) * (double)num7 + (double)num8 + 0.5;
			double num40 = num39 - 0.55;
			int[] array2;
			double num41;
			int num42;
			if (num26 > 1f)
			{
				array2 = vegetables3;
				num41 = num39;
				num42 = 4;
			}
			else if (num26 > 0.5f)
			{
				array2 = vegetables4;
				num41 = num40;
				num42 = 1;
			}
			else if (num26 > 0f)
			{
				array2 = vegetables5;
				num41 = num40;
				num42 = 1;
			}
			else
			{
				array2 = null;
				num41 = num39;
				num42 = 1;
			}
			if (flag && num29 < num38 && array != null && array.Length != 0)
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
					num17 = data.QueryHeight(vege.pos);
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

	private static float diff(float a, float b)
	{
		if (!(a > b))
		{
			return b - a;
		}
		return a - b;
	}
}
