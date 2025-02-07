using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using MEC;

public class PlayerMovement : MonoBehaviour {

    // Player components
    [HideInInspector] public Rigidbody2D rb;
    private PlayerInput playerInput; // the player input component
    private InputAction playerMove; // the player move action

    // Fuel system
    [SerializeField] private int startingMaxFuel;
    private float fuel; // current fuel level (depleted at 1/s)
    private int maxFuel; // maximum fuel
    private Slider fuelBarSlider; // slider showing fuel level

    // Movement system
    [SerializeField] private float fastAccel; // acceleration at speed 0 to f (b)
    [SerializeField] private float softMaxSpeed; // the speed at which fast-accel stops (f)
    [SerializeField] private float slowAccel; // acceleration at speed f to m (d)
    [SerializeField] private float maxSpeed; // speed cap (m)
    [SerializeField] private float brakeConstant; // the multiplier for braking
    [SerializeField] private float decelConstant; // the multiplier for deceleration
    [SerializeField] private float decelThreshold; // above what velocity the deceleration can occur

    private bool dashQueued = false;
    private bool dashEnding = false;

    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashAccel;
    [SerializeField] private float dashDuration;

    [SerializeField] private float deathSequenceTime;

    // Spawnpoint
    private Vector3 startingOrigin = new Vector3(0, 2, 0); // the player's initial spawnpoint
    private Vector3 origin; // the player's spawnpoint

    // GameController ref
    private GameController gControl;

    [SerializeField] private GameObject playerRespawn;

    void Start() {
        fuelBarSlider = GameObject.FindWithTag("FuelBar").GetComponent<Slider>();
        gControl = GameObject.FindWithTag("GameController").GetComponent<GameController>();

        rb = GetComponent<Rigidbody2D>();

        playerInput = GetComponent<PlayerInput>();
        playerMove = playerInput.actions.FindAction("Move");

        EventManager.onPlayerDeath += DeathSequence;

        EventManager.onNewUniverse += InitializeMovement;
        EventManager.onSetSpawn += SetSpawn;
    }

    public void InitializeMovement() {
        origin = startingOrigin;
        maxFuel = startingMaxFuel;
        SetFuel(maxFuel);
        fuelBarSlider.maxValue = maxFuel;
        fuelBarSlider.value = fuel;
    }

    // regain fuel
    public void SetFuel(float fuelToGain) {
        fuel += fuelToGain;
        if (fuel >= maxFuel) {
            fuel = maxFuel;
        }
        fuelBarSlider.value = fuel;
    }

    public void IncreaseMaxFuel(int fp) {
        maxFuel += fp;
        fuelBarSlider.maxValue = maxFuel;
        SetFuel(fp);
    }

    public void SetSpawn(Vector3 spawn) {
        origin = spawn;
    }

    public void QueueDash() {
        Timing.RunCoroutine(_Dash());
    }

    private IEnumerator<float> _Dash() {
        Vector2 normMoveDir = playerMove.ReadValue<Vector2>().normalized;
        float speed = rb.velocity.magnitude;

        // if pressing anything, dash in direction of press
        if (normMoveDir.magnitude > 0.5) {
            dashQueued = true; // lets FixedUpdate know to dash
            if (speed > dashSpeed) {
                rb.velocity = speed * normMoveDir;
            } else {
                rb.velocity = dashSpeed * normMoveDir;
            }
            yield return Timing.WaitForSeconds(dashDuration / 2);
            dashEnding = true;
            dashQueued = false;
            yield return Timing.WaitForSeconds(dashDuration / 2);
            dashEnding = false;
        }
    }

    void FixedUpdate() {

        Vector2 moveDir = playerMove.ReadValue<Vector2>(); // get player input
        Vector2 normMoveDir = moveDir.normalized; // normalized player input vector
        Vector2 normVel = rb.velocity.normalized; // normalized player velocity

        // check if player is over the speed limit, and limit them if so
        /*float speedDifference = rb.velocity.magnitude - maxSpeed;
        if (speedDifference > 0 && !dashQueued) {
            rb.AddForce(-normVel * slowAccel * 2, ForceMode2D.Impulse);
        }*/
        if (normMoveDir == Vector2.zero && rb.velocity.magnitude > decelThreshold) {
            rb.AddForce(-normVel * decelConstant, ForceMode2D.Impulse);
        } else if (!dashQueued && !dashEnding) {
            // consider x and y axes seperately
            for (int axis = 0; axis <= 1; axis++) {
                Vector2 acceleration = Vector2.zero;

                float moveComponent = normMoveDir[axis];
                float velComponent = rb.velocity[axis];
                
                if (moveComponent * velComponent >= 0) { // if same dir
                    acceleration[axis] = accelCurve(Mathf.Abs(rb.velocity[axis])) * moveComponent;
                    rb.AddForce(acceleration, ForceMode2D.Impulse);
                } else { // opposite dir
                    acceleration[axis] = brakeConstant * fastAccel * moveComponent;
                    rb.AddForce(acceleration, ForceMode2D.Impulse);
                }
            }
        } else if (dashQueued) {
            rb.AddForce(dashAccel * normVel, ForceMode2D.Impulse);
        } else {
            rb.AddForce(-dashAccel * normVel, ForceMode2D.Impulse);
        }
        
        // spend fuel
        if ((moveDir != Vector2.zero) && !dashQueued && !dashEnding) {
            fuel -= Time.deltaTime; // spend 1 fuel/sec during movement
            fuelBarSlider.value = fuel;
            if (fuel <= 0) {
                EventManager.PlayerDeath();
                gControl.crackBar(fuelBarSlider);
            }
        }
    }

    // helper function that returns the amount to accelerate defined by a piecewise function
    private float accelCurve(float speed) {
        // this is a piecewise constant function
        if (speed <= softMaxSpeed) {
            return fastAccel;
        } else if (speed <= maxSpeed) {
            return slowAccel;
        } else {
            return 0;
        }
    }

    // the player dies and their movement is suspended
    public void DeathSequence() {
        playerInput.actions.FindActionMap("Player").Disable();
        Timing.RunCoroutine(_DeathTransport());
    }

    private IEnumerator<float> _DeathTransport() {
        yield return Timing.WaitForSeconds(gControl.timeToMove);
        gameObject.GetComponent<Animator>().SetTrigger("Respawn");
        transform.position = origin;
        rb.velocity = Vector2.zero;
        GameObject pr = Instantiate(playerRespawn, transform.position, Quaternion.identity);
        yield return Timing.WaitForSeconds(gControl.timeToRespawn);
        EventManager.PlayerRespawn();
        RespawnSequence(pr);
    }

    public void RespawnSequence(GameObject pr) {
        Destroy(pr);
        SetFuel(maxFuel);
        gControl.uncrackBar(fuelBarSlider);
        playerInput.actions.FindActionMap("Player").Enable();
    }
}