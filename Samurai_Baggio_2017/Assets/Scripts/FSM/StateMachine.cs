using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Transition
{

    public delegate void TransitionFunction();
    private TransitionFunction m_transitionFunction;
    public TransitionFunction transitionFunction
    {
        get
        {
            return m_transitionFunction;
        }
    }

    private int m_from;
    private int m_to;

    public int from
    {
        get
        {
            return m_from;
        }
    }

    public int to
    {
        get
        {
            return m_to;
        }
    }

    public Transition()
    {

    }

    public Transition(int from, int to, TransitionFunction func)
    {
        m_from = from;
        m_to = to;
        m_transitionFunction = func;
    }
}

public class State
{
    public delegate void StateFunction();
    private StateFunction m_stateFunction;
    private StateFunction m_fixedStateFunction;
    private StateFunction m_lateStateFunction;
    public StateFunction stateFunction
    {
        get
        {
            return m_stateFunction;
        }
    }
    public StateFunction fixedStateFunction
    {
        get
        {
            return m_fixedStateFunction;
        }
    }

    public StateFunction lateStateFunction
    {
        get
        {
            return m_lateStateFunction;
        }
    }
    private int m_id;
    private Dictionary<int,Transition> m_transitions;

    public int id
    {
        get
        {
            return m_id;
        }
    }
    public State()
    {

    }

    public State(int id_, StateFunction func)
    {
        m_transitions = new Dictionary<int, Transition>();
        m_id = id_;
        m_stateFunction = func;
    }

    public State(int id_, StateFunction func, StateFunction fixedFunc, StateFunction lateFunc)
    {
        m_transitions = new Dictionary<int, Transition>();
        m_id = id_;
        m_stateFunction = func;
        m_fixedStateFunction = fixedFunc;
        m_lateStateFunction = lateFunc;
    }

    public void addTransition(int destination, Transition.TransitionFunction func)
    {
        Transition t = new Transition(this.m_id, destination, func);
        m_transitions.Add(destination, t);
    }

    public Transition.TransitionFunction getTransition(int id_)
    {
        if (!m_transitions.ContainsKey(id_)) return null;
        return m_transitions[id_].transitionFunction;
    }

    public bool canTransitTo(int id_)
    {
        return m_transitions.ContainsKey(id_);
    }

    public void clear()
    {
        m_transitions.Clear();
    }
}

public class StateMachine
{
    List<State> m_states;
    private State m_currentState;
    public State currentState
    {
        get
        {
            return m_currentState;
        }
    }

    public StateMachine()
    {
        m_states = new List<State>();
    }

    public int addState(State.StateFunction func)
    {
        int id = m_states.Count;
        State s = new State(id, func);
        m_states.Add(s);
        return id;
    }

    public int addState(State.StateFunction func, State.StateFunction fixedFunc, State.StateFunction lateFunc)
    {
        int id = m_states.Count;
        State s = new State(id, func, fixedFunc,lateFunc);
        m_states.Add(s);
        return id;
    }

    public void addTransition(int from, int to, Transition.TransitionFunction func)
    {
        m_states[from].addTransition(to, func);
    }


    public bool setState(int state)
    {
        
        if (m_currentState != null)
        {
            if (m_currentState.canTransitTo(state))
            {
                if (m_currentState.getTransition(state) != null)
                {
                    m_currentState.getTransition(state)();
                }
                m_currentState = m_states[state];
                return true;
            }
            return false;
        }
        m_currentState = m_states[state];
        return true;
    }

    public void update()
    {
        if(m_currentState != null)
            if(m_currentState.stateFunction != null) m_currentState.stateFunction();
    }

    public void fixedUpdate()
    {
        if (m_currentState != null)
            if (m_currentState.fixedStateFunction != null) m_currentState.fixedStateFunction();
    }

    public void lateUpdate()
    {
        if (m_currentState != null)
            if (m_currentState.lateStateFunction != null) m_currentState.lateStateFunction();
    }
    
    public void clear()
    {
        for(int i = 0; i<m_states.Count;i++)
        {
            m_states[i].clear();
        }

        m_states.Clear();
    }
}
