using System;
using System.IO;

public class GameDesc
{
	public DateTime creationTime;

	public Version creationVersion;

	public int galaxyAlgo;

	public int galaxySeed;

	public int starCount;

	public int playerProto;

	public float resourceMultiplier;

	public int[] savedThemeIds;

	public bool achievementEnable;

	public bool isPeaceMode;

	public bool isSandboxMode;

	public CombatSettings combatSettings;

	public EGoalLevel goalLevel;

	public const float RARE_RESOURCE_MULTIPLIER = 0.1f;

	public const float INFINITE_RESOURCE_MULTIPLIER = 100f;

	public float oilAmountMultiplier
	{
		get
		{
			if (!(resourceMultiplier <= 0.1001f))
			{
				return 1f;
			}
			return 0.5f;
		}
	}

	public bool isInfiniteResource => resourceMultiplier >= 99.5f;

	public bool isRareResource => resourceMultiplier <= 0.1001f;

	public float enemyDropMultiplier
	{
		get
		{
			float num = 1f;
			if (resourceMultiplier > 1f)
			{
				num = 1f + resourceMultiplier * 0.1f;
				if (num > 2f)
				{
					num = 2f;
				}
			}
			else
			{
				num = (float)(Math.Round(Math.Sqrt(resourceMultiplier) * 10.0) / 10.0);
			}
			return num;
		}
	}

	public string clusterString
	{
		get
		{
			string text = ((resourceMultiplier > 9.95f) ? "99" : (resourceMultiplier * 10f).ToString("00"));
			string text2 = "-A";
			if (isSandboxMode)
			{
				text2 = "-S";
				return galaxySeed.ToString("00000000") + "-" + starCount + text2 + text;
			}
			if (isCombatMode)
			{
				text2 = "-Z";
				int num = combatModeDifficultyNumber;
				string text3 = ((1 * 100 + num) % 100).ToString("00");
				return galaxySeed.ToString("00000000") + "-" + starCount + text2 + text + "-" + text3;
			}
			return galaxySeed.ToString("00000000") + "-" + starCount + text2 + text;
		}
	}

	public string clusterStringLong
	{
		get
		{
			string text = ((resourceMultiplier > 9.95f) ? "99" : (resourceMultiplier * 10f).ToString("00"));
			string text2 = " - A";
			if (isSandboxMode)
			{
				text2 = " - S";
				return galaxySeed.ToString("0000 0000") + " - " + starCount + text2 + text;
			}
			if (isCombatMode)
			{
				text2 = " - Z";
				int num = combatModeDifficultyNumber;
				string text3 = ((1 * 100 + num) % 100).ToString("00");
				return galaxySeed.ToString("0000 0000") + " - " + starCount + text2 + text + " - " + text3;
			}
			return galaxySeed.ToString("0000 0000") + " - " + starCount + text2 + text;
		}
	}

	public long seedKey64
	{
		get
		{
			int num = galaxySeed;
			int num2 = starCount;
			int num3 = (int)((double)(resourceMultiplier * 10f) + 0.5);
			if (num2 > 999)
			{
				num2 = 999;
			}
			else if (num2 < 1)
			{
				num2 = 1;
			}
			if (num3 > 99)
			{
				num3 = 99;
			}
			else if (num3 < 1)
			{
				num3 = 1;
			}
			int num4 = 0;
			if (isSandboxMode)
			{
				num4 = 999;
			}
			else if (isCombatMode)
			{
				int num5 = combatModeDifficultyNumber;
				num4 = 1 * 100 + num5;
			}
			return (long)num * 100000000L + (long)num2 * 100000L + (long)num3 * 1000L + num4;
		}
	}

	public bool isCombatMode => !isPeaceMode;

	public float propertyMultiplier
	{
		get
		{
			float num = 0f;
			if (isSandboxMode)
			{
				return 0f;
			}
			float num2 = (isCombatMode ? combatSettings.difficulty : 0f);
			num = ((resourceMultiplier <= 0.15f) ? 4f : ((resourceMultiplier <= 0.45f) ? 3f : ((resourceMultiplier > 0.45f && resourceMultiplier <= 0.65f) ? 2f : ((resourceMultiplier > 0.65f && resourceMultiplier <= 0.9f) ? 1.5f : ((resourceMultiplier > 0.9f && resourceMultiplier <= 1.25f) ? 1f : ((resourceMultiplier > 1.25f && resourceMultiplier <= 1.75f) ? 0.9f : ((resourceMultiplier > 1.75f && resourceMultiplier <= 2.5f) ? 0.8f : ((resourceMultiplier > 2.5f && resourceMultiplier <= 4f) ? 0.7f : ((resourceMultiplier > 4f && resourceMultiplier <= 6.5f) ? 0.6f : ((!(resourceMultiplier > 6.5f) || !(resourceMultiplier <= 8.5f)) ? 0.4f : 0.5f))))))))));
			num += num2 * (num * 0.5f + 0.5f);
			if (num2 >= 9.999f)
			{
				num++;
			}
			return (float)(int)((double)(num * 100f) + 0.5) / 100f;
		}
	}

	public int combatModeDifficultyNumber
	{
		get
		{
			float difficulty = combatSettings.difficulty;
			int num = (int)(difficulty * 10f + 0.001f);
			if (num >= 100)
			{
				num = 99;
			}
			if (num == 0 && difficulty > 0.001f)
			{
				num = 1;
			}
			return num;
		}
	}

	static GameDesc()
	{
	}

	public void SetForNewGame(int _galaxyAlgo, int _galaxySeed, int _starCount, int _playerProto, float _resourceMultiplier)
	{
		creationTime = DateTime.UtcNow;
		creationVersion = GameConfig.gameVersion;
		galaxyAlgo = _galaxyAlgo;
		galaxySeed = _galaxySeed;
		starCount = _starCount;
		playerProto = _playerProto;
		resourceMultiplier = _resourceMultiplier;
		ThemeProtoSet themes = LDB.themes;
		int length = themes.Length;
		savedThemeIds = new int[length];
		for (int i = 0; i < length; i++)
		{
			savedThemeIds[i] = themes.dataArray[i].ID;
		}
		achievementEnable = DateTime.Compare(t2: new DateTime(2021, 9, 29, 0, 0, 0), t1: creationTime) > 0;
		isPeaceMode = true;
		isSandboxMode = false;
		combatSettings.SetDefault();
	}

	public void CopyTo(GameDesc other)
	{
		if (other != null)
		{
			other.creationTime = creationTime;
			other.creationVersion = creationVersion;
			other.galaxyAlgo = galaxyAlgo;
			other.starCount = starCount;
			other.playerProto = playerProto;
			other.resourceMultiplier = resourceMultiplier;
			if (other.savedThemeIds == null || other.savedThemeIds.Length != savedThemeIds.Length)
			{
				other.savedThemeIds = new int[savedThemeIds.Length];
			}
			Array.Copy(savedThemeIds, other.savedThemeIds, savedThemeIds.Length);
			other.achievementEnable = achievementEnable;
			other.isPeaceMode = isPeaceMode;
			other.isSandboxMode = isSandboxMode;
			other.combatSettings = combatSettings;
		}
	}

	public void Export(BinaryWriter w)
	{
		w.Write(9);
		w.Write(creationTime.Ticks);
		w.Write(creationVersion.Major);
		w.Write(creationVersion.Minor);
		w.Write(creationVersion.Release);
		w.Write(creationVersion.Build);
		w.Write(galaxyAlgo);
		w.Write(galaxySeed);
		w.Write(starCount);
		w.Write(playerProto);
		w.Write(resourceMultiplier);
		int num = savedThemeIds.Length;
		w.Write(num);
		for (int i = 0; i < num; i++)
		{
			w.Write(savedThemeIds[i]);
		}
		w.Write(achievementEnable);
		w.Write(isPeaceMode);
		w.Write(isSandboxMode);
		combatSettings.Export(w);
		w.Write((int)goalLevel);
	}

	public void Import(BinaryReader r)
	{
		int num = r.ReadInt32();
		if (num >= 3)
		{
			creationTime = new DateTime(r.ReadInt64());
		}
		else
		{
			creationTime = new DateTime(2021, 1, 21, 6, 58, 30);
		}
		creationTime = DateTime.SpecifyKind(creationTime, DateTimeKind.Utc);
		creationVersion = new Version(0, 0, 0);
		if (num >= 5)
		{
			if (num >= 6)
			{
				creationVersion.Major = r.ReadInt32();
				creationVersion.Minor = r.ReadInt32();
				creationVersion.Release = r.ReadInt32();
				creationVersion.Build = r.ReadInt32();
			}
			else
			{
				creationVersion.Build = r.ReadInt32();
			}
		}
		galaxyAlgo = r.ReadInt32();
		galaxySeed = r.ReadInt32();
		starCount = r.ReadInt32();
		playerProto = r.ReadInt32();
		if (num >= 2)
		{
			resourceMultiplier = r.ReadSingle();
		}
		else
		{
			resourceMultiplier = 1f;
		}
		if (num >= 1)
		{
			int num2 = r.ReadInt32();
			savedThemeIds = new int[num2];
			for (int i = 0; i < num2; i++)
			{
				savedThemeIds[i] = r.ReadInt32();
			}
		}
		else
		{
			ThemeProtoSet themes = LDB.themes;
			int length = themes.Length;
			savedThemeIds = new int[length];
			for (int j = 0; j < length; j++)
			{
				savedThemeIds[j] = themes.dataArray[j].ID;
			}
		}
		if (num >= 4)
		{
			achievementEnable = r.ReadBoolean();
		}
		else
		{
			achievementEnable = DateTime.Compare(t2: new DateTime(2021, 9, 29, 0, 0, 0), t1: creationTime) > 0;
		}
		if (num >= 7)
		{
			isPeaceMode = r.ReadBoolean();
			isSandboxMode = r.ReadBoolean();
		}
		else
		{
			isPeaceMode = true;
			isSandboxMode = false;
		}
		if (num >= 8)
		{
			combatSettings.Import(r);
		}
		else
		{
			combatSettings.SetDefault();
		}
		if (num >= 9)
		{
			goalLevel = (EGoalLevel)r.ReadInt32();
		}
		else
		{
			goalLevel = EGoalLevel.Full;
		}
	}

	public static bool IsCombatModeSeedKey(long seedKey)
	{
		long num = seedKey % 1000;
		if (num >= 100)
		{
			return num <= 199;
		}
		return false;
	}
}
