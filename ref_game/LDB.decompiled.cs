using UnityEngine;

public static class LDB
{
	private static string protoResDir = "Prototypes/";

	private static PlayerProtoSet _players;

	private static ThemeProtoSet _themes;

	private static VegeProtoSet _veges;

	private static VeinProtoSet _veins;

	private static ItemProtoSet _items;

	private static EnemyProtoSet _enemies;

	private static FleetProtoSet _fleets;

	private static CreationPartProtoSet _creationParts;

	private static ModelProtoSet _models;

	private static RecipeProtoSet _recipes;

	private static TechProtoSet _techs;

	private static SignalProtoSet _signals;

	private static AudioProtoSet _audios;

	private static MIDIProtoSet _MIDIs;

	private static EffectEmitterProtoSet _effectEmitters;

	private static PromptProtoSet _prompts;

	private static AdvisorTipProtoSet _advisorTips;

	private static TutorialProtoSet _tutorial;

	private static AchievementProtoSet _achievements;

	private static MilestoneProtoSet _milestones;

	private static JournalPatternProtoSet _journalPatterns;

	private static GoalProtoSet _goals;

	private static CosmicMessageProtoSet _cosmicMessages;

	private static DoodadProtoSet _doodads;

	private static AbnormalityProtoSet _abnormalities;

	public static PlayerProtoSet players => LoadTable(ref _players);

	public static ThemeProtoSet themes => LoadTable(ref _themes);

	public static VegeProtoSet veges => LoadTable(ref _veges);

	public static VeinProtoSet veins => LoadTable(ref _veins);

	public static ItemProtoSet items => LoadTable(ref _items);

	public static EnemyProtoSet enemies => LoadTable(ref _enemies);

	public static FleetProtoSet fleets => LoadTable(ref _fleets);

	public static CreationPartProtoSet creationParts => LoadTable(ref _creationParts);

	public static ModelProtoSet models => LoadTable(ref _models);

	public static RecipeProtoSet recipes => LoadTable(ref _recipes);

	public static TechProtoSet techs => LoadTable(ref _techs);

	public static SignalProtoSet signals => LoadTable(ref _signals);

	public static AudioProtoSet audios => LoadTable(ref _audios);

	public static MIDIProtoSet MIDIs => LoadTable(ref _MIDIs);

	public static EffectEmitterProtoSet effectEmitters => LoadTable(ref _effectEmitters);

	public static PromptProtoSet prompts => LoadTable(ref _prompts);

	public static AdvisorTipProtoSet advisorTips => LoadTable(ref _advisorTips);

	public static TutorialProtoSet tutorial => LoadTable(ref _tutorial);

	public static AchievementProtoSet achievements => LoadTable(ref _achievements);

	public static MilestoneProtoSet milestones => LoadTable(ref _milestones);

	public static JournalPatternProtoSet journalPatterns => LoadTable(ref _journalPatterns);

	public static GoalProtoSet goals => LoadTable(ref _goals);

	public static CosmicMessageProtoSet cosmicMessages => LoadTable(ref _cosmicMessages);

	public static DoodadProtoSet doodads => LoadTable(ref _doodads);

	public static AbnormalityProtoSet abnormalities => LoadTable(ref _abnormalities);

	private static T LoadTable<T>(ref T tmp) where T : ProtoTable
	{
		if (tmp != null)
		{
			return tmp;
		}
		string path = protoResDir + typeof(T).Name;
		tmp = Resources.Load(path) as T;
		return tmp;
	}

	public static string ItemName(int itemId)
	{
		ItemProto itemProto = items.Select(itemId);
		if (itemProto != null)
		{
			return itemProto.name;
		}
		return "";
	}

	public static string RecipeName(int recipeId)
	{
		RecipeProto recipeProto = recipes.Select(recipeId);
		if (recipeProto != null)
		{
			return recipeProto.name;
		}
		return "";
	}
}
