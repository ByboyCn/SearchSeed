using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIStarDetail : ManualBehaviour
{
	private UIGame uiGame;

	[SerializeField]
	public UIAstroLogisticRoutePanel uiRoutePanel;

	[SerializeField]
	public UIAstroTodoPanel uiTodoPanel;

	[SerializeField]
	public RectTransform rectTrans;

	[SerializeField]
	public UIGenericMenuButton menuButton;

	[SerializeField]
	public UIResAmountEntry entryPrafab;

	[SerializeField]
	public InputField nameInput;

	[SerializeField]
	public UIInputField nameUIInput;

	[SerializeField]
	public Text typeText;

	[SerializeField]
	public UIComboBox displayCombo;

	[SerializeField]
	public Image displayComboColorCard;

	[SerializeField]
	public RectTransform paramGroup;

	[SerializeField]
	public Text massValueText;

	[SerializeField]
	public Text spectrValueText;

	[SerializeField]
	public Text radiusValueText;

	[SerializeField]
	public Text luminoValueText;

	[SerializeField]
	public Text temperatureValueText;

	[SerializeField]
	public Text ageValueText;

	[SerializeField]
	public Sprite unknownResIcon;

	[SerializeField]
	public GameObject trslBg;

	[SerializeField]
	public GameObject imgBg;

	[SerializeField]
	public GameObject loadingTextGo;

	[SerializeField]
	public GameObject baseInfoGroupGo;

	[SerializeField]
	public UIButton baseInfoBtn;

	[SerializeField]
	public UIButton routeBtn;

	[SerializeField]
	public UIButton memoBtn;

	[NonSerialized]
	public List<UIResAmountEntry> pool;

	[NonSerialized]
	public List<UIResAmountEntry> entries;

	[NonSerialized]
	public TodoModule[] starTodos;

	public Color displayComboNormalColor;

	public Color displayComboFilterColor;

	private int tabIndex;

	private UIResAmountEntry tipEntry;

	private UIResAmountEntry speedTipEntry;

	private RectTransform signalTagPickerRT;

	private const int kFixedTabHeight = 446;

	private int resourcesTabHeight;

	private long[] veinAmounts;

	private int[] veinCounts;

	private HashSet<int> tmp_ids = new HashSet<int>();

	private StarData _star;

	private bool calculated;

	private bool observed;

	private int defaultWidth => 240 + (int)"可变参数3".TranslateParam(0f);

	public int denialWidth => (int)(rectTrans.sizeDelta.x + 40f + ((UIGame.viewMode == EViewMode.Starmap) ? 72f : 0f) + 0.5f);

	public StarData star
	{
		get
		{
			return _star;
		}
		set
		{
			if (value != _star)
			{
				_star = value;
				OnStarDataSet();
			}
		}
	}

	public void SetResCount(int count)
	{
		int num = count * 20;
		if (num < 46)
		{
			num = 46;
		}
		resourcesTabHeight = 226 + num;
		rectTrans.sizeDelta = new Vector2(defaultWidth, resourcesTabHeight);
		paramGroup.anchoredPosition = new Vector2(paramGroup.anchoredPosition.x, -106 - num);
	}

	private void OnStarDataSet()
	{
		uiRoutePanel.astroId = ((_star != null) ? _star.astroId : 0);
		if (uiRoutePanel.active)
		{
			uiRoutePanel.RefreshEntries();
			uiRoutePanel.Refresh();
		}
		if (uiTodoPanel.active)
		{
			uiTodoPanel.SetData((_star != null) ? _star.astroId : 0);
		}
		menuButton.SetInfo(UIGenericMenuButton.EInfoType.StarmapAstro, (_star != null) ? _star.astroId : 0, uiGame.veinAmountDisplayFilter, 0, 0);
		for (int i = 0; i < entries.Count; i++)
		{
			UIResAmountEntry uIResAmountEntry = entries[i];
			uIResAmountEntry.SetEmpty();
			pool.Add(uIResAmountEntry);
		}
		entries.Clear();
		tipEntry = null;
		speedTipEntry = null;
		bool flag = false;
		if (veinAmounts == null)
		{
			veinAmounts = new long[64];
		}
		if (veinCounts == null)
		{
			veinCounts = new int[64];
		}
		Array.Clear(veinAmounts, 0, veinAmounts.Length);
		Array.Clear(veinCounts, 0, veinCounts.Length);
		calculated = false;
		if (star == null)
		{
			return;
		}
		if (!star.scanned)
		{
			star.RunScanningThread();
		}
		calculated = star.scanned;
		double magnitude = (star.uPosition - GameMain.mainPlayer.uPosition).magnitude;
		int num = ((star == GameMain.localStar) ? 2 : ((magnitude < 14400000.0) ? 3 : 4));
		observed = GameMain.history.universeObserveLevel >= num;
		if (calculated && observed)
		{
			star.CalcVeinAmounts(ref veinAmounts, tmp_ids, uiGame.veinAmountDisplayFilter);
			star.CalcVeinCounts(ref veinCounts, tmp_ids, uiGame.veinAmountDisplayFilter);
		}
		if (!nameInput.isFocused)
		{
			nameInput.text = star.displayName;
		}
		nameInput.ClearMemory();
		typeText.text = star.typeString;
		massValueText.text = star.mass.ToString("0.000") + " M    ";
		spectrValueText.text = star.spectr.ToString();
		radiusValueText.text = star.radius.ToString("0.00") + " R    ";
		double num2 = star.dysonLumino;
		luminoValueText.text = num2.ToString("0.000") + " L    ";
		temperatureValueText.text = star.temperature.ToString("#,##0") + " K";
		if (Localization.isKMG)
		{
			ageValueText.text = (star.age * star.lifetime).ToString("#,##0 ") + "百万亿年".Translate();
		}
		else
		{
			ageValueText.text = (star.age * star.lifetime * 0.01f).ToString("#,##0.00 ") + "百万亿年".Translate();
		}
		int num3 = 0;
		if (observed)
		{
			for (int j = 1; j < 15; j++)
			{
				int num4 = j;
				VeinProto veinProto = LDB.veins.Select(num4);
				ItemProto itemProto = LDB.items.Select(veinProto.MiningItem);
				if (!observed && j >= 7)
				{
					continue;
				}
				bool flag2 = veinAmounts[j] > 0;
				if ((veinProto != null && itemProto != null) & flag2)
				{
					UIResAmountEntry entry = GetEntry();
					entries.Add(entry);
					entry.SetInfo(num3, itemProto.name, veinProto.iconSprite, veinProto.description, j >= 7, highlightValue: false, (j == 7) ? "         /s" : "                ");
					entry.refId = num4;
					num3++;
					if (j == 7 && uiGame.veinAmountDisplayFilter == 1)
					{
						flag = true;
					}
				}
			}
		}
		if (observed)
		{
			for (int k = 0; k < star.planetCount; k++)
			{
				int waterItemId = star.planets[k].waterItemId;
				Sprite sprite = null;
				string text = "无".Translate();
				if (waterItemId <= 0)
				{
					continue;
				}
				ItemProto itemProto2 = LDB.items.Select(waterItemId);
				if (itemProto2 == null)
				{
					continue;
				}
				sprite = itemProto2.iconSprite;
				text = itemProto2.name;
				if (uiGame.veinAmountDisplayFilter == 0)
				{
					UIResAmountEntry entry2 = GetEntry();
					entries.Add(entry2);
					entry2.SetInfo(num3, text, sprite, itemProto2.description, itemProto2 != null && waterItemId != 1000, highlightValue: false, "");
					entry2.valueString = "海洋".Translate();
					num3++;
				}
				else
				{
					if (uiGame.veinAmountDisplayFilter != 1)
					{
						continue;
					}
					float num5 = 0f;
					_ = GameMain.history.miningSpeedScale;
					PlanetFactory factory = star.planets[k].factory;
					if (factory != null)
					{
						MinerComponent[] minerPool = factory.factorySystem.minerPool;
						int minerCursor = factory.factorySystem.minerCursor;
						PowerConsumerComponent[] consumerPool = factory.powerSystem.consumerPool;
						for (int l = 0; l < minerCursor; l++)
						{
							ref MinerComponent reference = ref minerPool[l];
							if (reference.id == l && reference.type == EMinerType.Water && consumerPool[reference.pcId].networkId > 0)
							{
								float num6 = (float)((double)reference.period / 600000.0);
								float num7 = 60f / num6;
								float num8 = (float)(0.0001 * (double)reference.speed);
								num5 += num8 * num7;
							}
						}
					}
					if (num5 > 0f)
					{
						UIResAmountEntry entry3 = GetEntry();
						entries.Add(entry3);
						entry3.SetInfo(num3, text ?? "", sprite, itemProto2.description, waterItemId != 1000, highlightValue: false, "");
						entry3.valueString = (num5 * GameMain.history.miningSpeedScale).ToString("0") + " /min";
						num3++;
						flag = true;
					}
				}
			}
		}
		if (observed)
		{
			for (int m = 0; m < star.planetCount; m++)
			{
				PlanetData planetData = star.planets[m];
				if (planetData.type != EPlanetType.Gas || planetData.gasItems == null)
				{
					continue;
				}
				for (int n = 0; n < planetData.gasItems.Length; n++)
				{
					ItemProto itemProto3 = LDB.items.Select(planetData.gasItems[n]);
					if (itemProto3 == null)
					{
						continue;
					}
					if (observed)
					{
						if (uiGame.veinAmountDisplayFilter == 0)
						{
							UIResAmountEntry entry4 = GetEntry();
							entries.Add(entry4);
							entry4.SetInfo(num3, itemProto3.name, itemProto3.iconSprite, itemProto3.description, highlightLabel: false, highlightValue: false, "        /s");
							StringBuilderUtility.WritePositiveFloat(entry4.sb, 0, 7, planetData.gasSpeeds[n]);
							entry4.DisplayStringBuilder();
							entry4.SetObserved(observed);
							num3++;
						}
						else if (uiGame.veinAmountDisplayFilter == 1)
						{
							double num9 = 0.0;
							float miningSpeedScale = GameMain.history.miningSpeedScale;
							PlanetFactory factory2 = planetData.factory;
							if (factory2 != null)
							{
								StationComponent[] stationPool = factory2.transport.stationPool;
								int stationCursor = factory2.transport.stationCursor;
								for (int num10 = 1; num10 < stationCursor; num10++)
								{
									StationComponent stationComponent = stationPool[num10];
									if (stationComponent == null || stationComponent.id != num10 || !stationComponent.isCollector)
									{
										continue;
									}
									for (int num11 = 0; num11 < stationComponent.storage.Length; num11++)
									{
										if (stationComponent.storage[num11].itemId == planetData.gasItems[n] && stationComponent.storage[num11].remoteLogic == ELogisticStorage.Supply)
										{
											PrefabDesc prefabDesc = LDB.items.Select(ItemProto.stationCollectorId).prefabDesc;
											double num12 = (double)prefabDesc.workEnergyPerTick * 60.0 / (double)prefabDesc.stationCollectSpeed;
											double num13 = miningSpeedScale;
											double num14 = num12;
											double gasTotalHeat = planetData.gasTotalHeat;
											double num15 = ((gasTotalHeat - num14 <= 0.0) ? 1f : ((float)((num13 * gasTotalHeat - num14) / (gasTotalHeat - num14))));
											double num16 = (double)stationComponent.collectionPerTick[num11] * num15;
											double num17 = 3600.0 * num16;
											if (num17 > 0.0)
											{
												num9 += num17;
												flag = true;
											}
										}
									}
								}
							}
							if (num9 > 0.0)
							{
								UIResAmountEntry entry5 = GetEntry();
								entries.Add(entry5);
								entry5.SetInfo(num3, itemProto3.name, itemProto3.iconSprite, itemProto3.description, highlightLabel: false, highlightValue: false, "");
								if (num9 < 10000.0)
								{
									entry5.valueString = $"{num9:0.0} /min";
								}
								else if (num9 < 1000000.0)
								{
									entry5.valueString = $"{num9 / 1000.0:0.00}k /min";
								}
								else
								{
									entry5.valueString = $"{num9 / 1000000.0:0.000}M /min";
								}
								entry5.SetObserved(observed);
								num3++;
							}
						}
						else
						{
							if (uiGame.veinAmountDisplayFilter != 2)
							{
								continue;
							}
							bool flag3 = false;
							PlanetFactory factory3 = planetData.factory;
							if (factory3 != null)
							{
								StationComponent[] stationPool2 = factory3.transport.stationPool;
								int stationCursor2 = factory3.transport.stationCursor;
								for (int num18 = 1; num18 < stationCursor2; num18++)
								{
									StationComponent stationComponent2 = stationPool2[num18];
									if (stationComponent2 != null && stationComponent2.id == num18 && stationComponent2.isCollector)
									{
										for (int num19 = 0; num19 < stationComponent2.storage.Length; num19++)
										{
											if (stationComponent2.storage[num19].itemId == planetData.gasItems[n] && stationComponent2.storage[num19].remoteLogic == ELogisticStorage.Supply)
											{
												flag3 = true;
												break;
											}
										}
									}
									if (flag3)
									{
										break;
									}
								}
							}
							if (!flag3)
							{
								UIResAmountEntry entry6 = GetEntry();
								entries.Add(entry6);
								entry6.SetInfo(num3, "可采集".Translate() + itemProto3.name, itemProto3.iconSprite, "环绕行星手动采集".Translate(), highlightLabel: false, highlightValue: false, "        /s");
								StringBuilderUtility.WritePositiveFloat(entry6.sb, 0, 7, planetData.gasSpeeds[n], 4);
								entry6.DisplayStringBuilder();
								entry6.SetObserved(observed);
								num3++;
							}
						}
						continue;
					}
					if (uiGame.veinAmountDisplayFilter != 1)
					{
						UIResAmountEntry entry7 = GetEntry();
						entries.Add(entry7);
						entry7.SetInfo(num3, "未知".Translate(), unknownResIcon, "", highlightLabel: false, highlightValue: false, "        /s");
						entry7.valueString = "未知".Translate();
						entry7.SetObserved(observed);
						num3++;
						continue;
					}
					bool flag4 = false;
					PlanetFactory factory4 = planetData.factory;
					if (factory4 != null)
					{
						StationComponent[] stationPool3 = factory4.transport.stationPool;
						int stationCursor3 = factory4.transport.stationCursor;
						for (int num20 = 1; num20 < stationCursor3; num20++)
						{
							StationComponent stationComponent3 = stationPool3[num20];
							if (stationComponent3 != null && stationComponent3.id == num20 && stationComponent3.isCollector)
							{
								for (int num21 = 0; num21 < stationComponent3.storage.Length; num21++)
								{
									if (stationComponent3.storage[num21].itemId == planetData.gasItems[n] && stationComponent3.storage[num21].remoteLogic == ELogisticStorage.Supply)
									{
										flag4 = true;
										break;
									}
								}
							}
							if (flag4)
							{
								break;
							}
						}
					}
					if (flag4)
					{
						UIResAmountEntry entry8 = GetEntry();
						entries.Add(entry8);
						entry8.SetInfo(num3, "未知".Translate(), unknownResIcon, "", highlightLabel: false, highlightValue: false, "        /s");
						entry8.valueString = "未知".Translate();
						entry8.SetObserved(observed);
						num3++;
					}
				}
			}
		}
		if (!observed)
		{
			UIResAmountEntry entry9 = GetEntry();
			entries.Add(entry9);
			entry9.SetInfo(num3, "", null, "", highlightLabel: true, highlightValue: true, "");
			tipEntry = entry9;
			num3++;
		}
		if (flag)
		{
			UIResAmountEntry entry10 = GetEntry();
			entries.Add(entry10);
			entry10.SetInfo(num3, "实际采集速度".Translate(), null, "", highlightLabel: false, highlightValue: false, "");
			speedTipEntry = entry10;
			speedTipEntry.valueString = "";
			num3++;
		}
		SetResCount(num3);
		RefreshDynamicProperties();
		RefreshTabPanel();
	}

	public void RefreshStarTodos()
	{
		if (GameMain.data == null)
		{
			return;
		}
		GameData gameData = GameMain.data;
		if (starTodos == null)
		{
			starTodos = new TodoModule[gameData.galaxy.starCount];
		}
		TodoModule[] buffer = gameData.galacticDigital.todos.buffer;
		int cursor = gameData.galacticDigital.todos.cursor;
		for (int i = 1; i < cursor; i++)
		{
			if (buffer[i].ownerType != ETodoModuleOwnerType.Astro)
			{
				continue;
			}
			int ownerId = buffer[i].ownerId;
			if (ownerId % 100 == 0)
			{
				int num = ownerId / 100 - 1;
				if (num >= 0 && num < starTodos.Length)
				{
					starTodos[num] = buffer[i];
				}
			}
		}
	}

	public void RefreshDynamicProperties()
	{
		bool isInfiniteResource = GameMain.data.gameDesc.isInfiniteResource;
		if (veinAmounts == null)
		{
			veinAmounts = new long[64];
		}
		if (veinCounts == null)
		{
			veinCounts = new int[64];
		}
		Array.Clear(veinAmounts, 0, veinAmounts.Length);
		Array.Clear(veinCounts, 0, veinCounts.Length);
		if (star != null)
		{
			if (!calculated && star.scanned)
			{
				OnStarDataSet();
				return;
			}
			calculated = star.scanned;
			bool num = observed;
			double magnitude = (star.uPosition - GameMain.mainPlayer.uPosition).magnitude;
			int num2 = ((star == GameMain.localStar) ? 2 : ((magnitude < 14400000.0) ? 3 : 4));
			observed = GameMain.history.universeObserveLevel >= num2;
			if (num != observed)
			{
				OnStarDataSet();
				return;
			}
			loadingTextGo.SetActive(observed && !calculated);
			if (calculated && observed)
			{
				star.CalcVeinAmounts(ref veinAmounts, tmp_ids, uiGame.veinAmountDisplayFilter);
				star.CalcVeinCounts(ref veinCounts, tmp_ids, uiGame.veinAmountDisplayFilter);
			}
			foreach (UIResAmountEntry entry in entries)
			{
				if (entry.refId <= 0)
				{
					continue;
				}
				if (observed)
				{
					long num3 = veinAmounts[entry.refId];
					long value = veinCounts[entry.refId];
					if (entry.refId == 7)
					{
						double num4 = (double)num3 * (double)VeinData.oilSpeedMultiplier;
						if (uiGame.veinAmountDisplayFilter == 1)
						{
							num4 *= (double)GameMain.history.miningSpeedScale;
						}
						StringBuilderUtility.WritePositiveFloat(entry.sb, 0, 8, (float)num4);
						entry.DisplayStringBuilder();
					}
					else
					{
						if (isInfiniteResource)
						{
							StringBuilderUtility.WriteCommaULong(entry.sb, 0, 16, (ulong)value);
						}
						else if (num3 < 1000000000)
						{
							StringBuilderUtility.WriteCommaULong(entry.sb, 0, 16, (ulong)num3);
						}
						else
						{
							StringBuilderUtility.WriteKMG(entry.sb, 15, num3);
						}
						entry.DisplayStringBuilder();
					}
					entry.SetObserved(_observed: true);
				}
				else
				{
					entry.valueString = "未知".Translate();
					if (entry.refId > 7)
					{
						entry.overrideLabel = "未知珍奇信号".Translate();
					}
					if (entry.refId > 7)
					{
						entry.SetObserved(_observed: false);
					}
					else
					{
						entry.SetObserved(_observed: true);
					}
				}
			}
			if (tipEntry != null)
			{
				if (!observed)
				{
					tipEntry.valueText.resizeTextForBestFit = true;
					tipEntry.valueString = string.Format("宇宙探索等级".Translate(), num2);
				}
				else
				{
					tipEntry.valueText.resizeTextForBestFit = false;
					tipEntry.valueString = "";
				}
				SetResCount(observed ? (entries.Count - 1) : entries.Count);
			}
		}
		displayCombo.transform.parent.SetAsLastSibling();
	}

	private UIResAmountEntry GetEntry()
	{
		if (pool.Count > 0)
		{
			UIResAmountEntry result = pool[pool.Count - 1];
			pool.RemoveAt(pool.Count - 1);
			return result;
		}
		return UnityEngine.Object.Instantiate(entryPrafab, entryPrafab.transform.parent);
	}

	protected override void _OnCreate()
	{
		uiGame = UIRoot.instance.uiGame;
		pool = new List<UIResAmountEntry>();
		entries = new List<UIResAmountEntry>();
		uiRoutePanel._Create();
		uiTodoPanel._Create();
	}

	protected override void _OnDestroy()
	{
		pool.Clear();
		pool = null;
		entries.Clear();
		entries = null;
		uiGame = null;
		uiRoutePanel._Destroy();
		uiTodoPanel._Destroy();
	}

	protected override bool _OnInit()
	{
		uiRoutePanel._Init(base.data);
		uiTodoPanel._Init(base.data);
		return true;
	}

	protected override void _OnFree()
	{
		uiRoutePanel._Free();
		uiTodoPanel._Free();
		if (starTodos != null)
		{
			Array.Clear(starTodos, 0, starTodos.Length);
		}
	}

	protected override void _OnOpen()
	{
		if (star == null)
		{
			_Close();
			return;
		}
		displayCombo.itemIndex = uiGame.veinAmountDisplayFilter;
		nameInput.onEndEdit.AddListener(OnNameInputEndEdit);
		displayCombo.onItemIndexChange.AddListener(OnDisplayFilterChange);
		nameUIInput.calcIconPickerPopupPos = CalcSignalIconPickerPos;
		baseInfoBtn.onClick += OnTabButtonClick;
		routeBtn.onClick += OnTabButtonClick;
		memoBtn.onClick += OnTabButtonClick;
		GameMain.galaxy.onAstroNameChange += OnAstroNameChange;
		SetToDefaultLayOut();
		RefreshStarTodos();
		double magnitude = (star.uPosition - GameMain.mainPlayer.uPosition).magnitude;
		int num = ((star == GameMain.localStar) ? 2 : ((magnitude < 14400000.0) ? 3 : 4));
		observed = GameMain.history.universeObserveLevel >= num;
		signalTagPickerRT = UIRoot.instance.uiGame.signalTagPicker.transform as RectTransform;
	}

	protected override void _OnClose()
	{
		nameInput.onEndEdit.RemoveAllListeners();
		displayCombo.onItemIndexChange.RemoveAllListeners();
		nameUIInput.calcIconPickerPopupPos = null;
		baseInfoBtn.onClick -= OnTabButtonClick;
		routeBtn.onClick -= OnTabButtonClick;
		memoBtn.onClick -= OnTabButtonClick;
		GameMain.galaxy.onAstroNameChange -= OnAstroNameChange;
		if (nameInput.isFocused)
		{
			EventSystem.current.SetSelectedGameObject(null);
		}
		star = null;
		uiRoutePanel._Close();
		uiTodoPanel._Close();
		observed = false;
	}

	protected override void _OnUpdate()
	{
		if (Time.frameCount % 30 == 0)
		{
			RefreshDynamicProperties();
		}
		float num = (uiGame.inZScreen ? 600 : (-20));
		if (Mathf.Abs(rectTrans.anchoredPosition.x - num) > 0.5f)
		{
			rectTrans.anchoredPosition = new Vector2(num, rectTrans.anchoredPosition.y);
		}
		bool flag = UIGame.viewMode == EViewMode.Starmap;
		trslBg.gameObject.SetActive(!flag);
		imgBg.gameObject.SetActive(flag);
		displayComboColorCard.color = ((uiGame.veinAmountDisplayFilter > 0) ? displayComboFilterColor : displayComboNormalColor);
		RefreshTabPanel();
	}

	protected override void _OnLateUpdate()
	{
		uiTodoPanel._LateUpdate();
	}

	private void RefreshTabPanel()
	{
		if (tabIndex == 0)
		{
			rectTrans.sizeDelta = new Vector2(defaultWidth, resourcesTabHeight);
			baseInfoBtn.highlighted = true;
			routeBtn.highlighted = false;
			memoBtn.highlighted = false;
		}
		else if (tabIndex == 1)
		{
			rectTrans.sizeDelta = new Vector2(340f, 446f);
			baseInfoBtn.highlighted = false;
			routeBtn.highlighted = true;
			memoBtn.highlighted = false;
			uiRoutePanel._Update();
		}
		else if (tabIndex == 2)
		{
			rectTrans.sizeDelta = new Vector2(300f, 446f);
			baseInfoBtn.highlighted = false;
			routeBtn.highlighted = false;
			memoBtn.highlighted = true;
			uiTodoPanel._Update();
		}
	}

	private void SetToDefaultLayOut()
	{
		if (tabIndex == 0)
		{
			baseInfoGroupGo.SetActive(value: true);
			uiRoutePanel._Close();
			menuButton.gameObject.SetActive(value: true);
			uiTodoPanel._Close();
		}
		else if (tabIndex == 1)
		{
			baseInfoGroupGo.SetActive(value: false);
			uiRoutePanel._Open();
			uiRoutePanel.RefreshEntries();
			menuButton.gameObject.SetActive(value: false);
			uiTodoPanel._Close();
		}
		else if (tabIndex == 2)
		{
			baseInfoGroupGo.SetActive(value: false);
			uiRoutePanel._Close();
			menuButton.gameObject.SetActive(value: false);
			uiTodoPanel.SetData((_star != null) ? _star.astroId : 0);
			uiTodoPanel._Open();
		}
		RefreshTabPanel();
	}

	public Vector2 CalcSignalIconPickerPos()
	{
		Vector3 position = (nameInput.transform as RectTransform).TransformPoint(new Vector3(0f - signalTagPickerRT.rect.width, 0f, 0f));
		RectTransform rectTransform = signalTagPickerRT.parent as RectTransform;
		return (Vector2)rectTransform.InverseTransformPoint(position) - rectTransform.rect.size * 0.5f;
	}

	private void OnNameInputEndEdit(string str)
	{
		if (star != null)
		{
			string text = nameInput.text;
			text = text.Trim();
			if (string.IsNullOrEmpty(text))
			{
				star.overrideName = "";
			}
			else if (string.Equals(text, star.name))
			{
				star.overrideName = "";
			}
			else
			{
				star.overrideName = text;
			}
			GameMain.galaxy.NotifyAstroNameChange(star.astroId);
			nameInput.text = star.displayName;
		}
		if (UISignalTagPicker.isOpened)
		{
			UISignalTagPicker.Close();
		}
	}

	private void OnDisplayFilterChange()
	{
		if (displayCombo.itemIndex >= 0)
		{
			uiGame.veinAmountDisplayFilter = displayCombo.itemIndex;
		}
		else
		{
			uiGame.veinAmountDisplayFilter = 0;
		}
		OnStarDataSet();
	}

	public void OnTabButtonClick(int data)
	{
		tabIndex = data;
		if (tabIndex == 0)
		{
			baseInfoGroupGo.SetActive(value: true);
			uiRoutePanel._Close();
			menuButton.gameObject.SetActive(value: true);
			uiTodoPanel._Close();
		}
		else if (tabIndex == 1)
		{
			baseInfoGroupGo.SetActive(value: false);
			uiRoutePanel._Open();
			uiRoutePanel.RefreshEntries();
			menuButton.gameObject.SetActive(value: false);
			uiTodoPanel._Close();
		}
		else if (tabIndex == 2)
		{
			baseInfoGroupGo.SetActive(value: false);
			uiRoutePanel._Close();
			menuButton.gameObject.SetActive(value: false);
			uiTodoPanel.SetData((_star != null) ? _star.astroId : 0);
			uiTodoPanel._Open();
		}
	}

	private void OnAstroNameChange(int astroId)
	{
		if (astroId == star?.astroId && !nameInput.isFocused)
		{
			nameInput.text = star.displayName;
		}
	}
}
