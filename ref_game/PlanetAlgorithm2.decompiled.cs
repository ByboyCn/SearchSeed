using System;
using UnityEngine;

public class PlanetAlgorithm2 : PlanetAlgorithm
{
	public override void GenerateTerrain(double modX, double modY)
	{
		modX = (3.0 - modX - modX) * modX * modX;
		double num = 0.0035;
		double num2 = 0.025 * modX + 0.0035 * (1.0 - modX);
		double num3 = 0.0035;
		double num4 = 3.0;
		double num5 = 1.0 + 1.3 * modY;
		num *= num5;
		num2 *= num5;
		num3 *= num5;
		DotNet35Random dotNet35Random = new DotNet35Random(planet.seed);
		int num6 = dotNet35Random.Next();
		int num7 = dotNet35Random.Next();
		SimplexNoise simplexNoise = new SimplexNoise(num6);
		SimplexNoise simplexNoise2 = new SimplexNoise(num7);
		PlanetRawData data = planet.data;
		for (int i = 0; i < data.dataLength; i++)
		{
			double num8 = data.vertices[i].x * planet.radius;
			double num9 = data.vertices[i].y * planet.radius;
			double num10 = data.vertices[i].z * planet.radius;
			double num11 = data.vertices[i].y;
			double num12 = 0.0;
			double num13 = 0.0;
			double num14 = simplexNoise.Noise3DFBM(num8 * num, num9 * num2, num10 * num3, 6, 0.45, 1.8);
			double num15 = simplexNoise2.Noise3DFBM(num8 * num * 2.0, num9 * num2 * 2.0, num10 * num3 * 2.0, 3);
			double value = num14 * num4 + num4 * 0.4;
			double num16 = 0.6 / (Math.Abs(value) + 0.6) - 0.25;
			double num17 = ((num16 < 0.0) ? (num16 * 0.3) : num16);
			double num18 = Math.Pow(Math.Abs(num11 * 1.01), 3.0) * 1.0;
			double num19 = ((num15 < 0.0) ? 0.0 : num15);
			double num20 = ((num18 > 1.0) ? 1.0 : num18);
			num12 = num17;
			num13 = num17 * 1.5 + num19 * 1.0 + num20;
			data.heightData[i] = (ushort)(((double)planet.radius + num12 + 0.1) * 100.0);
			data.biomoData[i] = (byte)Mathf.Clamp((float)(num13 * 100.0), 0f, 200f);
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
		float num = 0.25f;
		float num2 = -0.48f;
		float num3 = 2.5f;
		float num4 = 0.25f;
		float num5 = -0.48f;
		float num6 = 1f;
		float num7 = 1.5f;
		float num8 = -0.7f;
		float num9 = 3f;
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
			num19 *= num19;
			double num20 = dotNet35Random2.NextDouble();
			float num21 = (float)dotNet35Random2.NextDouble() - 0.5f;
			float num22 = (float)dotNet35Random2.NextDouble() - 0.5f;
			float num23 = (float)Math.Sqrt(dotNet35Random2.NextDouble());
			float num24 = (float)dotNet35Random2.NextDouble() * 360f;
			float num25 = (float)dotNet35Random2.NextDouble();
			float num26 = (float)dotNet35Random2.NextDouble();
			float num27 = 1.3f;
			float num28 = 0.75f;
			float num29 = 1f;
			int[] array;
			if ((double)num18 < 0.800000011920929 - planet.mod_y * 0.30000001192092896)
			{
				array = vegetables;
				num27 = num;
				num28 = num2;
				num29 = num3;
			}
			else
			{
				array = vegetables2;
				num27 = num4;
				num28 = num5;
				num29 = num6;
			}
			double num30 = simplexNoise.Noise3DFBM(num14 * 0.02, num15 * 0.02, num16 * 0.02, 2) * (double)num27 + (double)num28 + 0.5 + planet.mod_y * 0.019999999552965164;
			double num31 = simplexNoise2.Noise(num14 * 0.06, num15 * 0.06, num16 * 0.06) * (double)num7 + (double)num8 + 0.13 + planet.mod_x * 0.37;
			double x = Math.Abs(data.vertices[i].y);
			x = 1.0 - Math.Pow(x, 3.0) * planet.mod_x;
			num30 *= x;
			int[] array2 = vegetables3;
			double num32 = num31;
			int num33 = 1;
			if (num20 < num30 && array != null && array.Length != 0)
			{
				vege.protoId = (short)array[(int)(num19 * (double)array.Length)];
				Quaternion quaternion = Quaternion.FromToRotation(Vector3.up, vector);
				Vector3 vector2 = quaternion * Vector3.forward;
				Vector3 vector3 = quaternion * Vector3.right;
				Vector3 vector4 = vector * num17;
				Vector3 normalized = (vector3 * num21 + vector2 * num22).normalized;
				float num34 = num23 * num29;
				Vector3 vector5 = normalized * (num34 * num11);
				Vector4 vector6 = vegeScaleRanges[vege.protoId];
				float num35 = num26 * (vector6.x + vector6.y) + (1f - vector6.x);
				float num36 = (num25 * (vector6.z + vector6.w) + (1f - vector6.z)) * num35;
				float num37 = 1f;
				if (vegeProtos[vege.protoId].Type != EVegeType.Detail)
				{
					num37 = 1f - (float)planet.mod_x;
				}
				vege.pos = (vector4 + vector5).normalized;
				num17 = data.QueryHeight(vege.pos);
				vege.pos *= num17;
				vege.rot = Maths.SphericalRotation(vege.pos, num24 * num37);
				vege.scl = new Vector3(num36, num35, num36);
				vege.modelIndex = (short)vegeProtos[vege.protoId].ModelIndex;
				vege.hash.InitHashBits(vege.pos.x, vege.pos.y, vege.pos.z);
				int num38 = data.AddVegeData(vege);
				data.vegeIds[i] = (ushort)num38;
			}
			if (!(num20 < num32) || array2 == null || array2.Length == 0)
			{
				continue;
			}
			vege.protoId = (short)array2[(int)(num19 * (double)array2.Length)];
			if (vegeProtos[vege.protoId].Type == EVegeType.VFX)
			{
				float num39 = Mathf.Max(0f, vector.x * vector.x + vector.z * vector.z - 0.1f);
				if (num20 > num32 * (double)num39)
				{
					continue;
				}
			}
			Quaternion quaternion2 = Quaternion.FromToRotation(Vector3.up, vector);
			Vector3 vector7 = quaternion2 * Vector3.forward;
			Vector3 vector8 = quaternion2 * Vector3.right;
			Vector4 vector9 = vegeScaleRanges[vege.protoId];
			for (int j = 0; j < num33; j++)
			{
				float num40 = (float)dotNet35Random2.NextDouble() - 0.5f;
				float num41 = (float)dotNet35Random2.NextDouble() - 0.5f;
				float num42 = (float)Math.Sqrt(dotNet35Random2.NextDouble());
				float num43 = (float)dotNet35Random2.NextDouble() * 360f;
				float num44 = (float)dotNet35Random2.NextDouble();
				float num45 = (float)dotNet35Random2.NextDouble();
				Vector3 vector10 = vector * num17;
				Vector3 normalized2 = (vector8 * num40 + vector7 * num41).normalized;
				float num46 = num42 * num9;
				Vector3 vector11 = normalized2 * (num46 * num11);
				float num47 = num45 * (vector9.x + vector9.y) + (1f - vector9.x);
				float num48 = (num44 * (vector9.z + vector9.w) + (1f - vector9.z)) * num47;
				float num49 = 1f;
				if (vegeProtos[vege.protoId].Type != EVegeType.Detail)
				{
					num49 = 1f - (float)planet.mod_x;
				}
				vege.pos = (vector10 + vector11).normalized;
				num17 = data.QueryHeight(vege.pos);
				vege.pos *= num17;
				vege.rot = Maths.SphericalRotation(vege.pos, num43 * num49);
				vege.scl = new Vector3(num48, num47, num48);
				vege.modelIndex = (short)vegeProtos[vege.protoId].ModelIndex;
				vege.hash.InitHashBits(vege.pos.x, vege.pos.y, vege.pos.z);
				int num50 = data.AddVegeData(vege);
				data.vegeIds[i] = (ushort)num50;
			}
		}
	}
}
