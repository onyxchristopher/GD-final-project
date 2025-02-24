using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

/*
The sentry turret has two states: IDLE and ATTACK.
It starts in IDLE, and when its trigger is entered by the player, moves to ATTACK.
In ATTACK, it shoots a projectile at the player every [delay] seconds.
It moves back to IDLE when its trigger is exited by the player.
*/

public class SentryTurretEnemy : Enemy
{
    [SerializeField] private GameObject projectile;
    private Rigidbody2D playerRB;
    [SerializeField] private float delay;
    private bool firedWithinDelay = false;

    // Awake encodes the enemy FSM
    void Awake() {
        Action sentryAttack = AttackLoop;
        enterStateLogic.Add(State.ATTACK, sentryAttack);
    }

    void Start() {
        playerRB = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
        gameObject.GetComponent<Damageable>().enemy = this;

        EventManager.onPlayerDeath += ResetToIdle;
    }

    void OnEnable() {
        ReassignSpawn(transform.position);
    }

    private void AttackLoop() {
        if (state == State.IDLE || firedWithinDelay) {
            return;
        }
        if (gameObject != null && gameObject.activeInHierarchy) {
            Timing.RunCoroutine(_SentryFire().CancelWith(gameObject));
        }
    }

    private IEnumerator<float> _SentryFire() {
        while (state != State.IDLE && !firedWithinDelay) {
            firedWithinDelay = true;
            Vector2 dirToPlayer = playerRB.position - (Vector2) transform.position;
            Instantiate(projectile, transform.position, Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, dirToPlayer)));
            yield return Timing.WaitForSeconds(delay);
            firedWithinDelay = false;
        }
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            collision.gameObject.GetComponent<PlayerCollision>().HullCollision();
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            state = State.ATTACK;
            StateTransition();
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            state = State.IDLE;
            StateTransition();
        }
    }

    public override void EnemyDeath() {
        EventManager.onPlayerDeath -= ResetToIdle;
        if (drop) {
            GameObject droppedFuel = Instantiate(drop, transform.position, Quaternion.Euler(0, 0, UnityEngine.Random.Range(45, 136)));
            droppedFuel.GetComponent<FuelDrop>().fuel = 10;
        }
        EventManager.EnemyDefeat();
        gameObject.SetActive(false);
    }
}
