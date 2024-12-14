using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

/*
The sentry turret has two states: IDLE and ATTACK.
It starts in IDLE, and when its trigger is entered by the player, moves to ATTACK.
In ATTACK, it shoots a projectile at the player every three seconds.
It moves back to IDLE when its trigger is exited by the player.
*/

public class SentryTurretEnemy : Enemy {
    [SerializeField] private GameObject projectile;
    private Rigidbody2D playerRB;
    private Vector2 spawnpoint;

    // Awake encodes the enemy FSM
    void Awake() {
        Action sentryAttack = AttackLoop;
        enterStateLogic.Add(State.ATTACK, sentryAttack);
    }

    void Start() {
        playerRB = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
        spawnpoint = new Vector2(transform.position.x, transform.position.y);
        gameObject.GetComponent<Damageable>().enemy = this;
    }

    private void AttackLoop() {
        if (state == State.IDLE) {
            return;
        }
        Timing.RunCoroutine(_SentryFire());
    }

    private IEnumerator<float> _SentryFire() {
        Vector2 dirToPlayer = playerRB.position - spawnpoint;
        Instantiate(projectile, transform.position, Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, dirToPlayer)));
        yield return Timing.WaitForSeconds(2);
        AttackLoop();
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
}
