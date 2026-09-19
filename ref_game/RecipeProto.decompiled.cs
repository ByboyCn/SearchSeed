using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RecipeProto : Proto
{
	public ERecipeType Type;

	public bool Handcraft;

	public bool Explicit;

	public int TimeSpend;

	public int[] Items;

	public int[] ItemCounts;

	public int[] Results;

	public int[] ResultCounts;

	public int GridIndex;

	public string IconPath;

	public string IconTag;

	public string Description;

	public bool NonProductive;

	public string iconTagString;

	public string iconNameTagString;

	[NonSerialized]
	public TechProto preTech;

	private Sprite _iconSprite;

	[NonSerialized]
	public bool hasIcon;

	public const int kMaxProtoId = 500;

	public static Dictionary<int, RecipeExecuteData> recipeExecuteData;

	public static RecipeProto[] fractionatorRecipes;

	public static int[] fractionatorNeeds;

	public bool productive { get; private set; }

	public string description { get; set; }

	public Sprite iconSprite => _iconSprite;

	public int index { get; private set; }

	public string madeFromString => Type switch
	{
		ERecipeType.None => "-", 
		ERecipeType.Smelt => "冶炼设备".Translate(), 
		ERecipeType.Chemical => "化工设备".Translate(), 
		ERecipeType.Refine => "精炼设备".Translate(), 
		ERecipeType.Assemble => "制造台".Translate(), 
		ERecipeType.Particle => "粒子对撞机".Translate(), 
		ERecipeType.Exchange => "能量交换器".Translate(), 
		ERecipeType.PhotonStore => "射线接收站".Translate(), 
		ERecipeType.Fractionate => "分馏设备".Translate(), 
		ERecipeType.Research => "科研设备".Translate(), 
		_ => "未知".Translate(), 
	};

	public void RefreshTranslation()
	{
		base.name = Name.Translate();
		description = Description.Translate();
	}

	public void Preload(int _index)
	{
		index = _index;
		base.name = Name.Translate();
		description = Description.Translate();
		iconTagString = "\\" + IconTag + ";";
		iconNameTagString = "\\" + IconTag + "-;";
		Assert.True(Items.Length == ItemCounts.Length);
		Assert.True(Results.Length == ResultCounts.Length);
		if (Type == ERecipeType.Fractionate)
		{
			Assert.True(Items.Length == 1);
			Assert.True(Results.Length == 1);
		}
		if (TimeSpend < 1)
		{
			TimeSpend = 1;
		}
		if (string.IsNullOrEmpty(IconPath))
		{
			if (Results.Length != 0)
			{
				ItemProto itemProto = LDB.items.Select(Results[0]);
				if (itemProto != null)
				{
					_iconSprite = itemProto.iconSprite;
				}
			}
		}
		else
		{
			_iconSprite = Resources.Load<Sprite>(IconPath);
			hasIcon = true;
		}
		if (!NonProductive)
		{
			productive = true;
			for (int i = 0; i < Items.Length; i++)
			{
				ItemProto itemProto2 = LDB.items.Select(Items[i]);
				productive &= itemProto2.Productive;
			}
		}
		else
		{
			productive = false;
		}
		FindPreTech();
	}

	public void FindPreTech()
	{
		for (int i = 0; i < LDB.techs.Length; i++)
		{
			TechProto techProto = LDB.techs.dataArray[i];
			for (int j = 0; j < techProto.UnlockRecipes.Length; j++)
			{
				if (techProto.UnlockRecipes[j] == ID)
				{
					preTech = techProto;
					return;
				}
			}
		}
		preTech = null;
	}

	public static void InitRecipeItems()
	{
		recipeExecuteData = new Dictionary<int, RecipeExecuteData>();
		RecipeProto[] dataArray = LDB.recipes.dataArray;
		for (int i = 0; i < dataArray.Length; i++)
		{
			RecipeExecuteData value = new RecipeExecuteData(dataArray[i].Items, dataArray[i].ItemCounts, dataArray[i].Results, dataArray[i].ResultCounts, dataArray[i].TimeSpend * 10000, dataArray[i].TimeSpend * 100000, dataArray[i].productive);
			recipeExecuteData.Add(dataArray[i].ID, value);
		}
	}

	public static void InitFractionatorNeeds()
	{
		RecipeProto[] dataArray = LDB.recipes.dataArray;
		List<RecipeProto> list = new List<RecipeProto>();
		List<int> list2 = new List<int>();
		for (int i = 0; i < dataArray.Length; i++)
		{
			if (dataArray[i].Type == ERecipeType.Fractionate)
			{
				list.Add(dataArray[i]);
				list2.Add(dataArray[i].Items[0]);
			}
		}
		fractionatorRecipes = list.ToArray();
		fractionatorNeeds = list2.ToArray();
	}
}
