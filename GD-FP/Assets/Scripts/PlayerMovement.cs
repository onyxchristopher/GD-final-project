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

    // Fuel
    private float fuel; // current fuel level (depleted at 1/s)
    [SerializeField] private int maxFuel; // maximum fuel
    private Slider fuelBarSlider; // slider showing fuel level

    // Script references
    private GameController gameController;

    // Movement parameters
    [SerializeField] private float moveSpeed;
    [SerializeField] private float maxSpeed;

    // Attack prefabs
    [SerializeField] private GameObject blade;
    [SerializeField] private GameObject trap;

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

    public void ResetPlayer() {
        Debug.Log("resetted");
        SetFuel(maxFuel);
    }

    public void SetFuel(float fuelToGain) {
        fuel += fuelToGain;
        if (fuel >= maxFuel) {
            fuel = maxFuel;
        }
    }

    void FixedUpdate() {
        Vector2 moveDir = playerMove.ReadValue<Vector2>();
        Vector2 normalMoveDir = moveDir.normalized;

        rb.AddForce(moveSpeed * normalMoveDir, ForceMode2D.Impulse);

        float speedDifference = rb.velocity.magnitude - maxSpeed;
        if (speedDifference > 0) {
            rb.AddForce(-rb.velocity.normalized * speedDifference, ForceMode2D.Impulse);
        }
        
        if (moveDir != Vector2.zero) {
            fuel -= Time.deltaTime; // spend 1 fuel/sec during movement
            fuelBarSlider.value = fuel;
            if (fuel <= 0) {
                EventManager.PlayerDeath();
            }
        }
    }
}