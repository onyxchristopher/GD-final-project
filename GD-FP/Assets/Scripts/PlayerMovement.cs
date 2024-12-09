using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour {
    
    private float fuel; // the current fuel level (depleted at 1/s)
    [SerializeField] private int maxFuel; // the maximum fuel
    private Slider fuelBarSlider;

    private GameController gameController;

    [HideInInspector] public Rigidbody2D rb;

    [SerializeField] private float maxSpeed;

    private PlayerInput playerInput;
    private InputAction playerMove;

    [SerializeField] private float moveSpeed;

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

        Debug.Log(rb.velocity.magnitude);

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

// F = a
// F = v/s
// F = 0.02v/0.02s
