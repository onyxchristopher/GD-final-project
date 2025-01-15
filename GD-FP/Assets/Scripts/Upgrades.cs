﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Upgrades : MonoBehaviour
{
    private GameObject player;
    private PlayerAbilities playerAbilities;
    private PlayerCollision playerCollision;
    private PlayerMovement playerMovement;

    [SerializeField] private float upgradedBladeLength;
    [SerializeField] private float upgradedTrapCooldown;
    [SerializeField] private float upgradedShieldCooldown;
    [SerializeField] private int upgradedBladeDamage;
    [SerializeField] private int upgradedTrapDamage;
    [SerializeField] private int upgradedShieldDuration;

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        playerAbilities = player.GetComponent<PlayerAbilities>();
        playerCollision = player.GetComponent<PlayerCollision>();
        playerMovement = player.GetComponent<PlayerMovement>();

        EventManager.onArtifactPickup += Upgrade;
    }

    public void Upgrade(int id) {
        int firstDigit = id / 10;
        int secondDigit = id % 10;
        
        // major upgrades
        if (secondDigit == 0) {
            switch (firstDigit) {
                case 1:
                    playerAbilities.SetBladeLength(upgradedBladeLength);
                    break;
                case 2:
                    playerAbilities.UnlockTrap();
                    break;
                case 3:
                    playerAbilities.SetTrapCooldown(upgradedTrapCooldown);
                    break;
                case 4:
                    playerAbilities.UnlockShield();
                    break;
                case 5:
                    playerAbilities.SetShieldCooldown(upgradedShieldCooldown);
                    break;
                case 6:
                    playerAbilities.SetBladeDamage(upgradedBladeDamage);
                    break;
                case 7:
                    playerAbilities.SetTrapDamage(upgradedTrapDamage);
                    break;
                case 8:
                    playerAbilities.SetShieldDuration(upgradedShieldDuration);
                    break;
                default:
                    Debug.Log("Artifact ID unset");
                    break;
            }
        } else { // minor upgrades
            switch (secondDigit) {
                case 1:
                    playerMovement.IncreaseMaxFuel(5);
                    break;
                case 2:
                    playerCollision.IncreaseMaxHealth(2);
                    break;
                case 3:
                    playerMovement.IncreaseMaxFuel(10);
                    break;
                case 4:
                    playerCollision.IncreaseMaxHealth(5);
                    break;
            }
        }
    }
}