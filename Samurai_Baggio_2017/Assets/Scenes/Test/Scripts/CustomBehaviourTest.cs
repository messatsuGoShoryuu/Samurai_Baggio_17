using UnityEngine;
using System.Collections;

public class CustomBehaviourTest : CustomBehaviour
{

	// Use this for initialization
	protected override void Awake ()
    {
        base.Awake();
        
        
	}

    void Start()
    {
        float time = Random.Range(0.5f, 1.0f);
        StartCoroutine(instantiate(time - 0.1f));
        GameObject.Destroy(this.gameObject, time);
    }

    IEnumerator instantiate(float time)
    {
        
        yield return new WaitForSeconds(Random.Range(0.2f, time));
        GameObject.Instantiate(this.gameObject);
    }

    

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }



    // Update is called once per frame
    public override void update ()
    {

	}
}
