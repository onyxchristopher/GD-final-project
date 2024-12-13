using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyState : MonoBehaviour
{
    // the current state
    public enum State {
        IDLE, ATTACK
    }
    
    [HideInInspector] public State state;
    private State prevState;

    public Dictionary<State, Action> enterStateLogic = new Dictionary<State, Action>();
    public Dictionary<State, Action> exitStateLogic = new Dictionary<State, Action>(); 

    // Start is called before the first frame update
    void Start() {
        state = State.IDLE;
        prevState = state;
    }

    void FixedUpdate() {
        if (state != prevState) {
            // tries to find any action to be taken on state transition
            Action exitAction = null;
            Action enterAction = null;
            if (exitStateLogic.TryGetValue(prevState, out exitAction)) {
                exitAction();
            }
            if (enterStateLogic.TryGetValue(state, out enterAction)) {
                enterAction();
            }
            prevState = state;
        }
    }
}
