using System.Collections;
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

    // multiplier for velocity on ejection
    [SerializeField] private float multiplier = -1;

    // the offset from the forcefield object where it respawns the player (zero or one if it does not)
    [SerializeField] private Vector3 offset = Vector3.zero;

    // sends a message to the echoceptor if true
    [SerializeField] private bool echoceptorLastField = false;

    void Start() {
        // copy over visual points to collider
        ec = gameObject.GetComponent<EdgeCollider2D>();
        LineRenderer line = gameObject.GetComponent<LineRenderer>();
        Vector2[] pts;

        // an extra point is needed for looped forcefields
        bool isLooped = gameObject.GetComponent<LineRenderer>().loop;
        if (isLooped) {
            pts = new Vector2[line.positionCount + 1];
        } else {
            pts = new Vector2[line.positionCount];
        }
        
        for (int i = 0; i < line.positionCount; i++) {
            pts[i] = (Vector2) line.GetPosition(i); // copy points
        }
        if (isLooped) {
            pts[line.positionCount] = pts[0]; // if looped, set last point to zeroth's location
        }
        ec.points = pts;

        // link Damageable objects so they send a message to the forcefield when destroyed
        for (int i = 0; i < linkedObjects.Length; i++) {
            linkedObjects[i].GetComponent<Damageable>().linkedForcefield = gameObject;
        }

        EventManager.onMinorObjectiveComplete += MinorForcefieldCheck;
    }

    public void ExplodeForcefieldCheck() {
        if (offset == Vector3.one) {
            CheckForcefield();
        }
    }

    public void MinorForcefieldCheck(int sectorId, int objectiveId) {
        int id = sectorId * 10 + objectiveId;
        if (id != 21 && id != 32 && id != 41 && id != 42 && id != 51 && id != 52) {
            return;
        }
        if ((transform.position - GameObject.FindWithTag("Player").transform.position).magnitude < 15) {
            CheckForcefield();
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
            if (echoceptorLastField) {
                Enemy boss = transform.parent.GetComponent<Enemy>();
                boss.state = Enemy.State.TRACK;
                boss.StateTransition();
            }
            for (int i = 0; i < protectingObjects.Length; i++) {
                protectingObjects[i].GetComponent<Damageable>().protectiveForcefield = null;
            }
            gameObject.SetActive(false);
        }
    }

    void OnCollisionEnter2D(Collision2D coll) {
        if (coll.gameObject.CompareTag("Player")) {
            Rigidbody2D rb = coll.gameObject.GetComponent<Rigidbody2D>();
            if (offset == Vector3.zero || offset == Vector3.one) {
                rb.velocity = Mathf.Max(25, rb.velocity.magnitude) * multiplier * coll.GetContact(0).normal;
                EventManager.ForcefieldBounce();
            } else {
                rb.position = transform.position + offset;
                rb.velocity = Vector2.zero;
                EventManager.ForcefieldHit();
            }
        }
    }

    void OnDestroy() {
        EventManager.onMinorObjectiveComplete -= MinorForcefieldCheck;
    }
}
