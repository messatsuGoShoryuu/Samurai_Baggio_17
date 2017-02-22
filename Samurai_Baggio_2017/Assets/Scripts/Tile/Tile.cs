using UnityEngine;
using System.Collections;

public enum DIRECTION
{
    LEFT,
    RIGHT,
    UP,
    DOWN
}

public enum AXIS
{
    HORIZONTAL,
    VERTICAL
}
[ExecuteInEditMode]
public class Tile : MonoBehaviour
{
    public delegate void RequestNewTile(Vector3 position);
    public static event RequestNewTile OnRequestNewTile;

    public AXIS axis;
    Vector3 m_cursor;
    public Vector2 metrics;
    public Vector2 m_defaultOffset;
    public static Vector3 staticCursor;

    BoxCollider2D m_boxCollider;
	// Use this for initialization
	void Start ()
    {
        m_boxCollider = GetComponent<BoxCollider2D>();
        if(m_boxCollider != null)
        {
            m_cursor = transform.position;
            m_defaultOffset = TileSpawner.getOffset();
            metrics = TileSpawner.getMetrics();
        }
	}

    public void reset()
    {
        if(m_boxCollider != null)
        {
            m_boxCollider.size = metrics;
            m_boxCollider.offset = m_defaultOffset;
            m_cursor = transform.position;
        }
    }

    public static GameObject checkForTile(Vector2 point)
    {  
        RaycastHit2D[] hits = Physics2D.RaycastAll(point,Vector2.zero);
        for(int i =0; i<hits.Length;i++)
        {
            if (hits[i].collider.GetComponent<Tile>() != null)
            {
                Debug.Log("GAMEOBJECT FOUND");
                return hits[i].collider.gameObject;
            }
        }
        Debug.Log("NO GAMEOBJECT FOUND");
        return null;
    }

    public void saveAsDefault()
    {
        metrics = m_boxCollider.size;
        m_defaultOffset = m_boxCollider.offset;
    }
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    AXIS getAxisFromDirection(DIRECTION dir)
    {
        switch(dir)
        {
            case DIRECTION.LEFT:
            case DIRECTION.RIGHT:
                return AXIS.HORIZONTAL;
            case DIRECTION.UP:
            case DIRECTION.DOWN:
                return AXIS.VERTICAL;
            default:return AXIS.HORIZONTAL;
        }
    }

    bool isValidDirection(DIRECTION dir)
    {
        if(m_boxCollider.size == metrics)
        {
            axis = getAxisFromDirection(dir);
            return true;
        }

        return getAxisFromDirection(dir) == axis;
    }

    Vector3 getDirectionOffset(DIRECTION dir)
    {
        switch(dir)
        {
            case DIRECTION.LEFT: return new Vector3(-metrics.x, 0.0f, 0.0f);
            case DIRECTION.RIGHT: return new Vector3(metrics.x, 0.0f, 0.0f);
            case DIRECTION.UP: return new Vector3(0.0f, metrics.y, 0.0f);
            case DIRECTION.DOWN: return new Vector3(0.0f, -metrics.y, 0.0f);
        }
        return Vector3.zero;
    }

    public void grow(DIRECTION dir)
    {
        if(transform.hasChanged)
        {
            reset();
            transform.hasChanged = false;
        }
        Vector3 offset = getDirectionOffset(dir);
        if (isValidDirection(dir))
        {
            GameObject g = checkForTile(m_cursor + offset);
            if (g == null || g == this.gameObject)
            {
                m_cursor += offset;
                updateCollider(offset);
                staticCursor = m_cursor;
            }
            else
            {
                staticCursor = m_cursor + offset;
                TileSpawner.select(g);
                if (m_cursor == transform.position) DestroyImmediate(this.gameObject);
            }
        }
        else if (OnRequestNewTile != null)
        {
            staticCursor += offset;
            OnRequestNewTile(staticCursor);
        }
    }

    public void setCursor(Vector3 point)
    {
        m_cursor = point;
    }

    void updateCollider(Vector2 offset)
    {
        if(m_boxCollider != null)
        {
            if (m_cursor == transform.position)
            {
                m_boxCollider.size = metrics;
                m_boxCollider.offset = m_defaultOffset;
                return;
            }
            m_boxCollider.offset += offset / 2.0f;
            Vector2 newOffset = offset;
            newOffset.x = Mathf.Sign(m_cursor.x - transform.position.x) * offset.x;
            newOffset.y = Mathf.Sign(m_cursor.y - transform.position.y) * offset.y;
            
            Vector2 newSize = m_boxCollider.size + newOffset;
            Debug.Log("cursor = "  + m_cursor + " size = "  + newSize);

            
            
            newSize.x = Mathf.Abs(newSize.x);
            newSize.y = Mathf.Abs(newSize.y);
            Debug.Log(newSize);
            m_boxCollider.size = newSize;
        }
    }

    public static void drawTile(Vector3 center, Vector3 extents, Color color)
    {
        float x = extents.x;
        float y = extents.y;

        Vector3 topLeft = center + new Vector3(-x, y);
        Vector3 topRight = center + new Vector3(x, y);
        Vector3 botRight = center + new Vector3(x, -y);
        Vector3 botLeft = center + new Vector3(-x, -y);

        Debug.DrawLine(topLeft, topRight, color, 0.0f, true);
        Debug.DrawLine(topRight, botRight, color, 0.0f, true);
        Debug.DrawLine(botRight, botLeft, color, 0.0f, true);
        Debug.DrawLine(botLeft, topLeft, color, 0.0f, true);

        Debug.DrawLine(topLeft, botRight, color, 0.0f, true);
        Debug.DrawLine(topRight, botLeft, color, 0.0f, true);
    }

    void LateUpdate()
    {

        drawTile(m_boxCollider.bounds.center, m_boxCollider.bounds.extents, Color.red);
        
    }

}
