using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
			{
				_size = new AnimFloat(20f);
			}
			if (_size.valueChanged == null)
			{
				_size = new AnimFloat(_size.target < 0.01f ? 20f : _size.target);
			}

			return _size;
		}
	}

	[SerializeField]
	AnimFloat _gridSpacing;
	public AnimFloat GridSpacing
	{
		get
		{
			if (_gridSpacing == null)
			{
				_gridSpacing = new AnimFloat(100f);
			}
			if (_gridSpacing.valueChanged == null)
			{
				_gridSpacing = new AnimFloat(_gridSpacing.target < 0.01f ? 100f : _gridSpacing.target);
			}

			return _gridSpacing;
		}
	}

	[SerializeField]
	AnimFloat _gridDetailLines;
	public AnimFloat GridDetailLines
	{
		get
		{
			if (_gridDetailLines == null)
			{
				_gridDetailLines = new AnimFloat(5);
			}
			if (_gridDetailLines.valueChanged == null)
			{
				_gridDetailLines = new AnimFloat(Math.Max(1, (int)_gridDetailLines.target));
			}

			return _gridDetailLines;
		}
	}

	#endregion

	public void Save()
	{
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	public static IconSetPrefs Load()
	{
		IconSetPrefs prefs = Resources.Load("Editor/IconSetPrefs") as IconSetPrefs;

		if (prefs == null)
		{
			prefs = CreateInstance<IconSetPrefs>();

			string path = "Assets/Pretend/Resources/Editor/";

			AssetDatabase.CreateAsset(prefs, path + "IconSetPrefs.asset");
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		return prefs;
	}
}
