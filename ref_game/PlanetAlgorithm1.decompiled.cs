using System;
using UnityEngine;

public class PlanetAlgorithm1 : PlanetAlgorithm
{
	public override void GenerateTerrain(double modX, double modY)
	{
		double num = 0.01;
		double num2 = 0.012;
		double num3 = 0.01;
		double num4 = 3.0;
		double num5 = -0.2;
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
			data.heightData[i] = (ushort)(((double)planet.radius + num15 + 0.2) * 100.0);
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
			float num18 = (float)(int)data.heightData[i + 1 + stride] * 0.01f;
			float num19 = (float)(int)data.heightData[i - 1 + stride] * 0.01f;
			float num20 = (float)(int)data.heightData[i + 1 - stride] * 0.01f;
			float num21 = (float)(int)data.heightData[i - 1 - stride] * 0.01f;
			float num22 = (float)(int)data.heightData[i + 1] * 0.01f;
			float num23 = (float)(int)data.heightData[i - 1] * 0.01f;
			float num24 = (float)(int)data.heightData[i + stride] * 0.01f;
			float num25 = (float)(int)data.heightData[i - stride] * 0.01f;
			float num26 = (float)(int)data.biomoData[i] * 0.01f;
			float num27 = planet.radius + 0.15f;
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
			if (flag && (vegetables6 == null || vegetables6.Length == 0))
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
			double num40 = num39 - 0.55;
			double num41 = num39 - 1.1;
			int[] array2;
			double num42;
			int num43;
			if (!flag)
			{
				if (num26 > 1f)
				{
					array2 = vegetables3;
					num42 = num39;
					num43 = ((vegetables6 == null || vegetables6.Length == 0) ? 4 : 2);
				}
				else if (num26 > 0.5f)
				{
					array2 = vegetables4;
					num42 = num40;
					num43 = 1;
				}
				else if (num26 > 0f)
				{
					array2 = vegetables5;
					num42 = num40;
					num43 = 1;
				}
				else
				{
					array2 = null;
					num42 = num39;
					num43 = 1;
				}
			}
			else
			{
				if (!(num17 < num27 - 1f) || !(num17 > num27 - 2.2f))
				{
					continue;
				}
				array2 = vegetables6;
				num42 = num41;
				num43 = 1;
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
				float num44 = num32 * num37;
				Vector3 vector6 = normalized * (num44 * num11);
				float num45 = num34 * (vector4.x + vector4.y) + (1f - vector4.x);
				float num46 = (num33 * (vector4.z + vector4.w) + (1f - vector4.z)) * num45;
				vege.pos = (vector5 + vector6).normalized;
				num17 = data.QueryHeight(vege.pos);
				vege.pos *= num17;
				vege.rot = Quaternion.FromToRotation(Vector3.up, vege.pos.normalized) * Quaternion.AngleAxis(angle, Vector3.up);
				vege.scl = new Vector3(num46, num45, num46);
				vege.modelIndex = (short)vegeProtos[vege.protoId].ModelIndex;
				vege.hash.InitHashBits(vege.pos.x, vege.pos.y, vege.pos.z);
				int num47 = data.AddVegeData(vege);
				data.vegeIds[i] = (ushort)num47;
			}
			if (num29 < num42 && array2 != null && array2.Length != 0)
			{
				vege.protoId = (short)array2[(int)(num28 * (double)array2.Length)];
				Quaternion quaternion2 = Quaternion.FromToRotation(Vector3.up, vector);
				Vector3 vector7 = quaternion2 * Vector3.forward;
				Vector3 vector8 = quaternion2 * Vector3.right;
				Vector4 vector9 = vegeScaleRanges[vege.protoId];
				for (int j = 0; j < num43; j++)
				{
					float num48 = (float)dotNet35Random2.NextDouble() - 0.5f;
					float num49 = (float)dotNet35Random2.NextDouble() - 0.5f;
					float num50 = (float)Math.Sqrt(dotNet35Random2.NextDouble());
					float angle2 = (float)dotNet35Random2.NextDouble() * 360f;
					float num51 = (float)dotNet35Random2.NextDouble();
					float num52 = (float)dotNet35Random2.NextDouble();
					Vector3 vector10 = vector * num17;
					Vector3 normalized2 = (vector8 * num48 + vector7 * num49).normalized;
					float num53 = num50 * num9;
					Vector3 vector11 = normalized2 * (num53 * num11);
					float num54 = num52 * (vector9.x + vector9.y) + (1f - vector9.x);
					float num55 = (num51 * (vector9.z + vector9.w) + (1f - vector9.z)) * num54;
					vege.pos = (vector10 + vector11).normalized;
					num17 = (flag ? num27 : data.QueryHeight(vege.pos));
					vege.pos *= num17;
					vege.rot = Quaternion.FromToRotation(Vector3.up, vege.pos.normalized) * Quaternion.AngleAxis(angle2, Vector3.up);
					vege.scl = new Vector3(num55, num54, num55);
					vege.modelIndex = (short)vegeProtos[vege.protoId].ModelIndex;
					vege.hash.InitHashBits(vege.pos.x, vege.pos.y, vege.pos.z);
					int num56 = data.AddVegeData(vege);
					data.vegeIds[i] = (ushort)num56;
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
