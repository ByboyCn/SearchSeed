using System.Collections.Generic;
using UnityEngine;

public class ProtoSet<T> : ProtoTable, ISerializationCallbackReceiver where T : Proto
{
	public T[] dataArray;

	private Dictionary<int, int> dataIndices;

	public override Proto this[int index]
	{
		get
		{
			return dataArray[index];
		}
		set
		{
			dataArray[index] = value as T;
		}
	}

	public override int Length => dataArray.Length;

	public override void Init(int length)
	{
		dataArray = new T[length];
	}

	public virtual void OnBeforeSerialize()
	{
	}

	public virtual void OnAfterDeserialize()
	{
		dataIndices = new Dictionary<int, int>();
		for (int i = 0; i < dataArray.Length; i++)
		{
			dataArray[i].name = dataArray[i].Name;
			dataArray[i].sid = dataArray[i].SID;
			dataIndices[dataArray[i].ID] = i;
		}
	}

	public T Select(int id)
	{
		if (dataIndices.ContainsKey(id))
		{
			return dataArray[dataIndices[id]];
		}
		return null;
	}

	public bool Exist(int id)
	{
		return dataIndices.ContainsKey(id);
	}
}
