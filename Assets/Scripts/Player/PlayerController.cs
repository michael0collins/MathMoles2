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

    public float attackDistance = 2f;
    public float attackFieldSize = 1f;

    public PlayerAnimationController playerAnimationController;

    [Header("Debug")]
    public bool debugMode = false;

    private Rigidbody rb;
    private Joystick joyStick;
    public NetworkPlayer networkPlayer { get; private set; }
    public bool isGrounded { get; private set; }
    public float currentSpeed { get; private set; } = 0f;

    [HideInInspector]
    public float AnimationSpeed = 0f;

    private void Awake()
    {
        networkPlayer = GetComponent<NetworkPlayer>();
        joyStick = FindObjectOfType<Joystick>();
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        isGrounded = RayCastCollisionCheck(transform.position, Vector3.down, .7f, "Ground");

        if (networkPlayer.isLocal || debugMode)
        {
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

            if (Input.GetKeyDown(KeyCode.Space))
                Jump();
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

    bool RayCastCollisionCheck(Vector3 position, Vector3 direction, float distance, string layer)
    {
        RaycastHit hit;
        return Physics.Raycast(position, direction, out hit, distance, 1 << LayerMask.NameToLayer(layer));
    }

    public void GetKnockedBack(Vector3 position)
    {
        rb.AddForce((transform.position - position) * attackForce);
    }

    public void Jump()
    {
        if (isGrounded)
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y + jumpForce, rb.velocity.z);
    }

    public void Attack()
    {
        if (!isGrounded)
            return;

        playerAnimationController.AttackTrigger();
        if (networkPlayer.isLocal) {
            RaycastHit hit;
            if (Physics.SphereCast(transform.position, attackFieldSize, transform.forward, out hit, attackDistance))
            {
                if (hit.transform.gameObject.tag == "Player")
                    networkPlayer.SendHitData(hit.transform.GetComponent<NetworkPlayer>(), hit.point);
            } else {
                networkPlayer.SendFailedHitData();
            }
        }
    }
}
