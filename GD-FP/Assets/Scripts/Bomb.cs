using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

public class Bomb : MonoBehaviour
{
    [SerializeField] private GameObject explosion;
    private Rigidbody2D rb;
    private Rigidbody2D playerRB;
    [SerializeField] private float speed = 20;

    [SerializeField] private float reflectScaling = 1.5f;

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start() {
        playerRB = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
        EventManager.ProjectileFire();
        rb.velocity = speed * transform.right;
        Timing.RunCoroutine(_Detonator().CancelWith(gameObject));
    }

    private IEnumerator<float> _Detonator() {
        yield return Timing.WaitForSeconds(2.5f);
        Explode();
    }
    
    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            Explode();
        } else if (other.CompareTag("Shield")) {
            playerRB = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
            Vector2 diffVector = (rb.position - playerRB.position).normalized;
            rb.velocity = reflectScaling * speed * diffVector;
            gameObject.layer = LayerMask.NameToLayer("Attack");
        }
    }

    public void Explode() {
        Instantiate(explosion, transform.position, Quaternion.identity);
        // Knock the player back if within radius
        Vector2 distToPlayer = playerRB.position - (Vector2) transform.position;
        if (distToPlayer.magnitude < 10) {
            playerRB.AddForce(distToPlayer.normalized * 15);
        }
        Destroy(gameObject);
    }
}
