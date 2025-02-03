using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

public class Pedestal : MonoBehaviour
{
    [SerializeField] private GameObject artifact;
    private float setSpawnDelay = 15;
    private bool withinSetSpawnDelay = false;

    void Start() {
        EventManager.onSetSpawn += CheckSpawnDelay;
    }

    public void CheckSpawnDelay(Vector3 spawn) {
        if (!withinSetSpawnDelay) {
            withinSetSpawnDelay = true;
            Timing.RunCoroutine(_SetSpawnTimer());
            // pop up the spawn set msg
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player" && !withinSetSpawnDelay) {
            EventManager.SetSpawn(transform.position + transform.up * 3);
        }
    }

    private IEnumerator<float> _SetSpawnTimer() {
        yield return Timing.WaitForSeconds(setSpawnDelay);
        withinSetSpawnDelay = false;
    }
}
