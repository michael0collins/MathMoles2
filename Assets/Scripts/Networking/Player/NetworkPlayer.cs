using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;

public class ReadOnlyAttribute : PropertyAttribute
{

}

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}
#endif

public class NetworkPlayer : MonoBehaviour
{
#if UNITY_EDITOR
    [ReadOnly]
#endif
    public uint uid = 0;
#if UNITY_EDITOR
    [ReadOnly]
#endif
    public string username;
#if UNITY_EDITOR
    [ReadOnly]
#endif
    public bool isLocal;

    [HideInInspector]
    public Vector3 newPosition;
    [HideInInspector]
    public Vector3 newRotation;
    [HideInInspector]
    public Vector3 oldPosition;
    [HideInInspector]
    public Vector3 oldRotation;

    public GameObject nametagCanvas;
    public Text nametag;

    private Rigidbody rb;
    public PlayerControllerV pc { get; private set; }

    private float _oldAnimationSpeed = 0f;

    private void Awake()
    {
        pc = GetComponent<PlayerControllerV>();
        rb = GetComponent<Rigidbody>();
    }

    public void UpdateData(Vector3 position, Vector3 rotation)
    {
        newPosition = position;
        newRotation = rotation;
    }

    public void UpdateAnimationSpeed(float speed)
    {
        pc.PlayerVelocity = speed;
    }

    public void SendHitData(NetworkPlayer hittedPlayer, Vector3 position)
    {
        NetworkManager.SendLocalHitData(this, hittedPlayer, position);
    }

    public void SendFailedHitData()
    {
        NetworkManager.SendLocalFailedHitData();
    }

    public void FixedUpdate()
    {
        if (!isLocal)
        {
            transform.position = Vector3.Lerp(transform.position, newPosition, Time.fixedDeltaTime * 10f);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(newRotation), 12.5f * Time.deltaTime);
        }
        else
        {
            if (oldPosition == null)
                oldPosition = transform.position;
            if (oldRotation == null)
                oldRotation = transform.rotation.eulerAngles;

            if (Vector3.Distance(transform.position, oldPosition) >= 0.1f || Quaternion.Angle(transform.rotation, Quaternion.Euler(oldRotation)) >= 1f || Mathf.Abs(_oldAnimationSpeed - pc.PlayerVelocity) >= 0.1f)
            {
                _oldAnimationSpeed = pc.PlayerVelocity;
                oldPosition = transform.position;
                oldRotation = transform.rotation.eulerAngles;
                NetworkManager.SendLocalCharacterData(this, transform.position, transform.eulerAngles, rb.velocity.magnitude);
            }
        }

        nametagCanvas.transform.rotation = Quaternion.LookRotation(nametagCanvas.transform.position - Camera.main.transform.position);
    }
}
