using System.IO;

public class BlueprintBuilding
{
	public int index;

	public int areaIndex;

	public float localOffset_x;

	public float localOffset_y;

	public float localOffset_z;

	public float localOffset_x2;

	public float localOffset_y2;

	public float localOffset_z2;

	public float pitch;

	public float pitch2;

	public float yaw;

	public float yaw2;

	public float tilt;

	public float tilt2;

	public short itemId;

	public short modelIndex;

	public BlueprintBuilding outputObj;

	public BlueprintBuilding inputObj;

	public int tempOutputObjIdx;

	public int tempInputObjIdx;

	public int outputToSlot;

	public int inputFromSlot;

	public int outputFromSlot;

	public int inputToSlot;

	public int outputOffset;

	public int inputOffset;

	public int recipeId;

	public int filterId;

	public int[] parameters;

	public string content;

	public void Import(BinaryReader r)
	{
		int num = r.ReadInt32();
		if (num <= -102)
		{
			index = r.ReadInt32();
			itemId = r.ReadInt16();
			modelIndex = r.ReadInt16();
			areaIndex = r.ReadSByte();
			localOffset_x = r.ReadSingle();
			localOffset_y = r.ReadSingle();
			localOffset_z = r.ReadSingle();
			yaw = r.ReadSingle();
			if (itemId > 2000 && itemId < 2010)
			{
				tilt = r.ReadSingle();
				pitch = 0f;
				localOffset_x2 = localOffset_x;
				localOffset_y2 = localOffset_y;
				localOffset_z2 = localOffset_z;
				yaw2 = yaw;
				tilt2 = tilt;
				pitch2 = 0f;
			}
			else if (itemId > 2010 && itemId < 2020)
			{
				tilt = r.ReadSingle();
				pitch = r.ReadSingle();
				localOffset_x2 = r.ReadSingle();
				localOffset_y2 = r.ReadSingle();
				localOffset_z2 = r.ReadSingle();
				yaw2 = r.ReadSingle();
				tilt2 = r.ReadSingle();
				pitch2 = r.ReadSingle();
			}
			else
			{
				tilt = 0f;
				pitch = 0f;
				localOffset_x2 = localOffset_x;
				localOffset_y2 = localOffset_y;
				localOffset_z2 = localOffset_z;
				yaw2 = yaw;
				tilt2 = 0f;
				pitch2 = 0f;
			}
			tempOutputObjIdx = r.ReadInt32();
			tempInputObjIdx = r.ReadInt32();
			outputToSlot = r.ReadSByte();
			inputFromSlot = r.ReadSByte();
			outputFromSlot = r.ReadSByte();
			inputToSlot = r.ReadSByte();
			outputOffset = r.ReadSByte();
			inputOffset = r.ReadSByte();
			recipeId = r.ReadInt16();
			filterId = r.ReadInt16();
			int num2 = r.ReadInt16();
			parameters = new int[num2];
			for (int i = 0; i < num2; i++)
			{
				parameters[i] = r.ReadInt32();
			}
			if (r.ReadInt32() > 0)
			{
				content = r.ReadString();
			}
			else
			{
				content = null;
			}
		}
		else if (num <= -101)
		{
			index = r.ReadInt32();
			itemId = r.ReadInt16();
			modelIndex = r.ReadInt16();
			areaIndex = r.ReadSByte();
			localOffset_x = r.ReadSingle();
			localOffset_y = r.ReadSingle();
			localOffset_z = r.ReadSingle();
			yaw = r.ReadSingle();
			if (itemId > 2000 && itemId < 2010)
			{
				tilt = r.ReadSingle();
				pitch = 0f;
				localOffset_x2 = localOffset_x;
				localOffset_y2 = localOffset_y;
				localOffset_z2 = localOffset_z;
				yaw2 = yaw;
				tilt2 = tilt;
				pitch2 = 0f;
			}
			else if (itemId > 2010 && itemId < 2020)
			{
				tilt = r.ReadSingle();
				pitch = r.ReadSingle();
				localOffset_x2 = r.ReadSingle();
				localOffset_y2 = r.ReadSingle();
				localOffset_z2 = r.ReadSingle();
				yaw2 = r.ReadSingle();
				tilt2 = r.ReadSingle();
				pitch2 = r.ReadSingle();
			}
			else
			{
				tilt = 0f;
				pitch = 0f;
				localOffset_x2 = localOffset_x;
				localOffset_y2 = localOffset_y;
				localOffset_z2 = localOffset_z;
				yaw2 = yaw;
				tilt2 = 0f;
				pitch2 = 0f;
			}
			tempOutputObjIdx = r.ReadInt32();
			tempInputObjIdx = r.ReadInt32();
			outputToSlot = r.ReadSByte();
			inputFromSlot = r.ReadSByte();
			outputFromSlot = r.ReadSByte();
			inputToSlot = r.ReadSByte();
			outputOffset = r.ReadSByte();
			inputOffset = r.ReadSByte();
			recipeId = r.ReadInt16();
			filterId = r.ReadInt16();
			int num3 = r.ReadInt16();
			parameters = new int[num3];
			for (int j = 0; j < num3; j++)
			{
				parameters[j] = r.ReadInt32();
			}
		}
		else if (num <= -100)
		{
			index = r.ReadInt32();
			areaIndex = r.ReadSByte();
			localOffset_x = r.ReadSingle();
			localOffset_y = r.ReadSingle();
			localOffset_z = r.ReadSingle();
			localOffset_x2 = r.ReadSingle();
			localOffset_y2 = r.ReadSingle();
			localOffset_z2 = r.ReadSingle();
			pitch = 0f;
			pitch2 = 0f;
			yaw = r.ReadSingle();
			yaw2 = r.ReadSingle();
			tilt = r.ReadSingle();
			tilt2 = 0f;
			itemId = r.ReadInt16();
			modelIndex = r.ReadInt16();
			tempOutputObjIdx = r.ReadInt32();
			tempInputObjIdx = r.ReadInt32();
			outputToSlot = r.ReadSByte();
			inputFromSlot = r.ReadSByte();
			outputFromSlot = r.ReadSByte();
			inputToSlot = r.ReadSByte();
			outputOffset = r.ReadSByte();
			inputOffset = r.ReadSByte();
			recipeId = r.ReadInt16();
			filterId = r.ReadInt16();
			int num4 = r.ReadInt16();
			parameters = new int[num4];
			for (int k = 0; k < num4; k++)
			{
				parameters[k] = r.ReadInt32();
			}
		}
		else
		{
			index = num;
			areaIndex = r.ReadSByte();
			localOffset_x = r.ReadSingle();
			localOffset_y = r.ReadSingle();
			localOffset_z = r.ReadSingle();
			localOffset_x2 = r.ReadSingle();
			localOffset_y2 = r.ReadSingle();
			localOffset_z2 = r.ReadSingle();
			pitch = 0f;
			pitch2 = 0f;
			yaw = r.ReadSingle();
			yaw2 = r.ReadSingle();
			tilt = 0f;
			tilt2 = 0f;
			itemId = r.ReadInt16();
			modelIndex = r.ReadInt16();
			tempOutputObjIdx = r.ReadInt32();
			tempInputObjIdx = r.ReadInt32();
			outputToSlot = r.ReadSByte();
			inputFromSlot = r.ReadSByte();
			outputFromSlot = r.ReadSByte();
			inputToSlot = r.ReadSByte();
			outputOffset = r.ReadSByte();
			inputOffset = r.ReadSByte();
			recipeId = r.ReadInt16();
			filterId = r.ReadInt16();
			int num5 = r.ReadInt16();
			parameters = new int[num5];
			for (int l = 0; l < num5; l++)
			{
				parameters[l] = r.ReadInt32();
			}
		}
	}

	public void Export(BinaryWriter w)
	{
		w.Write(-102);
		w.Write(index);
		w.Write(itemId);
		w.Write(modelIndex);
		w.Write((sbyte)areaIndex);
		w.Write(localOffset_x);
		w.Write(localOffset_y);
		w.Write(localOffset_z);
		w.Write(yaw);
		if (itemId > 2000 && itemId < 2010)
		{
			w.Write(tilt);
		}
		else if (itemId > 2010 && itemId < 2020)
		{
			w.Write(tilt);
			w.Write(pitch);
			w.Write(localOffset_x2);
			w.Write(localOffset_y2);
			w.Write(localOffset_z2);
			w.Write(yaw2);
			w.Write(tilt2);
			w.Write(pitch2);
		}
		w.Write((outputObj == null) ? (-1) : outputObj.index);
		w.Write((inputObj == null) ? (-1) : inputObj.index);
		w.Write((sbyte)outputToSlot);
		w.Write((sbyte)inputFromSlot);
		w.Write((sbyte)outputFromSlot);
		w.Write((sbyte)inputToSlot);
		w.Write((sbyte)outputOffset);
		w.Write((sbyte)inputOffset);
		w.Write((short)recipeId);
		w.Write((short)filterId);
		int num = ((parameters != null) ? parameters.Length : 0);
		w.Write((short)num);
		for (int i = 0; i < num; i++)
		{
			w.Write(parameters[i]);
		}
		if (string.IsNullOrEmpty(content))
		{
			w.Write(0);
			return;
		}
		w.Write(content.Length);
		w.Write(content);
	}
}
