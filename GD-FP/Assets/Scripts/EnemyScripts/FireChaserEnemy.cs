﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

/*
The fire chaser has two states: IDLE and ATTACK.
It starts in IDLE, and when its trigger is entered by the player, moves to ATTACK.
In ATTACK, it moves towards the player at a constant rate.
It also fires at the player every [delay] seconds.
It moves back to IDLE, returning to its spawnpoint, when its trigger is exited by the player.
*/

public class FireChaserEnemy : Enemy {
    private Damageable dmg;
    [SerializeField] private GameObject projectile;
    private Rigidbody2D playerRB;
    private Rigidbody2D rb;
    [SerializeField] private float delay;
    [SerializeField] private float speed;
    private bool firedWithinDelay = false;
    public bool mobile = false;
    public bool firstMove = true;

    // Awake encodes the enemy FSM
    void Awake() {
        Action chaserAttack = Moving;
        enterStateLogic.Add(State.ATTACK, chaserAttack);

        Action chaserReturn = ReturnLoop;
        exitStateLogic.Add(State.ATTACK, chaserReturn);
    }

    void Start() {
        playerRB = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        dmg = gameObject.GetComponent<Damageable>();
        dmg.enemy = this;

        EventManager.onPlayerDeath += ResetToIdle;
    }

    private void Moving() {
        mobile = true;
        if (firstMove) {
            ReassignSpawn(transform.position);
            firstMove = false;
        }
        dmg.MobilityChange(mobile);
        ChaseLoop();
        FireLoop();
    }

    private void ChaseLoop() {
        if (state == State.IDLE) {
            return;
        }
        Timing.RunCoroutine(_Chase().CancelWith(gameObject));
    }

    private IEnumerator<float> _Chase() {
        // set velocity and rotation to chase the player
        Vector2 dirToPlayer = playerRB.position - rb.position;
        rb.velocity = dirToPlayer.normalized * speed;

        float angle = Vector2.SignedAngle(Vector2.right, dirToPlayer);
        rb.rotation = angle;
        
        yield return Timing.WaitForOneFrame;
        ChaseLoop();
    }

    private void FireLoop() {
        if (state == State.IDLE || firedWithinDelay) {
            return;
        }

        if ((playerRB.position - spawnpoint).magnitude > 50) {
            state = State.IDLE;
            StateTransition();
            return;
        }
        
        Timing.RunCoroutine(_Fire().CancelWith(gameObject));
    }

    private IEnumerator<float> _Fire() {
        // fire every delay in the direction of the player
        firedWithinDelay = true;
        Vector2 dirToPlayer = playerRB.position - rb.position;
        Instantiate(projectile, transform.position, Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, dirToPlayer)));
        yield return Timing.WaitForSeconds(delay);
        firedWithinDelay = false;
        FireLoop();
    }

    private void ReturnLoop() {
        if (state != State.IDLE) {
            return;
        }

        if ((playerRB.position - spawnpoint).magnitude < 40) {
            state = State.ATTACK;
            StateTransition();
            return;
        }
        
        Timing.RunCoroutine(_Return().CancelWith(gameObject));
    }
    
    private IEnumerator<float> _Return() {
        // move at a constant speed back to spawnpoint
        Vector2 dirToSpawn = spawnpoint - rb.position;
        rb.rotation = Vector2.SignedAngle(Vector2.right, dirToSpawn);
        rb.velocity = dirToSpawn.normalized * speed;
        if (dirToSpawn.magnitude < 1) {
            rb.velocity = Vector2.zero;
            mobile = false;
            dmg.MobilityChange(mobile);
            yield break;
        }
        yield return Timing.WaitForOneFrame;
        ReturnLoop();
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Player") {
            collision.gameObject.GetComponent<PlayerCollision>().HullCollision();
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player") {
            state = State.ATTACK;
            StateTransition();
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.tag == "Player") {
            state = State.IDLE;
            StateTransition();
        }
    }

    public override void EnemyDeath() {
        if (drop) {
            GameObject droppedFuel = Instantiate(drop, transform.position, Quaternion.Euler(0, 0, UnityEngine.Random.Range(45, 136)));
            droppedFuel.GetComponent<FuelDrop>().fuel = 10;
        }
        EventManager.EnemyDefeat();
        gameObject.SetActive(false);
    }
}
