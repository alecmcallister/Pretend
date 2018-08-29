using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class IconPoint : IconGridElement
{
	public bool dragging;
	public bool drawRefLines { get { return dragging && Event.current.control; } }

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

				MouseDrag(e);
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
			IconCanvasGuide.Guides.ForEach(g => g.hovered = false);
		}
	}

	public void MouseDrag(Event e)
	{
		if (e.button == 0 && dragging)
		{
			localPosition = IconSetWindow.Instance.canvas.GetLocalMousePositionOnGrid();
			localPosition = IconCanvasGuide.GetSnappedLocation(this);

			if (e.control)
			{
				localPosition = new Vector2(Handles.SnapValue(localPosition.x, 1f / IconSetWindow.Instance.canvas.cells), Handles.SnapValue(localPosition.y, 1f / IconSetWindow.Instance.canvas.cells));
			}

			e.Use();
			dirty = true;
		}
	}
}
