using System;
using System.Collections;
using System.Collections.Generic;
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

	#endregion

	#region Fields

	#region Instance

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

	#region Grid Offset

	Vector2 _gridOffset;
	public Vector2 GridOffset
	{
		get
		{
			if (_gridOffset == null)
				_gridOffset = Vector2.zero;

			return _gridOffset;
		}
		set
		{
			_gridOffset = value;
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

		Prefs.Size.valueChanged.AddListener(UpdateVertices);
		Prefs.GridSpacing.valueChanged.AddListener(Repaint);
		Prefs.GridDetailLines.valueChanged.AddListener(Repaint);

		ResetGrid();
	}

	void OnDisable()
	{
		Prefs.Save();
	}

	#endregion

	#region OnGUI

	void OnGUI()
	{
		DrawGrid();
		DoGridCursor();
		DrawVertices();
		DoSelectionBox();

		DrawToolbar();

		ProcessVertexEvents(Event.current);
		ProcessEvents(Event.current);

		PurgeDestroyList();

		if (GUI.changed)
			Repaint();
	}

	#endregion


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
		GUILayout.Label(" (" + GridOffset.x.ToString("0.0") + ", " + GridOffset.y.ToString("0.0") + ") ", EditorStyles.toolbarTextField);
		GUILayout.Space(5f);
		GUILayout.Label(" Mouse on grid: " + MouseOverGrid() + " ", EditorStyles.toolbarTextField);
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
		gridRect.position = Vector2.zero;
		gridRect.size = position.size;
		gridRect = gridRect.TrimEdge(RectEdge.Top, ToolbarHeight);
	}

	void DrawGrid()
	{
		TrimGrid();

		DrawGridLines(Prefs.GridSpacing.value / (int)Prefs.GridDetailLines.target, Color.gray, 0.2f);
		DrawGridLines(Prefs.GridSpacing.value, Color.gray, 0.4f);
	}

	void DrawGridLines(float spacing, Color color, float alpha)
	{
		int xDivs = Mathf.CeilToInt((gridRect.width + spacing) / spacing);
		int yDivs = Mathf.CeilToInt((gridRect.height + spacing) / spacing);

		Handles.BeginGUI();
		Handles.color = new Color(color.r, color.g, color.b, alpha);

		Vector3 newOffset = new Vector3(GridOffset.x % spacing, GridOffset.y % spacing, 0);

		Vector3 from = Vector3.zero;
		Vector3 to = Vector3.zero;

		for (int x = 0; x < xDivs; x++)
		{
			from.x = to.x = newOffset.x + (spacing * x);
			from.y = newOffset.y - spacing;
			to.y = newOffset.y + gridRect.height + spacing;
			Handles.DrawLine(from, to);
		}
		for (int y = 0; y < yDivs; y++)
		{
			from.y = to.y = newOffset.y + (spacing * y);
			from.x = newOffset.x - spacing;
			to.x = newOffset.x + gridRect.width + spacing;
			Handles.DrawLine(from, to);
		}

		Handles.color = Color.white;
		Handles.EndGUI();
	}

	void ResetGrid()
	{
		GridOffset = Vector2.zero;
		UpdateVertices();
	}

	bool MouseOverGrid()
	{
		return gridRect.Contains(Event.current.mousePosition);
	}

	#region Zoom

	float cameraDistance = 2f;
	Vector3 pivot = Vector3.zero;

	private void HandleScrollWheel(bool zoomTowardsCenter)
	{
		float g = 2.5f;

		float initialDistance = cameraDistance;
		Vector2 pivotVector = Event.current.mousePosition;

		float zoomDelta = Event.current.delta.y;

		Prefs.GridSpacing.target = Mathf.Abs(Prefs.GridSpacing.value) * (zoomDelta * .015f + 1.0f);

		float percentage = 1f - (cameraDistance / initialDistance);
		if (!zoomTowardsCenter)
			GridOffset += pivotVector * percentage;

		Event.current.Use();
	}

	#endregion

	#endregion

	#region Events

	void ProcessEvents(Event e)
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
				if (e.isScrollWheel)
					HandleScrollWheel(true);
				break;
		}
	}

	void ShowContextMenu(Vector2 pos)
	{
		//GenericMenu generic = new GenericMenu();
		//generic.AddItem(new GUIContent("Add Vertex"), false, () => { CreateVertex(pos); });
		//generic.ShowAsContext();

		CreateVertex(pos);
	}

	void OnMouseDown(Event e)
	{
		if (!MouseOverGrid())
			return;

		if (e.button == 0)
		{
			selectingRect = new Rect(e.mousePosition, Vector2.zero);
			selectingCanvas = true;
		}
		if (e.button == 1)
		{
			ShowContextMenu(e.mousePosition);
		}
		if (e.button == 2)
		{
			draggingCanvas = true;
		}

		GUI.changed = true;
	}

	void OnMouseUp(Event e)
	{
		if (e.button == 0)
		{
			selectingCanvas = false;
		}
		if (e.button == 2)
		{
			draggingCanvas = false;
		}

		GUI.changed = true;
	}

	bool draggingCanvas;
	bool selectingCanvas;
	Rect selectingRect;

	void OnCanvasDrag(Event e)
	{
		if (e.button == 0 && selectingCanvas)
		{
			selectingRect.max += e.delta;
			GUI.changed = true;
		}
		if (e.button == 2 && draggingCanvas)
		{
			GridOffset += e.delta;
			UpdateVertices();
		}
	}

	void DoGridCursor()
	{
		if (draggingCanvas)
			EditorGUIUtility.AddCursorRect(gridRect, MouseCursor.Pan);
	}

	void DoSelectionBox()
	{
		if (!selectingCanvas)
			return;

		EditorGUI.DrawRect(selectingRect, Color.red.WithAlpha(0.2f));
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

		if (Event.current.type == EventType.MouseMove)
		{
			editorWindow.Repaint();
		}
	}

	#endregion

	#region Draw

	void DrawHeader()
	{
		EditorGUILayout.BeginVertical(bg);
		IconSetWindow.Instance.Prefs.Size.target = DrawSlider("Size", IconSetWindow.Instance.Prefs.Size.target, 10f, 30f);
		IconSetWindow.Instance.Prefs.GridSpacing.target = DrawSlider("Grid spacing", IconSetWindow.Instance.Prefs.GridSpacing.target, 100f, 500f);
		IconSetWindow.Instance.Prefs.GridDetailLines.target = (int)DrawSlider("Grid detail lines", (int)IconSetWindow.Instance.Prefs.GridDetailLines.target, 1, 12);
		EditorGUILayout.EndVertical();
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

	#endregion

	public override Vector2 GetWindowSize()
	{
		Vector2 size = new Vector2(300f, 450f);
		return size;
	}

	public override void OnOpen()
	{
		if (!init)
			Init();
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

		bg = new GUIStyle(GUI.skin.FindStyle("In BigTitle"));
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

	static GUIStyle[] _style;
	public static GUIStyle[] Style
	{
		get
		{
			if (_style == null)
			{
				_style = new GUIStyle[] { new GUIStyle(), new GUIStyle() };
				_style[0].normal.background = (Texture2D)EditorGUIUtility.IconContent("node1").image;
				_style[1].normal.background = (Texture2D)EditorGUIUtility.IconContent("node2").image;
			}
			return _style;
		}
	}

	#endregion

	public Rect rect { get; set; }
	public Vector2 position { get; set; }
	public Vector2 localPosition { get; set; }
	public bool hovered { get { return rect.Contains(Event.current.mousePosition); } }

	public bool dirty { get; set; }

	public IconGridElement()
	{
		rect = new Rect();
		position = Vector2.zero;
		localPosition = Vector2.zero;
	}

	public virtual void Draw()
	{
		GUI.Box(rect, "", Style[hovered ? 1 : 0]);
		EditorGUIUtility.AddCursorRect(rect, MouseCursor.MoveArrow);
	}

	public virtual void SetPosition(Vector2 pos)
	{
		position = pos;
	}

	public virtual void SetLocalPosition(Vector2 pos)
	{
		localPosition = pos;
	}

	public virtual void Move(Vector2 delta)
	{
		position += delta;
	}

	public virtual void MoveLocal(Vector2 delta)
	{
		localPosition += delta;
	}

	public virtual void Snap(IIconGridSnapTarget target)
	{
		localPosition = target.GetSnapPoint();
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
	public Vector2 pivot;
	public Vector2 offset;

	public int width;
	public int height;

	public int majorLineInterval;
	public int minorLineInterval;

	public float scale = 100f;
	public float zoom = 1f;

	public List<IconGlyph> glyphs;
	public List<UnityEngine.Object> toDestroy;

	public IconCanvas()
	{
		glyphs = new List<IconGlyph>();
		toDestroy = new List<UnityEngine.Object>();
	}

	public void SetContainerRect(Rect trimmed)
	{
		container = new Rect(trimmed);
	}

	public void Draw()
	{
		DrawGrid();
		DrawGlyphs();
	}

	#region Refactoring

	void DrawGrid()
	{
		DrawGridLines(scale / minorLineInterval, Color.gray, 0.2f);
		DrawGridLines(scale, Color.gray, 0.4f);
	}

	void DrawGridLines(float spacing, Color color, float alpha)
	{
		int xDivs = Mathf.CeilToInt((container.width + spacing) / spacing);
		int yDivs = Mathf.CeilToInt((container.height + spacing) / spacing);

		Handles.BeginGUI();
		Handles.color = new Color(color.r, color.g, color.b, alpha);

		Vector3 newOffset = new Vector3(offset.x % spacing, offset.y % spacing, 0);

		Vector3 from = Vector3.zero;
		Vector3 to = Vector3.zero;

		for (int x = 0; x < xDivs; x++)
		{
			from.x = to.x = newOffset.x + (spacing * x);
			from.y = newOffset.y - spacing;
			to.y = newOffset.y + container.height + spacing;
			Handles.DrawLine(from, to);
		}
		for (int y = 0; y < yDivs; y++)
		{
			from.y = to.y = newOffset.y + (spacing * y);
			from.x = newOffset.x - spacing;
			to.x = newOffset.x + container.width + spacing;
			Handles.DrawLine(from, to);
		}

		Handles.color = Color.white;
		Handles.EndGUI();
	}

	void ResetGrid()
	{
		offset = Vector2.zero;
	}

	bool MouseOverGrid()
	{
		return container.Contains(Event.current.mousePosition);
	}

	#endregion

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
		foreach (IconGlyph glyph in glyphs)
		{
			glyph.Draw();
		}
	}

	public void Move(Vector2 delta)
	{
		position += delta;
		glyphs.ForEach(g => g.Move(delta));
	}

	public void MoveLocal(Vector2 delta)
	{
		localPosition += delta;
		glyphs.ForEach(g => g.MoveLocal(delta));
	}

	public void SetPosition(Vector2 pos)
	{
		position = pos;
		// do something to the glyphs
	}

	public void SetLocalPosition(Vector2 pos)
	{
		localPosition = pos;
		// do something to the glyphs
	}

	public void SetZoom(float val, Vector2 pivot)
	{

	}

}

[Serializable]
public class IconGlyph : IconGridElement
{
	public List<IIconGridElement> vertices;
	public Vector2 origin;

	public IconGlyph()
	{
		vertices = new List<IIconGridElement>();
	}

	public override void Draw()
	{
		foreach (IIconGridElement vertex in vertices)
		{
			vertex.Draw();
		}
	}

	public void AddVertex(IIconGridElement vertex)
	{
		vertices.Add(vertex);
	}

	public void RemoveVertex(IIconGridElement vertex)
	{
		vertices.Remove(vertex);
	}

	public override void Move(Vector2 delta)
	{
		vertices.ForEach(v => v.Move(delta));
	}

	public override void MoveLocal(Vector2 delta)
	{
		vertices.ForEach(v => v.MoveLocal(delta));
	}
}