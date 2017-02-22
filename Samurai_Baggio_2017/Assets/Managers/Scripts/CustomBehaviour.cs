using UnityEngine;
using System.Collections;

public abstract class CustomBehaviour : MonoBehaviour
{
    
	// Use this for initialization
	protected virtual void Awake ()
    {
        UpdateManager.register(this);
    }

    protected virtual void OnDestroy()
    {
        UpdateManager.unregister(this);
    }

    

    public virtual void update()
    {

    }

    public virtual void fixedUpdate()
    {

    }
    public virtual void lateUpdate()
    {

    }
}
