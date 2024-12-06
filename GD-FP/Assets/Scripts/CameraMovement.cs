using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {
    private GameObject player;
    private Vector3 cameraOffset = new Vector3(0, 0, -10);
    private Vector3 velocity = Vector3.zero;
    [SerializeField] private float smoothingTime;

    // Start is called before the first frame update
    void Start() {
        player = GameObject.FindWithTag("Player");
    }

    public void SnapToPlayer() {
        transform.position = player.transform.position + cameraOffset;
    }

    // Update is called once per frame
    void Update() {
        transform.position = Vector3.SmoothDamp(transform.position,
        player.transform.position + cameraOffset, ref velocity, smoothingTime);
    }
}
