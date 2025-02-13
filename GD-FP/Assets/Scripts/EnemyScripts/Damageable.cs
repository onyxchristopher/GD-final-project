using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MEC;

public class Damageable : MonoBehaviour
{

    private GameController gameController;
    public int maxHealth;
    [HideInInspector] public int health;
    [SerializeField] private bool isBoss;
    private bool invuln = false;
    private float invulnDuration = 0.3f;
    public Enemy enemy;
    private GameObject linkedForcefield;
    private LineRenderer lr;
    public GameObject protectiveForcefield;
    [SerializeField] private int lineSortingOrder;
    [SerializeField] private GameObject healthbar;
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0, 3.5f, 0);
    private Slider healthBarSlider;

    void Start() {
        gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();

        health = maxHealth;
    }
    
    public void Damage(int damage, bool suppressSound = false) {
        if (!invuln || !protectiveForcefield){
            health -= damage;

            // a normal enemy should have hit sounds at all times
            // a boss should have hit sounds but not on death
            if (isBoss) {
                gameController.SetBossHealthBar(health);
            }

            if (health <= 0) {
                enemy.EnemyDeath();
                if (!isBoss && !suppressSound) {
                    EventManager.EnemyHit();
                }
                if (linkedForcefield) {
                    linkedForcefield.GetComponent<Forcefield>().CheckForcefield();
                }
            } else {
                if (!isBoss) {
                    DisplayHealthbar(); // display healthbar if non-boss not dead
                }
                if (!suppressSound) {
                    EventManager.EnemyHit();
                }
            }

            invuln = true;
            Timing.RunCoroutine(_IFrames());
        }
    }

    private IEnumerator<float> _IFrames() {
        yield return Timing.WaitForSeconds(invulnDuration);
        invuln = false;
    }

    // Non-boss health bars

    private void DisplayHealthbar() {
        if (healthbar) {
            if (!healthBarSlider) {
                GameObject hp = Instantiate(healthbar, transform.position, Quaternion.identity,
                                            GameObject.FindWithTag("WSCanvas").transform);
                healthBarSlider = hp.GetComponent<Slider>();
                healthBarSlider.maxValue = maxHealth;
                Timing.RunCoroutine(_MoveHealthbar(hp), "movehp");
            }
            healthBarSlider.value = health;
        }
    }

    private IEnumerator<float> _MoveHealthbar(GameObject hp) {
        while (true) {
            hp.transform.position = transform.position + healthBarOffset;
            yield return Timing.WaitForOneFrame;
        }
    }
    
    // Functions for linkedForcefield linking

    public void FieldLink(GameObject field, Color fieldColor, bool drawLineToLinked) {
        if (!drawLineToLinked) {
            return;
        }
        linkedForcefield = field;
        lr = gameObject.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.widthCurve = AnimationCurve.Constant(0, 1, 0.10f);
        lr.material = field.GetComponent<LineRenderer>().material;
        Color connector = fieldColor;
        connector.a = 0.5f;
        lr.startColor = connector;
        lr.endColor = connector;
        lr.sortingOrder = lineSortingOrder;

        Vector3 pos = transform.position;
        Vector3 forcefieldPos = linkedForcefield.transform.position;
        Vector3[] positions = new Vector3[2] {forcefieldPos, pos};

        lr.SetPositions(positions);

    }

    public void MobilityChange(bool mobility) {
        // must have a linkedForcefield
        if (linkedForcefield) {
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
        Vector3 forcefieldPos = linkedForcefield.transform.position;
        Vector3[] positions = new Vector3[2] {forcefieldPos, pos};

        // update positions while active
        while (gameObject.activeSelf) {
            positions[0] = transform.position;
            lr.SetPositions(positions);
            yield return Timing.WaitForOneFrame;
        }
    }

    void OnDisable() {
        if (healthBarSlider) {
            Timing.KillCoroutines("movehp");
            Destroy(healthBarSlider.gameObject);
        }
    }
}
