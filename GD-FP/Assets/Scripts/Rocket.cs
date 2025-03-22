﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

public class Rocket : MonoBehaviour
{
    private RocketeerEnemy rocketeer;
    [SerializeField] private GameObject explosion;
    private Rigidbody2D rb;
    private Rigidbody2D playerRB;
    [SerializeField] private float speed;
    [SerializeField] private float maxTurn;
    [SerializeField] private float timeToDetonate;
    [SerializeField] private int damage;
    [SerializeField] private float reflectScaling;
    [SerializeField] private Sprite shieldedRocket;
    private bool reflected = false;
    [SerializeField] private bool longerReflectTime = false;

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start() {
        playerRB = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
        EventManager.RocketFire();
        rb.velocity = speed * transform.right;
        Timing.RunCoroutine(_Detonator().CancelWith(gameObject), "rocket");
        Destroy(gameObject, 10);
    }

    public void SetSource(RocketeerEnemy source) {
        rocketeer = source;
    }

    private IEnumerator<float> _Detonator() {
        float time = 0;
        while (time < timeToDetonate && !reflected) {
            Vector2 dirToPlayer = playerRB.position - rb.position;
            float angle = Vector2.SignedAngle(transform.right, dirToPlayer);
            if (angle >= maxTurn) {
                rb.rotation += maxTurn;
            } else if (angle <= -maxTurn) {
                rb.rotation -= maxTurn;
            }
            rb.velocity = speed * transform.right;
            yield return Timing.WaitForOneFrame;
            time += Time.deltaTime;
        }
        if (!reflected) {
            Explode();
        } else {
            if (longerReflectTime) {
                yield return Timing.WaitForSeconds(4);
            } else {
                yield return Timing.WaitForSeconds(1.5f);
            }
            Explode();
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            Vector2 distToPlayer = playerRB.position - (Vector2) transform.position;
            playerRB.AddForce(distToPlayer.normalized * 25, ForceMode2D.Impulse);
            EventManager.PlayerDamage(damage);
            Explode();
        } else if (other.CompareTag("Shield")) {
            Vector2 diffVector = (rb.position - playerRB.position).normalized;
            rb.rotation = Vector2.SignedAngle(Vector2.right, diffVector);
            rb.velocity = reflectScaling * speed * diffVector;
            gameObject.layer = LayerMask.NameToLayer("Attack");
            reflected = true;
            EventManager.ShieldReflect();
            GetComponent<SpriteRenderer>().sprite = shieldedRocket;
        } else if (!other.isTrigger && other.CompareTag("Damageable")) {
            other.GetComponent<Damageable>().Damage(2 * damage);
            Explode();
        } else if (other.CompareTag("Forcefield")) {
            other.GetComponent<Forcefield>().ExplodeForcefieldCheck();
            Explode();
        } else if (!other.isTrigger) {
            Explode();
        }
    }

    public void Explode() {
        EventManager.RocketExplode();
        Instantiate(explosion, transform.position, Quaternion.AngleAxis(rb.rotation, Vector3.forward));
        // rocket must tell its source that it is exploding
        if (rocketeer) {
            rocketeer.activeRocket = false;
            rocketeer.Launch();
        }
        Destroy(gameObject);
    }
}
