using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    protected Vector2 spawnpoint;
    protected Vector2 location;
    [SerializeField] protected int health;
    protected Rigidbody2D playerRB;

    [SerializeField] private GameObject fuelDrop;

    // the current state
    protected enum State {
        IDLE, ATTACK
    }
    
    [HideInInspector] protected State state;
    private State prevState;

    protected Dictionary<State, Action> enterStateLogic = new Dictionary<State, Action>();
    protected Dictionary<State, Action> exitStateLogic = new Dictionary<State, Action>(); 

    void Start() {
        spawnpoint = new Vector2(transform.position.x, transform.position.y);
        location = new Vector2(transform.position.x, transform.position.y);
        playerRB = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();

        state = State.IDLE;
        prevState = state;
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

    public void Damage(int damage) {
        health -= damage;
        if (health <= 0) {
            EnemyDeath();
        }
    }

    protected virtual void EnemyDeath() {
        GameObject droppedFuel = Instantiate(fuelDrop, transform.position, Quaternion.identity);
        droppedFuel.GetComponent<FuelDrop>().fuel = 5;
        Destroy(gameObject);
    }
}
