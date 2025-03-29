using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    // the item the enemy drops (fuel, artifact)
    [SerializeField] protected GameObject drop;

    // the enemy's spawnpoint
    [HideInInspector] protected Vector2 spawnpoint;

    // the current state
    public enum State {
        IDLE, TRACK, ATTACK
    }
    
    [HideInInspector] public State state;
    [HideInInspector] public State prevState;

    protected Dictionary<State, Action> enterStateLogic = new Dictionary<State, Action>();
    protected Dictionary<State, Action> exitStateLogic = new Dictionary<State, Action>();

    void Start() {
        state = State.IDLE;
        prevState = state;
    }

    public void ReassignSpawn(Vector3 newSpawn) {
        spawnpoint = (Vector2) newSpawn;
    }

    public void StateTransition() {
        // if transitioning to a different state from the current one
        if (state != prevState) {
            // tries to find any action to be taken on state transition
            Action exitAction = null;
            Action enterAction = null;
            // if prevState has any associated exit actions, execute them
            if (exitStateLogic.TryGetValue(prevState, out exitAction)) {
                exitAction();
            }
            // if state has any associated entry actions, execute them
            if (enterStateLogic.TryGetValue(state, out enterAction)) {
                enterAction();
            }
            prevState = state; // update prevState so it can be used for the next transition
        }
    }

    public virtual void ResetToIdle() {
        state = State.IDLE;
        StateTransition();
    }

    public virtual void EnemyDeath() {
        gameObject.SetActive(false);
    }
}
