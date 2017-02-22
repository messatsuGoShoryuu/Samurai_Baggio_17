using UnityEngine;
using System.Collections;
using UnityEditor;

[ExecuteInEditMode]
public class TileSpawner : MonoBehaviour
{
    public Tile tile;
    public Tile m_lastSpawnedObject;
    
	// Use this for initialization
	void Start ()
    {
	
	}

    public static Vector2 getMetrics()
    {
        return GameObject.FindObjectOfType<TileSpawner>().tile.metrics;
    }

    public static Vector2 getOffset()
    {
        return GameObject.FindObjectOfType<TileSpawner>().tile.m_defaultOffset;
    }

    public void instantiate(Vector3 position)
    {
        Vector3 newPos = position;
        newPos.x = Mathf.FloorToInt(position.x / tile.metrics.x) * tile.metrics.x + tile.metrics.x / 2.0f;
        newPos.y = Mathf.FloorToInt(position.y / tile.metrics.y) * tile.metrics.y + tile.metrics.y / 2.0f;
        newPos.z = transform.position.z;

        m_lastSpawnedObject = GameObject.Instantiate(tile, newPos, Quaternion.identity) as Tile;
        m_lastSpawnedObject.transform.parent = this.transform;
    }

    void OnEnable()
    {
        Tile.OnRequestNewTile += Tile_OnRequestNewTile;
    }

    private void Tile_OnRequestNewTile(Vector3 position)
    {
        instantiate(position);
        select(m_lastSpawnedObject.gameObject);
    }

    public static void select(GameObject g)
    {
        Selection.activeGameObject = g;
    }

    void OnDisable()
    {
        Tile.OnRequestNewTile -= Tile_OnRequestNewTile;
    }

    public GameObject getLastSpawned()
    {
        return m_lastSpawnedObject.gameObject;
    }
	
	// Update is called once per frame
	void LateUpdate ()
    {
        Tile.drawTile(Tile.staticCursor, tile.metrics / 2.0f, Color.blue);
    }
}
