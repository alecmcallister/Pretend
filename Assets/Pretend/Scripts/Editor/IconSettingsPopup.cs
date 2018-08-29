using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

/// <summary>
/// Using styles from the Unity Editor source code:
/// https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/GUI/EditorStyles.cs
/// https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Annotation/AnnotationWindow.cs
/// </summary>
public class IconSettingsPopup : PopupWindowContent
{
	static bool init;

	#region OnGUI

	public override void OnGUI(Rect rect)
	{
		ProcessEvents(Event.current);

		DrawHeader();

		#region Scroll stuff

		// Scroll
		bool even = true;
		pos = EditorGUILayout.BeginScrollView(pos, false, true, GUIStyle.none, GUI.skin.verticalScrollbar, GUI.skin.scrollView);

		for (int i = 0; i < boolList.Count; i++)
		{
			boolList[i] = DrawListToggleItem(boolList[i], "Toggle " + i, even ? item1 : item2);
			even = !even;
		}

		EditorGUILayout.EndScrollView();

		#endregion

		if (GUI.changed)
		{
			editorWindow.Repaint();
		}
	}

	#endregion

	#region Events

	void ProcessEvents(Event e)
	{
		switch (e.type)
		{
			case EventType.MouseDown:
			case EventType.MouseUp:
			case EventType.MouseMove:
			case EventType.MouseDrag:
			case EventType.KeyDown:
			case EventType.KeyUp:
			case EventType.ScrollWheel:
			case EventType.DragUpdated:
			case EventType.DragPerform:
			case EventType.DragExited:
			case EventType.MouseEnterWindow:
			case EventType.MouseLeaveWindow:
				GUI.changed = true;
				break;
		}
	}

	#endregion

	#region Draw

	float windowHeight = 350f;

	[SerializeField]
	static AnimBool _showExtraFields;
	public static AnimBool ShowExtraFields
	{
		get
		{
			if (_showExtraFields == null)
				_showExtraFields = new AnimBool(false);
			if (_showExtraFields.valueChanged == null)
				_showExtraFields.valueChanged = new UnityEngine.Events.UnityEvent();

			return _showExtraFields;
		}
	}

	void DrawHeader()
	{
		EditorGUILayout.BeginVertical(bg);

		IconSetWindow.Instance.Prefs.DrawRulers.value = DrawToggle("Draw guide rulers", IconSetWindow.Instance.Prefs.DrawRulers.value);

		ShowExtraFields.target = EditorGUILayout.Foldout(ShowExtraFields.target, new GUIContent("Colors"), true);
		if (EditorGUILayout.BeginFadeGroup(ShowExtraFields.faded))
		{

			using (EditorGUI.IndentLevelScope m = new EditorGUI.IndentLevelScope())
			{
				IconSetWindow.Instance.Prefs.GridBGTint = DrawColorField("BG gradient top", (Color)IconSetWindow.Instance.Prefs.GridBGTint,
					() => { IconSetWindow.Instance.Prefs.GridBGTint = null; return (Color)IconSetWindow.Instance.Prefs.GridBGTint; });

				IconSetWindow.Instance.Prefs.GridGradientTint = DrawColorField("BG gradient bottom", (Color)IconSetWindow.Instance.Prefs.GridGradientTint,
					() => { IconSetWindow.Instance.Prefs.GridGradientTint = null; return (Color)IconSetWindow.Instance.Prefs.GridGradientTint; });

				IconSetWindow.Instance.Prefs.GridForeground = DrawColorField("Grid color 1", (Color)IconSetWindow.Instance.Prefs.GridForeground,
					() => { IconSetWindow.Instance.Prefs.GridForeground = null; return (Color)IconSetWindow.Instance.Prefs.GridForeground; });

				IconSetWindow.Instance.Prefs.GridBackground = DrawColorField("Grid color 2", (Color)IconSetWindow.Instance.Prefs.GridBackground,
					() => { IconSetWindow.Instance.Prefs.GridBackground = null; return (Color)IconSetWindow.Instance.Prefs.GridBackground; });

				IconSetWindow.Instance.Prefs.GridMajorLineColor = DrawColorField("Major line color", (Color)IconSetWindow.Instance.Prefs.GridMajorLineColor,
					() => { IconSetWindow.Instance.Prefs.GridMajorLineColor = null; return (Color)IconSetWindow.Instance.Prefs.GridMajorLineColor; });

				IconSetWindow.Instance.Prefs.GridMinorLineColor = DrawColorField("Minor line color", (Color)IconSetWindow.Instance.Prefs.GridMinorLineColor,
					() => { IconSetWindow.Instance.Prefs.GridMinorLineColor = null; return (Color)IconSetWindow.Instance.Prefs.GridMinorLineColor; });
			}
		}

		EditorGUILayout.EndFadeGroup();
		EditorGUILayout.EndVertical();
	}

	public void ResetGridForeground()
	{
		IconSetWindow.Instance.Prefs.GridForeground = null;
		IconSetWindow.Instance.Repaint();
		editorWindow.Repaint();
	}

	public void ResetGridBackground()
	{
		IconSetWindow.Instance.Prefs.GridBackground = null;
		IconSetWindow.Instance.Repaint();
		editorWindow.Repaint();
	}

	bool DrawToggle(string name, bool val)
	{
		EditorGUILayout.BeginHorizontal(header);
		val = EditorGUILayout.ToggleLeft(name, val);
		EditorGUILayout.EndHorizontal();
		return val;
	}

	float DrawSlider(string name, float val, float min, float max)
	{
		EditorGUILayout.BeginHorizontal(header);
		GUILayout.Label(name, GUILayout.Width(100f));
		GUILayout.Label(val.ToString("0.0"), EditorStyles.miniLabel, GUILayout.Width(50f));
		val = GUILayout.HorizontalSlider(val, min, max);
		EditorGUILayout.EndHorizontal();
		return val;
	}

	Color DrawColorField(string name, Color val, Func<Color> onDefault = null)
	{
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.BeginHorizontal(header);

		val = EditorGUILayout.ColorField(new GUIContent(name), val, false, true, false, GUILayout.Height(15f));

		if (GUILayout.Button(resetContent, resetStyle))
		{
			if (onDefault != null)
			{
				val = onDefault.Invoke();
				GUI.changed = true;
			}
		}

		EditorGUILayout.EndHorizontal();
		if (EditorGUI.EndChangeCheck())
			IconSetWindow.Instance.Repaint();

		return val;
	}

	#endregion

	public override Vector2 GetWindowSize()
	{
		Vector2 size = new Vector2(300f, windowHeight);
		return size;
	}

	GUIContent resetContent;
	GUIStyle resetStyle;

	public override void OnOpen()
	{
		if (!init)
			Init();

		ShowExtraFields.valueChanged.AddListener(editorWindow.Repaint);

		resetContent = EditorGUIUtility.IconContent("d_RotateTool");

		GUIStyle temp = EditorStyles.miniButtonRight;
		resetStyle = new GUIStyle().ActuallyCopyFrom(temp);

		resetStyle.fixedWidth = resetStyle.fixedHeight = 16f;
		resetStyle.imagePosition = ImagePosition.ImageOnly;
		resetStyle.margin = new RectOffset().UniformOffset(0);
		resetStyle.overflow = new RectOffset().UniformOffset(0);
		resetStyle.padding = new RectOffset().UniformOffset(3);
		resetStyle.stretchHeight = false;
		resetStyle.stretchWidth = false;
	}

	public override void OnClose()
	{
		ShowExtraFields.valueChanged.RemoveAllListeners();
		//IconSetWindow.Instance.Prefs.Save();
	}

	#region List of toggle buttons

	[SerializeField]
	static Dictionary<int, bool> boolList;

	Vector2 pos = Vector2.zero;

	static GUIStyle bg;
	static GUIStyle header;
	static GUIStyle item1;
	static GUIStyle item2;

	bool DrawListToggleItem(bool value, string label, GUIStyle style)
	{
		EditorGUILayout.BeginHorizontal(style);
		value = GUILayout.Toggle(value, new GUIContent(label));
		EditorGUILayout.EndHorizontal();
		return value;
	}

	#endregion

	void Init()
	{
		init = true;

		bg = new GUIStyle().ActuallyCopyFrom(GUI.skin.FindStyle("In BigTitle"));
		bg.padding = new RectOffset().UniformOffset(5);
		bg.margin = new RectOffset();

		#region List stuff 

		header = new GUIStyle();
		header.padding = new RectOffset(5, 5, 2, 2);
		header.margin = new RectOffset();

		if (boolList == null)
		{
			boolList = new Dictionary<int, bool>();
			for (int i = 0; i < 20; i++)
				boolList.Add(i, false);
		}

		Texture2D tex1 = new Texture2D(1, 1);
		Texture2D tex2 = new Texture2D(1, 1);
		Texture2D tex3 = new Texture2D(1, 1);

		Color color1 = Color.white.WithAlpha(0.02f);
		Color color2 = Color.white.WithAlpha(0f);
		Color color3 = Color.white.WithAlpha(0.1f);

		tex1.SetPixel(0, 0, color1);
		tex2.SetPixel(0, 0, color2);
		tex3.SetPixel(0, 0, color3);
		tex1.Apply();
		tex2.Apply();
		tex3.Apply();

		item1 = new GUIStyle();
		item2 = new GUIStyle();

		item1.normal.background = tex1;
		item2.normal.background = tex2;
		item1.hover.background = item2.hover.background = tex3;

		item1.padding = item2.padding = new RectOffset(10, 0, 2, 2);
		item1.margin = item2.margin = new RectOffset().UniformOffset(0);

		#endregion
	}
}


