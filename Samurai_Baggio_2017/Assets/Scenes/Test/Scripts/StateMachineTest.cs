using UnityEngine;
using System.Collections;

public class StateMachineTest : MonoBehaviour
{
    StateMachine m_stateMachine;

    int STATE_1;
    int STATE_2;
    int STATE_3;

	// Use this for initialization
	void Start ()
    {
        m_stateMachine = new StateMachine();

        STATE_1 = m_stateMachine.addState(state1);
        STATE_2 = m_stateMachine.addState(state2);
        STATE_3 = m_stateMachine.addState(state3);

        m_stateMachine.addTransition(STATE_1, STATE_2, state1_to_2);
        m_stateMachine.addTransition(STATE_2, STATE_3, state2_to_3);
        m_stateMachine.addTransition(STATE_3, STATE_2, state3_to_2);
        m_stateMachine.addTransition(STATE_2, STATE_1, state2_to_1);
    }


    void state1()
    {
        Debug.Log("state 1");
    }

    void state1_to_2()
    {
        Debug.Log("1 -> 2");
    }

    void state2_to_3()
    {
        Debug.Log("2 -> 3");
    }

    void state3_to_2()
    {
        Debug.Log("3 -> 2");
    }

    void state2_to_1()
    {
        Debug.Log("2 -> 1");
    }

    void state2()
    {
        Debug.Log("state 2");
    }

    void state3()
    {
        Debug.Log("state 3");
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) m_stateMachine.setState(STATE_1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) m_stateMachine.setState(STATE_2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) m_stateMachine.setState(STATE_3);
        m_stateMachine.update();
	}
}
