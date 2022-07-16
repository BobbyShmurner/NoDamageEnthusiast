using BepInEx.Logging;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

using System;

namespace ULTRAINTERFACE {
	public static class Settings {
		public static ManualLogSource Log { get; private set; }
 
		public static CustomScrollView OptionsScroll { get; private set; }
		public static RectTransform OptionsMenu { get; private set; }
 
		public static GameObject ScrollRectPrefab { get; private set; }
		public static GameObject ScrollbarPrefab { get; private set; }
		public static GameObject ButtonPrefab { get; private set; }
		public static GameObject TextPrefab { get; private set; }

		public static CustomScrollView CreateScrollView(RectTransform parent, int width = 620, int height = 520, TextAnchor childAlignment = TextAnchor.UpperCenter, string name = "Custom Scroll View") {
			if (Settings.OptionsMenu == null) Settings.InitSettings();

			RectTransform scrollViewRect = new GameObject(name, new Type[]{typeof(RectTransform)}).GetComponent<RectTransform>();
			scrollViewRect.sizeDelta = new Vector2(width, height);
			scrollViewRect.localPosition = Vector3.zero;
			scrollViewRect.SetParent(parent, false);

			HorizontalLayoutGroup scrollViewLayoutGroup = scrollViewRect.gameObject.AddComponent<HorizontalLayoutGroup>();
			scrollViewLayoutGroup.childControlWidth = false;
			scrollViewLayoutGroup.childControlHeight = false;
			scrollViewLayoutGroup.spacing = 5;

			ScrollRect scrollRect = GameObject.Instantiate(Settings.ScrollRectPrefab, scrollViewRect).GetComponent<ScrollRect>();
			Scrollbar scrollbar = GameObject.Instantiate(Settings.ScrollbarPrefab, scrollViewRect).GetComponent<Scrollbar>();

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

		public static SettingsMenu CreateSettingsMenu(string title, bool forceCaps = true) {
			if (OptionsMenu == null) Settings.InitSettings();
			if (forceCaps) title = title.ToUpper();

			CustomScrollView scrollView = CreateScrollView(Settings.OptionsMenu, 620, 520, TextAnchor.MiddleCenter, title + " Options");
			Button optionsButton = CreateButton(Settings.OptionsScroll.Content, title, 160, 50);
			GameObject.Destroy(scrollView.GetComponent<HorizontalLayoutGroup>());
			scrollView.gameObject.AddComponent<HudOpenEffect>();

			Text titleText = CreateText(scrollView.GetComponent<RectTransform>(), $"--{title}--");
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

			Text optionsButtonText = optionsButton.GetComponentInChildren<Text>();
			optionsButtonText.text = title;

			SettingsMenu settingsMenu = scrollView.gameObject.AddComponent<SettingsMenu>();
			settingsMenu.Init(scrollView, optionsButton, optionsButtonText);

			return settingsMenu;
		}

		public static Button CreateButton(RectTransform parent, string text = "New Button", int width = 160, int height = 50) {
			GameObject buttonGO = GameObject.Instantiate(ButtonPrefab, parent);
			buttonGO.name = text;

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

			return button;
		}

		public static Text CreateText(RectTransform parent, string displayText = "New Text", int fontSize = 24, TextAnchor anchor = TextAnchor.MiddleCenter, int width = 240, int height = 30) {
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

		public static void InitSettings() {
			Log = new ManualLogSource("Settings");
			BepInEx.Logging.Logger.Sources.Add(Log);

			SceneManager.sceneLoaded += SetupSettings;
			SetupSettings();
		}

		static void SetupSettings(Scene scene, LoadSceneMode loadSceneMode) { SetupSettings(); }
		static void SetupSettings() {
			OptionsMenuToManager optionsMenuToManager = GameObject.FindObjectOfType<OptionsMenuToManager>();

			if (optionsMenuToManager == null) {
				Log.LogWarning("Failed to find the OptionsMenu, will attempt to setup settings on next scene load");
				return;
			}

			OptionsMenu = optionsMenuToManager.transform.Find("OptionsMenu").GetComponent<RectTransform>();
			
			// If "Options Scroll View" exists then another mod has set it up already
			if (OptionsMenu.Find("Options Scroll View")) return;

			ScrollRectPrefab = OptionsMenu.Find("Gameplay Options").Find("Scroll Rect (1)").gameObject;
			ScrollbarPrefab = OptionsMenu.Find("Gameplay Options").Find("Scrollbar (1)").gameObject;

			OptionsScroll = CreateScrollView(OptionsMenu, 215, 470, TextAnchor.UpperCenter, "Options Scroll View");
			RectTransform optionsScrollRect = OptionsScroll.GetComponent<RectTransform>();
			optionsScrollRect.anchorMin = new Vector2(0, 0.5f);
			optionsScrollRect.anchorMax = new Vector2(0, 0.5f);
			optionsScrollRect.pivot = new Vector2(0, 0.5f);
			optionsScrollRect.anchoredPosition = new Vector3(20, 0, 3);

			// Move Buttons to the scroll view
			MoveOptionToOptionScroll("Gameplay");
			MoveOptionToOptionScroll("Controls");
			MoveOptionToOptionScroll("Video");
			MoveOptionToOptionScroll("Audio");
			MoveOptionToOptionScroll("HUD");
			MoveOptionToOptionScroll("Assist");
			MoveOptionToOptionScroll("Colors");
			MoveOptionToOptionScroll("Saves");

			TextPrefab = OptionsMenu.Find("Gameplay Options").Find("Text").gameObject;
			ButtonPrefab = OptionsScroll.Content.Find("Gameplay").gameObject;

			Log.LogInfo("Initalised Settings");
		}

		static void MoveOptionToOptionScroll(string optionName) {
			RectTransform option = OptionsMenu.Find(optionName).GetComponent<RectTransform>();
			option.SetParent(OptionsScroll.Content, false);
			option.anchoredPosition = Vector2.zero;
		}
	}

	public class CustomScrollView : MonoBehaviour {
		public void Init(RectTransform content, ScrollRect scrollRect, Scrollbar scrollbar) {
			if (Content != null) {
				Settings.Log.LogError($"Scroll View \"{gameObject.name}\" already initalised, returning...");
				return;
			}

			this.Content = content;
			this.ScrollRect = scrollRect;
			this.Scrollbar = scrollbar;
		}

		public RectTransform Content {get; private set; }
		public ScrollRect ScrollRect {get; private set; }
		public Scrollbar Scrollbar {get; private set; }
	}

	public class SettingsMenu : MonoBehaviour {
		public void Init (CustomScrollView scrollView, Button optionsButton, Text title) {
			if (ScrollView != null) {
				Settings.Log.LogError($"Settings Menu \"{gameObject.name}\" already initalised, returning...");
				return;
			}

			this.ScrollView = scrollView;
			this.OptionsButton = optionsButton;
			this.Title = title;
		}

		public void SetTitle(string title, bool forceCaps = true) {
			if (forceCaps) title = title.ToUpper();
			OptionsButton.GetComponentInChildren<Text>().text = title;
		}

		public CustomScrollView ScrollView {get; private set; }
		public Button OptionsButton {get; private set; }
		public Text Title {get; private set; }
	}
}