using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

public class Pedestal : MonoBehaviour
{
    [SerializeField] private GameObject artifact;
    private float setSpawnDelay = 15;
    private bool withinSetSpawnDelay = false;
    private float fuelPerSecond = 20;

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

    void OnTriggerStay2D(Collider2D other) {
        if (other.gameObject.tag == "Player") {
            other.gameObject.GetComponent<PlayerMovement>().SetFuel(Time.deltaTime * fuelPerSecond);
        }
    }

    private IEnumerator<float> _SetSpawnTimer() {
        yield return Timing.WaitForSeconds(setSpawnDelay);
        withinSetSpawnDelay = false;
    }
}
