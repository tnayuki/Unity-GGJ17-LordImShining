using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage : ScriptableObject {
	[System.Serializable]
	public class DrumSet {
		public bool hihat;
		public bool snare;
		public bool perc;
		public bool kick;
	}

	//public float requiredScore;
	public int megaFlashSpeed;
	public int megaFlashDimmer;
	public DrumSet[] drums;
}
