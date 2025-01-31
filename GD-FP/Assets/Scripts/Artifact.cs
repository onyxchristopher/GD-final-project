using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Artifact : MonoBehaviour
{
    [SerializeField] private int id;

    public void setId(int newId) {
        id = newId;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player") {
            EventManager.ArtifactPickup(id);
            Destroy(gameObject);
        }
    }
}
