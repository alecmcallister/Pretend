using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[Serializable]
public class IconVertex : ScriptableObject
{
	static bool init;
	public static IconVertex currentHovered;
	public static bool hitRect;

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
		set
		{
			_style = value;
		}
	}

	#endregion

	#region Instance

	public Rect rect;
	public bool dragging;
	public bool hovered;

	bool dirty;

	Vector2 _pos;

	public Vector2 OffsetPos
	{
		get
		{
			//return RawPos + IconSetWindow.Instance.GridOffset;
			return RawPos;
		}
		set
		{
			//_pos = value - IconSetWindow.Instance.GridOffset;
			_pos = value;
		}
	}

	public Vector2 RawPos
	{
		get
		{
			if (_pos == null)
				_pos = Vector2.zero;

			return _pos;
		}
		set
		{
			_pos = value;
		}
	}

	Vector2 MousePosition
	{
		get
		{
			//return Event.current.mousePosition - IconSetWindow.Instance.GridOffset;
			return Event.current.mousePosition;
		}
	}

	public IconVertex Initialize(Vector2 position)
	{
		OffsetPos = position;
		rect = new Rect();
		UpdateRect();

		return this;
	}

	public void UpdateRect()
	{
		float val = IconSetWindow.Instance.Prefs.Size.value;
		rect.Set(OffsetPos.x - (val / 2f), OffsetPos.y - (val / 2f), val, val);
		dirty = true;
	}

	public void Drag()
	{
		RawPos = MousePosition;
		UpdateRect();
	}

	// Add more snap targets (such as other vertices
	public void Drag(float interval, bool globalSnap)
	{
		if (globalSnap)
		{
			RawPos = new Vector2(Handles.SnapValue(MousePosition.x, interval), Handles.SnapValue(MousePosition.y, interval));
		}

		UpdateRect();
	}

	public void Draw()
	{
		GUI.Box(rect, "", Style[hovered ? 1 : 0]);
		EditorGUIUtility.AddCursorRect(rect, MouseCursor.MoveArrow);
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

		if (dirty)
		{
			dirty = false;
			return true;
		}
		else
		{
			dirty = false;
			return false;
		}
	}

	void MouseMove(Event e)
	{
		if (rect.Contains(e.mousePosition))
		{
			if (currentHovered == null)
			{
				if (!hovered)
				{
					MouseEnter(e);
				}
			}
			else if (currentHovered == this)
			{
				hitRect = true;
			}
			else if (currentHovered != this && !hitRect)
			{
				MouseEnter(e);
			}
			else
			{
				if (hovered)
					MouseExit(e);
			}
		}
		else if (hovered)
		{
			MouseExit(e);
		}
	}

	void MouseEnter(Event e)
	{
		if (currentHovered != null && currentHovered != this)
			currentHovered.MouseExit(e);

		currentHovered = this;
		hitRect = true;
		hovered = true;
		dirty = true;
	}

	void MouseExit(Event e)
	{
		if (currentHovered == this)
			currentHovered = null;

		hovered = false;
		dirty = true;
		hitRect = false;
	}

	void MouseDown(Event e)
	{
		if (e.button == 0)
		{
			if (rect.Contains(e.mousePosition))
			{
				e.Use();
				dragging = true;
				MouseDrag(e);
			}
		}
		if (e.button == 1)
		{
			if (rect.Contains(e.mousePosition))
			{
				e.Use();
				IconSetWindow.Instance.DeleteVertex(this);
				dirty = true;
			}
		}
	}

	void MouseUp(Event e)
	{
		if (e.button == 0 && dragging)
		{
			MouseExit(e);
			e.Use();
			dragging = false;
			dirty = true;
		}
	}

	void MouseDrag(Event e)
	{
		if (e.button == 0 && dragging)
		{
			if (e.control)
			{
				Drag(IconSetWindow.Instance.Prefs.GridSpacing.target / IconSetWindow.Instance.Prefs.GridDetailLines.target, true);
			}
			else
			{
				Drag();
			}
			e.Use();
			dirty = true;
		}
	}

	#endregion

	public void FollowGrid()
	{
		float spacing = IconSetWindow.Instance.Prefs.GridSpacing.value;
		//RawPos = new Vector2(Handles.SnapValue(MousePosition.x, interval), Handles.SnapValue(MousePosition.y, interval));

	}
}
