using UnityEditor;
using UnityEngine;
using System.Collections;


[CustomEditor(typeof(TileSpawner))]

public class TileSpawnerInput : Editor
{
    SerializedProperty platform;
	// Use this for initialization
	void Start ()
    {
	
	}
	
	void OnEnable()
    {
        platform = serializedObject.FindProperty("platform");
    }

    void OnSceneGUI()
    {
        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        if (Event.current.type == EventType.layout) HandleUtility.AddDefaultControl(controlID);

        if (Event.current.type == EventType.keyDown)
        {
            if(Event.current.keyCode == KeyCode.S)
            {
                TileSpawner t = target as TileSpawner;
                Selection.activeGameObject = t.getLastSpawned();
            }
        }


            if (Event.current.type == EventType.mouseDown && Event.current.button == 0)
        {

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            TileSpawner t = target as TileSpawner;
            if(Tile.checkForTile(ray.origin) == null)
                t.instantiate(ray.origin);
        }
        if (Event.current.type == EventType.layout) HandleUtility.AddDefaultControl(controlID);
    }
}
