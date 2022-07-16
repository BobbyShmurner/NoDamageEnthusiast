using BepInEx.Logging;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

using System;

namespace ULTRAKILL {
	public static class Settings {
		public static ManualLogSource Log { get; private set; }
 
		public static CustomScrollView OptionsScroll { get; private set; }
		public static RectTransform OptionsMenu { get; private set; }
 
		public static GameObject ScrollRectPrefab { get; private set; }
		public static GameObject ScrollbarPrefab { get; private set; }
		public static GameObject ButtonPrefab { get; private set; }

		public static CustomScrollView CreateScrollView(RectTransform parent, int width = 620, int height = 520, string name = "Custom Scroll View") {
			return new CustomScrollView(parent, width, height, name);
		}

		public static SettingsMenu CreateSettingsMenu(string title, bool forceCaps = true) {
			return new SettingsMenu(title, forceCaps);
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

			OptionsScroll = new CustomScrollView(OptionsMenu, 215, 470, "Options Scroll View");
			OptionsScroll.Container.anchorMin = new Vector2(0, 0.5f);
			OptionsScroll.Container.anchorMax = new Vector2(0, 0.5f);
			OptionsScroll.Container.pivot = new Vector2(0, 0.5f);
			OptionsScroll.Container.anchoredPosition = new Vector3(20, 0, 3);

			// Move Buttons to the scroll view
			MoveOptionToOptionScroll("Gameplay");
			MoveOptionToOptionScroll("Controls");
			MoveOptionToOptionScroll("Video");
			MoveOptionToOptionScroll("Audio");
			MoveOptionToOptionScroll("HUD");
			MoveOptionToOptionScroll("Assist");
			MoveOptionToOptionScroll("Colors");
			MoveOptionToOptionScroll("Saves");

			ButtonPrefab = OptionsScroll.Content.Find("Gameplay").gameObject;

			Log.LogInfo("Initalised Settings");
		}

		static void MoveOptionToOptionScroll(string optionName) {
			RectTransform option = OptionsMenu.Find(optionName).GetComponent<RectTransform>();
			option.SetParent(OptionsScroll.Content, false);
			option.anchoredPosition = Vector2.zero;
		}
	}

	public class CustomScrollView {
		public CustomScrollView(RectTransform parent, int width = 620, int height = 520, string name = "Custom Scroll View") {
			if (Settings.OptionsMenu == null) Settings.InitSettings();

			RectTransform scrollView = new GameObject(name, new Type[]{typeof(RectTransform)}).GetComponent<RectTransform>();
			scrollView.sizeDelta = new Vector2(width, height);
			scrollView.localPosition = Vector3.zero;
			scrollView.SetParent(parent, false);

			HorizontalLayoutGroup scrollViewLayoutGroup = scrollView.gameObject.AddComponent<HorizontalLayoutGroup>();
			scrollViewLayoutGroup.childControlWidth = false;
			scrollViewLayoutGroup.childControlHeight = false;
			scrollViewLayoutGroup.spacing = 5;

			ScrollRect scrollRect = GameObject.Instantiate(Settings.ScrollRectPrefab, scrollView).GetComponent<ScrollRect>();
			Scrollbar scrollbar = GameObject.Instantiate(Settings.ScrollbarPrefab, scrollView).GetComponent<Scrollbar>();

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
			scrollRectContentLayout.childAlignment = TextAnchor.UpperCenter;
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

			this.Container = scrollView;
			this.Content = scrollRectContent;
		}

		public RectTransform Container {get; private set; }
		public RectTransform Content {get; private set; }
	}

	public class SettingsMenu {
		public SettingsMenu(string title, bool forceCaps = true) {
			if (Settings.OptionsMenu == null) Settings.InitSettings();

			if (forceCaps) title = title.ToUpper();

			OptionsButton = Settings.CreateButton(Settings.OptionsScroll.Content, title, 160, 50);
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