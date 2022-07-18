using BepInEx.Logging;
using BepInEx.Configuration;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

using System;
using System.Linq;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using System.Reflection;

namespace ULTRAINTERFACE {
	public static class UI {
		public static ManualLogSource Log { get; private set; }

		public static GameObject ScrollRectPrefab { get; internal set; }
		public static GameObject ScrollbarPrefab { get; internal set; }
		public static GameObject ButtonPrefab { get; internal set; }
		public static GameObject TextPrefab { get; internal set; }

		public static CustomScrollView CreateScrollView(RectTransform parent, int width = 620, int height = 520, TextAnchor childAlignment = TextAnchor.UpperCenter, string name = "Custom Scroll View") {
			if (Log == null) Init();

			RectTransform scrollViewRect = new GameObject(name, new Type[]{typeof(RectTransform)}).GetComponent<RectTransform>();
			scrollViewRect.sizeDelta = new Vector2(width, height);
			scrollViewRect.localPosition = Vector3.zero;
			scrollViewRect.SetParent(parent, false);

			HorizontalLayoutGroup scrollViewLayoutGroup = scrollViewRect.gameObject.AddComponent<HorizontalLayoutGroup>();
			scrollViewLayoutGroup.childControlWidth = false;
			scrollViewLayoutGroup.childControlHeight = false;
			scrollViewLayoutGroup.spacing = 5;

			ScrollRect scrollRect = GameObject.Instantiate(ScrollRectPrefab, scrollViewRect).GetComponent<ScrollRect>();
			Scrollbar scrollbar = GameObject.Instantiate(ScrollbarPrefab, scrollViewRect).GetComponent<Scrollbar>();

			scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
			scrollRect.verticalScrollbar = scrollbar;

			RectTransform scrollbarRect = scrollbar.GetComponent<RectTransform>();
			scrollbarRect.sizeDelta = new Vector2(30, height);
			scrollbarRect.localPosition = Vector3.zero;

			RectTransform scrollRectTrans = scrollRect.GetComponent<RectTransform>();
			scrollRectTrans.sizeDelta = new Vector2(width - 35, height);
			scrollRectTrans.localPosition = Vector3.zero;

			RectTransform scrollRectContent = scrollRect.transform.GetChild(0).GetComponent<RectTransform>();
			scrollRectContent.sizeDelta = new Vector2(width, height + 160);
			scrollRectContent.localPosition = Vector3.zero;

			VerticalLayoutGroup scrollRectContentLayout = scrollRectContent.gameObject.AddComponent<VerticalLayoutGroup>();
			scrollRectContentLayout.childAlignment = childAlignment;
			scrollRectContentLayout.childForceExpandHeight = false;
			scrollRectContentLayout.childForceExpandWidth = false;
			scrollRectContentLayout.childControlHeight = false;
			scrollRectContentLayout.childControlWidth = false;
			scrollRectContentLayout.spacing = 10;

			ContentSizeFitter scrollRectContentFitter = scrollRectContent.gameObject.AddComponent<ContentSizeFitter>();
			scrollRectContentFitter.verticalFit = ContentSizeFitter.FitMode.MinSize;

			foreach (Transform child in scrollRectContent) {
				GameObject.Destroy(child.gameObject);
			}

			CustomScrollView scrollView = scrollViewRect.gameObject.AddComponent<CustomScrollView>();
			scrollView.Init(scrollRectContent, scrollRect, scrollbar);

			return scrollView;
		}

		public static Button CreateButton(RectTransform parent, string text = "New Button", int width = 160, int height = 50, bool forceCaps = true) {
			if (Log == null) Init();
			if (forceCaps) text = text.ToUpper();

			GameObject buttonGO = GameObject.Instantiate(ButtonPrefab, parent);
			buttonGO.name = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(text.ToLower());

			RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
			buttonRect.sizeDelta = new Vector2(width, height);
			buttonRect.anchoredPosition = Vector2.zero;

			Button button = buttonGO.GetComponent<Button>();
			button.onClick.RemoveAllListeners();

			// Disable all the persisten listeners
			for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++) {
				button.onClick.SetPersistentListenerState(i, UnityEventCallState.Off);
			}

			Text buttonText = buttonGO.GetComponentInChildren<Text>();
			buttonText.text = text;
			
			buttonGO.AddComponent<SelectableUI>();

			return button;
		}

		public static Text CreateText(RectTransform parent, string displayText = "New Text", int fontSize = 24, int width = 240, int height = 30, TextAnchor anchor = TextAnchor.MiddleCenter, bool forceCaps = true) {
			if (Log == null) Init();
			if (forceCaps) displayText = displayText.ToUpper();

			GameObject textGO = GameObject.Instantiate(TextPrefab, parent);
			textGO.name = "Text";

			RectTransform textRect = textGO.GetComponent<RectTransform>();
			textRect.sizeDelta = new Vector2(width, height);
			textRect.anchoredPosition = Vector2.zero;

			Text text = textGO.GetComponent<Text>();
			text.fontSize = fontSize;
			text.text = displayText;
			text.alignment = anchor;

			return text;
		}

		public static void Unload() {
			SceneManager.sceneLoaded -= SetupUI;

			Options.Unload();
		}

		internal static void Init() {
			Log = new ManualLogSource("ULTRAINTERFACE");
			BepInEx.Logging.Logger.Sources.Add(Log);

			SceneManager.sceneLoaded += SetupUI;
			SetupUI();
		}

		static void SetupUI(Scene scene, LoadSceneMode loadSceneMode) { SetupUI(); }
		static void SetupUI() {
			OptionsMenuToManager optionsMenuToManager = GameObject.FindObjectOfType<OptionsMenuToManager>();

			if (optionsMenuToManager == null) {
				Log.LogError("Failed to find the OptionsMenu, will attempt to setup UI on next scene load");
				return;
			}

			Options.OptionsMenu = optionsMenuToManager.transform.Find("OptionsMenu").GetComponent<RectTransform>();
			
			ScrollRectPrefab = Options.OptionsMenu.Find("Gameplay Options").Find("Scroll Rect (1)").gameObject;
			ScrollbarPrefab = Options.OptionsMenu.Find("Gameplay Options").Find("Scrollbar (1)").gameObject;

			// If "Options Scroll View" exists then another mod has set it up already
			Transform existingMenuTrans = Options.OptionsMenu.Find("Options Scroll View");
			if (!existingMenuTrans) {
				Options.OptionsScroll = CreateScrollView(Options.OptionsMenu, 215, 470, TextAnchor.UpperCenter, "Options Scroll View");
				RectTransform optionsScrollRect = Options.OptionsScroll.GetComponent<RectTransform>();
				optionsScrollRect.anchorMin = new Vector2(0, 0.5f);
				optionsScrollRect.anchorMax = new Vector2(0, 0.5f);
				optionsScrollRect.pivot = new Vector2(0, 0.5f);
				optionsScrollRect.anchoredPosition = new Vector3(20, 0, 3);

				// Move Buttons to the scroll view
				Options.MoveOptionToOptionScroll("Gameplay");
				Options.MoveOptionToOptionScroll("Controls");
				Options.MoveOptionToOptionScroll("Video");
				Options.MoveOptionToOptionScroll("Audio");
				Options.MoveOptionToOptionScroll("HUD");
				Options.MoveOptionToOptionScroll("Assist");
				Options.MoveOptionToOptionScroll("Colors");
				Options.MoveOptionToOptionScroll("Saves");
			} else {
				Options.OptionsScroll = existingMenuTrans.GetComponent<CustomScrollView>();

				if (Options.OptionsScroll == null) {
					Options.OptionsScroll = existingMenuTrans.gameObject.AddComponent<CustomScrollView>();

					Options.OptionsScroll.Init(
						existingMenuTrans.GetChild(0).GetChild(0).GetComponent<RectTransform>(),
						existingMenuTrans.GetChild(0).GetComponent<ScrollRect>(),
						existingMenuTrans.GetChild(1).GetComponent<Scrollbar>()
					);
				}
			}

			TextPrefab = Options.OptionsMenu.Find("Gameplay Options").Find("Text").gameObject;
			ButtonPrefab = Options.OptionsScroll.Content.Find("Gameplay").gameObject;

			// Create Registered Menus
			if (Options.RegisteredMenus == null) Options.RegisteredMenus = new Dictionary<string, OptionsMenuEvents>();

			foreach (string menuId in Options.RegisteredMenus.Keys.ToList()) {
				Options.CreateOptionsMenu(menuId, false);
			}

			Log.LogInfo($"Initalised Options");
		}
	}

	public static class Options {
		public static Dictionary<string, OptionsMenuEvents> RegisteredMenus { get; internal set; }
		public static CustomScrollView OptionsScroll { get; internal set; }
		public static RectTransform OptionsMenu { get; internal set; }

		public static OptionsMenu CreateOptionsMenu(string title, Action<OptionsMenu> createAction, bool forceCaps = true) {
			OptionsMenu optionsMenu = CreateOptionsMenu(title, forceCaps);
			optionsMenu.Events.Create.AddAndExecute(createAction, optionsMenu);

			return optionsMenu;
		}

		public static OptionsMenu CreateOptionsMenu(string title, bool forceCaps = true) {
			if (OptionsMenu == null) UI.Init();
			if (forceCaps) title = title.ToUpper();
			if (!Options.RegisteredMenus.ContainsKey(title)) Options.RegisteredMenus.Add(title, new OptionsMenuEvents());

			CustomScrollView scrollView = UI.CreateScrollView(Options.OptionsMenu, 620, 520, TextAnchor.MiddleCenter, CultureInfo.InvariantCulture.TextInfo.ToTitleCase(title.ToLower()) + " Options");
			Button optionsButton = UI.CreateButton(Options.OptionsScroll.Content, title, 160, 50);
			GameObject.Destroy(scrollView.GetComponent<HorizontalLayoutGroup>());
			scrollView.gameObject.AddComponent<HudOpenEffect>();

			Text titleText = UI.CreateText(scrollView.GetComponent<RectTransform>(), $"--{title}--");
			titleText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -75);
			titleText.transform.SetAsFirstSibling();

			RectTransform scrollViewRect = scrollView.GetComponent<RectTransform>();
			scrollViewRect.anchoredPosition = Vector2.zero;
			scrollViewRect.gameObject.SetActive(false);
			scrollViewRect.sizeDelta = Vector2.zero;
			scrollViewRect.anchorMin = Vector2.zero;
			scrollViewRect.anchorMax = Vector2.one;
			scrollViewRect.pivot = Vector2.one / 2f;

			RectTransform scrollRectRect = scrollView.ScrollRect.GetComponent<RectTransform>();
			scrollRectRect.anchoredPosition = new Vector2(0, -50);

			RectTransform scrollbarRect = scrollView.Scrollbar.GetComponent<RectTransform>();
			scrollbarRect.anchoredPosition = new Vector2(330, -50);

			RectTransform scrollRectContentRect = scrollView.Content.GetComponent<RectTransform>();
			scrollRectContentRect.anchorMin = new Vector2(0.5f, 0.5f);
			scrollRectContentRect.anchorMax = new Vector2(0.5f, 0.5f);
			scrollRectContentRect.pivot = new Vector2(0.5f, 0.5f);
			scrollRectContentRect.anchoredPosition = Vector2.zero;

			// Disable these options when clicked on the other buttons
			for (int i = 0; i < OptionsScroll.Content.transform.childCount; i++) {
				Button button = OptionsScroll.Content.transform.GetChild(i).GetComponent<Button>();
				if (button == null) continue;
				
				button.onClick.AddListener(() => { scrollView.gameObject.SetActive(false); });
			}

			// Disable the other options when this button is clicked
			for (int i = 0; i < OptionsMenu.childCount; i++) {
				Transform child = OptionsMenu.GetChild(i);
				if (!child.name.EndsWith(" Options")) continue;

				optionsButton.onClick.AddListener(() => { child.gameObject.SetActive(child == scrollView.transform); });
			}

			GamepadObjectSelector scrollViewGOS = scrollView.gameObject.AddComponent<GamepadObjectSelector>();
			typeof(GamepadObjectSelector).GetField("selectOnEnable", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(scrollViewGOS, false);
			typeof(GamepadObjectSelector).GetField("dontMarkTop", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(scrollViewGOS, true);

			BackSelectEvent scrollViewBSE = scrollView.gameObject.AddComponent<BackSelectEvent>();

			UnityEvent onBack = new UnityEvent();
			onBack.AddListener(() => { scrollViewGOS.PopTop(); } );
			typeof(BackSelectEvent).GetField("m_OnBack", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(scrollViewBSE, onBack);

			optionsButton.onClick.AddListener(() => { scrollViewGOS.Activate(); });
			optionsButton.onClick.AddListener(() => { scrollViewGOS.SetTop(); });

			Text optionsButtonText = optionsButton.GetComponentInChildren<Text>();
			optionsButtonText.text = title;

			OptionsMenu optionsMenu = scrollView.gameObject.AddComponent<OptionsMenu>();
			optionsMenu.Init(title, scrollView, optionsButton, optionsButtonText);

			optionsMenu.Events.LateCreate.Add((menu) => {
				SelectableUI firstSelectableUI = menu.ScrollView.Content.GetComponentInChildren<SelectableUI>();
				UI.Log.LogInfo($"selectable: {firstSelectableUI}");

				typeof(GamepadObjectSelector).GetField("target", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(menu.ScrollView.GetComponent<GamepadObjectSelector>(), firstSelectableUI ? firstSelectableUI.gameObject : null);
			});

			optionsMenu.Events.FirstShown.Add((menu) => { menu.ScrollToTop(); });

			return optionsMenu;
		}

		internal static void Unload() {
			RegisteredMenus.Clear();

			if (OptionsScroll != null) {
				while (OptionsScroll.Content.childCount > 0) {
					Transform buttonTrans = OptionsScroll.Content.GetChild(0);
					buttonTrans.SetParent(OptionsMenu, false);

					Button button = buttonTrans.GetComponent<Button>();
					if (button) button.onClick.RemoveAllListeners();
				}
				GameObject.Destroy(OptionsScroll.gameObject);
			}

			foreach (OptionsMenu menu in Resources.FindObjectsOfTypeAll<OptionsMenu>()) {
				GameObject.Destroy(GameObject.Find(menu.gameObject.name.Replace(" Options", "")));
				GameObject.Destroy(menu.gameObject);
			}
		}

		internal static void Init(RectTransform optionsMenu, CustomScrollView optionsScroll) {
			if (OptionsMenu != null) {
				UI.Log.LogError($"Options class already initalised, returning...");
				return;
			}
		}

		internal static void MoveOptionToOptionScroll(string optionName) {
			RectTransform option = OptionsMenu.Find(optionName).GetComponent<RectTransform>();
			option.SetParent(OptionsScroll.Content, false);
			option.anchoredPosition = Vector2.zero;
		}
	}

	public class CustomScrollView : MonoBehaviour {
		public RectTransform Content { get; private set; }
		public ScrollRect ScrollRect { get; private set; }
		public Scrollbar Scrollbar { get; private set; }

		internal void Init(RectTransform content, ScrollRect scrollRect, Scrollbar scrollbar) {
			if (Content != null) {
				UI.Log.LogError($"Scroll View \"{gameObject.name}\" already initalised, returning...");
				return;
			}

			this.Content = content;
			this.ScrollRect = scrollRect;
			this.Scrollbar = scrollbar;
		}
	}

	public class OptionsMenu : MonoBehaviour {
		public OptionsMenuEvents Events { 
			get {
				return Options.RegisteredMenus[ID];
			}
		}

		public CustomScrollView ScrollView { get; private set; }
		public Button OptionsButton { get; private set; }
		public Text Title { get; private set; }

		public bool HasBeenShown { get; private set; }
		public string ID { get; private set; }

		internal void Init (string id, CustomScrollView scrollView, Button optionsButton, Text title) {
			if (ScrollView != null) {
				UI.Log.LogError($"Options Menu \"{gameObject.name}\" already initalised, returning...");
				return;
			}

			this.ScrollView = scrollView;
			this.OptionsButton = optionsButton;
			this.Title = title;
			this.ID = id;

			this.HasBeenShown = false;

			foreach (Action<OptionsMenu> action in Events.Create) {
				action(this);
			}

			Camera.main.GetComponent<MonoBehaviour>().InvokeNextFrame(() => {
				foreach (Action<OptionsMenu> action in Events.LateCreate) {
					action(this);
				}
			});
		}

		public void ScrollToTop() {
			float movementVal = (ScrollView.Content.sizeDelta.y - ScrollView.ScrollRect.GetComponent<RectTransform>().sizeDelta.y) * -0.5f;
			if (movementVal > 0) return;

			ScrollView.Content.anchoredPosition = new Vector2(ScrollView.Content.anchoredPosition.x, movementVal);
		}

		public void SetTitle(string titleText, bool forceCaps = true) {
			if (forceCaps) titleText = titleText.ToUpper();
			if (Options.RegisteredMenus.ContainsKey(titleText)) {
				UI.Log.LogError($"Failed to change options title from \"{ID}\" to \"{titleText}\". Option \"{titleText}\" Already exists");
				return;
			}
			
			OptionsButton.GetComponentInChildren<Text>().text = titleText;
			Title.text = $"--{titleText}--";

			Options.RegisteredMenus.Add(titleText, Events);
			Options.RegisteredMenus.Remove(ID);

			ID = titleText;
		}

		void OnEnable() {
			if (!HasBeenShown) {
				HasBeenShown = true;

				this.InvokeNextFrame(() => {
				foreach (Action<OptionsMenu> action in Events.FirstShown) {
					action(this);
				}
			});
			}
		}
	}

	// This is a class simply because the constructer wasnt running for some reason
	public class OptionsMenuEvents {
		public List<Action<OptionsMenu>> Create = new List<Action<OptionsMenu>>();
		public List<Action<OptionsMenu>> LateCreate = new List<Action<OptionsMenu>>();
		public List<Action<OptionsMenu>> FirstShown = new List<Action<OptionsMenu>>();
	}

	// This is simply used for gamepad selection
	public class SelectableUI : MonoBehaviour {}
}