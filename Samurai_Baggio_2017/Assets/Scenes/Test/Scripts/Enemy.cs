using UnityEngine;
using System.Collections;

public class Enemy : CustomBehaviour
{

    protected override void Awake()
    {
        UpdateManager.register(this);
    }

    protected override void OnDestroy()
    {
        UpdateManager.unregister(this);
    }
    public float maxHealth;
    [SerializeField]float m_health;

    HealthBar m_healthBar;
	// Use this for initialization
	void Start ()
    {
        m_health = maxHealth;
        m_healthBar = GetComponentInChildren<HealthBar>();
	}


    public void takeDamage(float damage)
    {
        m_health -= damage;
        if(m_healthBar == null) m_healthBar = GetComponentInChildren<HealthBar>();
        m_healthBar.setHealth(m_health / maxHealth);
        if (m_health <= 0) die();
    }

    public void die()
    {
        GameObject.Destroy(this.gameObject);
    }
	
	// Update is called once per frame
	void update ()
    {
	
	}
}
