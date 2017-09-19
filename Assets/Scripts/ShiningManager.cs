using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using OscJack;

public class ShiningManager : MonoBehaviour {
	public AudioSource hihatAudioSource;
	public AudioSource snareAudioSource;
	public AudioSource percAudioSource;
	public AudioSource kickAudioSource;

	public float bpm = 130.0f;
	public float score = 0f;
	public AnimationCurve flashSpeedCurve;

	public bool areStepsContinuous;

	private OSCClient orpheOSCClient;
	private OSCClient openDMXUSBOSCClient;

	private float elapsedTimeAfterLastBeat;
	private float timeToNextBeat;
	private int beatCount;

	private List<float> stepTimeArray = new List<float>();

	void Start () {
		orpheOSCClient = new OSCClient ("localhost", 4321);
		orpheOSCClient.SendSimpleMessage ("/BOTH/setLightOff", 0);

		openDMXUSBOSCClient = new OSCClient ("localhost", 7770);

		timeToNextBeat = 60f / bpm;
	}

	void Update () {
		elapsedTimeAfterLastBeat += Time.deltaTime;

		CaptureOrphe ();

		if (elapsedTimeAfterLastBeat > timeToNextBeat) {
			elapsedTimeAfterLastBeat -= timeToNextBeat;

			beatCount = (beatCount + 1) % 4;

//			if (hihat || snare || perc || kick) {
//				kickAudioSource.Play ();
//
//				hihat = snare = perc = kick = false;
//			}
			if (areStepsContinuous) {
				score += 0.025f;
				if (score > 1.0f) {
					score = 1.0f;
				}

				if (score > 0.6f) {
					Debug.Log (4);
					kickAudioSource.Play ();
					snareAudioSource.Play ();
					hihatAudioSource.Play ();

					orpheOSCClient.SendSimpleMessage ("/BOTH/triggerLightWithRGBColor", 1, 255, 255, 255);
				} else if (score > 0.4f) {
					Debug.Log (3);
					kickAudioSource.Play ();
					hihatAudioSource.Play ();
					percAudioSource.Play ();

					orpheOSCClient.SendSimpleMessage ("/BOTH/triggerLightWithRGBColor", 1, 255, 0, 0);
				} else if (score > 0.2f) {
					Debug.Log (2);
					kickAudioSource.Play ();
					hihatAudioSource.Play ();

					orpheOSCClient.SendSimpleMessage ("/BOTH/triggerLightWithRGBColor", 1, 0, 255, 0);
				} else if (score > 0.0f) {
					Debug.Log (1);
					kickAudioSource.Play ();
					orpheOSCClient.SendSimpleMessage ("/BOTH/triggerLightWithRGBColor", 1, 0, 0, 255);
				}
			} else {
				score -= 0.15f;
				if (score < 0.0f) {
					score = 0.0f;
				}
			}

			TransVibrator.VibrateAll (1.0f);
			Invoke ("StopTransVibrator", 0.1f);

			areStepsContinuous = false;
			timeToNextBeat = 60f / bpm;
		}

		float flashSpeed = flashSpeedCurve.Evaluate (score);
		if (flashSpeed > 1.0f) {
			flashSpeed = 1.0f;
		}
		Debug.Log (flashSpeed);
		openDMXUSBOSCClient.SendSimpleMessage ("/dmx/universe/0", new byte[] { (byte)(flashSpeed * 255.0f), (byte)(score == 0 ? 0 : 255 )});
	}

	private void CaptureOrphe () {
		foreach (string address in new string[] { "/LEFT/gesture", "/RIGHT/gesture" }) {
			if (OscMaster.HasData(address)) {
				object[] data = OscMaster.GetData(address);

				areStepsContinuous = true;
				/*
				if (data [1].Equals ("LEFT") || data [1].Equals ("RIGHT")) {
					areStepsContinuous = 0;

				} else if (data [1].Equals ("CENTER")) {
					stepComplexity++;
				} else if (data [1].Equals ("BACK") || data [1].Equals ("FRONT")) {
					stepComplexity++;
				}
				*/
				OscMaster.ClearData(address);
			}
		}
	}

	private void StopTransVibrator() {
		TransVibrator.VibrateAll (0.0f);
	}
}
