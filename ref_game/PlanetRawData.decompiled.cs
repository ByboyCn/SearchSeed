using System;
using UnityEngine;

public class PlanetRawData
{
	public int precision;

	public ushort[] heightData;

	public byte[] modData;

	public ushort[] vegeIds;

	public byte[] biomoData;

	public short[] temprData;

	public Vector3[] vertices;

	public Vector3[] normals;

	public int[] indexMap;

	public int indexMapPrecision;

	public int indexMapDataLength;

	public int indexMapFaceStride;

	public int indexMapCornerStride;

	private static Vector3[] verts200;

	private static Vector3[] verts80;

	private static int[] indexMap200;

	private static int[] indexMap80;

	public VeinData[] veinPool;

	public int veinCursor = 1;

	private int veinCapacity;

	public VegeData[] vegePool;

	public int vegeCursor = 1;

	private int vegeCapacity;

	public static Vector3[] poles = new Vector3[6]
	{
		Vector3.right,
		Vector3.left,
		Vector3.up,
		Vector3.down,
		Vector3.forward,
		Vector3.back
	};

	public int dataLength => (precision + 1) * (precision + 1) * 4;

	public int stride => (precision + 1) * 2;

	public int substride => precision + 1;

	public PlanetRawData(int _precision)
	{
		precision = _precision;
		int num = dataLength;
		heightData = new ushort[num];
		vegeIds = new ushort[num];
		biomoData = new byte[num];
		temprData = new short[num];
		vertices = new Vector3[num];
		normals = new Vector3[num];
		indexMapPrecision = precision >> 2;
		indexMapFaceStride = indexMapPrecision * indexMapPrecision;
		indexMapCornerStride = indexMapFaceStride * 3;
		indexMapDataLength = indexMapCornerStride * 8;
		indexMap = new int[indexMapDataLength];
		SetVegeCapacity(32);
		SetVeinCapacity(32);
	}

	public byte[] InitModData(byte[] refModData)
	{
		if (refModData != null)
		{
			modData = refModData;
		}
		else
		{
			modData = new byte[dataLength / 2];
		}
		return modData;
	}

	public void Free()
	{
		precision = 0;
		heightData = null;
		modData = null;
		vegeIds = null;
		biomoData = null;
		temprData = null;
		vertices = null;
		normals = null;
		indexMap = null;
		veinPool = null;
		vegePool = null;
		indexMapPrecision = 0;
		indexMapDataLength = 0;
		indexMapFaceStride = 0;
		indexMapCornerStride = 0;
		veinCursor = 1;
		veinCapacity = 0;
		vegeCursor = 1;
		vegeCapacity = 0;
	}

	public void CalcVerts()
	{
		if (precision == 200 && verts200 != null)
		{
			Array.Copy(verts200, vertices, verts200.Length);
			Array.Copy(indexMap200, indexMap, indexMap200.Length);
			return;
		}
		if (precision == 80 && verts80 != null)
		{
			Array.Copy(verts80, vertices, verts80.Length);
			Array.Copy(indexMap80, indexMap, indexMap80.Length);
			return;
		}
		for (int i = 0; i < indexMapDataLength; i++)
		{
			indexMap[i] = -1;
		}
		int num = (precision + 1) * 2;
		int num2 = precision + 1;
		for (int j = 0; j < dataLength; j++)
		{
			int num3 = j % num;
			int num4 = j / num;
			int num5 = num3 % num2;
			int num6 = num4 % num2;
			int num7 = (((num3 >= num2) ? 1 : 0) + ((num4 >= num2) ? 1 : 0) * 2) * 2 + ((num5 < num6) ? 1 : 0);
			float num8 = ((num5 >= num6) ? (precision - num5) : num5);
			float num9 = ((num5 >= num6) ? num6 : (precision - num6));
			float num10 = (float)precision - num9;
			num9 /= (float)precision;
			num8 = ((num10 > 0f) ? (num8 / num10) : 0f);
			int num11 = 0;
			Vector3 a;
			Vector3 a2;
			Vector3 b;
			switch (num7)
			{
			case 0:
				a = poles[2];
				a2 = poles[0];
				b = poles[4];
				num11 = 7;
				break;
			case 1:
				a = poles[3];
				a2 = poles[4];
				b = poles[0];
				num11 = 5;
				break;
			case 2:
				a = poles[2];
				a2 = poles[4];
				b = poles[1];
				num11 = 6;
				break;
			case 3:
				a = poles[3];
				a2 = poles[1];
				b = poles[4];
				num11 = 4;
				break;
			case 4:
				a = poles[2];
				a2 = poles[1];
				b = poles[5];
				num11 = 2;
				break;
			case 5:
				a = poles[3];
				a2 = poles[5];
				b = poles[1];
				num11 = 0;
				break;
			case 6:
				a = poles[2];
				a2 = poles[5];
				b = poles[0];
				num11 = 3;
				break;
			case 7:
				a = poles[3];
				a2 = poles[0];
				b = poles[5];
				num11 = 1;
				break;
			default:
				a = poles[2];
				a2 = poles[0];
				b = poles[4];
				num11 = 7;
				break;
			}
			vertices[j] = Vector3.Slerp(Vector3.Slerp(a, b, num9), Vector3.Slerp(a2, b, num9), num8);
			int num12 = PositionHash(vertices[j], num11);
			if (indexMap[num12] == -1)
			{
				indexMap[num12] = j;
			}
		}
		int num13 = 0;
		for (int k = 1; k < indexMapDataLength; k++)
		{
			if (indexMap[k] == -1)
			{
				indexMap[k] = indexMap[k - 1];
				num13++;
			}
		}
		if (precision == 200)
		{
			if (verts200 == null)
			{
				verts200 = new Vector3[vertices.Length];
				indexMap200 = new int[indexMap.Length];
				Array.Copy(vertices, verts200, vertices.Length);
				Array.Copy(indexMap, indexMap200, indexMap.Length);
			}
		}
		else if (precision == 80 && verts80 == null)
		{
			verts80 = new Vector3[vertices.Length];
			indexMap80 = new int[indexMap.Length];
			Array.Copy(vertices, verts80, vertices.Length);
			Array.Copy(indexMap, indexMap80, indexMap.Length);
		}
	}

	public int QueryIndex(Vector3 vpos)
	{
		vpos.Normalize();
		int num = PositionHash(vpos);
		int num2 = indexMap[num];
		float num3 = MathF.PI / (float)(precision * 2) * 0.25f;
		num3 *= num3;
		int num4 = stride;
		float num5 = 10f;
		int result = num2;
		for (int i = -1; i <= 3; i++)
		{
			for (int j = -1; j <= 3; j++)
			{
				int num6 = num2 + i + j * num4;
				if ((uint)num6 < dataLength)
				{
					float sqrMagnitude = (vertices[num6] - vpos).sqrMagnitude;
					if (sqrMagnitude < num3)
					{
						return num6;
					}
					if (sqrMagnitude < num5)
					{
						num5 = sqrMagnitude;
						result = num6;
					}
				}
			}
		}
		return result;
	}

	public int GetModLevel(int index)
	{
		return (modData[index >> 1] >> ((index & 1) << 2)) & 3;
	}

	public short GetModPlane(int index)
	{
		return (short)(((modData[index >> 1] >> ((index & 1) << 2) + 2) & 3) * 133 + 20020);
	}

	public void SetModLevel(int index, int level)
	{
		int num = (index & 1) << 2;
		int num2 = ~(3 << num);
		int num3 = (level & 3) << num;
		modData[index >> 1] &= (byte)num2;
		modData[index >> 1] |= (byte)num3;
	}

	public void SetModPlane(int index, int plane)
	{
		if (plane > 3)
		{
			plane = 3;
		}
		else if (plane < 0)
		{
			plane = 0;
		}
		int num = ((index & 1) << 2) + 2;
		int num2 = ~(3 << num);
		int num3 = (plane & 3) << num;
		modData[index >> 1] &= (byte)num2;
		modData[index >> 1] |= (byte)num3;
	}

	public bool AddModLevel(int index, int level)
	{
		int num = (modData[index >> 1] >> ((index & 1) << 2)) & 3;
		level += num;
		if (level > 3)
		{
			level = 3;
		}
		if (level == num)
		{
			return false;
		}
		int num2 = (index & 1) << 2;
		int num3 = ~(3 << num2);
		int num4 = (level & 3) << num2;
		modData[index >> 1] &= (byte)num3;
		modData[index >> 1] |= (byte)num4;
		return true;
	}

	public void EraseVegetableAtPoint(Vector3 vpos)
	{
		vpos.Normalize();
		int num = PositionHash(vpos);
		int num2 = indexMap[num];
		int num3 = stride;
		for (int i = -3; i <= 3; i++)
		{
			for (int j = -3; j <= 3; j++)
			{
				int num4 = num2 + i + j * num3;
				if ((uint)num4 < dataLength)
				{
					int num5 = vegeIds[num4];
					vegePool[num5].SetNull();
				}
			}
		}
	}

	public float QueryHeight(Vector3 vpos)
	{
		vpos.Normalize();
		int num = PositionHash(vpos);
		int num2 = indexMap[num];
		float num3 = MathF.PI / (float)(precision * 2) * 1.2f;
		float num4 = num3 * num3;
		float num5 = 0f;
		float num6 = 0f;
		int num7 = stride;
		for (int i = -1; i <= 3; i++)
		{
			for (int j = -1; j <= 3; j++)
			{
				int num8 = num2 + i + j * num7;
				if ((uint)num8 < dataLength)
				{
					float sqrMagnitude = (vertices[num8] - vpos).sqrMagnitude;
					if (!(sqrMagnitude > num4))
					{
						float num9 = 1f - Mathf.Sqrt(sqrMagnitude) / num3;
						float num10 = (int)heightData[num8];
						num5 += num9;
						num6 += num10 * num9;
					}
				}
			}
		}
		if (num5 == 0f)
		{
			Debug.LogWarning("bad query");
			return (float)(int)heightData[0] * 0.01f;
		}
		return num6 / num5 * 0.01f;
	}

	public float QueryModifiedHeight(Vector3 vpos)
	{
		vpos.Normalize();
		int num = PositionHash(vpos);
		int num2 = indexMap[num];
		float num3 = MathF.PI / (float)(precision * 2) * 1.2f;
		float num4 = num3 * num3;
		float num5 = 0f;
		float num6 = 0f;
		int num7 = stride;
		for (int i = -1; i <= 3; i++)
		{
			for (int j = -1; j <= 3; j++)
			{
				int num8 = num2 + i + j * num7;
				if ((uint)num8 >= dataLength)
				{
					continue;
				}
				float sqrMagnitude = (vertices[num8] - vpos).sqrMagnitude;
				if (sqrMagnitude > num4)
				{
					continue;
				}
				float num9 = 1f - Mathf.Sqrt(sqrMagnitude) / num3;
				int modLevel = GetModLevel(num8);
				float num10 = (int)heightData[num8];
				if (modLevel > 0)
				{
					float num11 = GetModPlane(num8);
					if (modLevel == 3)
					{
						num10 = num11;
					}
					else
					{
						float num12 = (float)modLevel * 0.3333333f;
						num10 = (float)(int)heightData[num8] * (1f - num12) + num11 * num12;
					}
				}
				num5 += num9;
				num6 += num10 * num9;
			}
		}
		if (num5 == 0f)
		{
			Debug.LogWarning("bad query");
			return (float)(int)heightData[0] * 0.01f;
		}
		return num6 / num5 * 0.01f;
	}

	private void SetVeinCapacity(int newCapacity)
	{
		VeinData[] array = veinPool;
		veinPool = new VeinData[newCapacity];
		if (array != null)
		{
			Array.Copy(array, veinPool, (newCapacity > veinCapacity) ? veinCapacity : newCapacity);
		}
		veinCapacity = newCapacity;
	}

	public int AddVeinData(VeinData vein)
	{
		vein.id = veinCursor++;
		if (vein.id == veinCapacity)
		{
			SetVeinCapacity(veinCapacity * 2);
		}
		veinPool[vein.id] = vein;
		return vein.id;
	}

	private void SetVegeCapacity(int newCapacity)
	{
		VegeData[] array = vegePool;
		vegePool = new VegeData[newCapacity];
		if (array != null)
		{
			Array.Copy(array, vegePool, (newCapacity > vegeCapacity) ? vegeCapacity : newCapacity);
		}
		vegeCapacity = newCapacity;
	}

	public int AddVegeData(VegeData vege)
	{
		vege.id = vegeCursor++;
		if (vege.id == vegeCapacity)
		{
			SetVegeCapacity(vegeCapacity * 2);
		}
		vegePool[vege.id] = vege;
		return vege.id;
	}

	private int trans(float x, int pr)
	{
		int num = (int)((Mathf.Sqrt(x + 0.23f) - 0.4795832f) / 0.6294705f * (float)pr);
		if (num >= pr)
		{
			num = pr - 1;
		}
		return num;
	}

	public int PositionHash(Vector3 v, int corner = 0)
	{
		if (corner == 0)
		{
			corner = ((v.x > 0f) ? 1 : 0) + ((v.y > 0f) ? 2 : 0) + ((v.z > 0f) ? 4 : 0);
		}
		if (v.x < 0f)
		{
			v.x = 0f - v.x;
		}
		if (v.y < 0f)
		{
			v.y = 0f - v.y;
		}
		if (v.z < 0f)
		{
			v.z = 0f - v.z;
		}
		if ((double)v.x < 1E-06 && (double)v.y < 1E-06 && (double)v.z < 1E-06)
		{
			return 0;
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		if (v.x >= v.y && v.x >= v.z)
		{
			num = 0;
			num2 = trans(v.z / v.x, indexMapPrecision);
			num3 = trans(v.y / v.x, indexMapPrecision);
		}
		else if (v.y >= v.x && v.y >= v.z)
		{
			num = 1;
			num2 = trans(v.x / v.y, indexMapPrecision);
			num3 = trans(v.z / v.y, indexMapPrecision);
		}
		else
		{
			num = 2;
			num2 = trans(v.x / v.z, indexMapPrecision);
			num3 = trans(v.y / v.z, indexMapPrecision);
		}
		return num2 + num3 * indexMapPrecision + num * indexMapFaceStride + corner * indexMapCornerStride;
	}
}
