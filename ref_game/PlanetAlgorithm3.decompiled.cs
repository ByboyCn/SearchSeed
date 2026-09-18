using System;
using UnityEngine;

public class PlanetAlgorithm3 : PlanetAlgorithm
{
	public override void GenerateTerrain(double modX, double modY)
	{
		double num = 0.007;
		double num2 = 0.007;
		double num3 = 0.007;
		DotNet35Random dotNet35Random = new DotNet35Random(planet.seed);
		int num4 = dotNet35Random.Next();
		int num5 = dotNet35Random.Next();
		SimplexNoise simplexNoise = new SimplexNoise(num4);
		SimplexNoise simplexNoise2 = new SimplexNoise(num5);
		PlanetRawData data = planet.data;
		for (int i = 0; i < data.dataLength; i++)
		{
			double num6 = data.vertices[i].x * planet.radius;
			double num7 = data.vertices[i].y * planet.radius;
			double num8 = data.vertices[i].z * planet.radius;
			num6 += Math.Sin(num7 * 0.15) * 3.0;
			num7 += Math.Sin(num8 * 0.15) * 3.0;
			num8 += Math.Sin(num6 * 0.15) * 3.0;
			double num9 = 0.0;
			double num10 = 0.0;
			double num11 = simplexNoise.Noise3DFBM(num6 * num * 1.0, num7 * num2 * 1.1, num8 * num3 * 1.0, 6, 0.5, 1.8);
			double num12 = simplexNoise2.Noise3DFBM(num6 * num * 1.3 + 0.5, num7 * num2 * 2.8 + 0.2, num8 * num3 * 1.3 + 0.7, 3) * 2.0;
			double num13 = simplexNoise2.Noise3DFBM(num6 * num * 6.0, num7 * num2 * 12.0, num8 * num3 * 6.0, 2) * 2.0;
			num13 = Lerp(num13, num13 * 0.1, modX);
			double num14 = simplexNoise2.Noise3DFBM(num6 * num * 0.8, num7 * num2 * 0.8, num8 * num3 * 0.8, 2) * 2.0;
			double num15 = num11 * 2.0 + 0.92;
			double num16 = num12 * (double)Mathf.Abs((float)num14 + 0.5f);
			num15 += (double)Mathf.Clamp01((float)(num16 - 0.35) * 1f);
			if (num15 < 0.0)
			{
				num15 *= 2.0;
			}
			double num17 = num15;
			num17 = Maths.Levelize2(num15);
			if (num17 > 0.0)
			{
				num17 = Maths.Levelize2(num15);
				num17 = Lerp(Maths.Levelize4(num17), num17, modX);
			}
			double b = ((!(num17 > 0.0)) ? ((double)Mathf.Lerp(-1f, 0f, (float)num17 + 1f)) : ((!(num17 > 1.0)) ? ((double)Mathf.Lerp(0f, 0.3f, (float)num17) + num13 * 0.1) : ((num17 > 2.0) ? ((double)Mathf.Lerp(1.2f, 2f, (float)num17 - 2f) + num13 * 0.12) : ((double)Mathf.Lerp(0.3f, 1.2f, (float)num17 - 1f) + num13 * 0.12))));
			double a = ((!(num17 > 0.0)) ? ((double)Mathf.Lerp(-4f, 0f, (float)num17 + 1f)) : ((!(num17 > 1.0)) ? ((double)Mathf.Lerp(0f, 0.3f, (float)num17) + num13 * 0.1) : ((num17 > 2.0) ? ((double)Mathf.Lerp(1.4f, 2.7f, (float)num17 - 2f) + num13 * 0.12) : ((double)Mathf.Lerp(0.3f, 1.4f, (float)num17 - 1f) + num13 * 0.12))));
			double num18 = Lerp(a, b, modX);
			if (num15 < 0.0)
			{
				num15 *= 2.0;
			}
			if (num15 < 1.0)
			{
				num15 = Maths.Levelize(num15);
			}
			num9 = num18;
			num10 = Mathf.Abs((float)num15);
			num10 = ((num10 > 0.0) ? ((num10 > 2.0) ? 2.0 : num10) : 0.0);
			num10 += ((num10 > 1.8) ? ((0.0 - num13) * 0.8) : (num13 * 0.2));
			data.heightData[i] = (ushort)(((double)planet.radius + num9 + 0.2) * 100.0);
			data.biomoData[i] = (byte)Mathf.Clamp((float)(num10 * 100.0), 0f, 200f);
		}
	}

	private double Lerp(double a, double b, double t)
	{
		return a + (b - a) * t;
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
		float num = 1f;
		float x = themeProto.ModY.x;
		float num2 = 2.5f;
		float num3 = 1f;
		float x2 = themeProto.ModY.x;
		float num4 = 1f;
		float num5 = 0.7f;
		float y = themeProto.ModY.y;
		float num6 = 1.4f;
		DotNet35Random dotNet35Random = new DotNet35Random(planet.seed);
		dotNet35Random.Next();
		dotNet35Random.Next();
		dotNet35Random.Next();
		DotNet35Random dotNet35Random2 = new DotNet35Random(dotNet35Random.Next());
		SimplexNoise simplexNoise = new SimplexNoise(dotNet35Random2.Next());
		SimplexNoise simplexNoise2 = new SimplexNoise(dotNet35Random2.Next());
		PlanetRawData data = planet.data;
		int stride = data.stride;
		int num7 = stride / 2;
		float num8 = planet.radius * 3.14159f * 2f / ((float)data.precision * 4f);
		VegeData vege = default(VegeData);
		VegeProto[] vegeProtos = PlanetModelingManager.vegeProtos;
		Vector4[] vegeScaleRanges = PlanetModelingManager.vegeScaleRanges;
		_ = PlanetModelingManager.vegeHps;
		for (int i = 0; i < data.dataLength; i++)
		{
			int num9 = i % stride;
			int num10 = i / stride;
			if (num9 > num7)
			{
				num9--;
			}
			if (num10 > num7)
			{
				num10--;
			}
			if (num9 % 2 != 1 || num10 % 2 != 1)
			{
				continue;
			}
			Vector3 vector = data.vertices[i];
			double num11 = data.vertices[i].x * planet.radius;
			double num12 = data.vertices[i].y * planet.radius;
			double num13 = data.vertices[i].z * planet.radius;
			float num14 = (float)(int)data.heightData[i] * 0.01f;
			float num15 = (float)(int)data.biomoData[i] * 0.01f;
			double num16 = dotNet35Random2.NextDouble();
			num16 *= num16;
			double num17 = dotNet35Random2.NextDouble() * 1.5;
			double num18 = dotNet35Random2.NextDouble() * 1.5;
			float num19 = (float)dotNet35Random2.NextDouble() - 0.5f;
			float num20 = (float)dotNet35Random2.NextDouble() - 0.5f;
			float num21 = (float)Math.Sqrt(dotNet35Random2.NextDouble());
			float angle = (float)dotNet35Random2.NextDouble() * 360f;
			float num22 = (float)dotNet35Random2.NextDouble();
			float num23 = (float)dotNet35Random2.NextDouble();
			float num24 = 1f;
			float num25 = 0.5f;
			float num26 = 1f;
			int[] array;
			if (num15 < 0.8f)
			{
				array = vegetables;
				num24 = num;
				num25 = x;
				num26 = num2;
			}
			else
			{
				array = vegetables2;
				num24 = num3;
				num25 = x2;
				num26 = num4;
			}
			double num27 = simplexNoise.Noise3DFBM(num11 * 0.016, num12 * 0.016, num13 * 0.016, 2) * (double)num24 + (double)num25;
			double num28 = simplexNoise2.Noise(num11 * 0.016, num12 * 0.016, num13 * 0.016) * (double)num5 + (double)y;
			if (num27 < 0.0)
			{
				num27 = 0.0;
			}
			else if (num27 > 1.0)
			{
				num27 = 1.0;
			}
			if (num28 < 0.0)
			{
				num28 = 0.0;
			}
			else if (num28 > 1.0)
			{
				num28 = 1.0;
			}
			num27 *= num27;
			num28 *= num28;
			num27 = Maths.Levelize(num27);
			num28 = Maths.Levelize(num28);
			int[] array2;
			int num29;
			if (num15 < 0.8f)
			{
				array2 = vegetables3;
				num29 = 1;
			}
			else
			{
				array2 = vegetables4;
				num29 = 1;
			}
			if (num17 <= num27 && array != null && array.Length != 0)
			{
				vege.protoId = (short)array[(int)(num16 * (double)array.Length)];
				Quaternion quaternion = Quaternion.FromToRotation(Vector3.up, vector);
				Vector3 vector2 = quaternion * Vector3.forward;
				Vector3 vector3 = quaternion * Vector3.right;
				Vector3 vector4 = vector * num14;
				Vector3 normalized = (vector3 * num19 + vector2 * num20).normalized;
				float num30 = num21 * num26;
				Vector3 vector5 = normalized * (num30 * num8);
				Vector4 vector6 = vegeScaleRanges[vege.protoId];
				float num31 = num23 * (vector6.x + vector6.y) + (1f - vector6.x);
				float num32 = (num22 * (vector6.z + vector6.w) + (1f - vector6.z)) * num31;
				vege.pos = (vector4 + vector5).normalized;
				num14 = data.QueryHeight(vege.pos);
				vege.pos *= num14;
				vege.rot = Quaternion.FromToRotation(Vector3.up, vege.pos.normalized) * Quaternion.AngleAxis(angle, Vector3.up);
				vege.scl = new Vector3(num32, num31, num32);
				vege.modelIndex = (short)vegeProtos[vege.protoId].ModelIndex;
				vege.hash.InitHashBits(vege.pos.x, vege.pos.y, vege.pos.z);
				int num33 = data.AddVegeData(vege);
				data.vegeIds[i] = (ushort)num33;
			}
			if (num18 < num28 && array2 != null && array2.Length != 0)
			{
				vege.protoId = (short)array2[(int)(num16 * (double)array2.Length)];
				Quaternion quaternion2 = Quaternion.FromToRotation(Vector3.up, vector);
				Vector3 vector7 = quaternion2 * Vector3.forward;
				Vector3 vector8 = quaternion2 * Vector3.right;
				Vector4 vector9 = vegeScaleRanges[vege.protoId];
				for (int j = 0; j < num29; j++)
				{
					float num34 = (float)dotNet35Random2.NextDouble() - 0.5f;
					float num35 = (float)dotNet35Random2.NextDouble() - 0.5f;
					float num36 = (float)Math.Sqrt(dotNet35Random2.NextDouble());
					float angle2 = (float)dotNet35Random2.NextDouble() * 360f;
					float num37 = (float)dotNet35Random2.NextDouble();
					float num38 = (float)dotNet35Random2.NextDouble();
					Vector3 vector10 = vector * num14;
					Vector3 normalized2 = (vector8 * num34 + vector7 * num35).normalized;
					float num39 = num36 * num6;
					Vector3 vector11 = normalized2 * (num39 * num8);
					float num40 = num38 * (vector9.x + vector9.y) + (1f - vector9.x);
					float num41 = (num37 * (vector9.z + vector9.w) + (1f - vector9.z)) * num40;
					vege.pos = (vector10 + vector11).normalized;
					num14 = data.QueryHeight(vege.pos);
					vege.pos *= num14;
					vege.rot = Quaternion.FromToRotation(Vector3.up, vege.pos.normalized) * Quaternion.AngleAxis(angle2, Vector3.up);
					vege.scl = new Vector3(num41, num40, num41);
					vege.modelIndex = (short)vegeProtos[vege.protoId].ModelIndex;
					vege.hash.InitHashBits(vege.pos.x, vege.pos.y, vege.pos.z);
					int num42 = data.AddVegeData(vege);
					data.vegeIds[i] = (ushort)num42;
				}
			}
		}
	}
}
