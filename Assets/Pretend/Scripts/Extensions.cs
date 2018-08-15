using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static partial class Extensions
{
	public static Color WithAlpha(this Color color, float alpha)
	{
		return new Color(color.r, color.g, color.b, alpha);
	}

	public static byte[] ToByteArray(this string s)
	{
		List<byte> b = new List<byte>();

		foreach (char c in s.ToCharArray())
			b.Add((byte)c);

		return b.ToArray();
	}

	/// <summary>
	/// Creates a uniform offset with the given value.
	/// </summary>
	/// <param name="rect"></param>
	/// <param name="val"></param>
	/// <returns></returns>
	public static RectOffset UniformOffset(this RectOffset rect, int val)
	{
		return new RectOffset(val, val, val, val);
	}

	public static GUIStyle ActuallyCopyFrom(this GUIStyle style, GUIStyle other)
	{

		style = new GUIStyle(other);

		style.active = other.active;
		style.hover = other.hover;
		style.focused = other.focused;
		style.normal = other.normal;

		style.onActive = other.onActive;
		style.onHover = other.onHover;
		style.onFocused = other.onFocused;
		style.onNormal = other.onNormal;

		return style;
	}
}

public enum RectEdge
{
	Left,
	Right,
	Top,
	Bottom
}
