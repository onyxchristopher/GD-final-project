using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

/*
The sentry turret has two states: IDLE and ATTACK.
It starts in IDLE, and when its trigger is entered by the player, moves to ATTACK.
In ATTACK, it shoots a rocket at the player when there is no other active rocket.
It moves back to IDLE when its trigger is exited by the player.
*/

public class RocketeerEnemy : Enemy
{
    [SerializeField] private GameObject rocket;
    private Rigidbody2D playerRB;
    private Damageable dmg;
    [SerializeField] private float timeToFire;
    [SerializeField] private GameObject deathParticles;
    [HideInInspector] public bool activeRocket = false;

    // Awake encodes the enemy FSM
    void Awake() {
        Action rocketAttack = Launch;
        enterStateLogic.Add(State.ATTACK, rocketAttack);
    }

    void Start() {
        playerRB = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
        dmg = gameObject.GetComponent<Damageable>();
        dmg.enemy = this;

        EventManager.onPlayerDeath += ResetToIdle;
    }

    void OnEnable() {
        ReassignSpawn(transform.position);
    }

    public void Launch() {
        if (state == State.IDLE) {
            return;
        }
        if (!activeRocket && gameObject != null && gameObject.activeInHierarchy) {
            Timing.RunCoroutine(_Fire().CancelWith(gameObject));
        }
    }

    private IEnumerator<float> _Fire() {
        activeRocket = true;
        yield return Timing.WaitForSeconds(timeToFire);
        Vector2 dirToPlayer = playerRB.position - (Vector2) transform.position;
        GameObject newRocket = Instantiate(rocket, transform.position,
        Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, dirToPlayer)));
        newRocket.GetComponent<Rocket>().SetSource(this);
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
        Instantiate(deathParticles, transform.position, Quaternion.identity);
        if (drop && drop.CompareTag("FuelDrop")) {
            GameObject droppedFuel = Instantiate(drop, transform.position, Quaternion.Euler(0, 0, UnityEngine.Random.Range(45, 136)));
            droppedFuel.GetComponent<FuelDrop>().fuel = 40;
        } else if (drop && drop.CompareTag("HealthDrop")) {
            GameObject droppedHealth = Instantiate(drop, transform.position, Quaternion.Euler(0, 0, UnityEngine.Random.Range(45, 136)));
            droppedHealth.GetComponent<HealthDrop>().health = 15;
        }
        EventManager.EnemyDefeat();
        gameObject.SetActive(false);
    }
}
