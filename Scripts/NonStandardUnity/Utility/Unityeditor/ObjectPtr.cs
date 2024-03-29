﻿using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using NonStandard.Extension;
using UnityEditor;
#endif

namespace NonStandard.Utility.UnityEditor {
	public interface IReference { object Dereference(); }

	[System.Serializable]
	public struct ObjectPtr : IReference {
		public Object data;
		public Object Data { get { return data; } set { data = value; } }
		public object Dereference() {
			return data;
		}
		public TYPE GetAs<TYPE>() where TYPE : class { return data as TYPE; }
	}

	[System.Serializable]
	// TODO
	public class StringWithDefaults {
		public string str;
		[HideInInspector] public Object defaultsSource;
		[HideInInspector] public string defaultsFunction;
		public void SetDefaultsSource(Object obj, string func) {
			// see EditorGUI_EnumPopup. there needs to be a way to get generalized drop-downs like that.
		}
	}

#if UNITY_EDITOR
	// enables the ObjectPtr property, not fully utilized by Timer, but certainly utilized by NonStandardAssets
	[CustomPropertyDrawer(typeof(NonStandard.Utility.UnityEditor.ObjectPtr))]
	public class PropertyDrawer_ObjectPtr : PropertyDrawer {
		delegate Object SelectNextObjectFunction();
		public static bool showLabel = true;
		public int choice = 0;
		string[] choices_name = { };
		SelectNextObjectFunction[] choices_selectFunc = { };
		Object choicesAreFor;
		System.Type[] possibleResponses;
		string[] cached_typeCreationList_names;
		SelectNextObjectFunction[] cached_TypeCreationList_function;

		public static string setToNull = "set to null", delete = "delete";
		public static float defaultOptionWidth = 16, defaultLabelWidth = 48, unitHeight = 16;
		/// <summary>The namespaces to get default selectable classes from</summary>
		protected virtual string[] GetNamespacesForNewComponentOptions() { return null; }

		public override float GetPropertyHeight(SerializedProperty _property, GUIContent label) {
			return StandardCalcPropertyHeight();
		}

		public static float StandardCalcPropertyHeight() {
			// SerializedProperty asset = _property.FindPropertyRelative("data");
			return unitHeight;//base.GetPropertyHeight (asset, label);
		}

		/// <summary>
		/// When the ObjectPtr points to nothing, this method generates the objects that can be created by default
		/// </summary>
		/// <param name="self">Self.</param>
		/// <param name="names">Names.</param>
		/// <param name="functions">Functions.</param>
		private void GenerateTypeCreationList(Component self, out string[] names, out SelectNextObjectFunction[] functions) {
			List<string> list = new List<string>();
			List<SelectNextObjectFunction> list_of_data = new List<SelectNextObjectFunction>();
			string[] theList = GetNamespacesForNewComponentOptions();
			if (theList != null) {
				for (int i = 0; i < theList.Length; ++i) {
					string namespaceName = theList[i];
					possibleResponses = System.Reflection.Assembly.GetExecutingAssembly().GetTypesInNamespace(namespaceName);
					list.AddRange(ReflectionExtension.TypeNamesWithoutNamespace(possibleResponses, namespaceName));
					for (int t = 0; t < possibleResponses.Length; t++) {
						System.Type nextT = possibleResponses[t];
						list_of_data.Add(() => {
							return CreateSelectedClass(nextT, self);
						});
					}
				}
			}
			list.Insert(0, (theList != null) ? "<-- select Object or create..." : "<-- select Object");
			list_of_data.Insert(0, null);
			names = list.ToArray();
			functions = list_of_data.ToArray();
		}

		private void CleanTypename(ref string typename) {
			int lastDot = typename.LastIndexOf('.');
			if (lastDot >= 0) { typename = typename.Substring(lastDot + 1); }
		}

		public static T EditorGUI_EnumPopup<T>(Rect _position, T value) {
			System.Type t = typeof(T);
			if (t.IsEnum) {
				string[] names = System.Enum.GetNames(t);
				string thisone = value.ToString();
				int index = System.Array.IndexOf(names, thisone);
				index = EditorGUI.Popup(_position, index, names);
				value = (T)System.Enum.Parse(t, names[index]);
			}
			return value;
		}

		private void GenerateChoicesForSelectedObject(Component self, out string[] names, out SelectNextObjectFunction[] functions) {
			List<string> components = new List<string>();
			List<SelectNextObjectFunction> nextSelectionFunc = new List<SelectNextObjectFunction>();
			string typename = choicesAreFor.GetType().ToString();
			CleanTypename(ref typename);
			components.Add(typename);
			nextSelectionFunc.Add(null);
			GameObject go = choicesAreFor as GameObject;
			bool addSetToNull = true;
			Object addDelete = null;
			if (go != null) {
				Component[] c = go.GetComponents<Component>();
				for (int i = 0; i < c.Length; i++) {
					Component comp = c[i];
					if (comp != self) {
						typename = comp.GetType().ToString();
						CleanTypename(ref typename);
						components.Add(typename);
						nextSelectionFunc.Add(() => { return comp; });
					}
				}
				addSetToNull = true;
			} else if (choicesAreFor is Component) {
				components.Add(".gameObject");
				GameObject gob = (choicesAreFor as Component).gameObject;
				nextSelectionFunc.Add(() => { return gob; });
				addSetToNull = true;
				addDelete = choicesAreFor;
			}
			if (addSetToNull) {
				components.Add(setToNull);
				nextSelectionFunc.Add(() => {
					choice = 0; return null;
				});
			}
			if (addDelete != null) {
				components.Add(delete);
				nextSelectionFunc.Add(() => {
					//Object.DestroyImmediate(addDelete); 
					itemsToCleanup.Add(addDelete);
					choice = 0; return null;
				});
			}
			names = components.ToArray();
			functions = nextSelectionFunc.ToArray();
		}

		[ExecuteInEditMode]
		private class IndirectCleaner : MonoBehaviour {
			public List<Object> itemsToCleanup;
			private void Update() {
				for (int i = itemsToCleanup.Count - 1; i >= 0; --i) {
					Object o = itemsToCleanup[i];
					if (o != null) { Object.DestroyImmediate(o); }
				}
				itemsToCleanup.Clear();
				DestroyImmediate(this); // cleaning lady disposes of herself too
			}
		}
		private List<Object> itemsToCleanup = new List<Object>();
		private void RequestCleanup(Component self) {
			// if any items need to be deleted, don't do it now! the UI is in the middle of being drawn!
			// create a separate process that will do it for you
			IndirectCleaner cleaner = self.gameObject.AddComponent<IndirectCleaner>();
			cleaner.itemsToCleanup = this.itemsToCleanup;
		}

		/// <summary>called right after an object is assigned</summary>
		public virtual Object FilterImmidiate(Object obj, Component self) {
			return obj;
		}

		/// <summary>called right after a new component is created to be assigned</summary>
		protected virtual Object FilterNewComponent(System.Type nextT, Component self, Component newlyCreatedComponent) {
			return newlyCreatedComponent;
		}

		/// <summary>called just before UI is finished. This is the last chance to adjust the new setting.</summary>
		public virtual Object FilterFinal(Object newObjToReference, Object prevObj, Component self) {
			return newObjToReference;
		}

		private Object CreateSelectedClass(System.Type nextT, Component self) {
			Object obj = null;
			if (self != null && self.gameObject != null) {
				GameObject go = self.gameObject;
				if (nextT.IsSubclassOf(typeof(ScriptableObject))) {
					obj = ScriptableObjectUtility.CreateAsset(nextT);
				} else {
					Component newComponent = go.AddComponent(nextT);
					obj = FilterNewComponent(nextT, self, newComponent);
				}
			}
			return obj;
		}

		public static SerializedProperty ObjectPtrAsset(SerializedProperty _property) {
			SerializedProperty asset = _property.FindPropertyRelative("data");
			//asset = asset.FindPropertyRelative("data");
			return asset;
		}

		public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
			EditorGUI.BeginProperty(_position, GUIContent.none, _property);
			SerializedProperty asset = ObjectPtrAsset(_property);
			int oldIndent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			if (PropertyDrawer_ObjectPtr.showLabel) {
				_position = EditorGUI.PrefixLabel(_position, GUIUtility.GetControlID(FocusType.Passive), _label);
			}
			Component self = _property.serializedObject.targetObject as Component;
			if (asset != null) {
				Object prevObj = asset.objectReferenceValue;
				asset.objectReferenceValue = EditorGUIObjectReference(_position, asset.objectReferenceValue, self);
				asset.objectReferenceValue = FilterFinal(asset.objectReferenceValue, prevObj, self);
				//Contingentable cself = self as Contingentable;
				//if(prevObj != asset.objectReferenceValue && cself != null && cself.ContingencyRecursionCheck() != null) {
				//	Debug.LogWarning("Disallowing recursion of " + asset.objectReferenceValue);
				//	asset.objectReferenceValue = prevObj;
				//}
			}
			EditorGUI.indentLevel = oldIndent;
			EditorGUI.EndProperty();
			if (itemsToCleanup.Count != 0) { RequestCleanup(self); }
		}

		// TODO rename this to DoGUI
		public virtual Object EditorGUIObjectReference(Rect _position, Object obj, Component self) {
			int oldIndent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			obj = StandardEditorGUIObjectReference(_position, obj, self);
			EditorGUI.indentLevel = oldIndent;
			return obj;
		}

		public Object ShowObjectPtrChoicesPopup(Rect _position, Object obj, Component self, bool recalculateChoices) {
			// if the object needs to have it's alternate forms calculated
			if (recalculateChoices || choicesAreFor != obj || choices_name.Length == 0) {
				choicesAreFor = obj;
				// if these choices are for an actual object
				if (choicesAreFor != null) {
					GenerateChoicesForSelectedObject(self, out choices_name, out choices_selectFunc);
					choice = 0;
				} else {
					if (cached_typeCreationList_names == null) {
						GenerateTypeCreationList(self,
							out cached_typeCreationList_names, out cached_TypeCreationList_function);
					}
					choices_name = cached_typeCreationList_names;
					choices_selectFunc = cached_TypeCreationList_function;
				}
			}
			// give the alternate options for the object
			int lastChoice = choice;
			_position.x += _position.width;
			_position.width = defaultOptionWidth;
			choice = EditorGUI.Popup(_position, choice, choices_name);
			if (lastChoice != choice) {
				if (choices_selectFunc[choice] != null) {
					obj = choices_selectFunc[choice]();
				}
			}
			return obj;
		}

		public Object StandardEditorGUIObjectReference(Rect _position, Object obj, Component self) {
			float originalWidth = _position.width;
			_position.width = originalWidth - defaultOptionWidth;
			Object prevSelection = obj;
			obj = EditorGUI.ObjectField(_position, obj, typeof(Object), true);
			obj = FilterImmidiate(obj, self);
			obj = ShowObjectPtrChoicesPopup(_position, obj, self, obj != prevSelection);
			return obj;
		}

		public Object DoGUIEnumLabeledString<T>(Rect _position, Object obj, Component self,
			ref T enumValue, ref string textValue) {
			int oldindent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			Rect r = _position;
			float w = defaultOptionWidth, wl = defaultLabelWidth;
			r.width = wl;
			enumValue = EditorGUI_EnumPopup<T>(r, enumValue);
			r.x += r.width;
			r.width = _position.width - w - wl;
			textValue = EditorGUI.TextField(r, textValue);
			obj = ShowObjectPtrChoicesPopup(r, obj, self, true);
			r.x += r.width;
			r.width = w;
			EditorGUI.indentLevel = oldindent;
			return obj;
		}
		public Object DoGUIEnumLabeledObject<T>(Rect _position, Object obj, Component self,
			ref T enumValue, ref Object objectValue) {
			int oldindent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			Rect r = _position;
			float w = defaultOptionWidth, wl = defaultLabelWidth;
			r.width = wl;
			enumValue = EditorGUI_EnumPopup<T>(r, enumValue);
			r.x += r.width;
			r.width = _position.width - w - wl;
			objectValue = EditorGUI.ObjectField(r, objectValue, typeof(Object), true);
			obj = FilterImmidiate(obj, self);
			obj = ShowObjectPtrChoicesPopup(r, obj, self, true);
			r.x += r.width;
			r.width = w;
			EditorGUI.indentLevel = oldindent;
			return obj;
		}
	}
#endif
}
