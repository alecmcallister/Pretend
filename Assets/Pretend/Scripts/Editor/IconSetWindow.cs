﻿using System;
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
	Rect gridRect;
	public IconCanvas canvas;

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

	#region Toolbar height

	float _toolbarHeight = -1;
	public float ToolbarHeight
	{
		get
		{
			if (_toolbarHeight < 0)
				_toolbarHeight = EditorStyles.toolbarButton.CalcSize(new GUIContent("Settings")).y;

			return _toolbarHeight;
		}
		set
		{
			_toolbarHeight = value;
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

	#region Vertices

	List<List<IconVertex>> _shapes;
	public List<List<IconVertex>> Shapes
	{
		get
		{
			if (_shapes == null)
				_shapes = new List<List<IconVertex>>();

			return _shapes;
		}
	}

	List<IconVertex> _vertices;
	public List<IconVertex> Vertices
	{
		get
		{
			if (_vertices == null)
				_vertices = new List<IconVertex>();

			return _vertices;
		}
		set
		{
			_vertices = value;
		}
	}

	#endregion

	#region Content

	GUIContent _clearVerticesContent;
	public GUIContent ClearVerticesContent
	{
		get
		{
			if (_clearVerticesContent == null)
				_clearVerticesContent = new GUIContent("Clear Vertices");
			return _clearVerticesContent;
		}
	}

	GUIContent _resetGridContent;
	public GUIContent ResetGridContent
	{
		get
		{
			if (_resetGridContent == null)
				_resetGridContent = new GUIContent("Reset Grid");
			return _resetGridContent;
		}
	}

	GUIContent _settingsContent;
	public GUIContent SettingsContent
	{
		get
		{
			if (_settingsContent == null)
				_settingsContent = new GUIContent("Settings");

			return _settingsContent;
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

		Prefs.Size.valueChanged.AddListener(UpdateVertices);

		if (!canvas)
		{
			canvas = CreateInstance(typeof(IconCanvas)) as IconCanvas;
			canvas.Size.valueChanged.AddListener(Repaint);
		}
	}

	void OnDisable()
	{
		Prefs.Size.valueChanged.RemoveAllListeners();

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
		DrawGrid();
		DrawToolbar();

		for (int i = canvas.points.Count - 1; i >= 0; i--)
			GUI.changed |= canvas.points[i].ProcessEvents(Event.current);

		canvas.ProcessEvents(Event.current);

		ProcessEvents(Event.current);

		if (GUI.changed)
			Repaint();
	}

	#endregion

	void ProcessEvents(Event e)
	{
		if (e.commandName != "")
			Debug.Log("Command recognized: " + e.commandName);
	}

	#region Toolbar

	void DrawToolbar()
	{
		GUILayout.BeginHorizontal(EditorStyles.toolbar);

		if (GUILayout.Button(ClearVerticesContent, EditorStyles.toolbarButton))
		{
			DeleteAllVertices();
		}

		if (GUILayout.Button(ResetGridContent, EditorStyles.toolbarButton))
		{
			ResetGrid();
		}

		GUILayout.Space(5f);
		GUILayout.Label(" (" + canvas.offset.x.ToString("0.0") + ", " + canvas.offset.y.ToString("0.0") + ") ", EditorStyles.toolbarTextField);
		GUILayout.FlexibleSpace();

		// Dropdown
		if (EditorGUILayout.DropdownButton(SettingsContent, FocusType.Passive, EditorStyles.toolbarDropDown))
		{
			// Show custom dropdown window
			PopupWindow.Show(settingsDropdownRect, new SettingsPopup());
		}

		if (Event.current.type == EventType.Repaint)
		{
			Rect last = GUILayoutUtility.GetLastRect();
			settingsDropdownRect = new Rect(last.position.x - 300f + last.width, last.position.y, last.width, last.height);
		}

		GUILayout.EndHorizontal();
	}

	#endregion

	#region Grid

	void TrimGrid()
	{
		gridRect.Set(0f, ToolbarHeight - 1f, position.width, position.height - (ToolbarHeight - 1f));
		canvas.SetContainerRect(gridRect);
	}

	void DrawGrid()
	{
		TrimGrid();

		canvas.Draw();
	}

	void ResetGrid()
	{
		canvas.Reset();
	}

	#endregion

	#region Vertex Stuff

	void CreateVertex(Vector2 pos)
	{
		IconVertex vertex = CreateInstance<IconVertex>().Initialize(pos);
		Vertices.Add(vertex);
	}

	void DrawVertices()
	{
		if (Vertices.Count > 2)
			FuckItUp();

		Vertices.ForEach(v => { v.Draw(); });
	}

	void ProcessVertexEvents(Event e)
	{
		IconVertex.hitRect = false;

		IconVertex dragged = null;

		for (int i = Vertices.Count - 1; i >= 0; i--)
		{
			GUI.changed = GUI.changed || Vertices[i].ProcessEvents(e);
			if (Vertices[i].hovered)
				dragged = Vertices[i];
		}

		if (dragged != null)
		{
			Vertices.Remove(dragged);
			Vertices.Add(dragged);
		}
	}

	public void UpdateVertices()
	{
		Vertices.ForEach(x => x.UpdateRect());
		Repaint();
	}

	#region Delete

	List<IconVertex> toDestroy = new List<IconVertex>();

	public void DeleteVertex(IconVertex vertex)
	{
		toDestroy.Add(vertex);
	}

	public void DeleteAllVertices()
	{
		toDestroy.AddRange(Vertices);
	}

	/// <summary>
	/// Actually do the deleting.
	/// Called after events have been processed in OnGUI.
	/// </summary>
	void PurgeDestroyList()
	{
		if (toDestroy.Count == 0)
			return;

		for (int i = toDestroy.Count - 1; i >= 0; i--)
		{
			if (Vertices.Contains(toDestroy[i]))
				Vertices.Remove(toDestroy[i]);

			DestroyImmediate(toDestroy[i]);
		}

		toDestroy.Clear();
		GUI.changed = true;
	}

	#endregion

	#region Fuck it up

	void FuckItUp()
	{
		List<Vector2> r = new List<Vector2>();

		Vertices.ForEach(v => r.Add(v.OffsetPos));
		r.Sort(new ClockwiseComparer(center(r)));

		Handles.BeginGUI();
		Handles.color = new Color(1f, 1f, 1f, 0.5f);

		List<Vector3> r3 = new List<Vector3>();

		r.ForEach(v => r3.Add(v));

		Handles.DrawAAConvexPolygon(r3.ToArray());

		Handles.EndGUI();
	}

	Vector2 center(List<Vector2> r)
	{
		Vector2 c = Vector2.zero;
		r.ForEach(v => c += v);
		return c / r.Count;
	}

	#endregion

	#endregion

}

/// <summary>
/// Using styles from the Unity Editor source code:
/// https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/GUI/EditorStyles.cs
/// https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Annotation/AnnotationWindow.cs
/// </summary>
public class SettingsPopup : PopupWindowContent
{
	static bool init;

	#region OnGUI

	public override void OnGUI(Rect rect)
	{
		// Header
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

		switch (Event.current.type)
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
				editorWindow.Repaint();
				break;
		}

		if (GUI.changed)
		{
			editorWindow.Repaint();
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
		IconSetWindow.Instance.canvas.Size.target = DrawSlider("Size", IconSetWindow.Instance.canvas.Size.target, 0.5f, 2f);

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



/// <summary>
/// Meant for tools used to create glpyhs
/// Ex. 
///		- Draw tool (place vertices to make shape)
///		- Join tool
///		- Select tool
///		- Shape tool (box/ ellipse/ other)
///		- Etc.
/// </summary>
public interface IIconCanvasTool
{
	Action GetClickFunction();

	void OnCtrlDown(Event e);
	void OnCtrlUp(Event e);

	void OnAltDown(Event e);
	void OnAltUp(Event e);

	void OnShiftDown(Event e);
	void OnShiftUp(Event e);

	void OnMouseDown(Event e);
	void OnMouseUp(Event e);
}

/// <summary>
/// Meant for elements on the creation canvas
/// Ex.
///		- Vertices
///		- Curve handles
///		- Lines
///		- Grid lines (maybe)
///		- Glyphs
/// </summary>
public interface IIconGridElement
{
	void Snap(IIconGridSnapTarget target);
	void Move(Vector2 delta);
	void MoveLocal(Vector2 delta);
	void Draw();
}

public abstract class IconGridElement : ScriptableObject, IIconGridElement
{
	#region Style

	static GUIStyle _style;
	public static GUIStyle Style
	{
		get
		{
			if (_style == null)
			{
				_style = new GUIStyle();
				_style.normal.background = (Texture2D)EditorGUIUtility.IconContent("slider thumb@2x").image;
				_style.hover.background = (Texture2D)EditorGUIUtility.IconContent("slider thumb act@2x").image;
				_style.normal.scaledBackgrounds = new Texture2D[] { (Texture2D)EditorGUIUtility.IconContent("slider thumb@2x").image };
				_style.hover.scaledBackgrounds = new Texture2D[] { (Texture2D)EditorGUIUtility.IconContent("slider thumb act@2x").image };
			}
			return _style;
		}
	}

	#endregion

	public Rect rect;
	public Rect drawnRect
	{
		get
		{
			return new Rect(rect.position - (rect.size / 2f), rect.size);
		}
	}

	public Vector2 localPosition;

	public bool hovered { get { return rect.Contains(Event.current.mousePosition); } }

	public bool dirty { get; set; }

	public IconGridElement()
	{
		rect = new Rect(0f, 0f, 70f, 70f);
	}

	public virtual void Draw()
	{
		//EditorGUI.DrawRect(drawnRect, Color.blue);
		GUI.Box(drawnRect, "", Style);
		EditorGUIUtility.AddCursorRect(drawnRect, MouseCursor.MoveArrow);
	}

	public virtual void SetPosition(Vector2 pos)
	{
		rect.position = pos;
	}

	/// <summary>
	/// Should be a normalized value
	/// </summary>
	/// <param name="pos"></param>
	public virtual void SetLocalPosition(Vector2 pos)
	{
		localPosition = pos;
	}

	public virtual void Move(Vector2 delta)
	{
		rect.position += delta;
	}

	/// <summary>
	/// CHANGE TO REFLECT THE ELEMENTS POSITION IN LOCAL GRID SPACE
	/// </summary>
	/// <param name="delta"></param>
	public virtual void MoveLocal(Vector2 delta)
	{
		localPosition += delta;
	}

	public virtual void Snap(IIconGridSnapTarget target)
	{
		rect.position = target.GetSnapPoint();
	}
}

public interface IIconGridSnapTarget
{
	void SetPriority(int priority);
	Vector2 GetSnapPoint();
}


public interface IIconGlyph
{
	void SetVertices(List<IIconGridElement> vertices);
	void SetLines(List<IIconGridElement> lines);
}

public class IconCanvas : ScriptableObject
{
	/// <summary>
	/// The area that we are allowed to draw in.
	/// </summary>
	public Rect container;
	public Vector2 position;
	public Vector2 localPosition;
	public Vector2 pivot = Vector2.zero;
	public Vector2 offset;
	Rect gridRect;

	public int cells = 24;
	public int outerCells = 6;

	const float gridWidth = 600f;
	const float checkVal = 280f;

	Vector2 center { get { return container.center + offset; } }
	Rect[] backgroundRects = new Rect[] { new Rect(), new Rect(), new Rect(), new Rect() };

	AnimFloat _size;
	public AnimFloat Size
	{
		get
		{
			if (_size == null)
				_size = new AnimFloat(1f);
			if (_size.valueChanged == null)
				_size.valueChanged = new UnityEngine.Events.UnityEvent();

			return _size;
		}
	}

	public List<IconGlyph> glyphs;
	public List<UnityEngine.Object> toDestroy;

	public bool draggingCanvas;

	Texture2D gridTex;
	Texture2D gradientTex;
	Color defaultBGColor;
	Color[] gridColors;

	public IconCanvas()
	{
		glyphs = new List<IconGlyph>();
		toDestroy = new List<UnityEngine.Object>();
	}

	void OnEnable()
	{
		Size.valueChanged.AddListener(OnSizeChanged);
		gridRect = new Rect();

		gridTex = Resources.Load("Textures/4x4_TileGrid_White") as Texture2D;
		gradientTex = Resources.Load("Textures/1x32_Gradient_White") as Texture2D;

		defaultBGColor = Color.HSVToRGB(0f, 0f, 0.22f);
		gridColors = new Color[16];
		UpdateGridColors();
	}

	public void SetContainerRect(Rect trimmed)
	{
		container = trimmed;
	}

	public void Reset()
	{
		SetOffset(Vector2.zero);
		Size.target = Size.value = 1f;
		if (points != null)
			points.Clear();
	}

	public void Draw()
	{
		DrawGrid();
		DrawPoints(gridRect);
	}

	void CheckOffset()
	{
		float halfW = (container.width / 2f) + (checkVal * Size.value);
		float halfH = (container.height / 2f) + (checkVal * Size.value);

		float x = offset.x;
		float y = offset.y;

		if (x < -halfW)
			x = -halfW;
		if (x > halfW)
			x = halfW;

		if (y < -halfH)
			y = -halfH;
		if (y > halfH)
			y = halfH;

		offset.x = x;
		offset.y = y;
	}

	Color prevColor1;
	Color prevColor2;

	public void UpdateGridColors()
	{
		SetGridColors((Color)IconSetWindow.Instance.Prefs.GridForeground, (Color)IconSetWindow.Instance.Prefs.GridBackground);
	}

	public void SetGridColors(Color color1, Color color2)
	{
		if (color1 == prevColor1 && color2 == prevColor2)
			return;

		gridColors[0] = gridColors[2] = gridColors[5] = gridColors[7] = gridColors[8] = gridColors[10] = gridColors[13] = gridColors[15] = color1;
		gridColors[1] = gridColors[3] = gridColors[4] = gridColors[6] = gridColors[9] = gridColors[11] = gridColors[12] = gridColors[14] = color2;

		gridTex.SetPixels(gridColors);
		gridTex.Apply();
	}

	void DrawGrid()
	{
		UpdateGridColors();

		CheckOffset();
		CalculateGridRect();
		float border = 5f * Size.value;

		DrawBackground(gridRect, (Color)IconSetWindow.Instance.Prefs.GridBGTint, border);

		GUI.DrawTexture(container, gradientTex, ScaleMode.StretchToFill, true, 0, (Color)IconSetWindow.Instance.Prefs.GridGradientTint, 0, 0);
		DrawRectWithBorder(gridRect, defaultBGColor, 1f, border);

		float sizeVal = gridWidth * Size.value;

		GUI.DrawTextureWithTexCoords(gridRect, gridTex, new Rect(Vector2.zero, cells * gridRect.size / sizeVal));

		DrawGridRect(gridRect, cells, (Color)IconSetWindow.Instance.Prefs.GridMinorLineColor, 2f);
		DrawGridRect(gridRect, outerCells, (Color)IconSetWindow.Instance.Prefs.GridMajorLineColor, 3f);
	}

	void CalculateGridRect()
	{
		float size = gridWidth * Size.value;
		gridRect.Set(center.x - (size / 2f), center.y - (size / 2f), size, size);
	}

	void DrawRectWithBorder(Rect rect, Color color, float alpha, float border)
	{
		Color bg = new Color(color.r, color.g, color.b, alpha);

		Rect bordered = new Rect(rect);
		bordered.min -= Vector2.one * border;
		bordered.max += Vector2.one * border;

		EditorGUI.DrawRect(bordered, bg);
	}

	void DrawBackground(Rect exclude, Color color, float border = 0f)
	{
		backgroundRects[0].Set(container.x, container.y, exclude.xMin - border, container.height); // Left
		backgroundRects[1].Set(exclude.xMax + border, container.y, container.xMax - exclude.xMax - border, container.height); // Right
		backgroundRects[2].Set(exclude.xMin - border, container.y, exclude.width + border + border, exclude.yMin - container.yMin - border); // Top
		backgroundRects[3].Set(exclude.xMin - border, exclude.yMax + border, exclude.width + border + border, container.yMax - exclude.yMax - border); // Bottom

		for (int i = 0; i < 4; i++)
			EditorGUI.DrawRect(backgroundRects[i], color);
	}

	void DrawGridRect(Rect grid, int dp, Color color, float width)
	{
		using (Handles.DrawingScope d = new Handles.DrawingScope(color))
		{
			Handles.BeginGUI();
			float spacing = grid.width / dp;
			Vector3 from = new Vector3(grid.x, grid.yMin);
			Vector3 to = new Vector3(grid.x, grid.yMax);
			for (int x = 0; x <= dp; x++)
			{
				Handles.DrawAAPolyLine(width, from, to);
				from.x += spacing;
				to.x += spacing;
			}
			from = new Vector3(grid.xMin, grid.y);
			to = new Vector3(grid.xMax, grid.y);
			for (int y = 0; y <= dp; y++)
			{
				Handles.DrawAAPolyLine(width, from, to);
				from.y += spacing;
				to.y += spacing;
			}
			Handles.EndGUI();
		}
	}

	bool MouseOverGrid()
	{
		return container.Contains(Event.current.mousePosition);
	}

	public IconGlyph CreateGlyph()
	{
		IconGlyph glyph = CreateInstance(typeof(IconGlyph)) as IconGlyph;
		glyphs.Add(glyph);
		return glyph;
	}

	public void DeleteGlyph(IconGlyph glyph)
	{
		glyphs.Remove(glyph);
		toDestroy.Add(glyph);
	}

	public void PurgeDestroyList()
	{
		for (int i = toDestroy.Count - 1; i >= 0; i--)
			DestroyImmediate(toDestroy[i]);

		toDestroy.Clear();
	}

	public void DrawGlyphs()
	{
		if (Glyph != null)
			Glyph.Draw();
	}

	public void Move(Vector2 delta)
	{
		offset += delta;
		if (Glyph != null)
			Glyph.Move(delta);
		//glyphs.ForEach(g => g.Move(delta));
	}

	public void MoveLocal(Vector2 delta)
	{
		localPosition += delta;
		//glyphs.ForEach(g => g.MoveLocal(delta));
	}

	public void SetPosition(Vector2 pos)
	{
		position = pos;
		if (Glyph != null)
			Glyph.SetPosition(pos);
		// do something to the glyphs
	}

	public void SetLocalPosition(Vector2 pos)
	{
		localPosition = pos;
		// do something to the glyphs
	}

	public void SetOffset(Vector2 offset)
	{
		this.offset = offset;
		if (Glyph != null)
			Glyph.SetPosition(offset);
	}

	#region Events

	public void ProcessEvents(Event e)
	{
		switch (e.type)
		{
			case EventType.MouseDown:
				OnMouseDown(e);
				break;

			case EventType.MouseUp:
				OnMouseUp(e);
				break;

			case EventType.MouseDrag:
				OnCanvasDrag(e);
				break;

			case EventType.ScrollWheel:
				OnScroll(e);
				break;
		}
	}

	void OnMouseDown(Event e)
	{
		if (!MouseOverGrid())
			return;

		if (e.button == 0 || e.button == 2)
		{
			draggingCanvas = true;
		}
		if (e.button == 1)
		{
			//if (Glyph == null)
			//{
			//	ShowContextMenu(e.mousePosition);
			//}
			//else
			//{
			//	Glyph.AddVertex(CreateVertex(e.mousePosition));
			//}

			AddPoint(e.mousePosition);
		}

		GUI.changed = true;
	}

	void OnMouseUp(Event e)
	{
		if (!draggingCanvas)
			return;

		if (e.button == 0 || e.button == 2)
		{
			draggingCanvas = false;
		}

		GUI.changed = true;
	}

	void OnCanvasDrag(Event e)
	{
		if (draggingCanvas)
		{
			Move(e.delta);

			GUI.changed = true;
		}
	}

	void ShowContextMenu(Vector2 pos)
	{
		GenericMenu generic = new GenericMenu();
		generic.AddItem(new GUIContent("Add Glyph"), false, () => { AddGlyph(pos); });
		generic.ShowAsContext();
	}

	float zoomSensitivity = 1f;

	void OnScroll(Event e)
	{
		if (!e.isScrollWheel)
			return;

		pivot = e.mousePosition;

		float prev = Size.value;
		Size.target = Size.value = Mathf.Abs(Size.value) * (-e.delta.y * zoomSensitivity * .015f + 1.0f);
		float perc = 1f - (Size.value / prev);

		Vector2 toCenter = pivot - center;

		offset += toCenter * perc;

		e.Use();
	}

	void OnSizeChanged()
	{

	}

	#endregion

	#region Glyph stuff

	public IconGlyph Glyph;

	public void AddGlyph(Vector2 pos)
	{
		if (Glyph != null)
			RemoveGlyph();

		Glyph = CreateInstance(typeof(IconGlyph)) as IconGlyph;
		Glyph.Move(pos);
	}

	public void RemoveGlyph()
	{
		DestroyImmediate(Glyph);
	}

	IIconGridElement CreateVertex(Vector2 pos)
	{
		IconPoint p = CreateInstance(typeof(IconPoint)) as IconPoint;
		p.SetPosition(pos);
		return p;
	}

	Vector2 origin = Vector2.zero;
	public List<IconPoint> points = new List<IconPoint>();

	public void DrawPoints(Rect canvas)
	{
		foreach (IconPoint p in points)
		{
			DrawReferenceLines(p);
		}
		foreach (IconPoint point in points)
		{
			Vector2 pos = canvas.position + new Vector2(canvas.width * point.localPosition.x, canvas.height * point.localPosition.y);
			float pointSize = canvas.width / 50f;
			point.rect.Set(pos.x, pos.y, pointSize, pointSize);
			point.Draw();
		}
	}

	public void AddPoint(Vector2 pos)
	{
		IconPoint point = CreateInstance(typeof(IconPoint)) as IconPoint;
		point.SetLocalPosition(GetLocalPositionOnCanvas(gridRect, pos));
		points.Add(point);
	}

	public Vector2 GetLocalPositionOnCanvas(Rect canvas, Vector2 pos)
	{
		pos -= canvas.position;
		return new Vector2(pos.x / canvas.width, pos.y / canvas.height);
	}

	public Vector2 GetLocalPositionOnGrid(Vector2 pos)
	{
		return GetLocalPositionOnCanvas(gridRect, pos);
	}

	public Vector2 GetLocalMousePositionOnGrid()
	{
		return GetLocalPositionOnCanvas(gridRect, Event.current.mousePosition);
	}

	#endregion

	public void DrawReferenceLines(IconPoint p)
	{
		if (!p.drawRefLines)
			return;

		Handles.BeginGUI();
		Handles.color = Color.red;


		Vector2 center = p.rect.position;
		Vector2 top = new Vector2(center.x, container.yMin);
		Vector2 bottom = new Vector2(center.x, container.yMax);
		Vector2 left = new Vector2(container.xMin, center.y);
		Vector2 right = new Vector2(container.xMax, center.y);

		Handles.DrawLine(left, right);
		Handles.DrawLine(top, bottom);


		Handles.color = Color.white;
		Handles.EndGUI();
	}
}

[Serializable]
public class IconGlyph : IconGridElement
{
	public List<IIconGridElement> points;
	public Vector2 origin;

	public IconGlyph()
	{
		points = new List<IIconGridElement>();
	}

	public override void Draw()
	{
		foreach (IIconGridElement point in points)
		{
			point.Draw();
		}
	}

	public void AddVertex(IIconGridElement point)
	{
		points.Add(point);
	}

	public void RemoveVertex(IIconGridElement point)
	{
		points.Remove(point);
	}

	public override void Move(Vector2 delta)
	{
		points.ForEach(v => v.Move(delta));
	}

	public override void MoveLocal(Vector2 delta)
	{
		points.ForEach(v => v.MoveLocal(delta));
	}
}

public class IconPoint : IconGridElement
{
	public override void Draw()
	{
		base.Draw();
	}

	public bool ProcessEvents(Event e)
	{
		switch (e.type)
		{
			case EventType.KeyDown:
			case EventType.KeyUp:
				dirty = true;
				break;

			case EventType.MouseMove:
				MouseMove(e);
				break;

			case EventType.MouseDown:
				MouseDown(e);
				break;

			case EventType.MouseUp:
				MouseUp(e);
				break;

			case EventType.MouseDrag:
				MouseDrag(e);
				break;
		}
		if (e.isKey)
		{
			MouseDrag(e);
			dirty = true;
		}
		bool temp = dirty;
		dirty = false;
		return temp;
	}

	bool dragging;
	bool hovered;
	public bool drawRefLines { get { return dragging && Event.current.control; } }

	public void MouseMove(Event e)
	{
		if (hovered && !drawnRect.Contains(e.mousePosition))
		{
			hovered = false;
			dirty = true;
		}
		if (!hovered && drawnRect.Contains(e.mousePosition))
		{
			hovered = true;
			dirty = true;
		}
		dirty |= dragging;
	}

	public void MouseDown(Event e)
	{
		if (e.button == 0)
		{
			if (drawnRect.Contains(e.mousePosition))
			{
				e.Use();
				dragging = true;
				dirty = true;
			}
		}
	}

	public void MouseUp(Event e)
	{
		if (e.button == 0 && dragging)
		{
			e.Use();
			dragging = false;
			dirty = true;
		}
	}

	public void MouseDrag(Event e)
	{
		if (e.button == 0 && dragging)
		{
			localPosition = IconSetWindow.Instance.canvas.GetLocalMousePositionOnGrid();

			if (e.control)
			{
				localPosition = new Vector2(Handles.SnapValue(localPosition.x, 1f / IconSetWindow.Instance.canvas.cells), Handles.SnapValue(localPosition.y, 1f / IconSetWindow.Instance.canvas.cells));
			}

			e.Use();
			dirty = true;
		}
	}

}
