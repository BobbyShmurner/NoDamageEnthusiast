using BepInEx.Logging;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using System;

namespace ULTRAKILL {
	public static class Settings {
		public static ManualLogSource Log { get; private set; }
 
		public static RectTransform OptionsMenu { get; private set; }
		public static CustomScrollView OptionsScroll { get; private set; }
 
		public static GameObject ScrollRectPrefab { get; private set; }
		public static GameObject ScrollbarPrefab { get; private set; }

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
}