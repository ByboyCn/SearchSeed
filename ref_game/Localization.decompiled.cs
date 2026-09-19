using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

public static class Localization
{
	public class Language
	{
		public int lcId;

		public string name;

		public string abbr;

		public string abbr2;

		public int fallback;

		public EGlyph glyph;

		public bool published;

		public Language()
		{
			lcId = 0;
			name = "";
			abbr = "";
			abbr2 = "";
			fallback = 0;
			glyph = EGlyph.Latin;
			published = false;
		}
	}

	public enum EGlyph
	{
		Latin = 0,
		CJK = 1,
		Other = 99
	}

	private static int currentLanguageIndex;

	private static int preSelectLanguageLCID = 0;

	private static Dictionary<string, int> namesIndexer;

	private static string[][] strings;

	private static float[][] floats;

	private static string[] currentStrings;

	private static float[] currentFloats;

	public const int LCID_ZHCN = 2052;

	public const int LCID_ENUS = 1033;

	public const int LCID_FRFR = 1036;

	public const int LCID_DEDE = 1031;

	public const int LCID_ESES = 3082;

	public const int LCID_JAJA = 1041;

	public const int LCID_KOKO = 1042;

	private static readonly string DefaultLoadingPath = "Locale/";

	private static string[] resourcePages;

	public static bool Loaded
	{
		get
		{
			if (Languages != null && namesIndexer != null)
			{
				return strings != null;
			}
			return false;
		}
	}

	public static Language[] Languages { get; private set; }

	public static int LanguageCount
	{
		get
		{
			if (Languages != null)
			{
				return Languages.Length;
			}
			return 0;
		}
	}

	public static int CurrentLanguageIndex
	{
		get
		{
			return currentLanguageIndex;
		}
		set
		{
			if (!LanguageLoaded(value) && Loaded)
			{
				LoadLanguage(value);
			}
			if (Languages != null && value >= 0 && value < Languages.Length)
			{
				currentStrings = strings[value];
				currentFloats = floats[value];
				preSelectLanguageLCID = Languages[value].lcId;
			}
			else
			{
				currentStrings = null;
				currentFloats = null;
				preSelectLanguageLCID = 0;
			}
			if (value == currentLanguageIndex)
			{
				return;
			}
			CultureInfo cultureInfo = null;
			try
			{
				cultureInfo = new CultureInfo(Languages[value].lcId);
			}
			catch
			{
			}
			if (cultureInfo == null)
			{
				try
				{
					cultureInfo = new CultureInfo(Languages[value].abbr);
				}
				catch
				{
				}
			}
			if (cultureInfo == null)
			{
				try
				{
					cultureInfo = new CultureInfo(Languages[value].abbr2);
				}
				catch
				{
				}
			}
			try
			{
				if (cultureInfo != null)
				{
					cultureInfo.NumberFormat.PercentSymbol = "%";
					cultureInfo.NumberFormat.PerMilleSymbol = "‰";
					CultureInfo.CurrentCulture = cultureInfo;
				}
				else
				{
					Debug.LogWarning("未查询到当前语言[" + Languages[value].name + "]相应的CultureInfo");
				}
			}
			catch (Exception arg)
			{
				Debug.LogWarning($"设置CultureInfo错误！\r\n{arg}");
			}
			currentLanguageIndex = value;
			NotifyLanguageChange();
		}
	}

	public static Language CurrentLanguage
	{
		get
		{
			if (currentLanguageIndex < 0 || currentLanguageIndex >= LanguageCount)
			{
				return null;
			}
			return Languages[currentLanguageIndex];
		}
	}

	public static int CurrentLanguageLCID
	{
		get
		{
			if (currentLanguageIndex < 0 || currentLanguageIndex >= LanguageCount)
			{
				return 0;
			}
			return Languages[currentLanguageIndex].lcId;
		}
		set
		{
			if (value > 0)
			{
				preSelectLanguageLCID = value;
				int num = -1;
				if (Loaded)
				{
					int languageCount = LanguageCount;
					for (int i = 0; i < languageCount; i++)
					{
						if (Languages[i].lcId == value)
						{
							num = i;
							break;
						}
					}
				}
				if (num >= 0)
				{
					CurrentLanguageIndex = num;
				}
			}
			else
			{
				preSelectLanguageLCID = 0;
			}
		}
	}

	public static int NamesCount
	{
		get
		{
			if (namesIndexer != null)
			{
				return namesIndexer.Count;
			}
			return 0;
		}
	}

	public static bool isZHCN => CurrentLanguageLCID == 2052;

	public static bool isENUS => CurrentLanguageLCID == 1033;

	public static bool isFRFR => CurrentLanguageLCID == 1036;

	public static bool isDEDE => CurrentLanguageLCID == 1031;

	public static bool isESES => CurrentLanguageLCID == 3082;

	public static bool isJAJA => CurrentLanguageLCID == 1041;

	public static bool isKOKO => CurrentLanguageLCID == 1042;

	public static bool isCJK
	{
		get
		{
			Language currentLanguage = CurrentLanguage;
			if (currentLanguage == null)
			{
				return false;
			}
			return currentLanguage.glyph == EGlyph.CJK;
		}
	}

	public static bool isPublished => CurrentLanguage?.published ?? true;

	public static bool isKMG
	{
		get
		{
			Language currentLanguage = CurrentLanguage;
			if (currentLanguage == null)
			{
				return true;
			}
			return currentLanguage.lcId != 2052;
		}
	}

	public static string ResourcesPath { get; private set; }

	public static event Action OnLanguageChange;

	public static void NotifyLanguageChange()
	{
		OnLanguageChange?.Invoke();
	}

	public static string Translate(this string s)
	{
		if (s == null)
		{
			return "";
		}
		if (namesIndexer == null || currentStrings == null || !namesIndexer.ContainsKey(s))
		{
			return s;
		}
		return currentStrings[namesIndexer[s]];
	}

	public static bool CanTranslate(string s)
	{
		if (s == null)
		{
			return false;
		}
		if (namesIndexer == null || currentStrings == null || !namesIndexer.ContainsKey(s))
		{
			return false;
		}
		return currentStrings[namesIndexer[s]] != null;
	}

	public static float TranslateParam(this string s, float default_value)
	{
		if (s == null)
		{
			return default_value;
		}
		if (namesIndexer == null || currentFloats == null || !namesIndexer.ContainsKey(s))
		{
			return default_value;
		}
		return currentFloats[namesIndexer[s]];
	}

	public static float? TranslateParam(this string s)
	{
		if (s == null)
		{
			return null;
		}
		if (namesIndexer == null || currentFloats == null || !namesIndexer.ContainsKey(s))
		{
			return null;
		}
		return currentFloats[namesIndexer[s]];
	}

	public static void Load()
	{
		if (Languages == null)
		{
			Load(DefaultLoadingPath);
		}
	}

	public static bool Load(string path)
	{
		if (Loaded)
		{
			Unload();
		}
		if (string.IsNullOrEmpty(path))
		{
			return false;
		}
		ResourcesPath = path.Replace('\\', '/');
		if (ResourcesPath[ResourcesPath.Length - 1] != '/')
		{
			ResourcesPath += "/";
		}
		if (!Directory.Exists(ResourcesPath))
		{
			ResourcesPath = null;
			return false;
		}
		if (LoadSettings())
		{
			int num = 0;
			int languageCount = LanguageCount;
			for (int i = 0; i < languageCount; i++)
			{
				if (Languages[i].lcId == preSelectLanguageLCID)
				{
					num = i;
					break;
				}
			}
			currentLanguageIndex = num;
			if (Languages.Length > num && LoadLanguage(num))
			{
				currentStrings = strings[num];
				currentFloats = floats[num];
				NotifyLanguageChange();
				return true;
			}
		}
		Unload();
		return false;
	}

	private static bool LoadSettings()
	{
		string path = ResourcesPath + "Header.txt";
		string text = ResourcesPath + "Names/";
		char[] separator = new char[1] { '=' };
		char[] separator2 = new char[1] { ',' };
		try
		{
			if (!File.Exists(path))
			{
				return false;
			}
			using StreamReader streamReader = new StreamReader(path, detectEncodingFromByteOrderMarks: true);
			if (streamReader.Peek() >= 0)
			{
				string text2 = streamReader.ReadLine();
				if (text2 == null || !text2.Equals("[Localization Project]"))
				{
					Debug.LogError("[Locale] 无效的项目头文件 - 标识不匹配");
					return false;
				}
				string text3 = streamReader.ReadLine();
				if (text3 == null)
				{
					Debug.LogError("[Locale] 无效的项目头文件 - 无法读取版本号");
					return false;
				}
				string[] array = text3.Split(separator, 2, StringSplitOptions.None);
				if (array.Length != 2 || !array[0].Equals("Version"))
				{
					Debug.LogError("[Locale] 无效的项目头文件 - 无法读取版本号");
					return false;
				}
				if (!float.TryParse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
				{
					Debug.LogError("[Locale] 无效的项目头文件 - 无法读取版本号");
					return false;
				}
				if ((int)(result * 100f + 0.5f) < 110)
				{
					Debug.LogError("[Locale] 项目版本号太旧，无法加载");
					return false;
				}
				List<Language> list = new List<Language>();
				while (true)
				{
					string text4 = streamReader.ReadLine();
					if (string.IsNullOrEmpty(text4))
					{
						break;
					}
					string[] array2 = text4.Split(separator2, StringSplitOptions.None);
					if (array2.Length < 2)
					{
						continue;
					}
					Language language = new Language();
					if (!int.TryParse(array2[0], out language.lcId))
					{
						Debug.LogWarning("[Locale] 语言代码 " + array2[0] + " 不合法，将不加载语言 " + array2[1]);
						continue;
					}
					bool flag = false;
					foreach (Language item in list)
					{
						if (item.lcId == language.lcId)
						{
							flag = true;
							break;
						}
					}
					if ((language.lcId <= 0) | flag)
					{
						Debug.LogWarning($"[Locale] 语言代码 {language.lcId} 不合法或已重复，将不加载语言 {array2[1]}");
						continue;
					}
					language.name = array2[1];
					if (array2.Length > 2)
					{
						language.abbr = array2[2];
					}
					else
					{
						language.abbr = language.name.ToLower();
					}
					if (array2.Length > 3)
					{
						language.abbr2 = array2[3];
					}
					else
					{
						language.abbr2 = language.abbr;
					}
					if (array2.Length > 4)
					{
						if (!int.TryParse(array2[4], out language.fallback))
						{
							language.fallback = 0;
						}
					}
					else
					{
						language.fallback = 0;
					}
					if (array2.Length > 5)
					{
						int result2 = 0;
						if (!int.TryParse(array2[5], out result2))
						{
							result2 = 0;
						}
						language.glyph = (EGlyph)result2;
					}
					else
					{
						language.glyph = EGlyph.Latin;
					}
					if (array2.Length > 6)
					{
						if (array2[6].Equals("0") || array2[6].Equals("false", StringComparison.OrdinalIgnoreCase))
						{
							language.published = false;
						}
					}
					else
					{
						language.published = true;
					}
					list.Add(language);
				}
				Languages = list.ToArray();
				list.Clear();
				list = null;
				List<string> list2 = new List<string>();
				List<int> list3 = new List<int>();
				while (true)
				{
					string text5 = streamReader.ReadLine();
					if (string.IsNullOrEmpty(text5))
					{
						break;
					}
					string[] array3 = text5.Split(separator, StringSplitOptions.None);
					if (array3.Length >= 2)
					{
						int result3 = 0;
						if (!int.TryParse(array3[1], out result3))
						{
							result3 = 0;
						}
						list2.Add(array3[0]);
						list3.Add(result3);
					}
				}
				int count = list3.Count;
				resourcePages = new string[count];
				for (int i = 0; i < count; i++)
				{
					int num = int.MaxValue;
					int index = 0;
					for (int j = 0; j < count; j++)
					{
						if (list3[j] < num)
						{
							num = list3[j];
							index = j;
						}
					}
					list3[index] = int.MaxValue;
					resourcePages[i] = list2[index];
				}
				list3.Clear();
				list2.Clear();
				list3 = null;
				list2 = null;
			}
		}
		catch
		{
			Debug.LogError("[Locale] 读取项目头文件遭遇异常");
			return false;
		}
		int num2 = Languages.Length;
		int num3 = resourcePages.Length;
		strings = new string[num2][];
		floats = new float[num2][];
		namesIndexer = new Dictionary<string, int>();
		StreamReader[] array4 = new StreamReader[num3];
		bool flag2 = true;
		try
		{
			for (int k = 0; k < num3; k++)
			{
				string text6 = "";
				text6 = text + resourcePages[k] + ".txt";
				if (new FileInfo(text6).Exists)
				{
					array4[k] = new StreamReader(text6, detectEncodingFromByteOrderMarks: true);
					array4[k].Peek();
				}
			}
		}
		catch (Exception ex)
		{
			flag2 = false;
			Debug.LogError("[Locale] 访问文件路径失败：" + ex.ToString());
		}
		if (flag2)
		{
			try
			{
				StringBuilder stringBuilder = new StringBuilder();
				StringBuilder stringBuilder2 = new StringBuilder();
				for (int l = 0; l < num3; l++)
				{
					if (array4[l] == null)
					{
						continue;
					}
					while (true)
					{
						string text7 = array4[l].ReadLine();
						if (text7 == null)
						{
							break;
						}
						if (text7 == string.Empty)
						{
							continue;
						}
						stringBuilder.Clear();
						int length = text7.Length;
						for (int m = 0; m < length; m++)
						{
							if (text7[m] == '\t')
							{
								stringBuilder.Append(text7, 0, m);
								break;
							}
						}
						string key = UnescapeString(stringBuilder, stringBuilder2).ToString();
						if (!namesIndexer.ContainsKey(key))
						{
							namesIndexer.Add(key, namesIndexer.Count);
						}
					}
				}
				stringBuilder.Clear();
				stringBuilder2.Clear();
				stringBuilder = null;
				stringBuilder2 = null;
			}
			catch (Exception ex2)
			{
				flag2 = false;
				Debug.LogError("[Locale] 读取过程失败：" + ex2.ToString());
			}
		}
		for (int n = 0; n < num3; n++)
		{
			if (array4[n] != null)
			{
				array4[n].Close();
				array4[n] = null;
			}
		}
		array4 = null;
		return flag2;
	}

	public static bool LoadLanguage(int index)
	{
		if (!Loaded)
		{
			return false;
		}
		int num = Languages.Length;
		int num2 = resourcePages.Length;
		int namesCount = NamesCount;
		if ((uint)index >= num)
		{
			return false;
		}
		string[] array = (strings[index] = new string[namesCount]);
		float[] array2 = (floats[index] = new float[namesCount]);
		StreamReader[] array3 = new StreamReader[num2];
		bool flag = true;
		try
		{
			for (int i = 0; i < num2; i++)
			{
				string text = "";
				text = $"{ResourcesPath}{Languages[index].lcId}/{resourcePages[i]}.txt";
				if (new FileInfo(text).Exists)
				{
					array3[i] = new StreamReader(text, detectEncodingFromByteOrderMarks: true);
					array3[i].Peek();
				}
			}
		}
		catch (Exception ex)
		{
			flag = false;
			Debug.LogError("[Locale] 访问文件路径失败：" + ex.ToString());
		}
		if (flag)
		{
			try
			{
				(new char[1])[0] = '\t';
				StringBuilder stringBuilder = new StringBuilder();
				StringBuilder stringBuilder2 = new StringBuilder();
				for (int j = 0; j < num2; j++)
				{
					if (array3[j] == null)
					{
						continue;
					}
					while (true)
					{
						string text2 = array3[j].ReadLine();
						if (text2 == null)
						{
							break;
						}
						if (text2.Equals(string.Empty))
						{
							continue;
						}
						int length = text2.Length;
						int num3 = 0;
						stringBuilder.Clear();
						for (int k = num3; k < length; k++)
						{
							if (text2[k] == '\t')
							{
								stringBuilder.Append(text2, 0, k);
								num3 = k + 1;
								break;
							}
						}
						string text3 = UnescapeString(stringBuilder, stringBuilder2).ToString();
						if (text3.Length == 0 || !namesIndexer.ContainsKey(text3))
						{
							continue;
						}
						int num4 = namesIndexer[text3];
						if (array[num4] != null)
						{
							continue;
						}
						bool flag2 = false;
						bool flag3 = false;
						for (int l = num3; l < length; l++)
						{
							if (text2[l] == '\t')
							{
								num3 = l + 1;
								break;
							}
							if (text2[l] == '#')
							{
								flag2 = true;
							}
						}
						for (int m = num3; m < length; m++)
						{
							if (text2[m] == '\t')
							{
								num3 = m + 1;
								flag3 = true;
								break;
							}
						}
						if (flag3)
						{
							stringBuilder.Clear();
							stringBuilder.Append(text2, num3, length - num3);
							string s = (array[num4] = UnescapeString(stringBuilder, stringBuilder2).ToString());
							if (flag2 && !float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out array2[num4]))
							{
								array2[num4] = 0f;
								Debug.LogWarning("[Locale] 标识名 [" + text3 + "] 参数解析失败");
							}
						}
						else
						{
							Debug.LogWarning("[Locale] 标识名 [" + text3 + "] 文本读取失败");
						}
					}
				}
				stringBuilder.Clear();
				stringBuilder2.Clear();
				stringBuilder = null;
				stringBuilder2 = null;
				int num5 = 0;
				foreach (KeyValuePair<string, int> item in namesIndexer)
				{
					if (array[item.Value] == null)
					{
						array[item.Value] = item.Key;
						num5++;
					}
				}
				if (num5 > 0)
				{
					Debug.LogWarning($"[Locale] {num5} 个词条未找到翻译文本");
				}
			}
			catch (Exception ex2)
			{
				flag = false;
				Debug.LogError("[Locale] 读取过程失败：" + ex2.ToString());
			}
		}
		for (int n = 0; n < num2; n++)
		{
			if (array3[n] != null)
			{
				array3[n].Close();
				array3[n] = null;
			}
		}
		array3 = null;
		return flag;
	}

	public static bool LanguageLoaded(int index)
	{
		if (strings != null && (uint)index < strings.Length)
		{
			return strings[index] != null;
		}
		return false;
	}

	public static void Unload()
	{
		int languageCount = LanguageCount;
		for (int i = 0; i < languageCount; i++)
		{
			UnloadLanguage(i);
		}
		strings = null;
		floats = null;
		currentStrings = null;
		currentFloats = null;
		if (namesIndexer != null)
		{
			namesIndexer.Clear();
			namesIndexer = null;
		}
		Languages = null;
		currentLanguageIndex = 0;
		ResourcesPath = null;
		resourcePages = null;
	}

	public static void UnloadLanguage(int index)
	{
		if (strings != null && (uint)index < strings.Length)
		{
			if (currentStrings == strings[index])
			{
				currentStrings = null;
			}
			strings[index] = null;
		}
		if (floats != null && (uint)index < floats.Length)
		{
			if (currentFloats == floats[index])
			{
				currentFloats = null;
			}
			floats[index] = null;
		}
	}

	public static StringBuilder UnescapeString(StringBuilder sb, StringBuilder ret)
	{
		ret.Clear();
		for (int i = 0; i < sb.Length; i++)
		{
			if (sb[i] == '\\' && i < sb.Length - 1)
			{
				i++;
				switch (sb[i])
				{
				case '\\':
					ret.Append("\\");
					break;
				case 'r':
					ret.Append("\r");
					break;
				case 'n':
					ret.Append("\n");
					break;
				case 't':
					ret.Append("\t");
					break;
				case 'v':
					ret.Append("\v");
					break;
				case 'f':
					ret.Append("\f");
					break;
				default:
					ret.Append("\\");
					ret.Append(sb[i]);
					break;
				}
			}
			else
			{
				ret.Append(sb[i]);
			}
		}
		return ret;
	}
}
