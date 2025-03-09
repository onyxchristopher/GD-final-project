using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

/*
The fumeling has two states: IDLE and ATTACK.
It starts in IDLE, and hovers in space.
When its trigger is entered by the player, it moves to ATTACK.
In ATTACK, it moves away from the player, firing exploding projectiles towards the player.
If the fumeling makes contact with the edge of the circle it inhabits, it moves tangentially.
The fumeling leaves a trail of poisonous fumes behind it.
*/
public class FumelingEnemy : Enemy
{
    private Damageable dmg;
    private Rigidbody2D playerRB;
    private Rigidbody2D rb;
    [SerializeField] private float domainRadius;
    [SerializeField] private float speed;
    [SerializeField] private float timeToFumigate;

    [SerializeField] private GameObject fumetrail;
    [SerializeField] private float timeToRecharge;
    [SerializeField] private GameObject fumebomb;
    [SerializeField] private GameObject deathParticles;
    private bool firstMove = true;

    
    // Awake encodes the enemy FSM
    void Awake() {
        Action fumelingAttack = AttackLoop;
        enterStateLogic.Add(State.ATTACK, fumelingAttack);

        Action teardown = Teardown;
        exitStateLogic.Add(State.IDLE, teardown);
    }

    void Start() {
        playerRB = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
        rb = GetComponent<Rigidbody2D>();
        dmg = GetComponent<Damageable>();
        dmg.enemy = this;

        EventManager.onPlayerDeath += ResetToIdle;
    }

    private void AttackLoop() {
        if (firstMove) {
            ReassignSpawn(transform.position);
            firstMove = false;
        }
        if (state != State.ATTACK) {
            return;
        }
        if (gameObject != null && gameObject.activeInHierarchy) {
            Timing.KillCoroutines("fumeling");
            Timing.RunCoroutine(_Kite().CancelWith(gameObject), "fumeling");
            Timing.RunCoroutine(_EmitTrail().CancelWith(gameObject), "fumeling");
            Timing.RunCoroutine(_Attack().CancelWith(gameObject), "fumeling");
        }
    }

    private IEnumerator<float> _Kite() {
        while (state == State.ATTACK) {
            Vector2 dirToPlayer = (playerRB.position - rb.position).normalized;
            Vector2 distToSpawn = spawnpoint - rb.position;
            // if on the edge of the circle
            if ((distToSpawn).magnitude > domainRadius) {
                // if angle between playervect and spawnvect more than 90, move regularly
                if (Vector2.Dot(dirToPlayer, distToSpawn) < 0) {
                    rb.velocity = -dirToPlayer * speed;
                } else {
                    Vector2 tangent = Vector2.Perpendicular(distToSpawn);
                    // move away from the player tangentially
                    if (Vector2.Dot(dirToPlayer, tangent) < 0) {
                        rb.velocity = tangent.normalized * speed;
                    } else {
                        rb.velocity = -tangent.normalized * speed;
                    }
                }
            } else {
                rb.velocity = -dirToPlayer * speed;
            }

            rb.rotation = Vector2.SignedAngle(Vector2.right, dirToPlayer);

            yield return Timing.WaitForOneFrame;
        }
    }

    private IEnumerator<float> _EmitTrail() {
        while (state == State.ATTACK) {
            Instantiate(fumetrail, transform.position,
            Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, rb.velocity)));
            yield return Timing.WaitForSeconds(timeToFumigate);
        }
    }

    private IEnumerator<float> _Attack() {
        while (state == State.ATTACK) {
            Vector2 dirToPlayer = playerRB.position - rb.position;
            Instantiate(fumebomb, transform.position,
            Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, dirToPlayer)));
            yield return Timing.WaitForSeconds(timeToRecharge);
        }
    }

    private void Teardown() {
        rb.velocity = Vector2.zero;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            GetComponent<CircleCollider2D>().radius += 20;
            state = State.ATTACK;
            StateTransition();
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            GetComponent<CircleCollider2D>().radius -= 20;
            state = State.IDLE;
            StateTransition();
        }
    }

    public override void EnemyDeath() {
        EventManager.onPlayerDeath -= ResetToIdle;
        Instantiate(deathParticles, transform.position, Quaternion.identity);
        if (drop) {
            GameObject droppedFuel = Instantiate(drop, transform.position, Quaternion.Euler(0, 0, UnityEngine.Random.Range(45, 136)));
            droppedFuel.GetComponent<FuelDrop>().fuel = 30;
        }
        EventManager.EnemyDefeat();
        gameObject.SetActive(false);
    }
}
