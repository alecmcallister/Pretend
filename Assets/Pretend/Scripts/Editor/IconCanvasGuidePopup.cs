using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class IconCanvasGuidePopup : PopupWindowContent
{
	public event Action<float, IconCanvasGuideType> Callback = new Action<float, IconCanvasGuideType>((f, t) => { });

	float val = 50f;
	IconCanvasGuideType type;

	public override void OnGUI(Rect rect)
	{
		ProcessEvents(Event.current);

		EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
		GUI.SetNextControlName("ValueField");
		val = Mathf.Clamp(EditorGUILayout.FloatField(val, EditorStyles.toolbarTextField, GUILayout.Width(30f)), 0f, 100f);
		GUILayout.Label("%", EditorStyles.toolbarButton, GUILayout.Width(28f));
		GUILayout.FlexibleSpace();

		GUI.SetNextControlName("TypeField");
		type = (IconCanvasGuideType)EditorGUILayout.EnumPopup(type, EditorStyles.toolbarDropDown);

		EditorGUILayout.EndHorizontal();

		FocusValueField();

		if (GUI.changed)
			editorWindow.Repaint();
	}

	bool rotateType = true;

	void ProcessEvents(Event e)
	{
		if (e.type == EventType.KeyDown)
		{
			if (e.keyCode == KeyCode.KeypadEnter || e.keyCode == KeyCode.Return)
			{
				CreateGuide();
				editorWindow.Close();
			}
			if (e.keyCode == KeyCode.H)
			{
				e.Use();
				type = IconCanvasGuideType.Horizontal;
			}
			if (e.keyCode == KeyCode.V)
			{
				e.Use();
				type = IconCanvasGuideType.Vertical;
			}
			if (e.keyCode == KeyCode.Tab && !e.shift)
			{
				if (GUI.GetNameOfFocusedControl() == "ValueField")
				{
					e.Use();
					if (rotateType)
						type = type == IconCanvasGuideType.Horizontal ? IconCanvasGuideType.Vertical : IconCanvasGuideType.Horizontal;
					focused = false;
				}
			}
			if (e.keyCode == KeyCode.Escape)
			{
				e.Use();
				editorWindow.Close();
			}
		}
	}

	bool focused = false;

	void FocusValueField()
	{
		if (focused)
			return;

		EditorGUI.FocusTextInControl("ValueField");
		focused = true;
	}

	void CreateGuide()
	{
		if (val >= 0f && val <= 100f)
			Callback(val / 100f, type);
	}

	public override Vector2 GetWindowSize()
	{
		return new Vector2(150f, 18f);
	}
}
