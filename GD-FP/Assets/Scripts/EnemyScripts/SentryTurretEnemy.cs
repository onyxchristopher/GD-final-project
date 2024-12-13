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

public class SentryTurretEnemy : MonoBehaviour
{
    private EnemyState fsm;
    private Vector2 location;
    private int health = 10;
    private Rigidbody2D playerRB;
    [SerializeField] GameObject projectile;

    // Awake encodes the enemy FSM
    void Awake() {
        fsm = gameObject.GetComponentInParent<EnemyState>();

        Action sentryAttack = AttackLoop;
        fsm.enterStateLogic.Add(EnemyState.State.ATTACK, sentryAttack);
    }

    void Start() {
        location = new Vector2(transform.position.x, transform.position.y);
        playerRB = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
    }

    private void AttackLoop() {
        if (fsm.state == EnemyState.State.IDLE) {
            return;
        }
        Timing.RunCoroutine(_SentryFire(), "sentryFire");
    }

    private IEnumerator<float> _SentryFire() {
        Vector2 dirToPlayer = new Vector2(playerRB.position.x, playerRB.position.y) - location;
        Instantiate(projectile, transform.position, Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, dirToPlayer)));
        yield return Timing.WaitForSeconds(2);
        AttackLoop();
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player") {
            fsm.state = EnemyState.State.ATTACK;
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.tag == "Player") {
            fsm.state = EnemyState.State.IDLE;
        }
    }

    public void Damage(int damage) {
        health -= damage;
        if (health <= 0) {
            Destroy(gameObject);
        }
    }
}
