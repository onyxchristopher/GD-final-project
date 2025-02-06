using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Forcefield : MonoBehaviour
{
    // the objects connected to the forcefield
    [SerializeField] private GameObject[] linkedObjects;
    private EdgeCollider2D ec;
    [SerializeField] public Color color;

    void Start() {
        // copy over visual points to collider
        ec = gameObject.GetComponent<EdgeCollider2D>();
        LineRenderer line = gameObject.GetComponent<LineRenderer>();
        Vector2[] pts = new Vector2[line.positionCount + 1];
        for (int i = 0; i < pts.Length - 1; i++) {
            pts[i] = (Vector2) line.GetPosition(i);
        }
        pts[line.positionCount] = pts[0];
        ec.points = pts;

        // link objects
        for (int i = 0; i < linkedObjects.Length; i++) {
            linkedObjects[i].GetComponent<Damageable>().FieldLink(gameObject, color);
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
        }
    }
}
