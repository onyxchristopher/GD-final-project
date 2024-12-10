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
    private float fuel; // current fuel level (depleted at 1/s)
    [SerializeField] private int maxFuel; // maximum fuel
    private Slider fuelBarSlider; // slider showing fuel level

    // Script references
    private GameController gameController;

    // Movement system
    [SerializeField] private float moveSpeed; // how fast the player accelerates
    [SerializeField] private float maxSpeed; // how fast the player can be
    private Vector3 origin; // the player's spawnpoint

    void Start() {
        fuel = maxFuel;
        fuelBarSlider = GameObject.FindWithTag("FuelBar").GetComponent<Slider>();
        fuelBarSlider.maxValue = maxFuel;
        fuelBarSlider.value = fuel;

        gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();
        rb = GetComponent<Rigidbody2D>();

        playerInput = GetComponent<PlayerInput>();
        playerMove = playerInput.actions.FindAction("Move");

        EventManager.onPlayerDeath += ResetPlayer;
    }

    // reset the player at their spawnpoint with max fuel
    public void ResetPlayer() {
        Debug.Log("resetted");
        SetFuel(maxFuel);
        transform.position = origin;
    }

    // regain fuel
    public void SetFuel(float fuelToGain) {
        fuel += fuelToGain;
        if (fuel >= maxFuel) {
            fuel = maxFuel;
        }
    }

    void FixedUpdate() {
        Vector2 moveDir = playerMove.ReadValue<Vector2>(); // get player input
        Vector2 normalMoveDir = moveDir.normalized; // normalize player input vector

        rb.AddForce(moveSpeed * normalMoveDir, ForceMode2D.Impulse); // accelerate

        // check if player is over the speed limit, and limit them if so
        float speedDifference = rb.velocity.magnitude - maxSpeed;
        if (speedDifference > 0) {
            rb.AddForce(-rb.velocity.normalized * speedDifference, ForceMode2D.Impulse);
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
}