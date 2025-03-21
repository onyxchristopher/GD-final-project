using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

/*
The Murkfang has two states: IDLE and ATTACK.
It starts in IDLE, and when its trigger is entered by the player, enacts a forcefield around its domain, moving to ATTACK.
In ATTACK, it turns toward the player and then fires three fumebombs in quick succession.
After a delay, it teleports in its direction of fire, provided that it remains within its domain.
*/
public class MurkfangBossEnemy : Enemy
{
    private Damageable dmg;
    private Rigidbody2D playerRB;
    private Rigidbody2D rb;
    public string bossName;
    [SerializeField] private GameObject forcefield;
    private GameObject field;

    [SerializeField] private float timeToCharge;
    [SerializeField] private float fireDelay;
    [SerializeField] private float timeToTeleport;
    [SerializeField] private float detonateScale;
    [SerializeField] private float timeToDetonate;
    [SerializeField] private float teleportDist;

    [SerializeField] private GameObject fumebomb;
    [SerializeField] private GameObject deathParticles;

    // Awake encodes the enemy FSM
    void Awake() {
        Action murkfangAttack = AttackLoop;
        murkfangAttack += SpawnForcefield;
        enterStateLogic.Add(State.ATTACK, murkfangAttack);

        Action teardown = Teardown;
        exitStateLogic.Add(State.ATTACK, teardown);
    }

    void Start() {
        playerRB = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
        rb = GetComponent<Rigidbody2D>();
        dmg = GetComponent<Damageable>();
        dmg.enemy = this;

        EventManager.onPlayerDeath += ResetToIdle;
    }

    private void AttackLoop() {
        if (state != State.ATTACK) {
            return;
        }
        if (gameObject != null && gameObject.activeInHierarchy) {
            Timing.RunCoroutine(_Rotate().CancelWith(gameObject));
            Timing.RunCoroutine(_Fire().CancelWith(gameObject));
        }
    }

    private IEnumerator<float> _Rotate() {
        float time = 0;
        float rotation = rb.rotation;
        while (time < timeToCharge) {
            rb.rotation = Mathf.LerpAngle(rotation, CalcPlayerDir(), time / timeToCharge);
            yield return Timing.WaitForOneFrame;
            time += Time.deltaTime;
        }

        while (state == State.ATTACK) {
            rb.rotation = CalcPlayerDir();
            yield return Timing.WaitForOneFrame;
        }
    }

    private IEnumerator<float> _Fire() {
        while (state == State.ATTACK) {
            yield return Timing.WaitForSeconds(timeToCharge);

            // Fire three bombs
            for (int i = 0; i < 3; i++) {
                yield return Timing.WaitForSeconds(fireDelay);
                GameObject bomb = Instantiate(fumebomb, transform.position + transform.right * 4,
                Quaternion.Euler(0, 0, CalcPlayerDir()));
                bomb.GetComponent<Bomb>().Detonate(detonateScale * i + timeToDetonate);
            }

            yield return Timing.WaitForSeconds(timeToTeleport);

            // Teleport a certain distance behind the player
            Vector2 distToPlayer = playerRB.position - rb.position;
            Vector2 teleportLocation = rb.position + distToPlayer.normalized * (distToPlayer.magnitude + teleportDist);
            Vector2 teleportOffset = teleportLocation - spawnpoint;
            if (Mathf.Abs(teleportOffset.x) <= 53 && Mathf.Abs(teleportOffset.y) <= 53) {
                rb.position = teleportLocation;
                EventManager.EnemyTeleport();
            }
        }
    }

    private float CalcPlayerDir() {
        return Vector2.SignedAngle(Vector2.right, playerRB.position - rb.position);
    }

    private void SpawnForcefield() {
        field = Instantiate(forcefield, transform.position, Quaternion.identity, transform.parent);
    }

    private void Teardown() {
        EventManager.ExitBossArea();
    }

    public override void EnemyDeath() {
        EventManager.onPlayerDeath -= ResetToIdle;
        Instantiate(deathParticles, transform.position, Quaternion.identity);
        GameObject.FindWithTag("MainCamera").GetComponent<CameraMovement>().ChangeSize(30, 1, 1);
        EventManager.BossDefeat(4);
        EventManager.ExitBossArea();
        if (drop) {
            GameObject artifact = Instantiate(drop, transform.parent.position + Vector3.right * -50, Quaternion.identity);
        }
        field.GetComponent<Forcefield>().CheckForcefield();
        GameObject.FindWithTag("MurkfangRespawnField").SetActive(false);
        gameObject.SetActive(false);
    }
}
