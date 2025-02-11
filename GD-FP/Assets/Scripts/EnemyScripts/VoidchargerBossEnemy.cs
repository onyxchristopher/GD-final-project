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

    [SerializeField] private float preChargeDuration;
    [SerializeField] private float chargeDuration;
    [SerializeField] private float chargeSpeed;
    [SerializeField] private float rechargeTime;
    private GameObject telegraphArrow;

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
        telegraphArrow = transform.GetChild(3).gameObject;

        EventManager.onPlayerDeath += ResetToIdle;
        EventManager.onPlayerDeath += PlayerDiedDuringFight;
    }

    private void AttackLoop() {
        if (state != State.ATTACK) {
            return;
        }
        if (gameObject != null && gameObject.activeInHierarchy) {
            Timing.RunCoroutine(_RotateAndCharge(), "atkloop");
        }
    }

    private IEnumerator<float> _RotateAndCharge() {
        // Precharge loop
        float time = 0;
        telegraphArrow.SetActive(true);
        while (time < preChargeDuration) {
            rb.rotation = CalcPlayerDir();
            yield return Timing.WaitForOneFrame;
            time += Time.deltaTime;
        }
        telegraphArrow.SetActive(false);
        // Charge loop
        time = 0;
        Vector2 normVel = new Vector2(Mathf.Cos(rb.rotation), Mathf.Sin(rb.rotation));
        while (time < chargeDuration) {
            rb.velocity = -transform.up * chargeSpeed * Mathf.Min(1, (3 - 3 * time / chargeDuration));
            yield return Timing.WaitForOneFrame;
            time += Time.deltaTime;
        }
        rb.velocity = Vector2.zero;
        // Recharge loop
        time = 0;
        float rotation = rb.rotation;
        while (time < rechargeTime) {
            rb.rotation = Mathf.Lerp(rotation, CalcPlayerDir(), time / rechargeTime);
            yield return Timing.WaitForOneFrame;
            time += Time.deltaTime;
        }
        AttackLoop();
    }

    private float CalcPlayerDir() {
        return Vector2.SignedAngle(Vector2.down, playerRB.position - rb.position);
    }

    private void SpawnForcefield() {
        field = Instantiate(forcefield, transform.position, Quaternion.identity, transform.parent);
    }

    private void EndLoop() {
        Timing.KillCoroutines("atkloop");
    }

    public void PlayerDiedDuringFight() {
        rb.velocity = Vector2.zero;
        EventManager.ExitBossArea();
    }

    void OnDisable() {
        Destroy(field);
    }

    public override void EnemyDeath() {
        EventManager.BossDefeat(bossName);
        EventManager.ExitBossArea();
        field.GetComponent<Forcefield>().CheckForcefield();
        gameObject.SetActive(false);
    }
}
