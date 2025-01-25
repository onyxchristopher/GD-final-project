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

    private bool dashQueued = false;
    private bool dashEnding = false;

    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashAccel;
    [SerializeField] private float dashDuration;
    
    private Vector3 startingOrigin = new Vector3(0, 0.5f, 0);
    private Vector3 origin; // the player's spawnpoint

    void Start() {
        fuelBarSlider = GameObject.FindWithTag("FuelBar").GetComponent<Slider>();

        rb = GetComponent<Rigidbody2D>();

        playerInput = GetComponent<PlayerInput>();
        playerMove = playerInput.actions.FindAction("Move");

        EventManager.onPlayerDeath += ResetPlayer;
        origin = transform.position;

        EventManager.onNewUniverse += InitializeMovement;
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

    public void IncreaseMaxFuel(int max) {
        maxFuel += max;
        fuelBarSlider.maxValue = maxFuel;
    }

    // reset the player at their spawnpoint with max fuel
    public void ResetPlayer() {
        SetFuel(maxFuel);
        transform.position = origin;
        rb.velocity = Vector2.zero;
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

        if (!dashQueued && !dashEnding) {
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
        if (moveDir != Vector2.zero) {
            fuel -= Time.deltaTime; // spend 1 fuel/sec during movement
            fuelBarSlider.value = fuel;
            if (fuel <= 0) {
                EventManager.PlayerDeath();
            }
        }
    }

    // helper function that defines the piecewise function that controls acceleration
    // returns the amount to accelerate
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
}