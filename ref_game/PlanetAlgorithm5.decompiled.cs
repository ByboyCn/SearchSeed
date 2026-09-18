using System;
using UnityEngine;

public class PlanetAlgorithm5 : PlanetAlgorithm
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
			double num15 = simplexNoise.Noise3DFBM(num5 * 0.06, num4 * 0.06, num3 * 0.06, 2);
			num6 -= num12 * 1.2 * num13;
			if (num6 >= 0.0)
			{
				num6 += num11 * 0.25 + num14 * 0.6;
			}
			num6 -= 0.1;
			num7 = num11 * 2.1;
			if (num7 < 0.0)
			{
				num7 *= 5.0;
			}
			num7 = ((!(num7 > -1.0)) ? (-1.0) : ((num7 > 2.0) ? 2.0 : num7));
			num7 += num15 * 0.6 * num7;
			double num16 = -0.3 - num6;
			if (num16 > 0.0)
			{
				double num17 = simplexNoise2.Noise(num3 * 0.16, num4 * 0.16, num5 * 0.16) - 1.0;
				num16 = ((num16 > 1.0) ? 1.0 : num16);
				num16 = (3.0 - num16 - num16) * num16 * num16;
				num6 = -0.3 - num16 * 3.700000047683716 + num16 * num16 * num16 * num16 * num17 * 0.5;
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
		float num2 = -0.2f;
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
		ushort num12 = (ushort)((planet.radius + planet.waterHeight - 0.25f) * 100f);
		float num13 = planet.radius + planet.waterHeight + 0.5f;
		for (int i = 0; i < data.dataLength; i++)
		{
			int num14 = i % stride;
			int num15 = i / stride;
			if (num14 > num10)
			{
				num14--;
			}
			if (num15 > num10)
			{
				num15--;
			}
			if (num14 % 3 != 1 || num15 % 3 != 1 || num14 <= 3 || num15 <= 3 || (num14 >= num10 - 4 && num14 <= num10 + 4) || (num15 >= num10 - 4 && num15 <= num10 + 4) || num14 >= stride - 4 || num15 >= stride - 4)
			{
				continue;
			}
			Vector3 vector = data.vertices[i];
			double num16 = data.vertices[i].x * planet.radius;
			double num17 = data.vertices[i].y * planet.radius;
			double num18 = data.vertices[i].z * planet.radius;
			float num19 = (float)(int)data.heightData[i] * 0.01f;
			float num20 = (float)(int)data.heightData[i + 1 + stride] * 0.01f;
			float num21 = (float)(int)data.heightData[i - 1 + stride] * 0.01f;
			float num22 = (float)(int)data.heightData[i + 1 - stride] * 0.01f;
			float num23 = (float)(int)data.heightData[i - 1 - stride] * 0.01f;
			float num24 = (float)(int)data.heightData[i + 1] * 0.01f;
			float num25 = (float)(int)data.heightData[i - 1] * 0.01f;
			float num26 = (float)(int)data.heightData[i + stride] * 0.01f;
			float num27 = (float)(int)data.heightData[i - stride] * 0.01f;
			float num28 = (float)(int)data.biomoData[i] * 0.01f;
			bool flag = false;
			if (num19 < planet.radius)
			{
				flag = true;
			}
			if (num20 < planet.radius)
			{
				flag = true;
			}
			if (num21 < planet.radius)
			{
				flag = true;
			}
			if (num22 < planet.radius)
			{
				flag = true;
			}
			if (num23 < planet.radius)
			{
				flag = true;
			}
			if (num24 < planet.radius)
			{
				flag = true;
			}
			if (num25 < planet.radius)
			{
				flag = true;
			}
			if (num26 < planet.radius)
			{
				flag = true;
			}
			if (num27 < planet.radius)
			{
				flag = true;
			}
			bool flag2 = false;
			ushort num29 = data.heightData[i];
			ushort num30 = data.heightData[i + 2 + stride * 2];
			ushort num31 = data.heightData[i - 2 + stride * 2];
			ushort num32 = data.heightData[i + 2 - stride * 2];
			ushort num33 = data.heightData[i - 2 - stride * 2];
			ushort num34 = data.heightData[i + 2];
			ushort num35 = data.heightData[i - 2];
			ushort num36 = data.heightData[i + stride * 2];
			ushort num37 = data.heightData[i - stride * 2];
			if (num29 < num12 && num30 < num12 && num31 < num12 && num32 < num12 && num33 < num12 && num34 < num12 && num35 < num12 && num36 < num12 && num37 < num12)
			{
				flag2 = true;
			}
			bool flag3 = true;
			if (diff((int)num29, (int)num30) > 40f)
			{
				flag3 = false;
			}
			if (diff((int)num29, (int)num31) > 40f)
			{
				flag3 = false;
			}
			if (diff((int)num29, (int)num32) > 40f)
			{
				flag3 = false;
			}
			if (diff((int)num29, (int)num33) > 40f)
			{
				flag3 = false;
			}
			double num38 = dotNet35Random2.NextDouble();
			num38 *= num38;
			double num39 = dotNet35Random2.NextDouble();
			float num40 = (float)dotNet35Random2.NextDouble() - 0.5f;
			float num41 = (float)dotNet35Random2.NextDouble() - 0.5f;
			float num42 = (float)Math.Sqrt(dotNet35Random2.NextDouble());
			float angle = (float)dotNet35Random2.NextDouble() * 360f;
			float num43 = (float)dotNet35Random2.NextDouble();
			float num44 = (float)dotNet35Random2.NextDouble();
			float num45 = 1f;
			float num46 = 0.5f;
			float num47 = 1f;
			int[] array;
			if (num28 < 0.8f)
			{
				array = (flag ? vegetables5 : vegetables);
				num45 = num;
				num46 = num2;
				num47 = (flag ? num3 : (num3 * 1.5f));
			}
			else
			{
				array = vegetables2;
				num45 = num4;
				num46 = num5;
				num47 = num6;
			}
			double num48 = simplexNoise.Noise(num16 * 0.07, num17 * 0.07, num18 * 0.07) * (double)num45 + (double)num46 + 0.5;
			double num49 = simplexNoise2.Noise(num16 * 0.4, num17 * 0.4, num18 * 0.4) * (double)num7 + (double)num8 + 0.5;
			double num50 = num49 - 0.2;
			int[] array2;
			double num51;
			int num52;
			if (num28 > 1f)
			{
				array2 = (flag ? vegetables6 : vegetables3);
				num51 = num49;
				num52 = 4;
			}
			else
			{
				array2 = vegetables4;
				num51 = num50;
				num52 = 1;
			}
			if ((!flag & flag3) && num39 < num48 && array != null && array.Length != 0)
			{
				vege.protoId = (short)array[(int)(num38 * (double)array.Length)];
				Quaternion quaternion = Quaternion.FromToRotation(Vector3.up, vector);
				Vector3 vector2 = quaternion * Vector3.forward;
				Vector3 vector3 = quaternion * Vector3.right;
				Vector4 vector4 = vegeScaleRanges[vege.protoId];
				Vector3 vector5 = vector * num19;
				Vector3 normalized = (vector3 * num40 + vector2 * num41).normalized;
				float num53 = num42 * num47;
				Vector3 vector6 = normalized * (num53 * num11);
				float num54 = num44 * (vector4.x + vector4.y) + (1f - vector4.x);
				float num55 = (num43 * (vector4.z + vector4.w) + (1f - vector4.z)) * num54;
				vege.pos = (vector5 + vector6).normalized;
				num19 = data.QueryHeight(vege.pos);
				vege.pos *= num19;
				vege.rot = Quaternion.FromToRotation(Vector3.up, vege.pos.normalized) * Quaternion.AngleAxis(angle, Vector3.up);
				vege.scl = new Vector3(num55, num54, num55);
				vege.modelIndex = (short)vegeProtos[vege.protoId].ModelIndex;
				vege.hash.InitHashBits(vege.pos.x, vege.pos.y, vege.pos.z);
				int num56 = data.AddVegeData(vege);
				data.vegeIds[i] = (ushort)num56;
			}
			if ((flag & flag2) && num39 < num48 + 0.55 && array != null && array.Length != 0)
			{
				vege.protoId = (short)array[(int)(num38 * (double)array.Length)];
				Quaternion quaternion2 = Quaternion.FromToRotation(Vector3.up, vector);
				Vector3 vector7 = quaternion2 * Vector3.forward;
				Vector3 vector8 = quaternion2 * Vector3.right;
				Vector4 vector9 = vegeScaleRanges[vege.protoId];
				Vector3 vector10 = vector * num19;
				Vector3 normalized2 = (vector8 * num40 + vector7 * num41).normalized;
				float num57 = num42 * num47;
				Vector3 vector11 = normalized2 * (num57 * num11);
				float num58 = num44 * (vector9.x + vector9.y) + (1f - vector9.x);
				float num59 = (num43 * (vector9.z + vector9.w) + (1f - vector9.z)) * num58;
				vege.pos = (vector10 + vector11).normalized;
				vege.pos *= num13;
				vege.rot = Quaternion.FromToRotation(Vector3.up, vege.pos.normalized) * Quaternion.AngleAxis(angle, Vector3.up);
				vege.scl = new Vector3(num59, num58, num59);
				vege.modelIndex = (short)vegeProtos[vege.protoId].ModelIndex;
				vege.hash.InitHashBits(vege.pos.x, vege.pos.y, vege.pos.z);
				int num60 = data.AddVegeData(vege);
				data.vegeIds[i] = (ushort)num60;
			}
			if (!flag && num39 < num51 && array2 != null && array2.Length != 0)
			{
				vege.protoId = (short)array2[(int)(num38 * (double)array2.Length)];
				Quaternion quaternion3 = Quaternion.FromToRotation(Vector3.up, vector);
				Vector3 vector12 = quaternion3 * Vector3.forward;
				Vector3 vector13 = quaternion3 * Vector3.right;
				Vector4 vector14 = vegeScaleRanges[vege.protoId];
				for (int j = 0; j < num52; j++)
				{
					float num61 = (float)dotNet35Random2.NextDouble() - 0.5f;
					float num62 = (float)dotNet35Random2.NextDouble() - 0.5f;
					float num63 = (float)Math.Sqrt(dotNet35Random2.NextDouble());
					float angle2 = (float)dotNet35Random2.NextDouble() * 360f;
					float num64 = (float)dotNet35Random2.NextDouble();
					float num65 = (float)dotNet35Random2.NextDouble();
					Vector3 vector15 = vector * num19;
					Vector3 normalized3 = (vector13 * num61 + vector12 * num62).normalized;
					float num66 = num63 * num9;
					Vector3 vector16 = normalized3 * (num66 * num11);
					float num67 = num65 * (vector14.x + vector14.y) + (1f - vector14.x);
					float num68 = (num64 * (vector14.z + vector14.w) + (1f - vector14.z)) * num67;
					vege.pos = (vector15 + vector16).normalized;
					num19 = data.QueryHeight(vege.pos);
					vege.pos *= num19;
					vege.rot = Quaternion.FromToRotation(Vector3.up, vege.pos.normalized) * Quaternion.AngleAxis(angle2, Vector3.up);
					vege.scl = new Vector3(num68, num67, num68);
					vege.modelIndex = (short)vegeProtos[vege.protoId].ModelIndex;
					vege.hash.InitHashBits(vege.pos.x, vege.pos.y, vege.pos.z);
					int num69 = data.AddVegeData(vege);
					data.vegeIds[i] = (ushort)num69;
				}
			}
			if ((flag & flag2) && num39 < num51 && array2 != null && array2.Length != 0)
			{
				vege.protoId = (short)array2[(int)(num38 * (double)array2.Length)];
				Quaternion quaternion4 = Quaternion.FromToRotation(Vector3.up, vector);
				Vector3 vector17 = quaternion4 * Vector3.forward;
				Vector3 vector18 = quaternion4 * Vector3.right;
				Vector4 vector19 = vegeScaleRanges[vege.protoId];
				for (int k = 0; k < num52; k++)
				{
					float num70 = (float)dotNet35Random2.NextDouble() - 0.5f;
					float num71 = (float)dotNet35Random2.NextDouble() - 0.5f;
					float num72 = (float)Math.Sqrt(dotNet35Random2.NextDouble());
					float angle3 = (float)dotNet35Random2.NextDouble() * 360f;
					float num73 = (float)dotNet35Random2.NextDouble();
					float num74 = (float)dotNet35Random2.NextDouble();
					Vector3 vector20 = vector * num19;
					Vector3 normalized4 = (vector18 * num70 + vector17 * num71).normalized;
					float num75 = num72 * num9;
					Vector3 vector21 = normalized4 * (num75 * num11);
					float num76 = num74 * (vector19.x + vector19.y) + (1f - vector19.x);
					float num77 = (num73 * (vector19.z + vector19.w) + (1f - vector19.z)) * num76;
					vege.pos = (vector20 + vector21).normalized;
					vege.pos *= num13;
					vege.rot = Quaternion.FromToRotation(Vector3.up, vege.pos.normalized) * Quaternion.AngleAxis(angle3, Vector3.up);
					vege.scl = new Vector3(num77, num76, num77);
					vege.modelIndex = (short)vegeProtos[vege.protoId].ModelIndex;
					vege.hash.InitHashBits(vege.pos.x, vege.pos.y, vege.pos.z);
					int num78 = data.AddVegeData(vege);
					data.vegeIds[i] = (ushort)num78;
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
