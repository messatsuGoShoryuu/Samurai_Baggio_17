using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class Hex_ScrollCamera : MonoBehaviour 
{
    public GameObject target;
    public float lerpSpeedX;
    public float lerpSpeedY;
    public float cameraZ;
    public float zoom;
    public float zoomSpeed;
    public Vector3 offset;
    public Camera camera;
    public bool lockedToTarget;
    public Vector3 staticPosition;
    // Use this for initialization

    static bool m_lateInitDone = false;
    public static bool initialized
    {
        get
        {
            return m_lateInitDone;
        }
    }
    private static Hex_ScrollCamera m_singleton;
    public static Hex_ScrollCamera singleton
    {
        get
        {
            if (m_singleton == null)
            {
                m_singleton = GameObject.FindObjectOfType<Hex_ScrollCamera>();
            }
            return m_singleton;
        }
    }

	void Start () 
    {
        
        camera = GetComponent<Camera>();
        
        zoom = camera.orthographicSize;
        m_lateInitDone = false;
        StartCoroutine(lateInit());
	}

    IEnumerator lateInit()
    {
        yield return new WaitForSeconds(0.01f);
        Vector3 newPosition = lockedToTarget ? target.transform.position : staticPosition + offset;
        newPosition.z = cameraZ;
        gameObject.transform.position = newPosition;
        m_lateInitDone = true;
    }
	
	// Update is called once per frame
	void Update () 
    {
        if (!m_lateInitDone) return;
        Vector3 destPosition = lockedToTarget ? target.transform.position + offset : staticPosition + offset;
        Vector3 newPosition = gameObject.transform.position;

        newPosition.x = Mathf.Lerp(this.transform.position.x,destPosition.x,lerpSpeedX);
        newPosition.y = Mathf.Lerp(this.transform.position.y, destPosition.y, lerpSpeedY);
        gameObject.transform.position = newPosition;
        camera.orthographicSize = Mathf.Lerp(camera.orthographicSize,zoom, zoomSpeed*Time.deltaTime);
	}
}
