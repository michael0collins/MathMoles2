using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    //Public Fields
    [Header("Player Movement")]
    public float groundedSpeed = 3.0f;
    public float airSpeed = 1.0f;
    public float fallMultiplier = 3.5f;
    public float lowJumpMultiplier = 2.5f;
    public float jumpForce = 9.0f;
    public bool jumpButtonIsPressed = false;
    public bool IsGrounded { get; private set; }
    public float CurrentSpeed { get; private set; } = 0f;

    [Header("Player Interaction")]
    public float attackCooldown = 1.0f;
    public float attackForce = 100f;
    public float attackDistance = 2f;
    public float attackFieldSize = 1f;
    public float jumpedOnHeadForce = 100f;
    
    //Attack Cooldown
    private float loggedSwingTime;
    private bool canSwing;

    //Buttons for mobile use.
    private Button jumpButton;
    private Button axeSwingButton;

    //Helmet Colors
    public GameObject helmetObject;

    //Animation
    [HideInInspector]
    public float AnimationSpeed = 0f;
    public PlayerAnimationController playerAnimationController;

    //Network
    public NetworkPlayer NetworkPlayer { get; private set; }

    [Header("Debug")]
    public bool debugMode = false;

    //Private Fields
    private Rigidbody rb;
    private Joystick joyStick;

    private void Awake()
    {
        //#if UNITY_IPHONE || UNITY_ANDROID
        //    PopulateMobileActionButtons
        //#endif

        NetworkPlayer = GetComponent<NetworkPlayer>();
        joyStick = FindObjectOfType<Joystick>();
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        IsGrounded = RayCastCollisionCheck(transform.position, Vector3.down, .7f, "Ground");
        canSwing = Time.time - loggedSwingTime > attackCooldown ? true : false;

        if (NetworkPlayer.isLocal || debugMode)
        {
            #if UNITY_IPHONE || UNITY_ANDROID
                ProcessUserInput(true);
            #else
                ProcessUserInput(false);
            #endif
            RaycastHit playerHeadOnJumped = RayCastPlayerHeadOnCheck(transform.position, Vector3.down, .25f, "Player");
            if (playerHeadOnJumped.collider != null)
            {
                Debug.Log("Above player");
                rb.AddForce((transform.up + (transform.forward / 4)) * jumpedOnHeadForce);
            }
        }
    }

    #region InputHandling
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

            JumpForceModifier();
        }
        else
            playerInput = new Vector2(joyStick.Horizontal, joyStick.Vertical);

        playerInput.Normalize();

        if (IsGrounded)
            CurrentSpeed = groundedSpeed;
        else
            CurrentSpeed = airSpeed;

        AnimationSpeed = 0;

        if (playerInput == Vector2.zero)
            return;

        AnimationSpeed = rb.velocity.magnitude;

        Vector3 velocity = new Vector3(playerInput.x * CurrentSpeed, rb.velocity.y, playerInput.y * CurrentSpeed);
        rb.velocity = velocity;
        Vector3 rotVector = velocity;
        rotVector.y = 0;
        transform.forward = rotVector;
    }

    private void PopulateMobileActionButtons()
    {
        if (GameObject.FindGameObjectWithTag("JumpButton").GetComponent<Button>() != null)
            jumpButton = GameObject.FindGameObjectWithTag("JumpButton").GetComponent<Button>();
        else
            Debug.Log("No button found with tag 'JumpButton'");

        if (GameObject.FindGameObjectWithTag("").GetComponent<Button>() != null)
            axeSwingButton = GameObject.FindGameObjectWithTag("SwingAxeButton").GetComponent<Button>();
        else
            Debug.Log("No button found with tag 'SwingAxeButton'");

        jumpButton.onClick.AddListener(Jump);
        axeSwingButton.onClick.AddListener(Attack);
    }
    #endregion

    #region Movement&Spatial
    public void Jump()
    {
        if (IsGrounded)
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y + jumpForce, rb.velocity.z);
    }

    private void JumpForceModifier()
    {
        if (rb.velocity.y < 0)
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        else if (rb.velocity.y > 0 && !Input.GetKeyDown(KeyCode.Space))
            rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        else if (rb.velocity.y > 0 && GameManager.IsJumpButtonDown())
            rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
    }

    public void GetKnockedBack(Vector3 position)
    {
        rb.AddForce((transform.position - position) * attackForce);
    }

    bool RayCastCollisionCheck(Vector3 position, Vector3 direction, float distance, string layer)
    {
        return Physics.Raycast(position, direction, out RaycastHit hit, distance, 1 << LayerMask.NameToLayer(layer));
    }

    RaycastHit RayCastPlayerHeadOnCheck(Vector3 position, Vector3 direction, float distance, string layer)
    {
        Physics.SphereCast(position, 0.3f, direction, out RaycastHit hit, distance, 1 << LayerMask.NameToLayer(layer));
        return hit;
    }
    #endregion

    #region Attack&Interaction
    public void Attack()
    {
        if (!IsGrounded || !canSwing)
            return;

        loggedSwingTime = Time.time;
        if (NetworkPlayer.isLocal)
        {
            if (Physics.SphereCast(transform.position, attackFieldSize, transform.forward, out RaycastHit hit, attackDistance))
                if (hit.transform.gameObject.tag == "Player")
                {
                    playerAnimationController.AttackTrigger();
                    NetworkPlayer.SendHitData(hit.transform.GetComponent<NetworkPlayer>(), hit.point);
                }
                else if (hit.transform.gameObject.tag == "GoalObject")
                {
                    //play dig animation;
                    //Reduce the goal object hit threshhold.
                    print("Hit goalobject");
                }
                else
                {
                    NetworkPlayer.SendFailedHitData();
                    //Missed axe attack animation.
                }
        }
    }
    #endregion
}
