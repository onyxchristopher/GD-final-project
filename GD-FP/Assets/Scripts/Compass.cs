﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class Compass : MonoBehaviour
{
     // the camera
    private Camera cam;

    // how much of the screen the compass radius takes
    [SerializeField] private float cameraRatio;

    // First and second-level clusters
    private Cluster[] level1;
    private Cluster[][] level2;

    // the cluster in which the player is currently located
    public int currCluster = 0;

    // the vector location of the next major objective
    private Vector2 major;

    // the vector locations of all nearby minor objectives
    private List<Vector2> minor;

    // player rigidbody
    private Rigidbody2D playerRB;

    // Progression tracker
    private List<int> majorProg = new List<int>();
    private List<int> minorProg = new List<int>();

    // Compass arrow references
    private RectTransform majorArrowTransform;
    private RectTransform[] minorArrowTransforms;

    // Visual elements
    [SerializeField] private GameObject majorCompassArrow;
    [SerializeField] private GameObject minorCompassArrow;

    // Whether to display the compass
    private bool showCompass = false;

    void Start()
    {
        playerRB = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();

        EventManager.onArtifactPickup += UpdateProgression;
        EventManager.onEnterCluster += EnteringCluster;
        EventManager.onExitCluster += LeavingCluster;
        EventManager.onEnterBossArea += HideCompass;
        EventManager.onExitBossArea += ShowCompass;
        EventManager.onPlayerDeath += ShowCompass;
    }

    public void InitializeCompass(Cluster[] l1, Cluster[][] l2) {
        level1 = l1;
        level2 = l2;
        UpdateMajor(1);

        // create arrows which are children of the compass
        minorArrowTransforms = new RectTransform[level2[0].Length];
        for (int i = 0; i < minorArrowTransforms.Length; i++) {
            GameObject minorArrow = Instantiate(minorCompassArrow, Vector3.zero, Quaternion.identity, transform);
            minorArrowTransforms[i] = minorArrow.GetComponent<RectTransform>();
            minorArrow.GetComponent<Image>().color = Color.clear;
        }

        GameObject majorArrow = Instantiate(majorCompassArrow, Vector3.zero, Quaternion.identity, transform);
        majorArrowTransform = majorArrow.GetComponent<RectTransform>();
        majorArrow.GetComponent<Image>().color = new Color(0, 0.5f, 1, 0.5f);

        showCompass = true;
    }

    public void EnteringCluster(int clusterNum) {
        currCluster = clusterNum;

        // update the minor arrows to the current cluster
        UpdateMinor(clusterNum);
        
        // show each arrow that should be shown
        for (int i = 0; i < minor.Count; i++) {
            minorArrowTransforms[i].gameObject.GetComponent<Animator>().SetTrigger("ShowCompass");
        }

        // set any remaining arrows as clear
        for (int i = minor.Count; i < level2[clusterNum - 1].Length; i++) {
            minorArrowTransforms[i].gameObject.GetComponent<Image>().color = Color.clear;
        }
        
    }

    public void LeavingCluster(int clusterNum) {
        currCluster = 0;
        
        for (int i = 0; i < minor.Count; i++) {
            minorArrowTransforms[i].gameObject.GetComponent<Animator>().SetTrigger("HideCompass");
        }
    }

    public void HideCompass(string bossName) {
        showCompass = false;
        if (currCluster == 0) {
            return;
        }
        for (int i = 0; i < minor.Count; i++) {
            minorArrowTransforms[i].gameObject.GetComponent<Animator>().SetTrigger("HideCompass");
        }
        majorArrowTransform.gameObject.GetComponent<Animator>().SetTrigger("HideCompass");
    }

    public void ShowCompass() {
        showCompass = true;
        if (currCluster == 0) {
            return;
        }
        for (int i = 0; i < minor.Count; i++) {
            minorArrowTransforms[i].gameObject.GetComponent<Animator>().SetTrigger("ShowCompass");
        }
        majorArrowTransform.gameObject.GetComponent<Animator>().SetTrigger("ShowCompass");
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
                    UpdateMajor(i);
                    break;
                }
            }
        } else { // minor
            minorProg.Add(id);
            UpdateMinor(firstDigit);
            minorArrowTransforms[secondDigit - 1].gameObject.SetActive(false);
        }
    }

    // point to the next major or zero if not
    private void UpdateMajor(int clusterNum) {
        if (clusterNum < level1.Length) {
            major = level1[clusterNum - 1].getCorePosition();
        } else {
            major = Vector2.zero;
        }
    }

    // point to the minor objectives of the cluster entered
    private void UpdateMinor(int clusterNum) {
        int clusterIndex = clusterNum - 1;
        int numMinor = level2[clusterIndex].Length;
        minor = new List<Vector2>(); // ACTIVE compass arrows
        for (int i = 0; i < numMinor; i++) {
            // if not already done that minor obj, add it to list
            if (!minorProg.Contains(clusterNum * 10 + i + 1)) {
                minorArrowTransforms[i].gameObject.SetActive(true);
                minor.Add(level2[clusterIndex][i].getCorePosition());
            }
        }
    }

    // calculate radius, re-called on camera rect change
    public void CalculateAnchorRadius(Rect cameraRect) {
        cam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        float aspect = cam.aspect;
        RectTransform canvasTransform = transform.parent.gameObject.GetComponent<RectTransform>();
        RectTransform compassTransform = gameObject.GetComponent<RectTransform>();
        if (aspect >= 1) { // landscape screen
            float insetDistance = (canvasTransform.rect.width - canvasTransform.rect.height) / 2;
            compassTransform.SetInsetAndSizeFromParentEdge(
                RectTransform.Edge.Left,
                insetDistance,
                canvasTransform.rect.width - 2 * insetDistance);
            compassTransform.SetInsetAndSizeFromParentEdge(
                RectTransform.Edge.Top,
                0,
                canvasTransform.rect.height);
        } else { // portrait
            float insetDistance = (canvasTransform.rect.height - canvasTransform.rect.width) / 2;
            compassTransform.SetInsetAndSizeFromParentEdge(
                RectTransform.Edge.Top,
                insetDistance,
                canvasTransform.rect.height - 2 * insetDistance);
            compassTransform.SetInsetAndSizeFromParentEdge(
                RectTransform.Edge.Left,
                0,
                canvasTransform.rect.width);
        }
    }

    private Vector2 DiffToCompassSpace(Vector2 diff) {
        // return a normalized anchor vector weighted from the center
        return (Vector2.one / 2) + diff.normalized * cameraRatio;
    }

    // update compass arrows
    void Update() {
        if (showCompass) {
            Vector2 majorDiff = major - playerRB.position;
            // major
            Vector2 majorArrowAnchor = DiffToCompassSpace(majorDiff);
            majorArrowTransform.anchorMin = majorArrowAnchor;
            majorArrowTransform.anchorMax = majorArrowAnchor;
            majorArrowTransform.anchoredPosition = Vector2.zero;
            majorArrowTransform.localRotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, majorDiff));
            // minor
            if (currCluster > 0) {
                for (int i = 0; i < minor.Count; i++) {
                    Vector2 minorDiff = minor[i] - playerRB.position;

                    Vector2 minorArrowAnchor = DiffToCompassSpace(minorDiff);
                    minorArrowTransforms[i].anchorMin = minorArrowAnchor;
                    minorArrowTransforms[i].anchorMax = minorArrowAnchor;
                    minorArrowTransforms[i].anchoredPosition = Vector2.zero;
                    minorArrowTransforms[i].localRotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, minorDiff));
                }
            }
        }
    }
}
