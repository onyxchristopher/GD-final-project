using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

public class Pedestal : MonoBehaviour
{
    private float setSpawnDelay = 10;
    private bool withinSetSpawnDelay = false;
    private float fuelPerSecond = 20;
    private PlayerCollision pColl;
    private PlayerMovement pMove;

    void Awake() {
        GameObject player = GameObject.FindWithTag("Player");
        pColl = player.GetComponent<PlayerCollision>();
        pMove = player.GetComponent<PlayerMovement>();
        EventManager.onSetSpawn += CheckSpawnDelay;
    }

    public void CheckSpawnDelay(Vector3 spawn) {
        if (!withinSetSpawnDelay) {
            withinSetSpawnDelay = true;
            Timing.RunCoroutine(_SetSpawnTimer());
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player") && !withinSetSpawnDelay) {
            EventManager.SetSpawn(transform.position + transform.up * 3);
        }
    }

    void OnTriggerStay2D(Collider2D other) {
        if (other.gameObject.CompareTag("Player")) {
            if (!pColl.inactive) {
                pMove.SetFuel(Time.deltaTime * fuelPerSecond);
                if (pMove.GetFuel() > pMove.maxFuel - 1) {
                    pColl.SetHealth(pColl.maxHealth);
                }
            }
        }
    }

    private IEnumerator<float> _SetSpawnTimer() {
        yield return Timing.WaitForSeconds(setSpawnDelay);
        withinSetSpawnDelay = false;
    }
}
