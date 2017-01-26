using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu( menuName = "ScriptableObject/Song", fileName="Song") ]
public class Song : ScriptableObject {
	public float bpm;
	public string title;
	public AnimationCurve flashSpeedCurve;
}
