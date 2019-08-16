using UnityEngine;
using UnityEngine.UI;

public class PlayerControllerV : MonoBehaviour
{
    [Header("Visuals")]
    public GameObject helmetObject;

    [Header("Player Attack")]
    public float attackCooldown = 1.0f;
    public float attackForce = 100f;
    public float attackDistance = 2f;
    public float attackFieldSize = 1f;

    [Header("Player Jumping on Heads")]
    public float jumpedOnCheckDistance = 1f;
    public float jumpedOnCheckRadius = 0.5f;
    public LayerMask playersMask;

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

    public bool Knocked { get; private set; } = false;
    public float KnockDecreaseRate = 0.1f;
    
    private float _freezeTime;
    private Vector3 _knockDirection = Vector3.zero;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        NetworkPlayer = GetComponent<NetworkPlayer>();
        joyStick = FindObjectOfType<Joystick>();

        loggedSwingTime = Time.time;

        _characterController.enabled = false;
        _freezeTime = Time.time;
    }

    private void FixedUpdate()
    {
        if (Time.time - _freezeTime > 1f)
            _characterController.enabled = true;

        Grounded = Physics.Raycast(transform.position, -transform.up, _characterController.height / 2f);

        if (NetworkPlayer.isLocal && _characterController.enabled)
        {
            CanSwing = Time.time - loggedSwingTime > attackCooldown ? true : false;
        #if UNITY_IPHONE || UNITY_ANDROID
            Vector2 input = new Vector2(joyStick.Horizontal, joyStick.Vertical);
        #else
            Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        #endif
            input.Normalize();

            float oldYVelocity = Grounded ? 0f : moveDirection.y;

            if (input != Vector2.zero && !Knocked)
                moveDirection = new Vector3(input.x, 0f, input.y);
            else if (Knocked) {
                moveDirection = new Vector3(_knockDirection.x, 0f, _knockDirection.z);
                if (_knockDirection.x > 0)
                    _knockDirection.x -= KnockDecreaseRate;
                else
                    _knockDirection.x += KnockDecreaseRate;

                if (_knockDirection.z > 0)
                    _knockDirection.z -= KnockDecreaseRate;
                else
                    _knockDirection.z += KnockDecreaseRate;

                if (Mathf.Abs(_knockDirection.x) <= 0.25f && Mathf.Abs(_knockDirection.z) <= 0.25f)
                    Knocked = false;
            }
            else
                moveDirection = Vector3.zero;

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

            PlayerVelocity = _characterController.velocity.magnitude;

            RaycastHit hit;
            if (Physics.SphereCast(transform.position, jumpedOnCheckRadius,-transform.up, out hit, jumpedOnCheckDistance, playersMask)) {
                moveDirection.y = 5f;
            }
        }

        footstepParticle.isGrounded = Grounded;
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
                    NetworkPlayer.SendGoalHitData(hit.transform.gameObject.GetComponent<GoalObject>().goalIndex);
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
        direction *= attackForce;
        _knockDirection.x = direction.x;
        _knockDirection.z = direction.z;
        Knocked = true;
    }
}
