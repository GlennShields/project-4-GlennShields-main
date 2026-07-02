using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float force = 10f;
    // Start is called before the first frame update
    void Start()
    {
        // Calculate the force based on our forward direction
        var forceInDirection = force * transform.forward;

        // Add the force to the projectile
        GetComponent<Rigidbody>().AddForce(forceInDirection, ForceMode.Impulse);
    }
}
