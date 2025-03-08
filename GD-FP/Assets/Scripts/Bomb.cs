using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

public class Bomb : MonoBehaviour
{
    [SerializeField] private GameObject explosion;
    private Rigidbody2D playerRB;

    void Start() {
        playerRB = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
        Timing.RunCoroutine(_Detonator().CancelWith(gameObject));
    }

    private IEnumerator<float> _Detonator() {
        yield return Timing.WaitForSeconds(2.5f);
        Explode();
    }
    
    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            Explode();
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
