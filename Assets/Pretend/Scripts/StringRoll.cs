using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class StringRoll : MonoBehaviour
{
	Text text;
	public int len;
	System.Random r;
	string s = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

	void Awake()
	{
		text = GetComponent<Text>();
		r = new System.Random();
	}

	void Update()
	{
		if (Input.GetKey(KeyCode.Space))
		{
			text.text = GenRandomString(len);
		}
	}

	string GenRandomString(int length)
	{
		return new string(Enumerable.Range(0, length).Select(x => s[r.Next(0, s.Length)]).ToArray());
	}
}
