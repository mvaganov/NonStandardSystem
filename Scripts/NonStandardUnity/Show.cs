﻿using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NonStandard {
#if NONSTANDARD_UTILITY
	[InitializeOnLoad] public static class Utility_Define { static Utility_Define() { Utility.Define.Add("NONSTANDARD_SHOW"); } }
#endif
	public partial class Show : MonoBehaviour {
		public GameObject routeOutputTo;
		public bool NewlineAfterEachMessage = true;
#if UNITY_EDITOR
		private static bool _routeToUnityDebugLog = false;
		public static bool RouteToUnityDebugLog {
			get => _routeToUnityDebugLog;
			set {
				Route.onLog -= UnityEngine.Debug.Log;
				Route.onError -= UnityEngine.Debug.LogError;
				Route.onWarning -= UnityEngine.Debug.LogWarning;
				_routeToUnityDebugLog = value;
				if (_routeToUnityDebugLog) {
					Route.onLog += UnityEngine.Debug.Log;
					Route.onError += UnityEngine.Debug.LogError;
					Route.onWarning += UnityEngine.Debug.LogWarning;
				}
			} 
		}
#endif
		static Show() {
			Route.onLog -= DefaultLog;
			Route.onError -= DefaultError;
			Route.onWarning -= DefaultWarning;
#if UNITY_EDITOR
			RouteToUnityDebugLog = _routeToUnityDebugLog;
#endif
		}

		void Awake() {
			bool routed = RouteTMP();
			if (!routed) { routed = RouteUiText(); }
		}
		private string endl => NewlineAfterEachMessage ? "\n" : "";
		private bool RouteTMP() {
			TMPro.TMP_Text tmp = routeOutputTo.GetComponent<TMPro.TMP_Text>();
			if (tmp != null) {
				if (tmp.richText) {
					Route.onLog += s => tmp.text += s + endl;
					Route.onError += s => tmp.text += "<#ff0000>" + s + "</color>" + endl;
					Route.onWarning += s => tmp.text += "<#ffff00>" + s + "</color>" + endl;
				} else {
					Route.onAnyMessage += s => tmp.text += s + "\n";
				}
				return true;
			}
			return false;
		}
		private bool RouteUiText() {
			UnityEngine.UI.Text txt = routeOutputTo.GetComponent<UnityEngine.UI.Text>();
			if (txt != null) {
				Route.onAnyMessage = s => txt.text += s + "\n";
				return true;
			}
			return false;
		}
		public void Print(string message) { Log(message); }
	}
}
