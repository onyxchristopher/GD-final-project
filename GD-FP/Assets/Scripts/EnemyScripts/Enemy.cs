using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    // the item the enemy drops (fuel, artifact)
    [SerializeField] protected GameObject drop;

    // the enemy's spawnpoint
    [HideInInspector] protected Vector2 spawnpoint;

    // the current state
    protected enum State {
        IDLE, TRACK, ATTACK
    }
    
    [HideInInspector] protected State state;
    private State prevState;

    protected Dictionary<State, Action> enterStateLogic = new Dictionary<State, Action>();
    protected Dictionary<State, Action> exitStateLogic = new Dictionary<State, Action>();

    void Start() {
        state = State.IDLE;
        prevState = state;
    }

    public void ReassignSpawn(Vector3 newSpawn) {
        spawnpoint = (Vector2) newSpawn;
    }

    protected void StateTransition() {
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

    public virtual void ResetToIdle() {
        state = State.IDLE;
        StateTransition();
    }

    public virtual void EnemyDeath() {
        Destroy(gameObject);
    }
}
