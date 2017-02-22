using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(Tile))]

public class TileInput : Editor
{

	// Use this for initialization
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    void OnSceneGUI()
    {
        int controlID = GUIUtility.GetControlID(FocusType.Passive);

        Tile t = target as Tile;

        if (Event.current.type == EventType.keyDown)
        {
            switch (Event.current.keyCode)
            {
                case KeyCode.A:
                    t.grow(DIRECTION.LEFT);
                    break;
                case KeyCode.D:
                    t.grow(DIRECTION.RIGHT);
                    break;
                case KeyCode.W:
                    t.grow(DIRECTION.UP);
                    break;
                case KeyCode.S:
                    t.grow(DIRECTION.DOWN);
                    break;
                case KeyCode.R:
                    t.reset();
                    break;
                case KeyCode.G:
                    t.saveAsDefault();
                    break;
                case KeyCode.T:
                    Selection.activeGameObject = GameObject.FindObjectOfType<TileSpawner>().gameObject;
                    break;
            }
        }

        if (Event.current.type == EventType.layout) HandleUtility.AddDefaultControl(controlID);
    }
}
