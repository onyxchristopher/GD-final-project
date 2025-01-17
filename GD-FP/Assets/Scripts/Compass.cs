using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Compass : MonoBehaviour
{
     // the camera
    private Camera cam;

    private Vector3 center;

    // how much of the screen the compass radius takes
    [SerializeField] private float cameraRatio;

    // the radius in non-rounded pixels
    private float radius;

    // First and second-level clusters
    private Cluster[] level1;
    private Cluster[][] level2;

    // the cluster in which the player is currently located
    private int currCluster = 0;

    // the vector location of the next major objective
    private Vector2 major;

    // the vector locations of all nearby minor objectives
    private List<Vector2> minor = new List<Vector2>();

    // player rigidbody
    private Rigidbody2D playerRB;

    // Progression tracker
    private List<int> majorProg = new List<int>();
    private List<int> minorProg = new List<int>();

    void Start()
    {
        cam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        if (cam.aspect >= 1) { // landscape screen
            radius = cam.orthographicSize * cameraRatio;
        } else { // portrait
            radius = cam.orthographicSize * cam.aspect * cameraRatio;
        }

        center = new Vector3(0.5f, 0.5f, 0);

        playerRB = transform.parent.gameObject.GetComponent<Rigidbody2D>();

        EventManager.onArtifactPickup += UpdateProgression;
        EventManager.onEnterCluster += UpdateMinor;
    }

    public void InitializeCompass(Cluster[] l1, Cluster[][] l2) {
        level1 = l1;
        level2 = l2;
        major = level1[0].getCorePosition();
    }

    private void LeavingCluster(int clusterNum) {
        currCluster = 0;
    }

    public void UpdateProgression(int id) {
        int firstDigit = id / 10;
        int secondDigit = id % 10;

        // major progression
        if (secondDigit == 0) {
            majorProg.Add(id);
            for (int i = 1; i <= level1.Length; i++) {
                // the next goal is the first artifact that is not in the list of reached artifacts
                if (!majorProg.Contains(i * 10)) {
                    UpdateMajor(firstDigit);
                }
            }
        } else { // minor
            minorProg.Add(id);
            UpdateMinor(firstDigit);
        }
    }

    // point to the next major or zero if not
    private void UpdateMajor(int firstDigit) {
        if (firstDigit < level1.Length) {
            major = level1[firstDigit].getCorePosition();
        } else {
            major = Vector2.zero;
        }
    }

    private void UpdateMinor(int clusterNum) {
        currCluster = clusterNum;
        int clusterIndex = clusterNum + 1;
        int numMinor = level2[clusterIndex].Length;
        for (int i = 1; i <= numMinor; i++) {
            if (!minorProg.Contains(clusterNum * 10 + i)) {
                minor.Add(level2[clusterIndex][i].getCorePosition());
            }
        }
    }

    // input angle theta, coord is costheta, sintheta
    private void calculateCoordinate(float theta) {
        float unitX = Mathf.Cos(theta);
        float unitY = Mathf.Sin(theta);
        
    }

    private Vector2 DiffToCompassSpace(Vector2 diff) {
        Debug.Log(radius);
        return radius * diff.normalized + playerRB.position;
    }

    void Update() {
        Vector2 majorDiff = major - playerRB.position;
        // major
        Debug.DrawLine((Vector3) DiffToCompassSpace(majorDiff), cam.ViewportToWorldPoint(center), Color.blue, 0.25f, false);
        // minor
        if (currCluster > 0) {

        }
    }
}
