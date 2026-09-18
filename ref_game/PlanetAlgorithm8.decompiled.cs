using System;
using UnityEngine;

public class PlanetAlgorithm8 : PlanetAlgorithm
{
	public override void GenerateTerrain(double modX, double modY)
	{
		double num = 0.002 * modX;
		double num2 = 0.002 * modX * modX * 6.66667;
		double num3 = 0.002 * modX;
		SimplexNoise simplexNoise = new SimplexNoise(new DotNet35Random(planet.seed).Next());
		PlanetRawData data = planet.data;
		for (int i = 0; i < data.dataLength; i++)
		{
			double num4 = data.vertices[i].x * planet.radius;
			double num5 = data.vertices[i].y * planet.radius;
			double num6 = data.vertices[i].z * planet.radius;
			double num7 = 0.0;
			double num8 = 0.0;
			float num9 = Mathf.Clamp((float)simplexNoise.Noise3DFBM(num4 * num, num5 * num2, num6 * num3, 6, 0.45, 1.8) + 1f + (float)modY * 0.01f, 0f, 2f);
			float num10 = 0f;
			if ((double)num9 < 1.0)
			{
				float f = Mathf.Cos(num9 * MathF.PI) * 1.1f;
				f = Mathf.Sign(f) * Mathf.Pow(f, 4f);
				f = Mathf.Clamp(f, -1f, 1f);
				num10 = 1f - (f + 1f) * 0.5f;
			}
			else
			{
				float f2 = Mathf.Cos((num9 - 1f) * MathF.PI) * 1.1f;
				f2 = Mathf.Sign(f2) * Mathf.Pow(f2, 4f);
				f2 = Mathf.Clamp(f2, -1f, 1f);
				num10 = 2f - (f2 + 1f) * 0.5f;
			}
			num7 = num10;
			num8 = num10;
			num8 = ((num8 < 1.0) ? (Math.Max(num8 - 0.2, 0.0) * 1.25) : num8);
			num8 = ((num8 > 1.0) ? Math.Min(num8 * num8, 2.0) : num8);
			num8 = Maths.Levelize2(num8);
			data.heightData[i] = (ushort)(((double)planet.radius + num7 + 0.1) * 100.0);
			data.biomoData[i] = (byte)Mathf.Clamp((float)(num8 * 100.0), 0f, 200f);
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
		_ = themeProto.Vegetables1;
		double num = 0.02;
		double num2 = 0.035;
		double num3 = 0.02;
		float num4 = 0.4f;
		float num5 = -0.4f;
		float num6 = 2.5f;
		DotNet35Random dotNet35Random = new DotNet35Random(planet.seed);
		dotNet35Random.Next();
		dotNet35Random.Next();
		dotNet35Random.Next();
		DotNet35Random dotNet35Random2 = new DotNet35Random(dotNet35Random.Next());
		SimplexNoise simplexNoise = new SimplexNoise(dotNet35Random2.Next());
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
			if (num9 % 2 == 1 && num10 % 2 == 1)
			{
				Vector3 vector = data.vertices[i];
				double num11 = data.vertices[i].x * planet.radius;
				double num12 = data.vertices[i].y * planet.radius;
				double num13 = data.vertices[i].z * planet.radius;
				float num14 = (float)(int)data.heightData[i] * 0.01f;
				_ = data.biomoData[i];
				double num15 = dotNet35Random2.NextDouble();
				num15 *= num15;
				double num16 = dotNet35Random2.NextDouble();
				float num17 = (float)dotNet35Random2.NextDouble() - 0.5f;
				float num18 = (float)dotNet35Random2.NextDouble() - 0.5f;
				float num19 = (float)Math.Sqrt(dotNet35Random2.NextDouble());
				float angle = (float)dotNet35Random2.NextDouble() * 360f;
				float num20 = (float)dotNet35Random2.NextDouble();
				float num21 = (float)dotNet35Random2.NextDouble();
				float num22 = 1f;
				float num23 = 0.5f;
				float num24 = 1f;
				int[] array = vegetables;
				num22 = num4;
				num23 = num5;
				num24 = num6;
				double num25 = simplexNoise.Noise3DFBM(num11 * num, num12 * num2, num13 * num3, 2) * (double)num22 + (double)num23 + 0.5;
				if (!(num16 > num25) && array != null && array.Length != 0)
				{
					vege.protoId = (short)array[(int)(num15 * (double)array.Length)];
					Quaternion quaternion = Quaternion.FromToRotation(Vector3.up, vector);
					Vector3 vector2 = quaternion * Vector3.forward;
					Vector3 vector3 = quaternion * Vector3.right;
					Vector3 vector4 = vector * num14;
					Vector3 normalized = (vector3 * num17 + vector2 * num18).normalized;
					float num26 = num19 * num24;
					Vector3 vector5 = normalized * (num26 * num8);
					Vector4 vector6 = vegeScaleRanges[vege.protoId];
					float num27 = num21 * (vector6.x + vector6.y) + (1f - vector6.x);
					float num28 = (num20 * (vector6.z + vector6.w) + (1f - vector6.z)) * num27;
					vege.pos = (vector4 + vector5).normalized;
					num14 = data.QueryHeight(vege.pos);
					vege.pos *= num14;
					vege.rot = Quaternion.FromToRotation(Vector3.up, vege.pos.normalized) * Quaternion.AngleAxis(angle, Vector3.up);
					vege.scl = new Vector3(num28, num27, num28);
					vege.modelIndex = (short)vegeProtos[vege.protoId].ModelIndex;
					vege.hash.InitHashBits(vege.pos.x, vege.pos.y, vege.pos.z);
					int num29 = data.AddVegeData(vege);
					data.vegeIds[i] = (ushort)num29;
				}
			}
		}
	}
}
