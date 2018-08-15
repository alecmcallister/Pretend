using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

[Serializable]
public class IconSetPrefs : ScriptableObject
{
	#region Fields

	[SerializeField]
	AnimFloat _size;
	public AnimFloat Size
	{
		get
		{
			if (_size == null)
				_size = new AnimFloat(20f);
			if (_size.valueChanged == null)
				_size = new AnimFloat(_size.target < 0.01f ? 20f : _size.target);

			return _size;
		}
	}

	#region Colors

	[SerializeField]
	Color _gridGradientTint;
	public Color? GridGradientTint
	{
		get
		{
			if (_gridGradientTint == null)
				_gridGradientTint = new Color(0.1f, 0.1f, 0.1f);

			return _gridGradientTint;
		}
		set
		{
			if (value == null)
				_gridGradientTint = new Color(0.1f, 0.1f, 0.1f);

			else
				_gridGradientTint = (Color)value;
		}
	}

	[SerializeField]
	Color _gridBGTint;
	public Color? GridBGTint
	{
		get
		{
			if (_gridBGTint == null)
				_gridBGTint = Color.clear;

			return _gridBGTint;
		}
		set
		{
			if (value == null)
				_gridBGTint = Color.clear;

			else
				_gridBGTint = (Color)value;
		}
	}

	[SerializeField]
	Color _gridMajorLineColor;
	public Color? GridMajorLineColor
	{
		get
		{
			if (_gridMajorLineColor == null)
				_gridMajorLineColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);

			return _gridMajorLineColor;
		}
		set
		{
			if (value == null)
				_gridMajorLineColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);

			else
				_gridMajorLineColor = (Color)value;
		}
	}

	[SerializeField]
	Color _gridMinorLineColor;
	public Color? GridMinorLineColor
	{
		get
		{
			if (_gridMinorLineColor == null)
				_gridMinorLineColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);

			return _gridMinorLineColor;
		}
		set
		{
			if (value == null)
				_gridMinorLineColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);

			else
				_gridMinorLineColor = (Color)value;
		}
	}

	[SerializeField]
	Color _gridForeground;
	public Color? GridForeground
	{
		get
		{
			if (_gridForeground == null)
				_gridForeground = new Color(0.1f, 0.1f, 0.1f, 0.2f);

			return _gridForeground;
		}
		set
		{
			if (value == null)
				_gridForeground = new Color(0.1f, 0.1f, 0.1f, 0.2f);

			else
				_gridForeground = (Color)value;
		}
	}

	[SerializeField]
	Color _gridBackground;
	public Color? GridBackground
	{
		get
		{
			if (_gridBackground == null)
				_gridBackground = new Color(0f, 0f, 0f, 0f);

			return _gridBackground;
		}
		set
		{
			if (value == null)
				_gridBackground = new Color(0f, 0f, 0f, 0f);

			else
				_gridBackground = (Color)value;
		}
	}

	#endregion

	public void SetValuesToDefault()
	{
		GridBackground = null;
		GridForeground = null;
		GridMinorLineColor = null;
		GridMajorLineColor = null;
		GridBGTint = null;
		GridGradientTint = null;
	}

	#endregion

	public void Save()
	{
		EditorUtility.SetDirty(this);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	bool saved;

	void OnEnable()
	{
		saved = false;
	}

	void OnDisable()
	{
		if (saved)
			return;

		Save();
		saved = true;
	}

	void OnDestroy()
	{
		if (saved)
			return;

		Save();
		saved = true;
	}

	public static IconSetPrefs Load()
	{
		IconSetPrefs prefs = Resources.Load("Editor/IconSetPrefs") as IconSetPrefs;

		if (prefs == null)
		{
			prefs = CreateInstance<IconSetPrefs>();

			Debug.Log("Creating new prefs with default values");
			prefs.SetValuesToDefault();

			string path = "Assets/Pretend/Resources/Editor/";

			AssetDatabase.CreateAsset(prefs, path + "IconSetPrefs.asset");
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		return prefs;
	}
}
