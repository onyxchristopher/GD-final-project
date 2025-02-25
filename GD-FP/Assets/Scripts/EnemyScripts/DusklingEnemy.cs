using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

/*
The duskling has three states: IDLE, TRACK, and ATTACK.
It starts in IDLE, rotating around its asteroid.
When its trigger is entered by the player, it moves to TRACK, performing the same rotation.
In TRACK, if it sees the player (direct line of sight) it moves to ATTACK.
In ATTACK, it rotates quickly to face the player and charges a laser, firing shortly after.
It moves back to TRACK if the player exits its line of sight, and back to IDLE if the player exits its trigger.
*/

public class DusklingEnemy : Enemy
{
    private Damageable dmg;
    private Rigidbody2D playerRB;
    private Vector3 asteroid;
    [SerializeField] private float timeToFire;
    [SerializeField] private float timeToRecharge;
    [SerializeField] private float idleRotationSpeed;
    [SerializeField] private float attackRotationSpeed;
    [SerializeField] private int damage;
    private bool firedWithinDelay = false;
    private RaycastHit2D[] raycastResults = new RaycastHit2D[1];
    [SerializeField] private GameObject laser;
    [SerializeField] private GameObject laserZap;
    private ContactFilter2D cf;
    private Sound sound;
    
    // Awake encodes the enemy FSM
    void Awake() {
        Action dusklingTrack = TrackLoop;
        enterStateLogic.Add(State.TRACK, dusklingTrack);

        Action dusklingAttack = AttackLoop;
        enterStateLogic.Add(State.ATTACK, dusklingAttack);

        Action dusklingIdle = IdleLoop;
        exitStateLogic.Add(State.ATTACK, dusklingIdle);
    }

    void Start() {
        playerRB = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
        dmg = GetComponent<Damageable>();
        dmg.enemy = this;
        asteroid = transform.position - 6.3f * Vector3.up;
        cf.SetLayerMask(LayerMask.GetMask("Planet", "Player"));
        cf.useTriggers = false;
        sound = GameObject.FindWithTag("Sound").GetComponent<Sound>();

        IdleLoop();

        EventManager.onPlayerDeath += ResetToIdle;
    }

    private void IdleLoop() {
        if (state == State.ATTACK) {
            return;
        }
        if (gameObject != null && gameObject.activeInHierarchy) {
            Timing.RunCoroutine(_IdleRotate().CancelWith(gameObject));
        }
    }

    private IEnumerator<float> _IdleRotate() {
        while (state != State.ATTACK) {
            transform.RotateAround(asteroid, Vector3.forward, idleRotationSpeed);
            yield return Timing.WaitForOneFrame;
        }
    }

    private void TrackLoop() {
        if (state != State.TRACK) {
            return;
        }
        if (gameObject != null && gameObject.activeInHierarchy) {
            Timing.RunCoroutine(_CheckLOS().CancelWith(gameObject));
        }
    }

    private IEnumerator<float> _CheckLOS() {
        while (state != State.IDLE) {
            Vector2 dirToPlayer = playerRB.position - (Vector2) transform.position;
            Physics2D.Raycast((Vector2) transform.position, dirToPlayer, cf, raycastResults, 40);
            
            if (!firedWithinDelay && raycastResults[0] && raycastResults[0].collider.gameObject.CompareTag("Player")) {
                firedWithinDelay = true;
                GameObject laserInstance = Instantiate(laser, transform.position, Quaternion.identity, transform);
                laserInstance.GetComponent<Laser>().SelfDestructInSeconds(timeToFire);
                state = State.ATTACK;
                StateTransition();
            } else if (state == State.ATTACK && raycastResults[0] && raycastResults[0].collider.gameObject.CompareTag("Planet")) {
                state = State.TRACK;
                StateTransition();
            }
            yield return Timing.WaitForSeconds(0.1f);
        }
    }

    private void AttackLoop() {
        if (state != State.ATTACK) {
            return;
        }
        if (gameObject != null && gameObject.activeInHierarchy) {
            Timing.RunCoroutine(_RotateToPlayer().CancelWith(gameObject));
            Timing.RunCoroutine(_Fire().CancelWith(gameObject));
        }
    }

    private IEnumerator<float> _RotateToPlayer() {
        while (state == State.ATTACK) {
            Vector2 dirToPlayer = playerRB.position - (Vector2) transform.position;
            float angle = Vector2.SignedAngle(transform.up, dirToPlayer);
            Debug.Log(angle);
            if (angle > 5) {
                transform.RotateAround(asteroid, Vector3.forward, attackRotationSpeed);
            } else if (angle < -5) {
                transform.RotateAround(asteroid, Vector3.forward, -attackRotationSpeed);
            }
            yield return Timing.WaitForOneFrame;
        }
    }

    private IEnumerator<float> _Fire() {
        while (state == State.ATTACK) {
            EventManager.LaserCharge();
            yield return Timing.WaitForSeconds(timeToFire);
            Vector2 dirToPlayer = playerRB.position - (Vector2) transform.position;
            Physics2D.Raycast((Vector2) transform.position, dirToPlayer, cf, raycastResults, 40);
            if (raycastResults[0].collider.gameObject.CompareTag("Player")) {
                raycastResults[0].collider.gameObject.GetComponent<PlayerCollision>().Damage(damage);
                playerRB.AddForce(dirToPlayer.normalized * 30, ForceMode2D.Impulse);
            }
            Instantiate(laserZap, raycastResults[0].point, Quaternion.identity);
            EventManager.LaserZap();

            yield return Timing.WaitForSeconds(timeToRecharge);
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
            state = State.TRACK;
            StateTransition();
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            state = State.IDLE;
            StateTransition();
        }
    }

    void Update() {
        Debug.Log(state);
    }
}
