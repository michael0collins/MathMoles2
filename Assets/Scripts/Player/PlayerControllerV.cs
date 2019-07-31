using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControllerV : MonoBehaviour
{
    [Header("Visuals")]
    public GameObject helmetObject;

    [Header("Player Interaction")]
    public float attackCooldown = 1.0f;
    public float attackForce = 100f;
    public float attackDistance = 2f;
    public float attackFieldSize = 1f;

    [Header("Player Movement")]
    public float speed = 6.0f;
    public float airSpeed = 5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;

    //Attack cooldown
    private float loggedSwingTime;
    private bool CanSwing = true;

    private Vector3 moveDirection = Vector3.zero;

    //Buttons for mobile use.
    private Button jumpButton;
    private Button axeSwingButton;
    private Joystick joyStick;

    //Animator
    public Animator playerAnimationController;

    //Footstep Particle
    public FootstepParticle footstepParticle;

    //Network
    public NetworkPlayer NetworkPlayer { get; private set; }

    private CharacterController _characterController;

    private bool Grounded = false;
    public float PlayerVelocity = 0f;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        NetworkPlayer = GetComponent<NetworkPlayer>();
        joyStick = FindObjectOfType<Joystick>();

        loggedSwingTime = Time.time;
    }

    private void Update()
    {
        Grounded = _characterController.isGrounded;
        PlayerVelocity = _characterController.velocity.magnitude;

        if (NetworkPlayer.isLocal)
        {
            CanSwing = Time.time - loggedSwingTime > attackCooldown ? true : false;
        #if UNITY_IPHONE || UNITY_ANDROID
            Vector2 input = new Vector2(joyStick.Horizontal, joyStick.Vertical);
        #else
            Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        #endif
            input.Normalize();

            float oldYVelocity = Grounded ? 0f : moveDirection.y;
            moveDirection = new Vector3(input.x, 0f, input.y);
            moveDirection *= Grounded ? speed : airSpeed;
            moveDirection.y = oldYVelocity;

            if (Grounded)
            {
                if (Input.GetKeyDown(KeyCode.F))
                    Attack();

                if (Input.GetKeyDown(KeyCode.Space))
                    Jump();
            }
            else
                moveDirection.y -= gravity * Time.deltaTime;

            if (input != Vector2.zero)
            {
            #if UNITY_IPHONE || UNITY_ANDROID
                Vector3 rotationVector = new Vector3(joyStick.Horizontal, 0, joyStick.Vertical);
            #else
                Vector3 rotationVector = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            #endif
                if (rotationVector != Vector3.zero)
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(rotationVector), 12.5f * Time.deltaTime);
            }

            _characterController.Move(moveDirection * Time.deltaTime);
        }

        footstepParticle.playerVelocity = PlayerVelocity;
        playerAnimationController.SetFloat("MovementSpeed", PlayerVelocity);
        playerAnimationController.SetBool("Grounded", Grounded);
    }

    public void Jump()
    {
        if (Grounded)
            moveDirection.y = jumpSpeed;
    }

    public void Attack()
    {
        if (!Grounded || !CanSwing)
            return;

        loggedSwingTime = Time.time;
        if (NetworkPlayer.isLocal)
        {
            if (Physics.SphereCast(transform.position, attackFieldSize, transform.forward, out RaycastHit hit, attackDistance))
                if (hit.transform.gameObject.tag == "Player")
                {
                    playerAnimationController.SetTrigger("Attack");
                    NetworkPlayer.SendHitData(hit.transform.GetComponent<NetworkPlayer>(), hit.point);
                }
                else if (hit.transform.gameObject.tag == "GoalObject")
                {
                    //play dig animation;
                    //Reduce the goal object hit threshhold.
                    playerAnimationController.SetTrigger("Attack");
                    print("Hit goalobject");
                }
                else
                {
                    NetworkPlayer.SendFailedHitData();
                    //Missed axe attack animation.
                }
        }
    }

    public void GetKnockedBack(Vector3 position)
    {
        Vector3 direction = transform.position - position;
        moveDirection.x = direction.x;
        moveDirection.z = direction.z;
    }
}
