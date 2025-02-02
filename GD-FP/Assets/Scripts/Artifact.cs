using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

public class Artifact : MonoBehaviour
{
    [SerializeField] private int id;
    private float timeToPickup = 2.5f;
    private Transform playerTransform;

    void Start() {

    }

    public void setId(int newId) {
        id = newId;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player") {
            gameObject.transform.GetChild(0).GetComponent<Animator>().SetTrigger("Pickup");
            gameObject.transform.GetChild(1).GetComponent<Animator>().SetTrigger("Pickup");
            EventManager.ArtifactPickup(id);
            Destroy(gameObject.GetComponent<BoxCollider2D>());
            Destroy(gameObject, timeToPickup + 0.5f);
            playerTransform = other.transform;
            Timing.RunCoroutine(_MoveArtifactToPlayer());
        }
    }

    private IEnumerator<float> _MoveArtifactToPlayer() {
        float time = 0;
        while (time < timeToPickup) {
            transform.position = Vector3.Lerp(transform.position, playerTransform.position, time / timeToPickup);
            yield return Timing.WaitForOneFrame;
            time += Time.deltaTime;
        }
    }
}
