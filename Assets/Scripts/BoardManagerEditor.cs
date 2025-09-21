using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoardGenerator))]
public class BoardGeneratorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector(); // Draws the default Inspector fields

		BoardGenerator myScript = (BoardGenerator)target;

		if (GUILayout.Button("Generate board"))
		{
			myScript.Initialize();
			myScript.GenerateBoard();
		}
	}
}
