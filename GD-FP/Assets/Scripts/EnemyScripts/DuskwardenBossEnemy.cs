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
    [SerializeField] private GameObject deathParticles;

    // Awake encodes the enemy FSM
    void Awake() {
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
        gameObject.SetActive(false);
    }

    public void Spawn() {
        Vector2 dirToPlayer = playerRB.position - (Vector2) transform.parent.position;
        float angle = Vector2.SignedAngle(Vector2.right, dirToPlayer);
        transform.RotateAround(transform.parent.position, Vector3.forward, angle + 180);
        gameObject.SetActive(true);
        state = State.TRACK;
        StateTransition();
        SpawnForcefield();
        GameObject.FindWithTag("MainCamera").GetComponent<CameraMovement>().ChangeSize(60, 0, 1);
        Timing.RunCoroutine(_CheckLOS().CancelWith(gameObject));
        EventManager.onPlayerDeath += ResetToIdle;
    }

    private IEnumerator<float> _CheckLOS() {
        // check LOS while not in idle state
        while (state != State.IDLE) {
            Vector2 dirToPlayer = playerRB.position - (Vector2) transform.position;
            Physics2D.Raycast((Vector2) transform.position, dirToPlayer, cf, raycastResults, 200);
            
            // if duskling has not fired lately and raycast gets a hit on player, move to ATTACK
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
            if (state == State.TRACK) {
                transform.RotateAround(transform.parent.position, Vector3.forward, idleRotationSpeed);
            }
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
        EventManager.onPlayerDeath -= ResetToIdle;
    }

    public override void EnemyDeath() {
        EventManager.onPlayerDeath -= ResetToIdle;
        Instantiate(deathParticles, transform.position, Quaternion.identity);
        GameObject.FindWithTag("MainCamera").GetComponent<CameraMovement>().ChangeSize(30, 1, 1);
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
