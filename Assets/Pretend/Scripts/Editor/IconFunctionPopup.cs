using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 
/// TODO: 
///		- Draw the search bar at the cursor/ above the cursor when close to the bottom of the window
///			- Use Unity's default searchbar layout (Project window search bar, or maybe Hierarchy search (with the "All" dropdown on the magnifying glass))
///				- Magnifying glass icon (potentially housing a dropdown for further options (Hierarchy search)
///				- Text field
///				- "X" button to clear text when text field isn't empty
///				- All within a rounded (circular) rectangle
///				
///		- Design the layout for the search items
///			- Appear in a scroll view below the search bar
///			- Gives brief summary of the action ("Add guide...", "Clear points", etc.)
///			- Are shown based on what is currently typed, i.e. will change as more letters are added
///			- Can be invoked by clicking, or by arrow keys + enter
///			- Closes the popup on invoke
///				- Depending on the action selected, might open a secondary popup to gather values ("Add guide..." -> popup that lets you specify position and direction)
///	
///		- Add the proper interaction functionality 
///			- Search gets keyboard focus on popup
///			- The first search result is automatically selected
///			- Pressing up/ down arrow keys will move the selection up/ down the result list
///			- Pressing enter will invoke the currently selected result
///			
/// </summary>
public class IconFunctionPopup : PopupWindowContent
{
	public event Action<string> FunctionCallback = new Action<string>(s => { });

	string searchText = "";
	bool focused = false;
	Vector2 scrollPos = Vector2.zero;
	//float scrollItemHeight = 25f;
	float scrollItemHeight = 20f;
	float scrollHeight = 140f;
	float searchHeight = 18f;

	static Styles styles;
	class Styles
	{
		public GUIContent m_search = EditorGUIUtility.TrTextContent("Search:");
		public GUIStyle m_ToolbarSearchField = new GUIStyle().ActuallyCopyFrom(GUI.skin.FindStyle("ToolbarSeachTextField"));
		public GUIStyle m_ToolbarSearchFieldCancelButton = new GUIStyle().ActuallyCopyFrom(GUI.skin.FindStyle("ToolbarSeachCancelButton"));
		public GUIStyle m_ToolbarSearchFieldCancelButtonEmpty = new GUIStyle().ActuallyCopyFrom(GUI.skin.FindStyle("ToolbarSeachCancelButtonEmpty"));
		public GUIStyle functionListItem1 = new GUIStyle() { normal = new GUIStyleState() { background = SmolTexture.ColorTex(Color.black.WithAlpha(0.1f)) } };
		public GUIStyle functionListItem2 = new GUIStyle() { normal = new GUIStyleState() { background = SmolTexture.ColorTex(Color.black.WithAlpha(0.15f)) } };
		public GUIStyle functionListItem3 = new GUIStyle() { normal = new GUIStyleState() { background = SmolTexture.ColorTex(Color.white.WithAlpha(0.25f)) } };
	}

	int selectedIndex = 0;

	string selectedFunc { get { return filteredFunctions.Count > 0 ? filteredFunctions[selectedIndex] : null; } }

	List<string> functions;
	List<string> filteredFunctions
	{
		get
		{
			return functions.Where(s => s.ToLower().Replace(" ", "").Contains(searchText.ToLower().Replace(" ", ""))).ToList();
		}
	}

	public override void OnOpen()
	{
		if (styles == null)
			styles = new Styles();

		functions = new List<string>() { "Add guide...", "Clear points", "Reset grid", "Toggle guide rulers", "Clear guides", "Box select", "Ellipse select", "Copy", "Paste", "Invert Selection", "Invert Selection", "Invert Selection", "Invert Selection", "Invert Selection" };
	}

	public override void OnGUI(Rect rect)
	{
		ProcessEvents(Event.current);

		DrawSearchField();
		DrawSearchResults(filteredFunctions);

		if (GUI.changed)
			editorWindow.Repaint();
	}

	public void ProcessEvents(Event e)
	{
		if (e.type == EventType.KeyDown)
		{
			if (e.keyCode == KeyCode.KeypadEnter || e.keyCode == KeyCode.Return)
			{
				e.Use();
				if (selectedFunc != null)
				{
					FunctionCallback(selectedFunc);
				}
				editorWindow.Close();
			}
			if (e.keyCode == KeyCode.UpArrow)
			{
				e.Use();
				OnUpArrow();
			}
			if (e.keyCode == KeyCode.DownArrow)
			{
				e.Use();
				OnDownArrow();
			}
			if (e.keyCode == KeyCode.Escape)
			{
				if (searchText != "")
					ClearSearchText();

				else
					editorWindow.Close();
			}
		}
	}


	void OnUpArrow()
	{
		PrevFunction();
		CheckScroll();
		GUI.changed = true;
	}

	void OnDownArrow()
	{
		NextFunction();
		CheckScroll();
		GUI.changed = true;
	}

	void CheckScroll()
	{
		float val = selectedIndex * scrollItemHeight;
		if (val >= scrollPos.y + scrollHeight)
		{
			scrollPos.y += scrollItemHeight;
		}
		else if (val < scrollPos.y)
		{
			scrollPos.y -= scrollItemHeight;
		}
	}

	string NextFunction()
	{
		selectedIndex++;
		if (selectedIndex == filteredFunctions.Count)
			selectedIndex--;

		return filteredFunctions[selectedIndex];
	}

	string PrevFunction()
	{
		selectedIndex--;
		if (selectedIndex < 0)
			selectedIndex++;

		return filteredFunctions[selectedIndex];
	}

	void DrawSearchField()
	{
		EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

		EditorGUI.BeginChangeCheck();
		GUI.SetNextControlName("SearchFilter");
		searchText = EditorGUILayout.TextField(searchText, styles.m_ToolbarSearchField);
		if (EditorGUI.EndChangeCheck())
			SearchTextChanged();

		if (GUILayout.Button(GUIContent.none, searchText != "" ? styles.m_ToolbarSearchFieldCancelButton : styles.m_ToolbarSearchFieldCancelButtonEmpty))
		{
			ClearSearchText();
			GUIUtility.keyboardControl = 0;
		}
		else
		{
			SetFocusOnSearch();
		}

		EditorGUILayout.EndHorizontal();
	}

	void SetFocusOnSearch()
	{
		if (focused)
			return;

		EditorGUI.FocusTextInControl("SearchFilter");
		focused = true;
	}

	void SearchTextChanged()
	{
		selectedIndex = 0;
		scrollPos = Vector2.zero;
	}

	void ClearSearchText()
	{
		searchText = "";
		focused = false;
	}

	void DrawSearchResults(List<string> results)
	{
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUIStyle.none, GUIStyle.none, GUILayout.Height(scrollHeight));
		bool even = true;

		for (int i = 0; i < results.Count; i++)
		{
			GUIStyle style = (i == selectedIndex) ? styles.functionListItem3 : (even ? styles.functionListItem1 : styles.functionListItem2);
			EditorGUILayout.BeginHorizontal(style, GUILayout.Height(scrollItemHeight));

			EditorGUILayout.LabelField(results[i], EditorStyles.whiteMiniLabel);

			EditorGUILayout.EndHorizontal();

			even = !even;
		}

		EditorGUILayout.EndScrollView();


	}

	public override Vector2 GetWindowSize()
	{
		return new Vector2(250f, searchHeight + scrollHeight);
	}
}

public class SmolTexture
{
	public Texture2D tex { get; private set; }

	public SmolTexture(Color color)
	{
		tex = new Texture2D(1, 1);
		tex.SetPixel(0, 0, color);
		tex.Apply();
	}

	public SmolTexture SetColor(Color color)
	{
		tex.SetPixel(0, 0, color);
		tex.Apply();
		return this;
	}

	public static Texture2D ColorTex(Color color)
	{
		return new SmolTexture(color);
	}

	public static implicit operator Texture2D(SmolTexture t)
	{
		return t.tex;
	}
}
