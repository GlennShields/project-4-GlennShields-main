using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // If a player touched the trigger
        var player = other.GetComponent<Player>();
        if (player != null )
        {
            // Give them the weapon
            player.hasWeapon = true;
        }
    }
}
