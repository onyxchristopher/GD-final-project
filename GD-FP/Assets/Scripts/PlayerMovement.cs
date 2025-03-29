using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using MEC;

public class PlayerMovement : MonoBehaviour
{

    // Player components
    [HideInInspector] public Rigidbody2D rb;
    public PlayerInput playerInput; // the player input component
    private InputAction playerMove; // the player move action

    // Fuel system
    [SerializeField] private int startingMaxFuel;
    private float fuel; // current fuel level (depleted at 1/s)
    [HideInInspector] public int maxFuel; // maximum fuel
    private Slider fuelBarSlider; // slider showing fuel level

    // Movement system
    [SerializeField] private float fastAccel; // Acceleration below softMaxSpeed
    [SerializeField] private float softMaxSpeed; // Speed at which acceleration slows down
    [SerializeField] private float slowAccel; // Acceleration between softMaxSpeed and maxSpeed
    [SerializeField] private float maxSpeed; // Speed at which acceleration stops
    [SerializeField] private float brakeConstant; // Multiplier for acceleration opposite to movement direction
    [SerializeField] private float decelConstant; // Multiplier for deceleration when no movement keys are pressed
    [SerializeField] private float decelThreshold; // Speed above which deceleration occurs
    [SerializeField] private float throttle; // Absolute max speed
    [SerializeField] private float dashSpeed; // Lowest possible starting speed of dash
    [SerializeField] private float dashAccel; // Acceleration and deceleration while dashing
    [SerializeField] private float dashDuration; // Duration of dash

    private bool dashQueued = false;
    private bool dashEnding = false;

    // Spawnpoint
    private Vector3 startingOrigin = new Vector3(0, 2, 0); // the player's initial spawnpoint
    private Vector3 origin; // the player's spawnpoint

    // GameController ref
    private GameController gControl;

    [SerializeField] private GameObject playerRespawn;
    [SerializeField] private GameObject playerDeath;

    void Start() {
        fuelBarSlider = GameObject.FindWithTag("FuelBar").GetComponent<Slider>();
        gControl = GameObject.FindWithTag("GameController").GetComponent<GameController>();

        rb = GetComponent<Rigidbody2D>();

        playerInput = GetComponent<PlayerInput>();
        playerMove = playerInput.actions.FindAction("Move");

        EventManager.onPlayerDeath += DeathSequence;
        EventManager.onNewUniverse += InitializeMovement;
        EventManager.onSetSpawn += SetSpawn;
        EventManager.onPlayAgain += InitializeMovement;
        EventManager.onNewGame += RestartActions;

        playerInput.actions.FindActionMap("UI").Disable();

        Timing.RunCoroutine(_EnableActions());
    }

    private IEnumerator<float> _EnableActions() {
        
        playerInput.actions.FindActionMap("Player").Disable();
        yield return Timing.WaitForSeconds(12.5f);
        playerInput.actions.FindActionMap("Player").Enable();
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

    public float GetFuel() {
        return fuel;
    }

    public void IncreaseMaxFuel(int fp) {
        maxFuel += fp;
        fuelBarSlider.maxValue = maxFuel;
        SetFuel(maxFuel);
    }

    public void SetSpawn(Vector3 spawn) {
        origin = spawn;
    }

    public void QueueDash() {
        Timing.RunCoroutine(_Dash());
    }

    private IEnumerator<float> _Dash() {
        // get normalized player input vector
        Vector2 normMoveDir = playerMove.ReadValue<Vector2>().normalized;
        float speed = rb.velocity.magnitude; // get player speed

        // if pressing any movement keys, dash in direction of keypress
        if (normMoveDir.magnitude > 0.5) {
            dashQueued = true; // lets FixedUpdate know to dash
            if (speed < dashSpeed) {
                // if below dashSpeed, dash at that speed
                rb.velocity = dashSpeed * normMoveDir;
            } else if (speed < throttle) {
                // if below throttle, dash at same speed
                rb.velocity = speed * normMoveDir;
            } else {
                // if above throttle, dash at that speed
                rb.velocity = throttle * normMoveDir;
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
        Vector2 normMoveDir = moveDir.normalized; // get normalized player input vector
        Vector2 normVel = rb.velocity.normalized; // get normalized player velocity

        // decelerate if no keys are pressed
        if (normMoveDir == Vector2.zero && rb.velocity.magnitude > decelThreshold) {
            rb.AddForce(-normVel * decelConstant, ForceMode2D.Impulse);
        } else if (!dashQueued && !dashEnding) { // while not dashing
            // consider x and y axes seperately
            for (int axis = 0; axis <= 1; axis++) {
                Vector2 acceleration = Vector2.zero;
                float moveComponent = normMoveDir[axis]; // get the component of player input
                float velComponent = rb.velocity[axis]; // get the component of player velocity
                
                if (moveComponent * velComponent >= 0) { // if components are same dir
                    // move according to the acceleration curve
                    acceleration[axis] = accelCurve(Mathf.Abs(rb.velocity[axis])) * moveComponent;
                    rb.AddForce(acceleration, ForceMode2D.Impulse);
                } else { // components are opposite dir
                    // reverse direction at a very fast constant acceleration
                    acceleration[axis] = brakeConstant * fastAccel * moveComponent;
                    rb.AddForce(acceleration, ForceMode2D.Impulse);
                }
            }
        } else if (dashQueued) { // first half of the dash
            rb.AddForce(dashAccel * normVel, ForceMode2D.Impulse);
        } else { // second half of the dash
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
        GameObject pd = Instantiate(playerDeath, transform.position, Quaternion.identity, transform);
        yield return Timing.WaitForSeconds(gControl.timeToMove);
        Destroy(pd);
        gameObject.GetComponent<Animator>().SetTrigger("Respawn");
        transform.position = origin;
        rb.velocity = Vector2.zero;
        GameObject pr = Instantiate(playerRespawn, transform.position, Quaternion.identity);
        GameObject.FindWithTag("MainCamera").GetComponent<CameraMovement>().ResetCameraSize();
        yield return Timing.WaitForSeconds(gControl.timeToRespawn);
        EventManager.PlayerRespawn();
        RespawnSequence(pr);
    }

    public void RespawnSequence(GameObject pr) {
        transform.position = origin;
        Destroy(pr);
        SetFuel(maxFuel);
        gControl.uncrackBar(fuelBarSlider);
        playerInput.actions.FindActionMap("Player").Enable();
    }

    public void DisableUIActions() {
        playerInput.actions.FindActionMap("UI").Disable();
    }

    private void RestartActions() {
        playerInput.actions.FindActionMap("Player").Enable();
    }
}