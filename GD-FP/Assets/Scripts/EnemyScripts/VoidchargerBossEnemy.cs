using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

/*
The voidcharger has two states: IDLE and ATTACK.
It starts in IDLE, and when its trigger is entered by the player, enacts a forcefield around its domain.
In ATTACK, it briefly telegraphs its movement and then charges at the player. If it hits the player,
it damages them and knocks them back. If it hits a forcefield wall, it bounces off with an approximation of
an elastic collision. It moves back to IDLE and removes the forcefield if the player dies.

The voidcharger is vulnerable on its sides and back.
*/

public class VoidchargerBossEnemy : Enemy
{
    private Rigidbody2D playerRB;
    private Rigidbody2D rb;
    public string bossName;
    [SerializeField] private GameObject forcefield;
    private GameObject field;
    private Damageable dmg;

    [SerializeField] private float rechargeTime;
    [SerializeField] private float turnTime;
    [SerializeField] private float prechargeTime;
    [SerializeField] private float chargeTime;
    [SerializeField] private float chargeSpeed;
    
    private GameObject telegraphArrow;

    private bool forcefieldTrigger = false;
    private bool hitPlayerTrigger = false;

    [SerializeField] private GameObject deathParticles;

    // Awake encodes the enemy FSM
    void Awake() {
        Action chargerAttack = AttackLoop;
        chargerAttack += SpawnForcefield;
        enterStateLogic.Add(State.ATTACK, chargerAttack);
        Action teardown = EndLoop;
        exitStateLogic.Add(State.ATTACK, teardown);
    }
    
    void Start() {
        playerRB = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        dmg = gameObject.GetComponent<Damageable>();
        dmg.enemy = this;
        telegraphArrow = transform.GetChild(2).gameObject;

        EventManager.onPlayerDeath += ResetToIdle;
    }

    void OnEnable() {
        ReassignSpawn(transform.position);
    }

    // ATTACK state functions

    private void AttackLoop() {
        if (state != State.ATTACK) {
            return;
        }
        if (gameObject != null && gameObject.activeInHierarchy) {
            Timing.RunCoroutine(_RotateAndCharge().CancelWith(gameObject));
        }
    }

    private IEnumerator<float> _RotateAndCharge() {
        while (state == State.ATTACK) {
            // Recharge loop
            yield return Timing.WaitForSeconds(rechargeTime);
            // Turn loop
            float time = 0;
            float rotation = rb.rotation;
            while (time < turnTime) {
                rb.rotation = Mathf.LerpAngle(rotation, CalcPlayerDir(), time / turnTime);
                yield return Timing.WaitForOneFrame;
                time += Time.deltaTime;
            }
            telegraphArrow.SetActive(true);
            
            // Precharge loop
            yield return Timing.WaitForSeconds(prechargeTime);
            telegraphArrow.SetActive(false);
            
            // Charge loop
            time = 0;
            forcefieldTrigger = false;
            hitPlayerTrigger = false;
            while (time < chargeTime) {
                if (forcefieldTrigger || hitPlayerTrigger) {
                    time = chargeTime;
                } else {
                    rb.velocity = -transform.up * chargeSpeed * Mathf.Min(1, (3 - 3 * time / chargeTime));
                }
                yield return Timing.WaitForOneFrame;
                time += Time.deltaTime;
            }
            rb.velocity = Vector2.zero;

            Vector2 pDistFromSpawn = playerRB.position - spawnpoint;
            if (pDistFromSpawn.x < -50 || pDistFromSpawn.x > 50 || pDistFromSpawn.y < -50 || pDistFromSpawn.y > 50) {
                playerRB.position = spawnpoint;
            }
        }
    }

    private float CalcPlayerDir() {
        return Vector2.SignedAngle(Vector2.down, playerRB.position - rb.position);
    }

    private void SpawnForcefield() {
        field = Instantiate(forcefield, transform.position, Quaternion.identity, transform.parent);
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Player") {
            collision.gameObject.GetComponent<PlayerCollision>().HullCollision();
            hitPlayerTrigger = true;
        } else if (collision.gameObject.tag == "Forcefield") {
            forcefieldTrigger = true;
        }
    }

    // IDLE state functions

    private void EndLoop() {
        rb.velocity = Vector2.zero;
        EventManager.ExitBossArea();
    }

    // death

    public override void EnemyDeath() {
        EventManager.onPlayerDeath -= ResetToIdle;
        Instantiate(deathParticles, transform.position, Quaternion.identity);
        EventManager.BossDefeat(2);
        EventManager.ExitBossArea();
        if (drop) {
            GameObject artifact = Instantiate(drop, transform.parent.position + Vector3.right * 57, Quaternion.identity);
        }
        field.GetComponent<Forcefield>().CheckForcefield();
        GameObject.FindWithTag("VoidchargerRespawnField").SetActive(false);
        gameObject.SetActive(false);
    }
}
