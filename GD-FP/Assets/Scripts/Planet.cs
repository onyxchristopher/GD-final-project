using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    private GameObject player;
    private Vector2 dist;
    [SerializeField] private float gravityScale;
    private float gravitationalConstant = 50;

    void Start() {
        player = GameObject.FindWithTag("Player");
    }

    void FixedUpdate() {
        
    }

    void OnTriggerStay2D(Collider2D other) {
        // get a vector between player and planet
        dist = transform.position - player.transform.position;
        Rigidbody2D playerRB = player.GetComponent<Rigidbody2D>();
        playerRB.AddForce(dist * (gravitationalConstant * gravityScale / dist.sqrMagnitude));
    }
}
