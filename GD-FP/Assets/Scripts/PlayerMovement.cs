using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour {
    private float fuel; // the current fuel level (depleted at 1/s)
    [SerializeField] private int maxFuel; // the maximum fuel

    private GameController gameController;

    private Rigidbody2D rb;

    private PlayerInput playerInput;
    private InputAction playerMove;

    [SerializeField] private int moveSpeed;

    [SerializeField] private GameObject blade;
    [SerializeField] private GameObject trap;

    void Start() {
        fuel = maxFuel;
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
        float frameBalanceConst = Time.deltaTime;

        if (moveDir != Vector2.zero) {
            fuel -= frameBalanceConst; // spend 1 fuel/sec during movement
            if (fuel <= 0) {
                EventManager.PlayerDeath();
            }
        }

        rb.velocity = moveSpeed * moveDir.normalized * frameBalanceConst * 50;
    }

    
}
