using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Variables")]
    public float groundedSpeed = 3.0f;
    public float airSpeed = 2.0f;
    public float jumpForce = 4.5f;
    public float attackForce = 100f;

    public PlayerAnimationController playerAnimationController;

    [Header("Debug")]
    public bool debugMode = false;

    private Rigidbody rb;
    private NetworkPlayer networkPlayer;
    private Joystick joyStick;
    public bool isGrounded { get; private set; }
    public float currentSpeed { get; private set; } = 0f;

    public float AnimationSpeed = 0f;

    private void Awake()
    {
        networkPlayer = GetComponent<NetworkPlayer>();
        joyStick = FindObjectOfType<Joystick>();
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (networkPlayer.isLocal || debugMode)
        {
            isGrounded = RayCastCollisionCheck(transform.position, Vector3.down, .7f, "Ground");
            #if UNITY_IPHONE || UNITY_ANDROID
                ProcessUserInput(true);
            #else
                ProcessUserInput(false);
            #endif
        }
    }

    private void ProcessUserInput(bool isMobile)
    {
        Vector2 playerInput;
        if (!isMobile)
        {
            playerInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            if (Input.GetKeyDown(KeyCode.F))
                Attack();

            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
                rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y + jumpForce, rb.velocity.z);
        } else
            playerInput = new Vector2(joyStick.Horizontal, joyStick.Vertical);

        playerInput.Normalize();

        if (isGrounded)
            currentSpeed = groundedSpeed;
        else
            currentSpeed = airSpeed;

        AnimationSpeed = rb.velocity.magnitude;

        if (playerInput == Vector2.zero)
            return;

        Vector3 velocity = new Vector3(playerInput.x * currentSpeed, rb.velocity.y, playerInput.y * currentSpeed);
        rb.velocity = velocity;
        Vector3 rotVector = velocity;
        rotVector.y = 0;
        transform.forward = rotVector;
    }     

    public void Attack()
    {
        playerAnimationController.AttackTrigger();
    }

    bool RayCastCollisionCheck(Vector3 position, Vector3 direction, float distance, string layer)
    {
        RaycastHit hit;
        return Physics.Raycast(position, direction, out hit, distance, 1 << LayerMask.NameToLayer(layer));
    }

    public void GetKnockedBack(float force)
    {
        rb.AddForce(-Vector3.one * force);
    }
}
