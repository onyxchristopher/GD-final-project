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
        playerTransform = GameObject.FindWithTag("Player").transform;
    }

    public void setId(int newId) {
        id = newId;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if ((playerTransform.position - transform.position).magnitude < 10) {
            if (gameObject.transform.childCount == 2) {
                gameObject.transform.GetChild(0).GetComponent<Animator>().SetTrigger("Pickup");
                gameObject.transform.GetChild(1).GetComponent<Animator>().SetTrigger("Pickup");
            } else {
                gameObject.GetComponent<Animator>().SetTrigger("Pickup");
            }
            
            EventManager.ArtifactPickup(id);
            if (id % 10 == 0) {
                EventManager.SetSpawn(transform.position);
            }
            
            Destroy(gameObject.GetComponent<BoxCollider2D>());
            Destroy(gameObject, timeToPickup + 0.5f);
            Timing.RunCoroutine(_MoveArtifactToPlayer());
        }
    }

    private IEnumerator<float> _MoveArtifactToPlayer() {
        float time = 0;
        Vector3 pos = transform.position;
        while (time < timeToPickup) {
            transform.position = Vector3.Lerp(pos, playerTransform.position, time / timeToPickup);
            yield return Timing.WaitForOneFrame;
            time += Time.deltaTime;
        }
    }
}
