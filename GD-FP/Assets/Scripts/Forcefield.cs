﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Forcefield : MonoBehaviour
{
    // the objects connected to the forcefield
    [SerializeField] private GameObject[] linkedObjects;

    // the objects the forcefield is protecting
    [SerializeField] private GameObject[] protectingObjects;

    // the forcefield's collider
    private EdgeCollider2D ec;

    // the forcefield's color
    [SerializeField] public Color color;

    // whether to draw a line to the center of the linked forcefield
    [SerializeField] bool drawLineToLinked = true;

    // multiplier for velocity on ejection
    [SerializeField] private float multiplier = -1;

    // the offset from the forcefield object where it respawns the player (zero if it does not)
    [SerializeField] private Vector3 offset = Vector3.zero;

    void Start() {
        // copy over visual points to collider
        ec = gameObject.GetComponent<EdgeCollider2D>();
        LineRenderer line = gameObject.GetComponent<LineRenderer>();
        Vector2[] pts;

        bool isLooped = gameObject.GetComponent<LineRenderer>().loop;
        if (isLooped) {
            pts = new Vector2[line.positionCount + 1];
        } else {
            pts = new Vector2[line.positionCount];
        }
        
        for (int i = 0; i < line.positionCount; i++) {
            pts[i] = (Vector2) line.GetPosition(i);
        }
        if (isLooped) {
            pts[line.positionCount] = pts[0];
        }
        ec.points = pts;

        // link objects
        for (int i = 0; i < linkedObjects.Length; i++) {
            linkedObjects[i].GetComponent<Damageable>().FieldLink(gameObject, color, drawLineToLinked);
        }

        EventManager.onArtifactPickup += ArtifactIdCheck;
    }

    public void ArtifactIdCheck(int id) {
        if (id % 10 != 0) {
            GameObject[] artifacts = GameObject.FindGameObjectsWithTag("Artifact");
            for (int i = 0; i < artifacts.Length; i++) {
                if ((artifacts[i].transform.position - transform.position).magnitude < 32) {
                    CheckForcefield();
                }
            }
        }
    }

    // if no linked objects remain, the forcefield is open
    public void CheckForcefield() {
        int activeLinkedObjCount = 0;
        for (int i = 0; i < linkedObjects.Length; i++) {
            if (linkedObjects[i].activeSelf) {
                activeLinkedObjCount++;
            }
        }
        if (activeLinkedObjCount == 0) {
            gameObject.SetActive(false);
            for (int i = 0; i < protectingObjects.Length; i++) {
                protectingObjects[i].GetComponent<Damageable>().protectiveForcefield = null;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D coll) {
        string goTag = coll.gameObject.tag;
        if (goTag == "Player") {
            Rigidbody2D rb = coll.gameObject.GetComponent<Rigidbody2D>();
            if (offset == Vector3.zero) {
                rb.velocity = Mathf.Max(25, rb.velocity.magnitude) * multiplier * coll.GetContact(0).normal;
                EventManager.ForcefieldBounce();
            } else {
                rb.position = transform.position + offset;
                EventManager.ForcefieldHit();
            }
        }
    }

    void OnDestroy() {
        EventManager.onArtifactPickup -= ArtifactIdCheck;
    }
}
