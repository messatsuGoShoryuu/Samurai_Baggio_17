using UnityEngine;
using System.Collections;

public class Rope : CustomBehaviour
{

    protected override void Awake()
    {
        UpdateManager.register(this);
    }

    protected override void OnDestroy()
    {
        UpdateManager.unregister(this);
    }

    public GameObject[] objects;
    Vector3[] m_defaultPositions;
    Quaternion[] m_defaultRotations;
    GameObject m_destination;
    SpriteRenderer[] m_spriteRenderers;
    bool m_destinationReached = false;
    public bool destinationReached
    {
        get
        {
            return m_destinationReached;
        }
    }

    public HingeJoint2D hook;
    public float throwSpeed;

    GameObject m_hand;

    StateMachine m_stateMachine;

    int IDLE;
    int PULL;
    int THROW;
    int SWING;

    public delegate void HookPlaced();
    public static event HookPlaced OnHookPlaced;

	// Use this for initialization
	void Start ()
    {
        int length = objects.Length;
        m_defaultPositions = new Vector3[length];
        m_defaultRotations = new Quaternion[length];
        m_spriteRenderers = new SpriteRenderer[length];
        for (int i = 0; i < length; i++)
        {
            m_defaultPositions[i] = objects[i].transform.localPosition;
            m_defaultRotations[i] = objects[i].transform.rotation;
            m_spriteRenderers[i] = objects[i].GetComponent<SpriteRenderer>();
        }

        initStateMachine();
        m_destination = null;
        m_stateMachine.setState(IDLE);
	}

    void initStateMachine()
    {
        m_stateMachine = new StateMachine();

        IDLE = m_stateMachine.addState(idle);
        THROW = m_stateMachine.addState(null, throwHook, null);
        PULL = m_stateMachine.addState(null, pullHook, null);
        SWING = m_stateMachine.addState(swing);

        m_stateMachine.addTransition(IDLE, THROW, initThrowHook);
        m_stateMachine.addTransition(THROW, SWING, initSwing);
        m_stateMachine.addTransition(SWING, PULL, initPullHook);
        m_stateMachine.addTransition(PULL, IDLE, finalize);
    }


    public void shoot(GameObject swingObject)
    {
        if (swingObject == null) return;
        m_destination = swingObject;
        m_stateMachine.setState(THROW);
    }

    public void pull()
    {
        m_stateMachine.setState(PULL);
    }
        
    public void idle()
    {
        
    }


    void resetPositions()
    {
        int l = objects.Length;
        for (int i = 0; i < l; i++)
        {
            objects[i].transform.position = m_hand.transform.position;
            objects[i].transform.rotation = m_defaultRotations[i];
        }
    }

    void setObject(GameObject swingObject)
    {
        m_destination = swingObject;
    }

    public void setHand(GameObject hand)
    {
        m_hand = hand;
    }

    void enableObjects(bool value)
    {
        transform.position = m_hand.transform.position;
        resetPositions();
        int l = objects.Length;
        for (int i = 0; i<l;i++)
        {
            objects[i].SetActive(value);
        }
        
    }

    void initSwing()
    {

        hook.enabled = true;
        hook.connectedBody = m_destination.GetComponent<Rigidbody2D>();
        m_destinationReached = true;
        hook.GetComponent<Rigidbody2D>().isKinematic = false;
        if (OnHookPlaced != null) OnHookPlaced();
    }

    void swing()
    {

    }

    void initThrowHook()
    {
        enableObjects(true);
        m_destinationReached = false;
        HingeJoint2D h = m_hand.GetComponent<HingeJoint2D>();
        h.connectedBody = objects[objects.Length - 1].GetComponent<Rigidbody2D>();
        h.connectedAnchor = Vector2.zero;
        h.anchor = Vector2.zero;
        hook.GetComponent<Rigidbody2D>().isKinematic = true;
    }

    void throwHook()
    {
        hook.transform.position = Vector3.MoveTowards(hook.transform.position, m_destination.transform.position, throwSpeed);
        if(hook.transform.position == m_destination.transform.position)
        {
            
            m_stateMachine.setState(SWING);
        }
    }


    void initPullHook()
    {
        hook.enabled = false;
        hook.connectedBody = null;
        hook.GetComponent<Rigidbody2D>().isKinematic = true;
    }

    void pullHook()
    {
        hook.transform.position = Vector3.MoveTowards(hook.transform.position, m_hand.transform.position, throwSpeed);
        if(hook.transform.position == m_hand.transform.position)
        {
            m_stateMachine.setState(IDLE);
        }
    }

    void finalize()
    {
        enableObjects(false);
        m_destination = null;
    }

    // Update is called once per frame
    public override void update ()
    {
        m_stateMachine.update();
	}

    public override void fixedUpdate()
    {
        m_stateMachine.fixedUpdate();
        
    }

    public override void lateUpdate()
    {
     
    }
    

}
