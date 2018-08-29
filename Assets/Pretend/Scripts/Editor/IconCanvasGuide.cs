using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Class that stores canvas-guide info
/// </summary>
public class IconCanvasGuide
{
	public IconCanvasGuideType type;
	public Vector2[] points = new Vector2[] { Vector2.zero, Vector2.zero };
	public float width = 2f;
	public float dash = 5f;
	public float space = 2f;
	public float snapThreshold = 5f;
	public Color color = new Color(0f, 1f, 1f, 0.5f);
	public Color hover = new Color(0f, 1f, 1f, 1f);

	public bool hovered;

	public float normalizedSnapThreshold
	{
		get
		{
			return snapThreshold / (type == IconCanvasGuideType.Horizontal ? canvas.gridRect.height : canvas.gridRect.width);
		}
	}
	public float x;
	public float y;

	public float Value
	{
		get
		{
			return (type == IconCanvasGuideType.Horizontal) ? y : x;
		}
	}

	IconCanvas canvas;

	static List<IconCanvasGuide> _guides;
	public static List<IconCanvasGuide> Guides
	{
		get
		{
			if (_guides == null)
				_guides = new List<IconCanvasGuide>();

			return _guides;
		}
	}

	public static Vector2 GetSnappedLocation(IconPoint p)
	{
		Vector2 pos = p.localPosition;

		float closestX = float.MaxValue;
		float closestY = float.MaxValue;

		foreach (IconCanvasGuide guide in Guides)
		{
			guide.hovered = false;
			if (guide.type == IconCanvasGuideType.Horizontal)
			{
				float distance = Mathf.Abs(pos.y - guide.y);
				if (distance < guide.normalizedSnapThreshold)
					if (distance < closestY)
					{
						closestY = guide.y;
						guide.hovered = p.dragging;
					}
			}
			else
			{
				float distance = Mathf.Abs(pos.x - guide.x);
				if (distance < guide.normalizedSnapThreshold)
					if (distance < closestX)
					{
						closestX = guide.x;
						guide.hovered = p.dragging;
					}
			}
		}

		pos.x = (closestX != float.MaxValue) ? closestX : pos.x;
		pos.y = (closestY != float.MaxValue) ? closestY : pos.y;

		return pos;
	}

	public static IconCanvasGuide GetGuideAtPos(Vector2 pos)
	{
		foreach (IconCanvasGuide guide in Guides)
		{
			if (guide.rect.Contains(pos))
				return guide;
		}

		return null;
	}

	public static void ClearAllGuides()
	{
		Guides.Clear();
	}

	/// <summary>
	/// Forms a box around the guide line. Used for snapping (i.e. if something is within this rect, snap it to the guide)
	/// </summary>
	public Rect rect
	{
		get
		{
			return new Rect(
				this[0].x - (type == IconCanvasGuideType.Vertical ? snapThreshold : 0f),
				this[0].y - (type == IconCanvasGuideType.Horizontal ? snapThreshold : 0f),
				type == IconCanvasGuideType.Vertical ? 2f * snapThreshold : canvas.container.width,
				type == IconCanvasGuideType.Horizontal ? 2f * snapThreshold : canvas.container.height);
		}
	}

	#region Constructor

	/// <summary>
	/// Add a guide to the screen
	/// </summary>
	/// <param name="bounds">The area we are allowed to draw in</param>
	/// <param name="canvas">The area that will be used for positioning</param>
	/// <param name="normalizedPos">The position (local to <paramref name="canvas"/>) that the guide will be placed</param>
	/// <param name="type">Horizontal or vertical</param>
	public IconCanvasGuide(IconCanvas canvas, float normalizedPos, IconCanvasGuideType type) : this(canvas, new Vector2(normalizedPos, normalizedPos), type) { }

	public IconCanvasGuide(IconCanvas canvas, Vector2 normalizedPos, IconCanvasGuideType type)
	{
		this.canvas = canvas;
		this.type = type;

		SetLocalPosition(normalizedPos);

		Guides.Add(this);
	}

	#endregion

	#region Remove

	public void DeleteGuide()
	{
		Guides.Remove(this);
	}

	#endregion

	#region Set local position

	public void SetLocalPosition(float pos)
	{
		x = y = pos;
	}

	public void SetLocalPosition(Vector2 pos)
	{
		x = pos.x;
		y = pos.y;
	}

	#endregion

	#region Update points

	public bool snap;

	void UpdatePoints()
	{
		if (snap)
		{
			x = Handles.SnapValue(x, 1f / IconSetWindow.Instance.canvas.cells);
			y = Handles.SnapValue(y, 1f / IconSetWindow.Instance.canvas.cells);
		}

		Vector2 screen = canvas.gridRect.position + new Vector2(canvas.gridRect.width * x, canvas.gridRect.height * y);

		switch (type)
		{
			case IconCanvasGuideType.Horizontal:
				points[0].y = points[1].y = screen.y;
				points[0].x = canvas.container.xMin;
				points[1].x = canvas.container.xMax;
				break;

			case IconCanvasGuideType.Vertical:
				points[0].x = points[1].x = screen.x;
				points[0].y = canvas.container.yMin;
				points[1].y = canvas.container.yMax;
				break;
		}
	}

	#endregion

	#region Indexer

	public Vector2 this[int i]
	{
		get { UpdatePoints(); return (i == 0 || i == 1) ? points[i] : Vector2.zero; }
		set { if (i == 0 || i == 1) points[i] = value; }
	}

	#endregion

}

public enum IconCanvasGuideType
{
	Horizontal,
	Vertical,
}
