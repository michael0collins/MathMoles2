using UnityEngine;
using UnityEngine.UI;

public class NetworkPlayer : MonoBehaviour
{
    public uint uid;
    public string username;
    public bool isLocal;

    public Vector3 newPosition;
    public Vector3 newRotation;

    public Vector3 oldPosition;
    public Vector3 oldRotation;

    public GameObject nametagCanvas;
    public Text nametag;

    private Rigidbody rb;
    private PlayerController pc;

    private void Awake()
    {
        pc = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody>();
    }

    public void UpdateData(Vector3 position, Vector3 rotation)
    {
        newPosition = position;
        newRotation = rotation;
    }

    public void UpdateAnimationSpeed(float speed)
    {
        pc.AnimationSpeed = speed;
    }

    public void FixedUpdate()
    {
        if (!isLocal)
        {
            transform.position = Vector3.Lerp(transform.position, newPosition, Time.fixedDeltaTime * 5f);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(newRotation), Time.fixedDeltaTime * 5f);
        }
        else
        {
            if (oldPosition == null)
                oldPosition = transform.position;
            if (oldRotation == null)
                oldRotation = transform.rotation.eulerAngles;

            if (Vector3.Distance(transform.position, oldPosition) >= 0.1f)
            {
                oldPosition = transform.position;
                oldRotation = transform.rotation.eulerAngles;
                NetworkManager.SendLocalCharacterData(this, transform.position, transform.eulerAngles, rb.velocity.magnitude);
            }
        }

        nametagCanvas.transform.LookAt(Camera.main.transform);
    }
}
