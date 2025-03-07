using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

public class Artifact : MonoBehaviour
{
    [SerializeField] private int id;
    [SerializeField] private float timeToPickup = 2.5f;
    private Transform playerTransform;

    void Start() {
        playerTransform = GameObject.FindWithTag("Player").transform;
    }

    public void setId(int newId) {
        id = newId;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if ((playerTransform.position - transform.position).magnitude < 10 && other.tag == "Player") {
            if (gameObject.transform.childCount == 2) {
                gameObject.transform.GetChild(0).GetComponent<Animator>().SetTrigger("Pickup");
                gameObject.transform.GetChild(1).GetComponent<Animator>().SetTrigger("Pickup");
            } else if (gameObject.transform.childCount == 3) {
                gameObject.transform.GetChild(0).GetChild(0).GetComponent<Animator>().SetTrigger("Pickup");
                gameObject.transform.GetChild(1).GetChild(0).GetComponent<Animator>().SetTrigger("Pickup");
                gameObject.transform.GetChild(2).GetChild(0).GetComponent<Animator>().SetTrigger("Pickup");
            } else if (gameObject.transform.childCount == 4) {
                gameObject.transform.GetChild(0).GetComponent<Animator>().SetTrigger("Pickup");
                gameObject.transform.GetChild(1).GetComponent<Animator>().SetTrigger("Pickup");
                gameObject.transform.GetChild(2).GetComponent<Animator>().SetTrigger("Pickup");
                gameObject.transform.GetChild(3).GetComponent<Animator>().SetTrigger("Pickup");
            } else if (gameObject.transform.childCount == 5) {
                gameObject.transform.GetChild(0).GetChild(0).GetComponent<Animator>().SetTrigger("Pickup");
                gameObject.transform.GetChild(1).GetChild(0).GetComponent<Animator>().SetTrigger("Pickup");
                gameObject.transform.GetChild(2).GetChild(0).GetComponent<Animator>().SetTrigger("Pickup");
                gameObject.transform.GetChild(3).GetChild(0).GetComponent<Animator>().SetTrigger("Pickup");
                gameObject.transform.GetChild(4).GetChild(0).GetComponent<Animator>().SetTrigger("Pickup");
            }
            
            EventManager.ArtifactPickup(id);
            if (id % 10 == 0) {
                EventManager.SetSpawn(transform.position);
                Timing.RunCoroutine(_MoveArtifactToPlayer());
                Destroy(gameObject.GetComponent<BoxCollider2D>());
                Destroy(gameObject, timeToPickup + 0.5f);
            } else {
                EventManager.MinorObjectiveComplete(id / 10, id % 10);
                Destroy(gameObject);
            }
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
        EventManager.ArtifactObtain(id / 10);
    }
}
