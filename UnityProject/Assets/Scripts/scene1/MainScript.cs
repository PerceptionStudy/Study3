using UnityEngine;
using Newtonsoft.Json;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System; 
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text; 
using System.Text.RegularExpressions;

public class Stimulus
{
	public int stimulusId;
	public int repeatId;
	public int amplitude;	
	public int waveLength;

	public Stimulus(int stimulusId, int repeatId, int waveLength, int amplitude)
	{
		this.stimulusId = stimulusId;
		this.repeatId = repeatId;
		this.waveLength = waveLength;		
		this.amplitude = amplitude;
	}

	public override string ToString()
	{
		return "Stimulus: " + stimulusId + "; repetition: " + repeatId + "; wave length: " + waveLength + "; amplitude: " + amplitude; 
	}
}

public class MainScript : MonoBehaviour 
{
	private int currentStimulusIndex = 0;	

	private Stimulus currentStimulus = null;
	private MolObject stimulusObject = null; 

	private bool setupGUI = true; 
	private bool countdown = false; 
	private bool intermediate = false; 
	private bool stimulus = false; 
	private bool stimulusEnd = false; 	
	private bool rating = false; 

	private MolObject[] molObjects;
	private GameObject collideBox;
	private List<Stimulus> stimuli = new List<Stimulus>();

	public static Color[] MolColors = {
		new Color(166,206,227), 
		new Color(31,120,180), 
		new Color(178,223,138), 
		new Color(51,160,44), 
		new Color(251,154,153), 
		new Color(227,26,28), 
		new Color(253,191,111), 
		new Color(255,127,0), 
		new Color(202,178,214), 
		new Color(106,61,154), 
		new Color(255,255,153), 
		new Color(177,89,40)
	}; 

	public static Vector3 BoxSize = new Vector3();
	public static bool Animate = false;

	private string userID = "default"; 
	private string conditionID = ""; 

	private Stopwatch stopWatch = new Stopwatch ();

	LogLib.Logger<int> distLogger; 
	LogLib.Logger<int> timeLogger; 
	LogLib.Logger<int> targetLogger; 
	LogLib.Logger<int> colorLogger; 
	LogLib.Logger<int> annoyanceLogger; 

	public void CreateMolObjects()
	{
		// Destroy previous game objects
		if(molObjects != null)
		{
			foreach(MolObject obj in molObjects)
			{
				Destroy(obj.gameObject);
			}
		}

		molObjects = new MolObject[(int)Settings.Values.molCount];		

//		for(int i = 0; i< (int)Settings.Values.molCount; i++)
//			molObjects[i] = MolObject.CreateNewMolObject(gameObject.transform, "object_" + i, new MolColor(MolColors[UnityEngine.Random.Range(0, MolColors.Count ())]));

		for(int i = 0; i< (int)Settings.Values.molCount; i++){
			int colorIndex = i%MolColors.Count (); 
			molObjects[i] = MolObject.CreateNewMolObject(gameObject.transform, "object_" + i, new MolColor(MolColors[colorIndex]), colorIndex);
		}

		stimulusObject = null; 
	}

	void LoadStimuli ()
	{
		stimuli.Clear();

		int[] amplitudeValues = {0, (int)Settings.Values.amplitude_1, (int)Settings.Values.amplitude_2, (int)Settings.Values.amplitude_3, (int)Settings.Values.amplitude_4};
		int[] waveLengthValues = {(int)Settings.Values.waveLength_1, (int)Settings.Values.waveLength_2, (int)Settings.Values.waveLength_3, (int)Settings.Values.waveLength_4, (int)Settings.Values.waveLength_5};

		int count = 0;

		for(int i = 0; i <= Settings.Values.repeat; i++)
		{
			for(int j = 0; j < waveLengthValues.Count(); j++)
			{
				for(int k = 0; k < amplitudeValues.Count(); k++)
				{
					Stimulus stimulus = new Stimulus(count, i, waveLengthValues[j], amplitudeValues[k]);
					stimuli.Add(stimulus);
					
					count ++;
				}		
			}
		}

		var shuffle = (from stimulus in stimuli orderby Guid.NewGuid() select stimulus);
		stimuli = shuffle.ToList();

		Stimulus[] dbg = shuffle.ToArray ();

		currentStimulusIndex = 0;
	}

	void LoadScene ()
	{
		LoadStimuli();		
		CreateMolObjects();
		
		setupGUI = true; 
		countdown = false; 
		intermediate = false; 
		stimulus = false; 
		stimulusEnd = false; 
	}
	
	void Start () 
	{
		BoxSize.x = Screen.width;
		BoxSize.y = Screen.height;
		BoxSize.z = Settings.Values.molScale * 2;
		
		collideBox = GameObject.Find("Box Object");
		collideBox.transform.localScale = new Vector3(BoxSize.x, BoxSize.y, BoxSize.z);
		
		Settings.LoadSettings();
		
		LoadScene();
	}

	private bool showHud = false;
	private const int guiTopOffset = 20;
	private const int guiDownOffset = 10;
	private const int guiIncrement = 15;
	private GUIStyle style = new GUIStyle();
	private Rect windowRect = new Rect(20, 20, 225, 0);
	private Dictionary<string, string> tempSettings = new Dictionary<string, string>();

	void OnGUI()
	{
		Color newColor = new Color(1,1,1,1.0f);
		
		GUI.color = newColor;

		if(showHud)
		{
			windowRect = GUI.Window(0, windowRect, DoMyWindow, "My Window");
		}

		if(setupGUI)
		{
			GUI.Window (1, new Rect(0.0f, 0.0f, Screen.width, Screen.height), SetupGUI, "Setup"); 
		}
		
		if(intermediate)
		{
			GUI.Window (2, new Rect(0.0f, 0.0f, Screen.width, Screen.height), IntermediateGUI, "Intermediate"); 
		}

		if(stimulusEnd && rating)
		{
			GUI.DrawTexture (new Rect (0.0f, 0.0f, Screen.width, Screen.height), (UnityEngine.Texture)Resources.Load ("rateBG")); 
			//GUI.Window (5, new Rect(0.0f, 0.0f, Screen.width, Screen.height), RatingGUI, "Rate the annoyance from 1 to 5"); 
		}

		if(countdown)
		{
			GUI.Window (3, new Rect(0.0f, 0.0f, Screen.width, Screen.height), CountdownGUI, "Countdown"); 
		}
		
		if(stimulusEnd && !rating)
		{
			GUI.DrawTexture (new Rect (0.0f, 0.0f, Screen.width, Screen.height), (UnityEngine.Texture)Resources.Load ("stimulusEnd")); 
			//GUI.Window (4, new Rect(0.0f, 0.0f, Screen.width, Screen.height), StimulusEndGUI, "Click at the target or press 'n'"); 
		}
	}

	void DoMyWindow(int windowID) 
	{
		style.fontSize = 12;
		style.normal.textColor = Color.white;
		
		if(tempSettings.Count() == 0)
		{
			tempSettings = Settings.GetDictionarySettings();
		}
		
		int count = 0;
		
		foreach( KeyValuePair<string, string> kvp in new Dictionary<string, string>(tempSettings) )
		{
			GUI.Label(new Rect(10, guiTopOffset + count * guiIncrement, 150, 30), kvp.Key + ": ", style);
			string stringValue = GUI.TextField(new Rect(175, guiTopOffset + count * guiIncrement, 50, 20), kvp.Value.ToString(), style);
			stringValue = Regex.Replace(stringValue, @"[^0-9.]", "");
			
			if(stringValue != kvp.Value)
			{
				tempSettings[kvp.Key] = stringValue;
			}
			
			if (Event.current.isKey && Event.current.keyCode == KeyCode.Return)		
			{				
				float tryParse = 0.0f;
				
				if(float.TryParse(stringValue, out tryParse))
				{
					tempSettings[kvp.Key] = tryParse.ToString();
				}
				else
				{
					print("Input field parsing failed");
					tempSettings[kvp.Key] = "0";
				}
			}
			
			count ++;
		}
		
		if (Event.current.isKey && Event.current.keyCode == KeyCode.Return)		
		{				
			print("Applying and Saving Settings");
			
			string json = JsonConvert.SerializeObject(tempSettings);
			SettingsValues v = JsonConvert.DeserializeObject<SettingsValues>(json);
			Settings.Values = (SettingsValues)v.Clone();
			Settings.SaveSettings();
		}
		
		windowRect.height = guiTopOffset + guiDownOffset + count * guiIncrement;
		
		GUI.DragWindow(new Rect(0, 0, 10000, 10000));
	}

	void StimulusEndGUI(int windowID)
	{
		GUI.DrawTexture (new Rect (0.0f, 0.0f, Screen.width, Screen.height), (UnityEngine.Texture)Resources.Load ("stimulusEnd")); 
	}

	void RatingGUI(int windowID)
	{
		GUI.DrawTexture (new Rect (0.0f, 0.0f, Screen.width, Screen.height), (UnityEngine.Texture)Resources.Load ("rateBG")); 
	}


	void CountdownGUI(int windowID)
	{
		int time = (int)stopWatch.ElapsedMilliseconds;

		if(time >= 2000)
		{
			countdown = false; 

			StartNewStimulus();

			stopWatch.Stop (); 
			stopWatch.Reset (); 
			stopWatch.Start (); 
		}

		if (countdown)
		{
			string texName = "1"; 
			if(time < 1000) texName = "2";
			//		if(time < 1000) texName = "3";
			GUI.DrawTexture (new Rect (0.0f, 0.0f, Screen.width, Screen.height), (UnityEngine.Texture)Resources.Load (texName)); 
		}

	}

	void IntermediateGUI(int windowID)
	{
		GUI.DrawTexture (new Rect (0.0f, 0.0f, Screen.width, Screen.height), (UnityEngine.Texture)Resources.Load ("intermediate")); 
	}

	void SetupGUI(int windowID)
	{
		const float elemWidth = 150.0f; 
		const float elemHeight = 20.0f; 

		float xOffset = (Screen.width / 2.0f) - 150; 
		float yOffset = (Screen.height / 2.0f) - 50; 

		GUI.DrawTexture (new Rect (0.0f, 0.0f, Screen.width, Screen.height), (UnityEngine.Texture)Resources.Load ("guiBG")); 

		GUI.Label (new Rect (xOffset, yOffset, elemWidth, elemHeight), "User ID: "); 
		userID = GUI.TextField (new Rect (xOffset + 100, yOffset, elemWidth, elemHeight), userID); 

		GUI.Label (new Rect (xOffset, yOffset + elemHeight + 5, elemWidth, elemHeight), "Condition: "); 
		conditionID = GUI.TextField (new Rect (xOffset + 100, yOffset + elemHeight + 5, elemWidth, elemHeight), conditionID); 

		if(GUI.Button (new Rect(xOffset + 100, yOffset + 2 * elemHeight + 10, elemWidth, elemHeight), "Start!") || (Event.current.isKey && Event.current.keyCode == KeyCode.Return))
		{
			if(userID.Length > 0)
			{
				distLogger = CreateLogger ("dist"); 
				targetLogger = CreateLogger ("target"); 
				timeLogger = CreateLogger ("time"); 
				colorLogger = CreateLogger ("color"); 
				annoyanceLogger = CreateLogger ("annoyance"); 
				setupGUI = false; 
				intermediate = true; 
			}
		}
	}

	LogLib.Logger<int> CreateLogger(String name)
	{
		LogLib.Logger<int> logger = new LogLib.Logger<int> (name, userID, conditionID); 
		// TODO: hardcoded for pilot1
		logger.AddFactor ("rep"); 
		logger.AddFactor ("amp"); 
		logger.AddFactor ("wav"); 
		return logger; 
	}

	void FiniLogger(LogLib.Logger<int> logger, String name)
	{
		string fileName = name + ".csv"; 
		StreamWriter fileWriter = new StreamWriter (fileName, true); 
		bool writeHeader = (new FileInfo(fileName).Length == 0); 
		logger.WriteSingleRowCSV(fileWriter, writeHeader);
	}

	void Log(LogLib.Logger<int> logger, Stimulus stimulus, int value)
	{
		logger.NewEntry(); 
		logger.Log ("rep", stimulus.repeatId.ToString()); 
		logger.Log ("amp", stimulus.amplitude.ToString ()); 
		logger.Log ("wav", stimulus.waveLength.ToString ()); 
		logger.Log (value); 
	}

	bool targetFound = false; 
	int dist = 0;
	Vector3 mousePos = new Vector3(-1.0f, -1.0f, -1.0f); 

	void Update () 
	{
		Camera.main.orthographicSize = Screen.height * 0.5f;
		
		BoxSize.x = Screen.width;
		BoxSize.y = Screen.height;
		BoxSize.z = Settings.Values.molScale * 2;
		
		collideBox.transform.localScale = new Vector3(BoxSize.x, BoxSize.y, BoxSize.z);

		int elapsedTime = (int)stopWatch.ElapsedMilliseconds; 

		if(stimulus)
		{
			if(Input.GetKeyDown("t")) 
			   stimulusObject.hint = true; 

			if(Input.GetKeyUp ("t"))
				stimulusObject.hint = false; 

			if(elapsedTime >= Settings.Values.timeout || Input.GetKeyDown("space"))
			{
				StopStimulus(); 

				if(elapsedTime < Settings.Values.timeout)
					targetFound = true; 
			}
		}

		bool stimulusEnd2 = true;

		if(stimulusEnd && !rating)
		{
			Animate = false;

			targetFound = false; 
			
			if(Input.GetMouseButtonUp(0))
			{
				mousePos = Input.mousePosition; 
				mousePos.x -= Screen.width / 2; 
				mousePos.y -= Screen.height / 2; 

				dist = (int)(Vector3.Distance(stimulusObject.transform.position, mousePos));

				rating = true;
				targetFound = true; 
				Animate = true;

				stimulusObject.RestartStimulus();
			}
			if(Input.GetKeyDown ("n"))
			{
				stimulusEnd2 = false; 
			}
		}

		int rate = -1;

		if(stimulusEnd && rating)
		{
			if(Input.GetKeyDown(KeyCode.Keypad1)) 
			{
				rate = 1;
				rating = false;
				stimulusEnd2 = false;
				Animate = false;
				stimulusObject.StopStimulus();
			}
			else if (Input.GetKeyDown(KeyCode.Keypad2)) 
			{
				rate = 2;

				rating = false;
				stimulusEnd2 = false;
				Animate = false;
				stimulusObject.StopStimulus();
			}
			else if (Input.GetKeyDown(KeyCode.Keypad3)) 
			{
				rate = 3;

				rating = false;
				stimulusEnd2 = false;
				Animate = false;
				stimulusObject.StopStimulus();
			}
			else if (Input.GetKeyDown(KeyCode.Keypad4)) 
			{
				rate = 4;

				rating = false;
				stimulusEnd2 = false;
				Animate = false;
				stimulusObject.StopStimulus();

			}
			else if (Input.GetKeyDown(KeyCode.Keypad5)) 
			{
				rate = 5;

				rating = false;
				stimulusEnd2 = false;
				Animate = false;
				stimulusObject.StopStimulus();
			}
		}

		if(!stimulusEnd2)
		{

			
			Log (distLogger, currentStimulus, dist); 
			Log (targetLogger, currentStimulus, Convert.ToInt32(targetFound)); 
			Log (timeLogger, currentStimulus, elapsedTime); 
			Log (colorLogger, currentStimulus, stimulusObject.colorIndex); 
			Log (annoyanceLogger, currentStimulus, rate); 

			print(targetFound);
			print(rate);
			print (dist);

			if(currentStimulusIndex >= stimuli.Count())
			{
				FiniLogger(distLogger, "dist");
				FiniLogger(timeLogger, "time");
				FiniLogger(targetLogger, "target"); 
				FiniLogger (colorLogger, "color"); 
				FiniLogger (annoyanceLogger, "annoyance"); 
				
				LoadScene();
			}
			else
			{
				intermediate = true; 
				stimulusEnd = false;
			}
		}

		if (Input.GetKeyDown (KeyCode.R))
		{
			LoadScene();
		}

		if (Input.GetKeyDown (KeyCode.H))
		{
			showHud = !showHud;
		}

		if (Input.GetKeyDown (KeyCode.Escape))
		{
			Application.Quit();
		}

		if(Input.GetKeyDown (KeyCode.Return))
		{
			if(intermediate)
			{
				countdown = true; 
				stopWatch.Reset(); 
				stopWatch.Start (); 
				intermediate = false; 
				
				Screen.showCursor = false; 
			}
		}
	}

	void StartNewStimulus ()
	{
		currentStimulus = stimuli[currentStimulusIndex];

		stimulusObject = molObjects [UnityEngine.Random.Range (0, molObjects.Count () - 1)];
		stimulusObject.StartStimulus (currentStimulus.waveLength, currentStimulus.amplitude);

		currentStimulusIndex ++;

		Animate = true;
		stimulus = true;

		print ("Stimulus: " + currentStimulus); 
	}

	void StopStimulus()
	{
		stopWatch.Stop(); 
		//stopWatch.Reset (); 
		
		stimulusObject.StopStimulus();


		stimulus = false; 
		stimulusEnd = true; 
		
		Screen.showCursor = true; 
	}

	void FixedUpdate()
	{

	}
}