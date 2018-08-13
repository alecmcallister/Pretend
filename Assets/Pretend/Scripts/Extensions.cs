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

	/// <summary>
	/// Haircut for your rect
	/// </summary>
	/// <param name="rect">The rect getting the haircut</param>
	/// <param name="edge">Which edge to trim</param>
	/// <param name="amount">Amount to trim</param>
	/// <returns>The original rect with a fancy new look</returns>
	public static Rect TrimEdge(this Rect rect, RectEdge edge, float amount)
	{
		switch (edge)
		{
			case RectEdge.Left:
				rect.xMin += amount;
				break;
			case RectEdge.Right:
				rect.xMax -= amount;
				break;
			case RectEdge.Top:
				// May be backwards
				rect.yMin += amount;
				break;
			case RectEdge.Bottom:
				// May be backwards
				rect.yMax -= amount;
				break;
		}
		return rect;
	}
}

public enum RectEdge
{
	Left,
	Right,
	Top,
	Bottom
}
