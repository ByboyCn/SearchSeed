using System;
using UnityEngine;

public class PlanetAlgorithm4 : PlanetAlgorithm
{
	private const int kCircleCount = 80;

	private Vector4[] circles = new Vector4[80];

	private double[] heights = new double[80];

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
		int num6 = dotNet35Random.Next();
		for (int i = 0; i < 80; i++)
		{
			VectorLF3 vectorLF = RandomTable.SphericNormal(ref num6, 1.0);
			Vector4 vector = new Vector4((float)vectorLF.x, (float)vectorLF.y, (float)vectorLF.z);
			vector.Normalize();
			vector *= planet.radius;
			vector.w = (float)vectorLF.magnitude * 8f + 8f;
			vector.w *= vector.w;
			circles[i] = vector;
			heights[i] = dotNet35Random.NextDouble() * 0.4 + 0.20000000298023224;
		}
		PlanetRawData data = planet.data;
		for (int j = 0; j < data.dataLength; j++)
		{
			double num7 = data.vertices[j].x * planet.radius;
			double num8 = data.vertices[j].y * planet.radius;
			double num9 = data.vertices[j].z * planet.radius;
			double num10 = 0.0;
			double num11 = 0.0;
			double num12 = simplexNoise.Noise3DFBM(num7 * num, num8 * num2, num9 * num3, 4, 0.45, 1.8);
			double num13 = simplexNoise2.Noise3DFBM(num7 * num * 5.0, num8 * num2 * 5.0, num9 * num3 * 5.0, 4);
			double num14 = num12 * 1.5;
			double num15 = num13 * 0.2;
			double num16 = num14 * 0.08 + num15 * 2.0;
			double num17 = 0.0;
			for (int k = 0; k < 80; k++)
			{
				double num18 = (double)circles[k].x - num7;
				double num19 = (double)circles[k].y - num8;
				double num20 = (double)circles[k].z - num9;
				double num21 = num18 * num18 + num19 * num19 + num20 * num20;
				if (!(num21 > (double)circles[k].w))
				{
					double num22 = num21 / (double)circles[k].w + num15 * 1.2;
					if (num22 < 0.0)
					{
						num22 = 0.0;
					}
					double num23 = num22 * num22;
					double num24 = num23 * num22;
					double num25 = -15.0 * num24 + 21.833333333334 * num23 - 7.533333333333 * num22 + 0.7 + num15;
					if (num25 < 0.0)
					{
						num25 = 0.0;
					}
					num25 *= num25;
					num25 *= heights[k];
					num17 = ((num17 > num25) ? num17 : num25);
				}
			}
			num10 = num17 + num16 + 0.2;
			num11 = num14 * 2.0 + 0.8;
			num11 = ((num11 > 2.0) ? 2.0 : ((num11 < 0.0) ? 0.0 : num11));
			num11 += ((num11 > 1.5) ? (0.0 - num17) : num17) * 0.5;
			num11 += num13 * 0.63;
			data.heightData[j] = (ushort)(((double)planet.radius + num10 + 0.1) * 100.0);
			data.biomoData[j] = (byte)Mathf.Clamp((float)(num11 * 100.0), 0f, 200f);
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
		double num = 0.005;
		double num2 = 0.02;
		double num3 = 0.005;
		float num4 = 0.18f;
		float num5 = -0.45f;
		float num6 = 2.5f;
		float num7 = 0.25f;
		float num8 = -0.45f;
		float num9 = 1f;
		DotNet35Random dotNet35Random = new DotNet35Random(planet.seed);
		dotNet35Random.Next();
		dotNet35Random.Next();
		dotNet35Random.Next();
		DotNet35Random dotNet35Random2 = new DotNet35Random(dotNet35Random.Next());
		SimplexNoise simplexNoise = new SimplexNoise(dotNet35Random2.Next());
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
			if (num12 % 2 == 1 && num13 % 2 == 1)
			{
				Vector3 vector = data.vertices[i];
				double num14 = data.vertices[i].x * planet.radius;
				double num15 = data.vertices[i].y * planet.radius;
				double num16 = data.vertices[i].z * planet.radius;
				float num17 = (float)(int)data.heightData[i] * 0.01f;
				float num18 = (float)(int)data.biomoData[i] * 0.01f;
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
					num26 = num4;
					num27 = num5;
					num28 = num6;
				}
				else
				{
					array = vegetables2;
					num26 = num7;
					num27 = num8;
					num28 = num9;
				}
				double num29 = simplexNoise.Noise3DFBM(num14 * num, num15 * num2, num16 * num3, 2) * (double)num26 + (double)num27 + 0.5;
				if (!(num20 > num29) && array != null && array.Length != 0)
				{
					vege.protoId = (short)array[(int)(num19 * (double)array.Length)];
					Quaternion quaternion = Quaternion.FromToRotation(Vector3.up, vector);
					Vector3 vector2 = quaternion * Vector3.forward;
					Vector3 vector3 = quaternion * Vector3.right;
					Vector3 vector4 = vector * num17;
					Vector3 normalized = (vector3 * num21 + vector2 * num22).normalized;
					float num30 = num23 * num28;
					Vector3 vector5 = normalized * (num30 * num11);
					Vector4 vector6 = vegeScaleRanges[vege.protoId];
					float num31 = num25 * (vector6.x + vector6.y) + (1f - vector6.x);
					float num32 = (num24 * (vector6.z + vector6.w) + (1f - vector6.z)) * num31;
					vege.pos = (vector4 + vector5).normalized;
					num17 = data.QueryHeight(vege.pos);
					vege.pos *= num17;
					vege.rot = Quaternion.FromToRotation(Vector3.up, vege.pos.normalized) * Quaternion.AngleAxis(angle, Vector3.up);
					vege.scl = new Vector3(num32, num31, num32);
					vege.modelIndex = (short)vegeProtos[vege.protoId].ModelIndex;
					vege.hash.InitHashBits(vege.pos.x, vege.pos.y, vege.pos.z);
					int num33 = data.AddVegeData(vege);
					data.vegeIds[i] = (ushort)num33;
				}
			}
		}
	}
}
