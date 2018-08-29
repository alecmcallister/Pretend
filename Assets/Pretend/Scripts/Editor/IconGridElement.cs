using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
				_style.normal.background = Resources.Load<Texture2D>("Editor/Textures/point_normal");
				_style.hover.background = Resources.Load<Texture2D>("Editor/Textures/point_active");
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
	public bool hovered;
	public bool dirty { get; set; }

	public IconGridElement()
	{
		rect = new Rect(0f, 0f, 50f, 50f);
	}

	public virtual void Draw()
	{
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
}