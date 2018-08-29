using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

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
	public Rect gridRect;

	public int cells = 24;
	public int outerCells = 6;

	const float gridBorderWidth = 5f;
	const float gridWidth = 600f;
	const float checkVal = 275f;

	Vector2 center { get { return container.center + offset; } }

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

	public bool draggingCanvas;

	Texture2D gridTex;
	Texture2D gradientTex;
	Color defaultBGColor;
	Color[] gridColors;

	public IconCanvas()
	{

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
	}

	public void ClearPoints()
	{
		if (points != null)
			points.Clear();
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

	public void DrawGrid()
	{
		UpdateGridColors();

		CheckOffset();
		CalculateGridRect();
		float border = 5f * Size.value;
		float sizeVal = gridWidth * Size.value;

		EditorGUI.DrawRect(container, (Color)IconSetWindow.Instance.Prefs.GridBGTint);
		GUI.DrawTexture(container, gradientTex, ScaleMode.StretchToFill, true, 0, (Color)IconSetWindow.Instance.Prefs.GridGradientTint, 0, 0);
		EditorGUI.DrawRect(gridRect, defaultBGColor);
		GUI.DrawTextureWithTexCoords(gridRect, gridTex, new Rect(Vector2.zero, cells * gridRect.size / sizeVal));

		DrawGridRect(gridRect, cells, (Color)IconSetWindow.Instance.Prefs.GridMinorLineColor, 1f);
		DrawGridRect(gridRect, outerCells, (Color)IconSetWindow.Instance.Prefs.GridMajorLineColor, 2f);

		IconSetWindow.DrawDottedBorder(gridRect, 2f, 5f, 3f, Color.yellow.WithAlpha(0.3f));
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

	public void Move(Vector2 delta)
	{
		offset += delta;
	}

	public void MoveLocal(Vector2 delta)
	{
		localPosition += delta;
	}

	public void SetPosition(Vector2 pos)
	{
		position = pos;
	}

	public void SetLocalPosition(Vector2 pos)
	{
		localPosition = pos;
	}

	public void SetOffset(Vector2 offset)
	{
		this.offset = offset;
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

	void OnScroll(Event e)
	{
		if (!e.isScrollWheel)
			return;

		pivot = e.mousePosition;

		float prev = Size.value;
		Size.target = Size.value = Mathf.Abs(Size.value) * (-e.delta.y * .015f + 1.0f);
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

	IIconGridElement CreateVertex(Vector2 pos)
	{
		IconPoint p = CreateInstance(typeof(IconPoint)) as IconPoint;
		p.SetPosition(pos);
		return p;
	}

	Vector2 origin = Vector2.zero;
	public List<IconPoint> points = new List<IconPoint>();

	public void DrawPoints()
	{
		foreach (IconPoint p in points)
		{
			if (p.drawRefLines)
			{
				//IconSetWindow.Instance.AddGuides(GetReferenceLines(p));
				// Move outside this method so that we can draw at a different time
				DrawReferenceLines(p);
			}

		}
		foreach (IconPoint point in points)
		{
			Vector2 pos = gridRect.position + new Vector2(gridRect.width * point.localPosition.x, gridRect.height * point.localPosition.y);
			float pointSize = gridRect.width / 50f;
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

	public static Vector2 GetLocalPositionOnCanvas(Rect canvas, Vector2 pos)
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

	public List<Vector2[]> GetReferenceLines(IconPoint p)
	{
		Vector2 center = p.rect.position;
		Vector2 top = new Vector2(center.x, container.yMin);
		Vector2 bottom = new Vector2(center.x, container.yMax);
		Vector2 left = new Vector2(container.xMin, center.y);
		Vector2 right = new Vector2(container.xMax, center.y);

		return new List<Vector2[]>() { new Vector2[] { left, right }, new Vector2[] { top, bottom } };
	}
}
