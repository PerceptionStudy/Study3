using UnityEngine;
using System;
using System.Diagnostics;

public class MolObject : MonoBehaviour
{
	public bool animate = true;
	public bool hint = false; 

	public int intensity;

	private bool up = true;
	private bool stimulus = false;	
	private bool firstWave = false;

	private int halfWaveLength = 0;
	private int amplitude = 0;

	private MolColor defaultColor;
	private MolColor currentColor; 

	public int colorIndex; 

	private Stopwatch stopWatch = new Stopwatch ();	

	private Stimulus currentStimlus;

	public static MolObject CreateNewMolObject (Transform parent, string name, MolColor color, int colorIndex)
	{
		var position = new Vector3 ((UnityEngine.Random.value - 0.5f) * MainScript.BoxSize.x, (UnityEngine.Random.value - 0.5f) * MainScript.BoxSize.y, (UnityEngine.Random.value - 0.5f) * MainScript.BoxSize.z);
		var molGameObject = Instantiate (Resources.Load ("MolPrefab"), position, Quaternion.identity) as GameObject;

		if (molGameObject != null) 
		{
			molGameObject.name = name;
			molGameObject.transform.parent = parent;

			var molObject = molGameObject.GetComponent<MolObject> ();

			molObject.defaultColor = color; 
			molObject.currentColor = color; 

			molObject.colorIndex = colorIndex; 

			molGameObject.GetComponent<MeshRenderer> ().material.color = color.rgba;



			return molObject;
		}
		return null;
	}

	void Start ()
	{

	}

	void FixedUpdate ()
	{
		if (!MainScript.Animate)
		{
			rigidbody.angularDrag = 10000;
			rigidbody.drag = 10000;
			return;
		}

		Vector3 force = UnityEngine.Random.insideUnitSphere * Settings.Values.randomForce;
		force.z = 0;

		rigidbody.AddForce (force);		
		rigidbody.drag = Settings.Values.drag;
	}

	public void RestartStimulus()
	{
		stimulus = true;
		firstWave = true;
		
		stopWatch.Reset ();
		stopWatch.Start ();
	}
	
	public void StartStimulus(int waveLength, int amplitude)
	{
		stimulus = true;
		firstWave = true;
		
		this.halfWaveLength = waveLength / 2;
		this.amplitude = amplitude;
		
		stopWatch.Reset ();
		stopWatch.Start ();
	}
	
	public void StopStimulus()
	{
		stimulus = false; 
		firstWave = false;
		
		currentColor = defaultColor; 
		gameObject.GetComponent<MeshRenderer> ().material.color = currentColor.rgba; 
		
		stopWatch.Stop();
		stopWatch.Reset();
	}
	
	private void StimulusUpdate()
	{
		int currentWaveTime = (int)stopWatch.ElapsedMilliseconds;
		
		if(firstWave)
		{
			currentWaveTime += halfWaveLength / 2;
		}

		int halfAmplitude = amplitude / 2;
		float progress = (float) currentWaveTime / halfWaveLength;	
		float intensityShift = Mathf.Clamp((up) ? progress * 2.0f - 1.0f : (1.0f-progress) * 2.0f - 1.0f, -1.0f, 1.0f) * (float)halfAmplitude;

		float currentIntensity = intensityShift + defaultColor.L; // = Mathf.Clamp(defaultColor.L + intensityShift, 0, 100);		

		int deltaUp = (int)defaultColor.L + halfAmplitude;
		int deltaDown = (int)defaultColor.L - halfAmplitude;

		if (deltaUp > 100)
		{
			currentIntensity -= deltaUp - 100;
		}
		else if (deltaDown < 0)
		{
			currentIntensity += Math.Abs(deltaDown);
		}

		currentIntensity = Mathf.Clamp (currentIntensity, 0.0f, 100.0f);
		currentColor = new MolColor(currentIntensity, defaultColor.a, defaultColor.b);

		gameObject.GetComponent<MeshRenderer> ().material.color = currentColor.rgba;
		
		if(currentWaveTime > halfWaveLength)
		{
			up = !up;
			stopWatch.Reset();
			stopWatch.Start();
			firstWave = false;
		}
	}

	int frameCount = 0;
	Vector3 lastPos = new Vector3();
	float speedAcc = 0; 

	void Update ()
	{
		intensity = (int)currentColor.L;

		if(stimulus) StimulusUpdate();

		if(hint){
			gameObject.GetComponent<MeshRenderer> ().material.color = Color.white; 
		}
		else if(!stimulus){
			gameObject.GetComponent<MeshRenderer> ().material.color = currentColor.rgba;
		}

		transform.localScale = new Vector3(Settings.Values.molScale, Settings.Values.molScale, Settings.Values.molScale);

		Vector3 temp = rigidbody.position;
		
		if(rigidbody.position.x > transform.parent.position.x + MainScript.BoxSize.x * 0.5f)
		{
			temp.x = transform.parent.position.x + MainScript.BoxSize.x * 0.5f - gameObject.GetComponent<SphereCollider>().radius * 2; 
		}
		else if(rigidbody.position.x < transform.parent.position.x - MainScript.BoxSize.x * 0.5f)
		{
			temp.x = transform.parent.position.x - MainScript.BoxSize.x * 0.5f + gameObject.GetComponent<SphereCollider>().radius * 2; 
		}
		
		if(rigidbody.position.y > transform.parent.position.y + MainScript.BoxSize.y * 0.5f)
		{
			temp.y = transform.parent.position.y + MainScript.BoxSize.y * 0.5f - gameObject.GetComponent<SphereCollider>().radius * 2; 
		}		
		else if(rigidbody.position.y < transform.parent.position.y - MainScript.BoxSize.y * 0.5f)
		{
			temp.y = transform.parent.position.y - MainScript.BoxSize.y * 0.5f + gameObject.GetComponent<SphereCollider>().radius * 2; 
		}

		temp.z = 0;
		
//		if(rigidbody.position.z > transform.parent.position.z + MainScript.BoxSize.z * 0.5f)
//		{
//			temp.z = transform.parent.position.z + MainScript.BoxSize.z * 0.5f - gameObject.GetComponent<SphereCollider>().radius * 2; 
//		}		
//		else if(rigidbody.position.z < transform.parent.position.z - MainScript.BoxSize.z * 0.5f)
//		{
//			temp.z = transform.parent.position.z - MainScript.BoxSize.z * 0.5f + gameObject.GetComponent<SphereCollider>().radius * 2; 
//		}
		
		rigidbody.position = temp; 

		if (!animate)
		{
			speedAcc += Vector3.Distance(transform.position, lastPos);
			frameCount ++;

			if(frameCount > 20)
			{
				print (speedAcc / (float)frameCount);
				speedAcc = 0;
				frameCount = 0;
			}

			lastPos = transform.position;
		}
	}
	
//	void LuminanceFlickerUpdate()
//	{		
//		if(!stopWatch_2.IsRunning) stopWatch_2.Start();
//
//		int currentTimeMillis = (int)stopWatch.ElapsedMilliseconds;
//		float progress_1 = Mathf.Clamp((float)currentTimeMillis / Settings.Values.interpolationDuration, 0.0f, 1.0f);
//		
//		float currentHalfWaveLength = Settings.Values.startHalfWaveLength + (Settings.Values.endHalfWaveLength - Settings.Values.startHalfWaveLength) * progress_1;
//		float currentAmplitude = Settings.Values.startAmplitude + (Settings.Values.endAmplitude - Settings.Values.startAmplitude) * progress_1;		
//		
//		int currentWaveTime = (int)stopWatch_2.ElapsedMilliseconds;
//		float progress_2 = (float) currentWaveTime / currentHalfWaveLength;	
//
//		float intensityShift = Mathf.Clamp((up) ? progress_2 * 2.0f - 1.0f : (1.0f-progress_2) * 2.0f - 1.0f, -1.0f, 1.0f) * currentAmplitude;
//		float currentIntensity = Mathf.Clamp(intensityShift + Settings.Values.amplitudeOffset + defaultColor.L, 0, 100);		
//
//		currentColor = new MolColor(currentIntensity, defaultColor.a, defaultColor.b);
//		gameObject.GetComponent<MeshRenderer> ().material.color = currentColor.rgba;
//
//		if(currentWaveTime > currentHalfWaveLength)
//		{
//			up = !up;
//			stopWatch_2.Reset();
//			stopWatch_2.Start();
//		}
//
//		if(currentTimeMillis > Settings.Values.totalDuration)
//		{
//			StopLuminanceFlicker();
//		}
//	}
}