using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using OscJack;

public class ShiningManager : MonoBehaviour {
	public Song song;

	public int numberOfFourBars;
	public float detectedBpm;

	public bool isOrpheStepped;

	public int progress = 0;

	private OSCClient orpheOSCClient;
	private OSCClient openDMXUSBOSCClient;

	private float elapsedTimeAfterLastNote;
	private float timeToNextQuarterNote;
	private int quarterNoteCount;

	private float timeToNextLoopStart;
	private bool isNextLoopScheduled;

	private List<AudioSource> audioSources = new List<AudioSource>();

	public List<float> stepTimeArray = new List<float> ();

	void Start () {
		orpheOSCClient = new OSCClient ("localhost", 4321);
		orpheOSCClient.SendSimpleMessage ("/BOTH/setLightOff", 0);

		openDMXUSBOSCClient = new OSCClient ("localhost", 7770);

		GameObject audioSourcesGameObject = new GameObject ("Song");
		for (int i = 0; ; i++) {
			AudioClip clip = (AudioClip)Resources.Load (string.Format("{0}/{1:000}", song.title , i));

			if (!clip) {
				numberOfFourBars = i;
				break;
			}
			
			AudioSource audioSource = audioSourcesGameObject.AddComponent<AudioSource> ();
			audioSource.clip = clip;
			audioSource.bypassEffects = true;
			audioSources.Add (audioSource);
		}

		audioSources[progress].loop = true;
		audioSources[progress].PlayDelayed (0.5f);

		timeToNextQuarterNote = 0.45f + 60f / song.bpm;
	}

	void Update () {
		CaptureOrphe ();

		if (isOrpheStepped) {
			stepTimeArray.Add (Time.timeSinceLevelLoad);

			isOrpheStepped = false;
		}

		if (stepTimeArray.Count > 0 && Time.timeSinceLevelLoad - 60f / song.bpm * 8 > stepTimeArray [0]) {
			stepTimeArray.RemoveAt (0);
		}

		float detectedBpmSum = 0.0f;
		for (int i = 1; i < stepTimeArray.Count; i++) {
			detectedBpmSum += 60.0f / (stepTimeArray[i] - stepTimeArray[i - 1]);
		}

		detectedBpm = detectedBpmSum / (stepTimeArray.Count - 1);

		if (audioSources[progress].timeSamples > audioSources[progress].clip.samples / 8 * 7 && !isNextLoopScheduled) {
			ulong delay = (ulong)(audioSources[progress].clip.samples - audioSources[progress].timeSamples);

			if (song.bpm * 0.8f < detectedBpm && detectedBpm < song.bpm * 1.2f) {
				audioSources[progress].loop = false;
				progress++;

				audioSources[progress].loop = true;
				audioSources[progress].Play (delay);

				openDMXUSBOSCClient.SendSimpleMessage ("/dmx/universe/0", new byte[] { (byte)(song.flashSpeedCurve.Evaluate((float)progress / numberOfFourBars)* 255.0f), (byte)255 });
			}

			isNextLoopScheduled = true;
		} else if (audioSources[progress].timeSamples < audioSources[progress].clip.samples / 8 * 7) {
			isNextLoopScheduled = false;
		}
			
		elapsedTimeAfterLastNote += Time.deltaTime;

		Beat ();
	}

	void OnDisable() {
		openDMXUSBOSCClient.SendSimpleMessage ("/dmx/universe/0", new byte[] { 0, 0 });
	}

	private void Beat() {
		if (elapsedTimeAfterLastNote > timeToNextQuarterNote) {
			elapsedTimeAfterLastNote -= timeToNextQuarterNote;

			orpheOSCClient.SendSimpleMessage ("/BOTH/triggerLightWithRGBColor", 1, quarterNoteCount == 0 || quarterNoteCount == 3 ? 255 : 0, quarterNoteCount == 1 || quarterNoteCount == 3 ? 255 : 0, quarterNoteCount == 2 || quarterNoteCount == 3 ? 255 : 0);

			quarterNoteCount = (quarterNoteCount + 1) % 4;

			timeToNextQuarterNote = 60f / song.bpm;
		}
	}

	private void CaptureOrphe () {
		if (OscMaster.HasData("/LEFT/gesture")) {
			object[] data = OscMaster.GetData("/LEFT/gesture");

			if (data [0].Equals ("STEP")) {
				isOrpheStepped = true;
			}

			OscMaster.ClearData("/LEFT/gesture");
		}

		if (OscMaster.HasData("/RIGHT/gesture")) {
			object[] data = OscMaster.GetData("/RIGHT/gesture");

			if (data [0].Equals ("STEP")) {
				isOrpheStepped = true;
			}

			OscMaster.ClearData("/RIGHT/gesture");
		}
	}
}
