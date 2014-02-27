using UnityEngine;
using Newtonsoft.Json;

using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class Settings : Singleton<Settings>
{
	public string settingsFilePath;
	public string settingsFileName = "scene1_settings.txt";

	private SettingsValues values;

	public static SettingsValues Values
	{
		get { return Instance.values; }
		set { Instance.values = value; }
	}

	protected Settings()
	{
		settingsFilePath = "C:/PerceptionStudySettings/Study2/" + settingsFileName;
		values = new SettingsValues();
	}

	public static string GetStringSettings()
	{
		return JsonConvert.SerializeObject(Settings.Values);
	}

	public static Dictionary<string, string> GetDictionarySettings()
	{
		Dictionary<string, string> values = new Dictionary<string, string>();

		string json = GetStringSettings();

		if(json != "" && json != "null") 
		{
			values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
		}

		return values;
	}

	public static void LoadSettings()
	{
		string json = System.IO.File.ReadAllText(Settings.Instance.settingsFilePath);
		
		if(json != "" && json != "null") 
		{
			SettingsValues values = JsonConvert.DeserializeObject<SettingsValues>(json);
			Settings.Values = (SettingsValues)values.Clone();
		}			
		else
		{
			print ("The settings file is not valid");
		}
	}

	public static void SaveSettings()
	{
		string json = GetStringSettings();
		
		if(json != "" && json != "null") 
		{
			System.IO.File.WriteAllText(Settings.Instance.settingsFilePath, json);
		}			
		else
		{
			print ("The settings string is not valid");
		}
	}

	void Update () 
	{
//		Dictionary<string, string> values = GetDictionarySettings();
//		char[] alphabet = Settings.Instance.alphabet.ToCharArray();
//
//		int count = 0;
//		int selectedIndex = -1;
//
//		foreach(char c in alphabet)
//		{
//			if(Input.GetKeyDown(c.ToString()))
//			{
//				selectedIndex = count;
//			}
//			count ++;
//		}
//
//		if(selectedIndex != -1 && selectedIndex < values.Count)
//		{
//			string key = values.Keys.ElementAt(selectedIndex);
//			float value = float.Parse(values.Values.ElementAt(selectedIndex));
//
//			if(Input.GetKey(KeyCode.UpArrow))
//			{
//				if(Input.GetKey(KeyCode.Keypad1))
//				{
//					value += 0.01f;
//				}
//
//				if(Input.GetKey(KeyCode.Keypad2))
//				{
//					value += 0.1f;
//				}
//
//				if(Input.GetKey(KeyCode.Keypad3))
//				{
//					value += 1.0f;
//				}
//
//				if(Input.GetKey(KeyCode.Keypad4))
//				{
//					value += 10.0f;					
//				}
//
//				if(Input.GetKey(KeyCode.Keypad5))
//				{
//					value += 100.0f;					
//				}
//			}
//			else if(Input.GetKey(KeyCode.DownArrow))
//			{
//				if(Input.GetKey(KeyCode.Keypad1))
//				{
//					value -= 0.01f;
//				}
//
//				if(Input.GetKey(KeyCode.Keypad2))
//				{
//					value -= 0.1f;
//				}
//
//				if(Input.GetKey(KeyCode.Keypad3))
//				{
//					value -= 1.0f;
//				}
//
//				if(Input.GetKey(KeyCode.Keypad4))
//				{
//					value -= 10.0f;					
//				}
//
//				if(Input.GetKey(KeyCode.Keypad5))
//				{
//					value -= 100.0f;					
//				}
//			}
//
//			values[key] = value.ToString();
//			string json = JsonConvert.SerializeObject(values);
//			SettingsValues v = JsonConvert.DeserializeObject<SettingsValues>(json);
//			Settings.Values = (SettingsValues)v.Clone();
//		}
	}

	new public void OnDestroy () 
	{
		Settings.SaveSettings();
		base.OnDestroy();
	}
}

[System.Serializable]
public class SettingsValues : ICloneable
{
	public float drag = 5.0f; 
	public float randomForce = 1000.0f;	
	public float debug = 0.0f;
	public float molScale = 20.0f;
	public float molCount = 333.0f;

	public float timeout = 10000.0f;
	public float repeat = 1.0f;

	// First technique
	public float amplitude_1 = 50;
	public float amplitude_2 = 75;
	public float amplitude_3 = 100;	
	public float amplitude_4 = 100;

	// Second technique
	public float waveLength_1 = 2000;
	public float waveLength_2 = 1000;
	public float waveLength_3 = 500;	
	public float waveLength_4 = 500;
	public float waveLength_5 = 500;

	public object Clone()
	{
		return this.MemberwiseClone();
	}
}

//public class CustomValues: SettingsValues
//{
//	public float drag = 5.0f; 
//	public float randomForce = 50.0f;
//
//	public float boxSizeX = 100.0f;
//	public float boxSizeY = 18.0f;
//	public float boxSizeZ = 1.0f;
//}