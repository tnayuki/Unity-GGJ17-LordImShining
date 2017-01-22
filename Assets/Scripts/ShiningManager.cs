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

	public Stage[] stages;

	public bool orpheStep;

	public int stageNumber = 0;
	public int noteIndex = 0;
	public float score = 0f;
	public int missed = 0;

	private OSCClient orpheOSCClient;
	private OSCClient openDMXUSBOSCClient;
	private float elapsedTimeAfterLastNote;
	private float timeToNextNote;
	private int quarterNoteCount;

	private float timeSinceStageStart;
	private List<float> sampleTimeArray;
	private List<float> stepTimeArray;

	void Start () {
		orpheOSCClient = new OSCClient ("localhost", 4321);
		orpheOSCClient.SendSimpleMessage ("/BOTH/setLightOff", 0);

		openDMXUSBOSCClient = new OSCClient ("localhost", 7770);

		timeToNextNote = 60f / bpm;

		StartStage ();
		Beat ();
	}

	void Update () {
		elapsedTimeAfterLastNote += Time.deltaTime;

		CaptureOrphe ();
		Beat ();
	}

	private void StartStage() {
		noteIndex = 0;

		openDMXUSBOSCClient.SendSimpleMessage ("/dmx/universe/0", new byte[] { (byte)stages[stageNumber].megaFlashSpeed, (byte)stages[stageNumber].megaFlashDimmer });

		sampleTimeArray = new List<float> ();
		stepTimeArray = new List<float> ();

		float time = Time.timeSinceLevelLoad;
		for (int i = 0; i < stages [stageNumber].drums.Length; i++) {
			Stage.DrumSet drumSet = stages [stageNumber].drums[i];

			if (drumSet.kick/* || drumSet.snare*/) {
				sampleTimeArray.Add (time);
			}
				
			time += 60f / bpm * 4 / 8;
		}
	}

	private void Beat() {
		if (elapsedTimeAfterLastNote > timeToNextNote) {
			elapsedTimeAfterLastNote -= timeToNextNote;

			if (noteIndex == 0) {
				JudgeStage ();

				StartStage ();
			}
				
			Stage.DrumSet drumSet = stages [stageNumber].drums[noteIndex];
			if (drumSet.hihat) hihatAudioSource.Play ();
			if (drumSet.snare) snareAudioSource.Play ();
			if (drumSet.perc) percAudioSource.Play ();
			if (drumSet.kick) kickAudioSource.Play ();

			if (noteIndex % 2 == 0) {
				orpheOSCClient.SendSimpleMessage ("/BOTH/triggerLightWithRGBColor", 1, quarterNoteCount == 0 || quarterNoteCount == 3 ? 255 : 0, quarterNoteCount == 1 || quarterNoteCount == 3 ? 255 : 0, quarterNoteCount == 2 || quarterNoteCount == 3 ? 255 : 0);

				quarterNoteCount = (quarterNoteCount + 1) % 4;
			}

			noteIndex = (noteIndex + 1) % stages [stageNumber].drums.Length;
			timeToNextNote = 60f / bpm * 4 / 8;
		}
	}

	private void CaptureOrphe () {
		if (OscMaster.HasData("/LEFT/gesture")) {
			object[] data = OscMaster.GetData("/LEFT/gesture");

			if (data [0].Equals ("STEP")) {
				orpheStep = true;
			}

			OscMaster.ClearData("/LEFT/gesture");
		}

		if (OscMaster.HasData("/RIGHT/gesture")) {
			object[] data = OscMaster.GetData("/RIGHT/gesture");

			if (data [0].Equals ("STEP")) {
				orpheStep = true;
			}

			OscMaster.ClearData("/RIGHT/gesture");
		}

		if (orpheStep) {
			Debug.Log ("Detect a step @" + Time.timeSinceLevelLoad) ;
			stepTimeArray.Add (Time.timeSinceLevelLoad);
		}

		orpheStep = false;
	}

	private void JudgeStage() {
		score = 0f;

		if (sampleTimeArray.Count != stepTimeArray.Count) {
			missed++;
		} else {
			for (int i = 0; i < sampleTimeArray.Count; i++) {
				score += (60f / bpm) - Mathf.Abs (sampleTimeArray [i] - stepTimeArray [i]);
			}

			if (score < 60f / bpm * 4) {
				missed--;
			} else {
				missed++;
			}
		}
			
		if (missed == 3) {
			stageNumber = stageNumber - 1 < 0 ? 0 : stageNumber - 1;
			missed = 0;
		} else if (missed == -3) {
			stageNumber = stageNumber + 1 >= stages.Length ? stages.Length - 1 : stageNumber + 1;
			missed = 0;
		}
	}
}
