using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;


/*
The Duskwarden behaves exactly like the duskling, but rotates on a larger track and stops to fire.
*/
public class DuskwardenBossEnemy : Enemy
{
    private Damageable dmg;
    private Rigidbody2D playerRB;
    [SerializeField] private float timeToFire;
    [SerializeField] private float timeToRecharge;
    [SerializeField] private float idleRotationSpeed;
    [SerializeField] private int damage;
    private bool firedWithinDelay = false;
    private RaycastHit2D[] raycastResults = new RaycastHit2D[1];
    [SerializeField] private GameObject laser;
    [SerializeField] private GameObject laserZap;
    private Laser laserInstance;
    private ContactFilter2D cf;
    public string bossName;
    [SerializeField] private GameObject forcefield;
    private GameObject field;

    // Awake encodes the enemy FSM
    void Awake() {
        Action duskwardenTrack = TrackLoop;
        duskwardenTrack += SpawnForcefield;
        enterStateLogic.Add(State.TRACK, duskwardenTrack);

        Action duskwardenAttack = AttackLoop;
        enterStateLogic.Add(State.ATTACK, duskwardenAttack);

        Action duskwardenIdle = EndLoop;
        enterStateLogic.Add(State.IDLE, duskwardenIdle);
    }

    void Start() {
        playerRB = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
        dmg = GetComponent<Damageable>();
        dmg.enemy = this;
        cf.SetLayerMask(LayerMask.GetMask("Planet", "Player"));
        cf.useTriggers = false;
        EventManager.onPlayerDeath += ResetToIdle;
        gameObject.SetActive(false);
    }

    public override void ResetToIdle() {
        state = State.IDLE;
        StateTransition();
        gameObject.SetActive(false);
    }

    public void Spawn() {
        Vector2 dirToPlayer = playerRB.position - (Vector2) transform.parent.position;
        float angle = Vector2.SignedAngle(Vector2.right, dirToPlayer);
        transform.RotateAround(transform.parent.position, Vector3.forward, angle + 180);
        gameObject.SetActive(true);
        state = State.TRACK;
        StateTransition();
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
        // check LOS while not in idle state or if a laser is active
        while (state == State.TRACK || laserInstance) {
            Vector2 dirToPlayer = playerRB.position - (Vector2) transform.position;
            Physics2D.Raycast((Vector2) transform.position, dirToPlayer, cf, raycastResults, 200);
            
            // if duskling has not fired lately and raycast gets a hit, move to ATTACK
            if (!firedWithinDelay && raycastResults[0] && raycastResults[0].collider.gameObject.CompareTag("Player")) {
                firedWithinDelay = true;
                GameObject laserobj = Instantiate(laser, transform.position, Quaternion.identity, transform);
                laserInstance = laserobj.GetComponent<Laser>();
                laserInstance.SelfDestructInSeconds(timeToFire);
                state = State.ATTACK;
                StateTransition();
            }
            if (laserInstance && raycastResults[0]) {
                if (raycastResults[0].collider.gameObject.CompareTag("Player")) {
                    laserInstance.UpdateAndSetPositions(transform.position, (Vector3) playerRB.position);
                } else {
                    laserInstance.UpdateAndSetPositions(transform.position, raycastResults[0].point);
                }
            }
            transform.RotateAround(transform.parent.position, Vector3.forward, idleRotationSpeed);
            yield return Timing.WaitForOneFrame;
        }
    }

    private void AttackLoop() {
        if (state != State.ATTACK) {
            return;
        }
        if (gameObject != null && gameObject.activeInHierarchy) {
            Timing.RunCoroutine(_Fire().CancelWith(gameObject));
        }
    }

    private IEnumerator<float> _Fire() {
        while (state == State.ATTACK) {
            EventManager.LaserCharge();
            
            yield return Timing.WaitForSeconds(timeToFire);
            Vector2 dirToPlayer = playerRB.position - (Vector2) transform.position;
            Physics2D.Raycast((Vector2) transform.position, dirToPlayer, cf, raycastResults, 200);
            if (raycastResults[0].collider.gameObject.CompareTag("Player")) {
                raycastResults[0].collider.gameObject.GetComponent<PlayerCollision>().Damage(damage);
                playerRB.AddForce(dirToPlayer.normalized * 30, ForceMode2D.Impulse);
            }
            laserInstance = null;
            Instantiate(laserZap, raycastResults[0].point, Quaternion.identity);
            EventManager.LaserZap();
            if (state == State.ATTACK) {
                state = State.TRACK;
                StateTransition();
            }
            yield return Timing.WaitForSeconds(timeToRecharge);
            firedWithinDelay = false;
        }
    }

    private void SpawnForcefield() {
        field = Instantiate(forcefield, transform.parent.position, Quaternion.identity, transform.parent);
    }

    private void EndLoop() {
        EventManager.ExitBossArea();
    }

    public override void EnemyDeath() {
        EventManager.onPlayerDeath -= ResetToIdle;
        EventManager.BossDefeat(3);
        EventManager.ExitBossArea();
        if (drop) {
            GameObject artifact = Instantiate(drop, transform.parent.position + Vector3.right * 16, Quaternion.identity);
        }
        field.GetComponent<Forcefield>().CheckForcefield();
        GameObject.FindWithTag("DuskwardenRespawnField").SetActive(false);
        gameObject.SetActive(false);
    }
}
