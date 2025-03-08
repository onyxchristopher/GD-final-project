using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

public class Bomb : MonoBehaviour
{
    [SerializeField] private GameObject explosion;
    private Rigidbody2D rb;
    private Rigidbody2D playerRB;
    [SerializeField] private float speed;

    [SerializeField] private float reflectScaling;

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
        yield return Timing.WaitForSeconds(1.5f);
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
        } else if (!other.isTrigger && other.CompareTag("Damageable")) {
            other.gameObject.GetComponent<Damageable>().Damage(18);
            Destroy(gameObject);
        } else if (!other.isTrigger) {
            Destroy(gameObject);
        }
    }

    public void Explode() {
        Instantiate(explosion, transform.position, Quaternion.identity);
        // Knock the player back if within radius
        Vector2 distToPlayer = playerRB.position - (Vector2) transform.position;
        if (distToPlayer.magnitude < 5) {
            playerRB.AddForce(distToPlayer.normalized * 25, ForceMode2D.Impulse);
        }
        Destroy(gameObject);
    }
}
