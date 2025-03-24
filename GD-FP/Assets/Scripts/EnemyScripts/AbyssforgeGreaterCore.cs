using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

public class AbyssforgeGreaterCore : Enemy
{
    private Damageable dmg;
    private Rigidbody2D playerRB;
    [SerializeField] private GameObject echo;
    [SerializeField] private float maxSize;
    [SerializeField] private float minSize;
    [SerializeField] private Vector3 rotationVector;
    private Transform megalaser;
    private int numHitsTaken = 0;
    private int totalHitsNeeded = 19;

    [SerializeField] private GameObject deathParticles;

    // Awake encodes the enemy FSM
    void Awake() {
        Action coreAttack = AttackLoop;
        enterStateLogic.Add(State.ATTACK, coreAttack);
    }

    void Start() {
        playerRB = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
        dmg = GetComponent<Damageable>();
        dmg.enemy = this;
    }

    public override void ResetToIdle() {
        state = State.IDLE;
        StateTransition();
        EventManager.ExitBossArea();
        EventManager.onPlayerDeath -= ResetToIdle;
    }

    public void GreaterCoreDamaged() {
        // echo
        Instantiate(echo, transform.position, Quaternion.identity);

        // Force of launch decreases with distance
        Vector2 dirToPlayer = playerRB.position - (Vector2) transform.position;
        playerRB.AddForce(dirToPlayer.normalized * 50, ForceMode2D.Impulse);

        // reduce in size
        if (numHitsTaken < totalHitsNeeded) {
            numHitsTaken++;
            Vector3 scaleReduction = Vector3.one * ((maxSize - minSize) / totalHitsNeeded);
            transform.GetChild(1).localScale -= 2 * scaleReduction;
            transform.GetChild(2).localScale -= 2 * scaleReduction;
            GetComponents<CircleCollider2D>()[0].radius -= scaleReduction.x;
        }
    }

    private void AttackLoop() {
        if (state != State.ATTACK) {
            return;
        }
        EventManager.onPlayerDeath += ResetToIdle;
        if (gameObject != null && gameObject.activeInHierarchy) {
            Timing.RunCoroutine(_Megalaser().CancelWith(gameObject), Segment.FixedUpdate);
        }
    }

    private IEnumerator<float> _Megalaser() {
        // telegraph the appearance
        transform.GetChild(3).gameObject.SetActive(true);
        EventManager.LaserCharge();

        // wait for the player to notice the telegraph
        yield return Timing.WaitForSeconds(1.5f);
        transform.GetChild(3).gameObject.SetActive(false);
        megalaser = transform.GetChild(0);
        megalaser.position = transform.position;
        megalaser.gameObject.SetActive(true);

        while (state == State.ATTACK) {
            yield return Timing.WaitForOneFrame;
            megalaser.Rotate(rotationVector);
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            state = State.ATTACK;
            StateTransition();
            GetComponents<CircleCollider2D>()[1].radius += 20;
            transform.GetChild(4).gameObject.SetActive(true);
            transform.GetChild(5).gameObject.SetActive(true);
            transform.GetChild(6).gameObject.SetActive(true);
            transform.GetChild(7).gameObject.SetActive(true);
        }
    }

    private void Teardown() {
        try {
            GetComponents<CircleCollider2D>()[1].radius -= 20;
        } catch (MissingReferenceException e) {
            Debug.Log(e);
        }
    }

    public override void EnemyDeath() {
        Instantiate(deathParticles, transform.position, Quaternion.identity);
        EventManager.onPlayerDeath -= ResetToIdle;
    }
}
