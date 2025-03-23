using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

/*
The Echoceptor has three states: IDLE, TRACK, and ATTACK.
It starts in IDLE, and when its trigger is entered by the player, enacts a forcefield around its domain, moving to ATTACK.
In ATTACK, it fires two rockets from the two rocket launchers closest to the player.
When the forcefields around the Echoceptor are destroyed, it moves to TRACK.
In TRACK, it continues its rocket-firing pattern but moves opposite the player around the center of the domain.
When the Echoceptor is hit, it emits an echo, knocking the player back.
*/

public class EchoceptorBossEnemy : Enemy
{
    private Damageable dmg;
    private Rigidbody2D playerRB;
    private Rigidbody2D rb;
    public string bossName;
    [SerializeField] private GameObject rocket;
    [SerializeField] private GameObject echo;
    [SerializeField] private GameObject forcefield;
    private GameObject field;

    [SerializeField] private float timeToStartMirroring;
    [SerializeField] private float timeToStartFiring;
    [SerializeField] private float timeBetweenFires;
    [SerializeField] private float gapBetweenFiring;
    [SerializeField] private GameObject deathParticles;

    // Awake encodes the enemy FSM
    void Awake() {
        Action echoceptorAttack = AttackLoop;
        echoceptorAttack += SpawnForcefield;
        enterStateLogic.Add(State.ATTACK, echoceptorAttack);

        Action echoceptorTrack = TrackLoop;
        enterStateLogic.Add(State.TRACK, echoceptorTrack);
    }

    void Start() {
        playerRB = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
        rb = GetComponent<Rigidbody2D>();
        dmg = GetComponent<Damageable>();
        dmg.enemy = this;

        EventManager.onPlayerDeath += ResetToIdle;
    }

    public void Spawn() {
        state = State.ATTACK;
        StateTransition();
        GameObject.FindWithTag("MainCamera").GetComponent<CameraMovement>().ChangeSize(45, 0, 1);
    }

    public override void ResetToIdle() {
        state = State.IDLE;
        StateTransition();
        EventManager.onEnemyHit -= Echo;
        EventManager.ExitBossArea();
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
        yield return Timing.WaitForSeconds(timeToStartFiring);
        while (state != State.IDLE) {
            Vector2 dirToPlayer = playerRB.position - (Vector2) transform.position;
            float angle = Vector2.SignedAngle(Vector2.right, dirToPlayer);

            if (angle >= 0 && angle <= 90) {
                // top right
                InstRocket("up");
                yield return Timing.WaitForSeconds(gapBetweenFiring);
                InstRocket("right");
            } else if (angle > 90) {
                // top left
                InstRocket("left");
                yield return Timing.WaitForSeconds(gapBetweenFiring);
                InstRocket("up");
            } else if (angle < 0 && angle >= -90) {
                // bottom right
                InstRocket("right");
                yield return Timing.WaitForSeconds(gapBetweenFiring);
                InstRocket("down");
            } else {
                // bottom left
                InstRocket("down");
                yield return Timing.WaitForSeconds(gapBetweenFiring);
                InstRocket("left");
            }

            yield return Timing.WaitForSeconds(timeBetweenFires);
        }
    }

    private void InstRocket(string orientation) {
        float dist = 5;
        if (orientation == "up") {
            Instantiate(rocket, transform.position + Vector3.up * dist,
            Quaternion.Euler(0, 0, 90));
        } else if (orientation == "right") {
            Instantiate(rocket, transform.position + Vector3.right * dist,
            Quaternion.Euler(0, 0, 0));
        } else if (orientation == "down") {
            Instantiate(rocket, transform.position - Vector3.up * dist,
            Quaternion.Euler(0, 0, -90));
        } else {
            Instantiate(rocket, transform.position - Vector3.right * dist,
            Quaternion.Euler(0, 0, 180));
        }
    }

    private void TrackLoop() {
        if (state != State.TRACK) {
            return;
        }
        if (gameObject != null && gameObject.activeInHierarchy) {
            Echo();
            EventManager.onEnemyHit += Echo;
            Timing.RunCoroutine(_Mirror().CancelWith(gameObject));
        }
    }

    private IEnumerator<float> _Mirror() {
        // Move to desired place
        float time = 0;
        Vector3 pos = transform.position;
        while (time < timeToStartMirroring) {
            transform.position = Vector3.Lerp(pos, CalcMirrorPosition(), time / timeToStartMirroring);
            yield return Timing.WaitForOneFrame;
            time += Time.deltaTime;
        }
        // Begin mirroring
        while (state == State.TRACK) {
            yield return Timing.WaitForOneFrame;
            transform.position = CalcMirrorPosition();
        }
    }

    private Vector3 CalcMirrorPosition() {
        Vector3 center = transform.parent.position;
        Vector3 playerpos = (Vector3) playerRB.position;
        return playerpos + 2 * (center - playerpos);
    }

    private void Echo() {
        Instantiate(echo, transform.position, Quaternion.identity);

        // Force of launch decreases with distance
        Vector2 dirToPlayer = playerRB.position - (Vector2) transform.position;
        playerRB.AddForce(dirToPlayer.normalized * Mathf.Max(25, 60 - dirToPlayer.magnitude / 2), ForceMode2D.Impulse);
    }

    private void SpawnForcefield() {
        field = Instantiate(forcefield, transform.parent.position, Quaternion.identity, transform.parent);
    }

    public override void EnemyDeath() {
        EventManager.onPlayerDeath -= ResetToIdle;
        EventManager.onEnemyHit -= Echo;
        Instantiate(deathParticles, transform.position, Quaternion.identity);
        GameObject.FindWithTag("MainCamera").GetComponent<CameraMovement>().ChangeSize(30, 1, 1);
        EventManager.BossDefeat(5);
        EventManager.ExitBossArea();
        if (drop) {
            GameObject artifact = Instantiate(drop,
            transform.parent.position + Vector3.right * 70 - Vector3.up * 7.5f, Quaternion.identity);
        }
        field.GetComponent<Forcefield>().CheckForcefield();
        GameObject.FindWithTag("EchoceptorRespawnField").SetActive(false);
        gameObject.SetActive(false);
    }
}
