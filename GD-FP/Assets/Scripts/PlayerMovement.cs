using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    [SerializeField] private float maxAccel; // acceleration at speed 0 to f (b)
    [SerializeField] private float softMaxSpeed; // the speed at which fast-accel stops (f)
    [SerializeField] private float softAccel; // acceleration at speed f to m (d)
    [SerializeField] private float maxSpeed; // speed cap (m)
    [SerializeField] private float brakeConstant; // the multiplier for braking

    private bool autoDecelerate = false;
    
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

    // reset the player at their spawnpoint with max fuel
    public void ResetPlayer() {
        SetFuel(maxFuel);
        transform.position = origin;
    }

    public void SetSpawn(Vector3 spawn) {
        origin = spawn;
    }

    void FixedUpdate() {
        Vector2 moveDir = playerMove.ReadValue<Vector2>(); // get player input
        Vector2 normMoveDir = moveDir.normalized; // normalized player input vector
        Vector2 normVel = rb.velocity.normalized; // normalized player velocity

        // check if player is over the speed limit, and limit them if so
        float speedDifference = rb.velocity.magnitude - maxSpeed;
        if (speedDifference > 0) {
            rb.AddForce(-normVel * speedDifference, ForceMode2D.Impulse);
        }

        // if movedir is zero vector and autodeceleration is on
        if (autoDecelerate && normMoveDir.magnitude < 0.5) {
            // if moving
            if (normVel.magnitude > 0.001) {
                rb.AddForce(-normVel * maxAccel, ForceMode2D.Impulse);
            }
        } else {
            // loop through x and y axes
            for (int axis = 0; axis <= 1; axis++) {
                Vector2 acceleration = Vector2.zero;

                float moveComponent = normMoveDir[axis];
                float velComponent = rb.velocity[axis];
                
                if (moveComponent * velComponent >= 0) { // if same dir
                    acceleration[axis] = accelCurve(Mathf.Abs(rb.velocity[axis])) * moveComponent;
                    rb.AddForce(acceleration, ForceMode2D.Impulse);
                } else { // opposite dir
                    acceleration[axis] = brakeConstant * maxAccel * moveComponent;
                    rb.AddForce(acceleration, ForceMode2D.Impulse);
                }
            }
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
            return maxAccel;
        } else if (speed <= maxSpeed) {
            return softAccel;
        } else {
            return 0;
        }
    }
}