using System;
using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Stage))]
public class StageEditor : Editor {
	bool folding;

	[MenuItem("Assets/Create/Stage")]
	public static void CreateAsset()
	{
		Stage item = ScriptableObject.CreateInstance<Stage>();

		string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Stages/Stage.asset");

		AssetDatabase.CreateAsset(item, path);
		AssetDatabase.SaveAssets();

		EditorUtility.FocusProjectWindow();
		Selection.activeObject = item;
	}

	public override void OnInspectorGUI()
	{
		Stage stage = (Stage)target;

		stage.megaFlashSpeed = EditorGUILayout.IntSlider ("Flash Speed", stage.megaFlashSpeed, 0, 255);
		stage.megaFlashDimmer = EditorGUILayout.IntSlider ("Flash Dimmer", stage.megaFlashDimmer, 0, 255);

		if (folding = EditorGUILayout.Foldout (folding, "Drum Notation")) {
			EditorGUI.BeginChangeCheck ();
			int size = EditorGUILayout.IntField ("Size", stage.drums.Length);

			if (EditorGUI.EndChangeCheck() && size != stage.drums.Length) {
				if (size == 0) {
					stage.drums = new Stage.DrumSet[0];
				} else {
					Stage.DrumSet[] temp = new Stage.DrumSet[size];
					Debug.Log (size.ToString() + "," + stage.drums.Length);
					Array.Copy(stage.drums, 0, temp, 0, size > stage.drums.Length ? stage.drums.Length : size);

					stage.drums = temp;
				}
			}

			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Drum Set", GUILayout.Width (120.0f));
			EditorGUILayout.LabelField ("H", GUILayout.Width (20.0f));
			EditorGUILayout.LabelField ("S", GUILayout.Width (20.0f));
			EditorGUILayout.LabelField ("P", GUILayout.Width (20.0f));
			EditorGUILayout.LabelField ("K", GUILayout.Width (20.0f));
			EditorGUILayout.EndHorizontal ();

			for (int i = 0; i < stage.drums.Length; i++) {
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("", GUILayout.Width (120.0f));
				stage.drums[i].hihat = EditorGUILayout.Toggle (stage.drums[i].hihat, GUILayout.Width (20.0f));
				stage.drums[i].snare = EditorGUILayout.Toggle (stage.drums[i].snare, GUILayout.Width (20.0f));
				stage.drums[i].perc = EditorGUILayout.Toggle (stage.drums[i].perc, GUILayout.Width (20.0f));
				stage.drums[i].kick = EditorGUILayout.Toggle (stage.drums[i].kick, GUILayout.Width (20.0f));
				EditorGUILayout.EndHorizontal ();
			}
		}		
	}
}
