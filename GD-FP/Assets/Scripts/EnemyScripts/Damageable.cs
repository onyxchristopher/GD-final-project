using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

public class Damageable : MonoBehaviour {

    private GameController gameController;
    public int maxHealth;
    [HideInInspector] public int health;
    [SerializeField] private bool isBoss;
    private bool invuln = false;
    private float invulnDuration = 0.3f;
    public Enemy enemy;
    private GameObject forcefield;
    private LineRenderer lr;

    void Start() {
        gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();

        health = maxHealth;
    }
    
    public void Damage(int damage) {
        if (!invuln){
            health -= damage;

            // a normal enemy should have hit sounds at all times
            // a boss should have hit sounds but not on death
            if (isBoss) {
                gameController.SetBossHealthBar(health);
            }

            if (health <= 0) {
                enemy.EnemyDeath();
                if (!isBoss) {
                    EventManager.EnemyHit();
                }
                if (forcefield) {
                    forcefield.GetComponent<Forcefield>().CheckForcefield();
                }
            } else {
                EventManager.EnemyHit();
            }

            invuln = true;
            Timing.RunCoroutine(_IFrames());
        }
    }

    private IEnumerator<float> _IFrames() {
        yield return Timing.WaitForSeconds(invulnDuration);
        invuln = false;
    }

    public void FieldLink(GameObject field, Color fieldColor) {
        forcefield = field;
        lr = gameObject.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.widthCurve = AnimationCurve.Constant(0, 1, 0.15f);
        lr.material = field.GetComponent<LineRenderer>().material;
        lr.startColor = fieldColor;
        lr.endColor = Color.clear;

        Vector3 pos = transform.position;
        Vector3 forcefieldPos = forcefield.transform.position;
        Vector3[] positions = new Vector3[2] {forcefieldPos, pos};

        lr.SetPositions(positions);

    }

    public void MobilityChange(bool mobility) {
        // must have a forcefield
        if (forcefield) {
            // update the positions when mobile, stop when not
            if (mobility) {
                Timing.RunCoroutine(_FieldUplink(), "u");
            } else {
                Timing.KillCoroutines("u");
            }
        }
    }

    private IEnumerator<float> _FieldUplink() {
        // calculate positions
        Vector3 pos = transform.position;
        Vector3 forcefieldPos = forcefield.transform.position;
        Vector3[] positions = new Vector3[2] {forcefieldPos, pos};

        // update positions while active
        while (gameObject.activeSelf) {
            positions[0] = transform.position;
            lr.SetPositions(positions);
            yield return Timing.WaitForOneFrame;
        }
    }
}
