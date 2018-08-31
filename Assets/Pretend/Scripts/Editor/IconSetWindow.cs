using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

/// <summary>
/// TODO:
/// - Side panel (resizeable) for holding things
/// - Search function on spacebar
/// - Grid that functions like the 2D scene view
/// - Open file browser to open/ create files (.ttf)
/// - 
/// 
/// http://gram.gs/gramlog/creating-node-based-editor-unity/
/// </summary>
public class IconSetWindow : EditorWindow
{
	#region Variables & properties

	Rect settingsDropdownRect;
	Rect toolbarRect;

	Rect canvasRect;
	public IconCanvas canvas;

	public List<IconCanvasGuide> guides = new List<IconCanvasGuide>();
	const float toolbarHeight = 17f; // actually 18, but the toolbar's y position is -1

	#endregion

	#region Fields

	#region Instance

	[SerializeField]
	static IconSetWindow _instance;
	public static IconSetWindow Instance
	{
		get
		{
			if (_instance == null)
				_instance = GetWindow<IconSetWindow>(typeof(SceneView));

			return _instance;
		}
		set
		{
			_instance = value;
		}
	}

	#endregion

	#region Prefs

	IconSetPrefs _prefs;
	public IconSetPrefs Prefs
	{
		get
		{
			if (_prefs == null)
			{
				_prefs = IconSetPrefs.Load();
			}

			return _prefs;
		}
		set
		{
			_prefs = value;
		}
	}

	#endregion

	#region Styles

	GUIStyle _rulerStyle;
	public GUIStyle RulerStyle
	{
		get
		{
			if (_rulerStyle == null)
			{
				_rulerStyle = new GUIStyle();
				_rulerStyle.normal.background = Resources.Load<Texture2D>("Editor/Textures/guide_border");
				_rulerStyle.border = new RectOffset().UniformOffset(_rulerStyle.normal.background.width / 2);
			}

			return _rulerStyle;
		}
	}

	GUIStyle _rulerTextStyleL;
	public GUIStyle RulerTextStyleL
	{
		get
		{
			if (_rulerTextStyleL == null)
			{
				_rulerTextStyleL = new GUIStyle();
				_rulerTextStyleL.fontSize = 5;
				_rulerTextStyleL.alignment = TextAnchor.UpperLeft;
				_rulerTextStyleL.wordWrap = false;
				_rulerTextStyleL.clipping = TextClipping.Clip;
			}
			return _rulerTextStyleL;
		}
	}

	GUIStyle _rulerTextStyleT;
	public GUIStyle RulerTextStyleT
	{
		get
		{
			if (_rulerTextStyleT == null)
			{
				_rulerTextStyleT = new GUIStyle();
				_rulerTextStyleT.fontSize = 5;
				_rulerTextStyleT.alignment = TextAnchor.UpperLeft;
				_rulerTextStyleT.wordWrap = false;
				_rulerTextStyleT.clipping = TextClipping.Clip;
			}
			return _rulerTextStyleT;
		}
	}

	Texture _guideCursorL;
	public Texture GuideCursorL
	{
		get
		{
			if (_guideCursorL == null)
				_guideCursorL = Resources.Load<Texture2D>("Editor/Textures/guide_cursor_left");

			return _guideCursorL;
		}
	}

	Texture _guideCursorT;
	public Texture GuideCursorT
	{
		get
		{
			if (_guideCursorT == null)
				_guideCursorT = Resources.Load<Texture2D>("Editor/Textures/guide_cursor_top");

			return _guideCursorT;
		}
	}

	#endregion

	#endregion

	#region Show

	[MenuItem("Window/Pretend")]
	public static void ShowWindow()
	{
		Instance.titleContent = new GUIContent("Pretend", Resources.Load<Texture>("Textures/TitleIcon"), "Make the thing!");
	}

	#endregion

	#region Enable/ Disable

	void OnEnable()
	{
		wantsMouseMove = true;
		wantsMouseEnterLeaveWindow = true;

		Prefs.DrawRulers.valueChanged.AddListener(Repaint);

		if (!canvas)
		{
			canvas = CreateInstance(typeof(IconCanvas)) as IconCanvas;
			canvas.Size.valueChanged.AddListener(Repaint);
		}
		canvas.Reset();
		IconCanvasGuide.ClearAllGuides();
	}

	void OnDisable()
	{
		DestroyImmediate(canvas);
		canvas = null;
	}

	void OnDestroy()
	{
		//Prefs.Save();
	}

	#endregion

	#region OnGUI

	void OnGUI()
	{
		ProcessEvents(Event.current);

		DrawGrid();
		DrawGuides();
		DrawPoints();

		if (Prefs.DrawRulers.value)
			DrawGuideRulers();

		DrawToolbar();

		HandlePopupDelegate();

		if (GUI.changed)
			Repaint();
	}

	#endregion

	void ProcessEvents(Event e)
	{
		for (int i = canvas.points.Count - 1; i >= 0; i--)
			GUI.changed |= canvas.points[i].ProcessEvents(Event.current);

		ProcessGuideEvents(e);

		canvas.ProcessEvents(Event.current);

		if (e.type == EventType.MouseMove || e.type == EventType.MouseEnterWindow || e.type == EventType.MouseLeaveWindow)
			GUI.changed = true;

		if (e.type == EventType.KeyDown)
		{
			if (e.keyCode == KeyCode.Space && canvas.container.Contains(e.mousePosition))
			{
				IconFunctionPopup functionPopup = new IconFunctionPopup();
				functionPopup.FunctionCallback += ReceiveFunctionCallback;
				PopupWindow.Show(new Rect(e.mousePosition, Vector2.zero), functionPopup);
			}
			e.Use();
		}
	}


	public delegate void PopupDelegate();
	public PopupDelegate popupDelegate;

	void HandlePopupDelegate()
	{
		if (popupDelegate != null && Event.current.type == EventType.Repaint)
		{
			popupDelegate.Invoke();
			popupDelegate = null;
		}
	}

	void ReceiveFunctionCallback(string function)
	{
		if (function == "Add guide...")
		{
			popupDelegate = OpenGuidePopup;
		}
		if (function == "Clear points")
		{
			canvas.ClearPoints();
		}
		if (function == "Reset grid")
		{
			canvas.Reset();
		}
		if (function == "Toggle guide rulers")
		{
			Prefs.DrawRulers.value = !Prefs.DrawRulers.value;
		}
		if (function == "Clear guides")
		{
			IconCanvasGuide.ClearAllGuides();
		}
	}

	public void OpenGuidePopup()
	{
		IconCanvasGuidePopup guidePopup = new IconCanvasGuidePopup();
		guidePopup.Callback += (float val, IconCanvasGuideType type) => { IconCanvasGuide.AddGuide(val, type); };
		PopupWindow.Show(new Rect(rulerSize + 5f, toolbarHeight + rulerSize + 5f, 0, 0), guidePopup);
	}

	#region Toolbar

	void DrawToolbar()
	{
		toolbarRect = EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

		if (GUILayout.Button("Clear Points", EditorStyles.toolbarButton))
			canvas.ClearPoints();

		if (GUILayout.Button("Reset Grid", EditorStyles.toolbarButton))
			canvas.Reset();

		GUILayout.FlexibleSpace();

		if (EditorGUILayout.DropdownButton(new GUIContent("Settings"), FocusType.Passive, EditorStyles.toolbarDropDown))
			PopupWindow.Show(settingsDropdownRect, new IconSettingsPopup());

		if (Event.current.type == EventType.Repaint)
		{
			Rect last = GUILayoutUtility.GetLastRect();
			settingsDropdownRect = new Rect(last.position.x - 300f + last.width, last.position.y, last.width, last.height);
		}

		EditorGUILayout.EndHorizontal();
	}

	#endregion

	#region Guide rulers

	IconCanvasGuide activeGuide = null;

	public void ProcessGuideEvents(Event e)
	{
		Vector2 localMouse = canvas.GetLocalMousePositionOnGrid();

		if (e.type == EventType.MouseDown && e.button == 0)
		{
			if (topRuler.Contains(e.mousePosition))
			{
				activeGuide = new IconCanvasGuide(canvas, localMouse, IconCanvasGuideType.Horizontal);
			}
			else if (leftRuler.Contains(e.mousePosition))
			{
				activeGuide = new IconCanvasGuide(canvas, localMouse, IconCanvasGuideType.Vertical);
			}
			else
			{
				activeGuide = IconCanvasGuide.GetGuideAtPos(e.mousePosition);

				// May not need this
				if (activeGuide != null)
				{
					activeGuide.snap = e.control;
					activeGuide.SetLocalPosition(localMouse);
				}
			}

			if (activeGuide != null)
			{
				e.Use();
				GUI.changed = true;
			}
		}

		if ((e.type == EventType.MouseMove || e.type == EventType.MouseDrag) && activeGuide != null)
		{
			activeGuide.snap = e.control;
			activeGuide.SetLocalPosition(localMouse);
			e.Use();
			GUI.changed = true;
		}

		if (e.type == EventType.MouseUp && e.button == 0 && activeGuide != null)
		{
			if (activeGuide.Value < 0f || activeGuide.Value > 1f)
				activeGuide.DeleteGuide();

			activeGuide = null;

			e.Use();
			GUI.changed = true;
		}

		if (activeGuide != null && e.isKey)
		{
			activeGuide.snap = e.control;
			activeGuide.SetLocalPosition(localMouse);
			GUI.changed = true;
		}
	}

	public void DrawGuides()
	{
		foreach (IconCanvasGuide guide in IconCanvasGuide.Guides)
		{
			DrawDottedLine(guide, canvas.gridRect.position - new Vector2(rulerSize, toolbarHeight + rulerSize));
		}
	}

	float rulerSize
	{
		get
		{
			return Prefs.DrawRulers.value ? 16f : 0f;
		}
	}

	Rect leftRuler;
	Rect topRuler;
	Rect cornerPiece;

	/// <summary>
	/// Move to separate class
	/// </summary>
	void DrawGuideRulers()
	{
		bool sideways = false;

		Rect textRect = new Rect();

		leftRuler = new Rect(0f, toolbarHeight + rulerSize, rulerSize, position.height - toolbarHeight - rulerSize);
		topRuler = new Rect(rulerSize, toolbarHeight, position.width - rulerSize, rulerSize);
		cornerPiece = new Rect(0f, toolbarHeight, rulerSize, rulerSize);
		GUI.Box(leftRuler, "", RulerStyle);
		GUI.Box(topRuler, "", RulerStyle);

		Color majorLineColor = Color.white;
		Color minorLineColor = Color.white.WithAlpha(0.5f);

		Handles.BeginGUI();
		float borderL = 6f;
		float borderS = 4f;

		bool major;
		int m = 0;
		int majorCells = canvas.cells / canvas.outerCells;

		float interval = canvas.gridRect.height / canvas.cells;

		float yMin = toolbarHeight + rulerSize + borderS;
		float yMax = position.height - borderS;

		float xMin = rulerSize + borderS;
		float xMax = position.width - borderS;

		Vector2 p1 = new Vector2(borderS, 0f);
		Vector2 p2 = new Vector2(rulerSize - borderS, 0f);

		float y = canvas.gridRect.position.y;
		float x = canvas.gridRect.position.x;

		if (y > yMin)
		{
			while (y > yMin + interval)
			{
				y -= interval;
				m--;
			}
		}
		else
		{
			while (y < yMin)
			{
				y += interval;
				m++;
			}
		}

		while (y < yMax)
		{
			major = m++ % majorCells == 0;
			Handles.color = RulerTextStyleL.normal.textColor = major ? majorLineColor : minorLineColor;

			p2.x = rulerSize - (major ? borderS : borderL);

			p1.y = p2.y = y;
			Handles.DrawLine(p1, p2);

			if (!sideways)
			{
				textRect.Set(borderS, y + 1f, rulerSize - borderS - borderS, interval - 1f);
				GUI.Label(textRect, (m - 1).ToString(), RulerTextStyleL);
			}
			else
			{
				textRect.Set(2f, y + borderS, interval, rulerSize);
				GUIUtility.RotateAroundPivot(-90f, new Vector2(0f, y));

				GUI.Label(textRect, (m - 1).ToString(), RulerTextStyleL);

				GUIUtility.RotateAroundPivot(90f, new Vector2(0f, y));
			}

			y += interval;
		}

		m = 0;
		p1.y = toolbarHeight + borderS;
		p2.y = gridTrimTop - borderS;

		if (x > xMin)
		{
			while (x > xMin + interval)
			{
				x -= interval;
				m--;
			}
		}
		else
		{
			while (x < xMin)
			{
				x += interval;
				m++;
			}
		}

		while (x < xMax)
		{
			major = m++ % majorCells == 0;
			Handles.color = RulerTextStyleT.normal.textColor = major ? majorLineColor : minorLineColor;

			p2.y = gridTrimTop - (major ? borderS : borderL);

			p1.x = p2.x = x;
			Handles.DrawLine(p1, p2);

			textRect.Set(x + 2f, toolbarHeight + borderS, interval - 2f, rulerSize - borderS - borderS);
			GUI.Label(textRect, (m - 1).ToString(), RulerTextStyleT);

			x += interval;
		}

		Handles.color = Color.white;
		Vector2 mPos = Event.current.mousePosition;

		Vector3 startL = new Vector3(rulerSize - borderS, mPos.y);
		Vector3 startT = new Vector3(mPos.x, gridTrimTop - borderS);

		Vector3[] pointsL = new Vector3[] { startL, new Vector3(startL.x - 5f, startL.y + 5f), new Vector3(startL.x - 5f, startL.y - 5f), startL };
		Vector3[] pointsT = new Vector3[] { startT, new Vector3(startT.x + 5f, startT.y - 5f), new Vector3(startT.x - 5f, startT.y - 5f), startT };

		Handles.DrawAAPolyLine(pointsL);
		Handles.DrawAAPolyLine(pointsT);

		GUI.Box(cornerPiece, "", RulerStyle);

		Handles.EndGUI();
	}

	public void DrawDottedLine(IconCanvasGuide guide)
	{
		DrawDottedLine(guide[0], guide[1], guide.width, guide.dash, guide.space, (guide.hovered || guide == activeGuide) ? guide.hover : guide.color);
	}

	public void DrawDottedLine(IconCanvasGuide guide, Vector2 offset)
	{
		DrawDottedLine(guide[0], guide[1], guide.width, guide.dash, guide.space, (guide.hovered || guide == activeGuide) ? guide.hover : guide.color, guide.type, offset);
	}

	public static void DrawDottedLine(Vector2 from, Vector2 to, float width, float dash, float space, Color color)
	{
		using (new Handles.DrawingScope(color))
		{
			Vector3 delta = to - from;
			Vector3 direction = delta.normalized;
			float distance = delta.magnitude;
			float interval = dash + space;

			int count = (int)(distance / interval);

			Vector3 inc = direction * interval;
			Vector3 inc2 = direction * dash;

			Vector3[] p = new Vector3[] { from, (Vector3)from + inc2 };

			for (int i = 0; i < count; i++)
			{
				Handles.DrawAAPolyLine(width, p);

				p[0] += inc;
				p[1] = p[0] + inc2;
			}

			if (((Vector3)to - p[0]).magnitude < dash)
				p[1] = to;

			Handles.DrawAAPolyLine(width, p);
		}
	}

	public static void DrawDottedLine(Vector2 from, Vector2 to, float width, float dash, float space, Color color, IconCanvasGuideType type, Vector2 offset)
	{
		using (new Handles.DrawingScope(color))
		{
			Vector2 direction = (to - from).normalized;
			float interval = dash + space;

			Vector2 del;
			if (type == IconCanvasGuideType.Horizontal)
				del = direction * (offset.x % interval);
			else
				del = direction * (offset.y % interval);

			Vector2 inv = (direction * interval) + del;

			Vector2 st;
			if (type == IconCanvasGuideType.Horizontal)
				st = from + (del.x > 0f ? del : inv);
			else
				st = from + (del.y > 0f ? del : inv);

			float distance = (to - from).magnitude;

			int count = (int)(distance / interval);

			Vector3 inc = direction * interval;
			Vector3 inc2 = direction * dash;
			Vector2 ds = from - st;

			if (ds.magnitude > space)
				Handles.DrawAAPolyLine(width, new Vector3[] { from, st - (direction * space) });

			Vector3[] p = new Vector3[] { st, (Vector3)st + inc2 };

			for (int i = 0; i < count; i++)
			{
				Handles.DrawAAPolyLine(width, p);

				p[0] += inc;
				p[1] = p[0] + inc2;
			}

			if (((Vector3)to - p[0]).magnitude < dash)
				p[1] = to;

			Handles.DrawAAPolyLine(width, p);
		}
	}

	public static void DrawDottedBorder(Rect rect, float width, float dash, float space, Color color)
	{
		DrawDottedLine(new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMax, rect.yMin), width, dash, space, color);
		DrawDottedLine(new Vector2(rect.xMin, rect.yMax), new Vector2(rect.xMax, rect.yMax), width, dash, space, color);
		DrawDottedLine(new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMin, rect.yMax), width, dash, space, color);
		DrawDottedLine(new Vector2(rect.xMax, rect.yMin), new Vector2(rect.xMax, rect.yMax), width, dash, space, color);
	}

	#endregion

	#region Grid

	float gridTrimTop
	{
		get
		{
			return toolbarHeight + rulerSize;
		}
	}

	float gridTrimLeft
	{
		get
		{
			return rulerSize;
		}
	}

	void TrimGrid()
	{
		canvasRect.Set(gridTrimLeft, gridTrimTop, position.width - gridTrimLeft, position.height - gridTrimTop);
		canvas.SetContainerRect(canvasRect);
	}

	void DrawGrid()
	{
		TrimGrid();

		canvas.DrawGrid();
	}

	void DrawPoints()
	{
		canvas.DrawPoints();
	}

	#endregion

}
