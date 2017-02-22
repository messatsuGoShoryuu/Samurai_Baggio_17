using UnityEngine;
using System.Collections;

public class Ball : CustomBehaviour
{

    protected override void Awake()
    {
        UpdateManager.register(this);
    }

    protected override void OnDestroy()
    {
        UpdateManager.unregister(this);
    }

    Rigidbody2D m_rigidBody;
    CircleCollider2D m_circleCollider;

    public Vector2 startForce;
    public float baseDamage;

    float m_defaultAirDrag;
    public float dribbleAirDrag;

    bool m_isDribbling = false;
    float m_dribbleTimeStamp = 0.0f;

    // Use this for initialization
    void Start ()
    {
        m_rigidBody = GetComponent<Rigidbody2D>();
        CircleCollider2D[] c = GetComponents<CircleCollider2D>();
        for (int i = 0; i < c.Length; i++) if (!c[i].isTrigger) m_circleCollider = c[i];
        m_defaultAirDrag = m_rigidBody.drag;

    }

    void OnEnable()
    {
        
    }

    // Update is called once per frame
    public override void update()
    {
        if (m_isDribbling)
        {
            if (Time.time - m_dribbleTimeStamp > 0.5f)
            {
                setDribblingMode(false);
            }
        }
    }
    public override void fixedUpdate()
    {
        
    }
    public override void lateUpdate()
    {
        
    }

    public void refresh(Vector3 position)
    {
        gameObject.SetActive(true);
        transform.position = position;

        if (m_rigidBody == null) m_rigidBody = GetComponent<Rigidbody2D>();
        m_rigidBody.velocity = Vector2.zero;
        m_rigidBody.angularVelocity = 0.0f;
        m_rigidBody.AddForce(startForce, ForceMode2D.Impulse);
        m_rigidBody.AddTorque(-startForce.x * 3.0f);
    }  

    public void controlledHit(Vector3 force, float torque)
    {
        m_rigidBody.velocity = Vector2.zero;
        m_rigidBody.angularVelocity = 0.0f;
        m_rigidBody.velocity = force;
        m_rigidBody.angularVelocity = torque;
    }
    
    public void hit(Vector3 force)
    {
        m_rigidBody.AddForce(force);
    }

    public void hit(Vector3 force, float torque)
    {
        m_rigidBody.AddForce(force);
        m_rigidBody.AddTorque(torque);
    }

    public void setPosition(Vector3 position)
    {
        transform.position = position;
    }

    public void setPath(float direction, float velocity)
    {
        m_rigidBody.velocity = Vector2.right * direction * velocity;
    }



    public void setDribblingMode(bool value)
    {
        m_rigidBody.drag = value ? dribbleAirDrag : m_defaultAirDrag;
        m_isDribbling = value;
        if (value)
        {
            m_dribbleTimeStamp = Time.time;
        }
    }

    public void stopBall()
    {
        m_rigidBody.velocity = Vector2.zero;
        m_rigidBody.angularVelocity = 0.0f;
    }

    public void ignoreCollision(string layer, bool value)
    {
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer(layer), value);
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        if(c.collider.tag == "Enemy")
        {
            c.collider.GetComponent<Enemy>().takeDamage(c.relativeVelocity.magnitude * baseDamage);
        }
    }


}
