using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

public class Laser : MonoBehaviour
{
    [HideInInspector] public Transform player;
    private Vector3[] positions = new Vector3[2];
    private LineRenderer lr;

    void Start() {
        player = GameObject.FindWithTag("Player").transform;
        lr = gameObject.GetComponent<LineRenderer>();
        Timing.RunCoroutine(_LaserUplink().CancelWith(gameObject));
    }

    public void SelfDestructInSeconds(float time) {
        Destroy(gameObject, time);
    }

    private IEnumerator<float> _LaserUplink() {
        // calculate positions
        Vector3 startPos = transform.position;
        Vector3 endPos = player.position;
        Vector3[] positions = new Vector3[2] {startPos, endPos};

        // update positions while active
        while (true) {
            positions[0] = transform.position;
            positions[1] = player.position;
            lr.SetPositions(positions);
            yield return Timing.WaitForOneFrame;
        }
    }
}
