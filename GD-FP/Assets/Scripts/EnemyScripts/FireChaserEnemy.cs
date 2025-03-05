using System;
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

public class FireChaserEnemy : Enemy
{
    private Damageable dmg;
    [SerializeField] private GameObject projectile;
    private Rigidbody2D playerRB;
    private Rigidbody2D rb;
    [SerializeField] private float delay;
    [SerializeField] private float speed;
    private bool firedWithinDelay = false;
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
        if (firstMove) {
            ReassignSpawn(transform.position);
            firstMove = false;
        }
        ChaseLoop();
        FireLoop();
    }

    private void ChaseLoop() {
        if (state == State.IDLE) {
            return;
        }
        if (gameObject != null && gameObject.activeInHierarchy) {
            Timing.RunCoroutine(_Chase().CancelWith(gameObject));
        }
    }

    private IEnumerator<float> _Chase() {
        while (state == State.ATTACK) {
            // set velocity and rotation to chase the player
            Vector2 dirToPlayer = playerRB.position - rb.position;
            rb.velocity = dirToPlayer.normalized * speed;

            float angle = Vector2.SignedAngle(Vector2.right, dirToPlayer);
            rb.rotation = angle;
            
            yield return Timing.WaitForOneFrame;
        }
    }

    private void FireLoop() {
        if (state == State.IDLE || firedWithinDelay) {
            return;
        }
        if (gameObject != null && gameObject.activeInHierarchy) {
            Timing.RunCoroutine(_Fire().CancelWith(gameObject));
        }
    }

    private IEnumerator<float> _Fire() {
        while (state == State.ATTACK && !firedWithinDelay) {
            // fire every delay in the direction of the player
            firedWithinDelay = true;
            Vector2 dirToPlayer = playerRB.position - rb.position;
            Instantiate(projectile, transform.position, Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, dirToPlayer)));
            yield return Timing.WaitForSeconds(delay);
            firedWithinDelay = false;
            
            // if the player is too far away, just go back to spawn
            if ((playerRB.position - spawnpoint).magnitude > 50) {
                state = State.IDLE;
                StateTransition();
            }
        }
    }

    private void ReturnLoop() {
        if (state != State.IDLE) {
            return;
        }
        if (gameObject != null && gameObject.activeInHierarchy) {
            Timing.RunCoroutine(_Return().CancelWith(gameObject));
        }
    }
    
    private IEnumerator<float> _Return() {
        Vector2 dirToSpawn = spawnpoint - rb.position;
        while (state == State.IDLE) {
            dirToSpawn = spawnpoint - rb.position;

            // move at a constant speed back to spawnpoint
            rb.rotation = Vector2.SignedAngle(Vector2.right, dirToSpawn);
            rb.velocity = dirToSpawn.normalized * speed;

            // if at spawn, stop
            if (dirToSpawn.magnitude < 1) {
                rb.velocity = Vector2.zero;
                yield break;
            }

            yield return Timing.WaitForOneFrame;

            // if player reenters area while returning
            if ((playerRB.position - spawnpoint).magnitude < 40) {
                state = State.ATTACK;
                StateTransition();
                yield break;
            }
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
