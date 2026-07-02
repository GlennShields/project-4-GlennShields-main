using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private bool jumpPressed;
    public LayerMask groundLayer;
    private Animator animator;
    public AudioClip landClip;
    public AudioClip jumpClip;
    private AudioSource audioSource;
    private Rigidbody rigidBody;
    public float jumpForce;
    private bool isSprinting;
    public int health = 100;
    public bool isAlive { get => health > 0; }

    public bool hasWeapon = false;
    public GameObject projectile;
    private CapsuleCollider capsuleCollider;
    private Vector2 desiredMovement;
    private Vector2 inputMovement;

    // Camera
    public Transform cameraTransform;
    public float cameraDistance = 5f;
    private float cameraYaw = 0f;
    private float cameraPitch = 0f;
    public float lookingSpeed = 0.25f;

    // Start is called before the first frame update
    void Start()
    {
        // Get all of the components necessary at the start
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        rigidBody = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();

    }

    // Update is called once per frame
    void Update()
    {
        HandleLooking();
    }

    // FixedUpdate is called once per fixed frame
    private bool wasInAir = true;
    private Vector3 momentum = Vector3.zero;
    private float speed = 1f;

    private void FixedUpdate()
    {
        // If I'm dead, don't do anything
        if (!isAlive) return;

        // Smoothly follow input
        desiredMovement = Vector2.Lerp(desiredMovement, inputMovement, 5f * Time.fixedDeltaTime);

        var isTouchingFloor = IsTouchingGround();
        var isMoving = desiredMovement.magnitude > 0f;

        // Update the animator with all the details of what the player is doing
        animator.SetBool("isMoving", isMoving);
        animator.SetFloat("xDir", desiredMovement.x);
        animator.SetFloat("yDir", desiredMovement.y);
        animator.SetBool("isTouchingFloor", isTouchingFloor);

        // If we are touching the floor
        if (isTouchingFloor)
        {
            // Move the rigidbody according to the animator's velocity
            var rigidbodyVelocityY = rigidBody.linearVelocity.y;
            var animVelocity = animator.velocity;
            animVelocity.y = rigidbodyVelocityY;
            rigidBody.linearVelocity = animVelocity;
        }

        // If I pressed jump and I was touching the floor
        if (jumpPressed && isTouchingFloor)
        {
            // Get the momentum we had from our animation
            momentum = animator.velocity;
            jumpPressed = false;
            
            // Set the jump trigger and add a force to the rigidbody
            animator.SetTrigger("jumpTrigger");
            rigidBody.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
        }

        // If I was not in the air previous, but now I'm NOT touching the floor
        if (!wasInAir && !isTouchingFloor)
        {
            // Add the momentum we had from our animation
            // This allows us to do a running jump
            rigidBody.AddForce(momentum, ForceMode.VelocityChange);
            audioSource.PlayOneShot(jumpClip);
            wasInAir = true;
        }

        // If I was in the air and now I'm touching the floor
        if (wasInAir && isTouchingFloor)
        {
            // Play the land sound
            audioSource.PlayOneShot(landClip);
            wasInAir = false;
        }

        // If I am sprinting and I am touching the floor
        if (isSprinting && isTouchingFloor)
        {
            // Gain speed but clamp it between 1 and 2
            speed += 0.025f;
            speed = Mathf.Clamp(speed, 1f, 2f);
        }
        // If I'm not spriting, or I'm in the air,
        else
        {
            // Lose speed but clamp it between 1 and 2
            speed -= 0.025f;
            speed = Mathf.Clamp(speed, 1f, 2f);
        }

        // Set the animators speed (only used for walking vs sprinting)
        animator.SetFloat("speed", speed);
    }

    private void OnAnimatorMove()
    {
        // This is a trick
        // Override the animator's movement by using the rigidbody's velocity
        transform.position += rigidBody.linearVelocity * Time.deltaTime;
    }

    public void RotatePlayer(Quaternion rotation)
    {
        // If we're dead, don't allow rotating the player
        if (!isAlive) return;
        transform.rotation = rotation;
    }

    public void DamagePlayer(int damage)
    {
        // Remove the damage from players health
        var wasAlive = isAlive;
        health -= damage;

        // If I'm still alive and have taken damage
        if (isAlive)
        {
            // Play the flinch animation
            animator.SetTrigger("TakeDamage");
        }

        // If I'm dead and but I was previously alive
        if (!isAlive && wasAlive)
        {
            // Play the death animation
            animator.SetTrigger("Dead");
        }
    }

    public void OnJump()
    {
        // If we pressed space, we want to jump
        jumpPressed = true;
    }

    public void OnMenu()
    {
        // If you press the escape key
        // Enable the menu
    }

    public void OnSprint(InputValue value)
    {
        isSprinting = value.isPressed;
    }

    public void OnMove(InputValue value)
    {
        // desiredMovement has 2 directions, X and Y, for horizontal and vertical
        inputMovement = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        // Lock the cursor on the screen
        Cursor.lockState = CursorLockMode.Locked;

        // Mouse input
        var look = value.Get<Vector2>();

        // Update angles
        cameraYaw += lookingSpeed * look.x;
        cameraPitch -= lookingSpeed * look.y;

        // Prevent our camera from going upside down
        cameraPitch = Mathf.Clamp(cameraPitch, -20f, 140f);
    }

    private void HandleLooking()
    {
        // Control where the player is looking
        // Calculate rotation and offset
        var lookRotation = Quaternion.Euler(cameraPitch, cameraYaw, 0);
        var offset = lookRotation * new Vector3(0f, -10f, -cameraDistance);

        // Apply position and rotation
        cameraTransform.position = transform.position + offset;
        cameraTransform.LookAt(transform.position);

        // Only rotate if alive
        if (isAlive) transform.rotation = Quaternion.Euler(new Vector3(0f, cameraTransform.eulerAngles.y, 0f));
    }

    public void OnAttack()
    {
        // If I'm alive and I have a weapon and I press the mouse button
        if (isAlive && hasWeapon)
        {
            // Create a new projectile (destroy after 5 seconds)
            var instantiatedProjectile = Instantiate(projectile);
            Destroy(instantiatedProjectile, 5f);
            instantiatedProjectile.transform.SetPositionAndRotation(transform.position + transform.forward + new Vector3(0f, 1.5f, 0f), Camera.main.transform.rotation * Quaternion.Euler(-5f, 0f, 0f));

            // Play a throw animation
            animator.SetTrigger("Throw");
        }
    }

    private bool IsTouchingGround()
    {
        // Check if we're touching the floor using a sphere under the player
        var sphereRadius = 0.1f;

        // Check slightly below the player's current position
        var checkPosition = transform.TransformPoint(capsuleCollider.center); // Calculate the capsule's center world position
        checkPosition.y = checkPosition.y - (capsuleCollider.height / 2f) - sphereRadius;

        // Returns true if there are any collisions with groundLayer at this position
        return Physics.CheckSphere(checkPosition, sphereRadius, groundLayer);
    }
}