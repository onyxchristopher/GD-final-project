using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

public class CameraMovement : MonoBehaviour
{
    private GameObject player;
    private Vector3 cameraOffset = new Vector3(0, 0, -10);
    private Vector3 moveVelocity = Vector3.zero;
    [SerializeField] private float smoothingTime;
    private Camera cam;

    // Start is called before the first frame update
    void Start() {
        player = GameObject.FindWithTag("Player");

        cam = gameObject.GetComponent<Camera>();
    }

    public void SnapToPlayer() {
        transform.position = player.transform.position + cameraOffset;
    }

    void FixedUpdate() {
        transform.position = Vector3.SmoothDamp(transform.position,
        player.transform.position + cameraOffset, ref moveVelocity, smoothingTime);
    }

    public void ResetCameraSize() {
        cam.orthographicSize = 30;
    }

    public void ChangeSize(int newSize, float waitTime, float timeToChange) {
        Timing.RunCoroutine(_SizeChange(newSize, waitTime, timeToChange));
    }

    private IEnumerator<float> _SizeChange(int newSize, float waitTime, float timeToChange) {
        float currentSize = cam.orthographicSize; // get current size
        yield return Timing.WaitForSeconds(waitTime); // wait for waitTime
        float time = 0;
        while (time < timeToChange) {
            float fraction = Mathf.Sin(Mathf.PI * time / (2 * timeToChange)); // specify size-function
            cam.orthographicSize = currentSize + fraction * (newSize - currentSize); // change size
            yield return Timing.WaitForOneFrame;
            time += Time.deltaTime;
        }
        cam.orthographicSize = newSize; // make sure the camera is exactly the desired new size
    }
}
