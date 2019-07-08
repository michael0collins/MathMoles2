using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayer : MonoBehaviour
{
    public uint uid;
    public string username;
    public bool isLocal;

    public Vector3 newPosition;
    public Vector3 newRotation;

    public Vector3 oldPosition;
    public Vector3 oldRotation;

    public void Awake()
    {
        newPosition = transform.position;
        newRotation = transform.rotation.eulerAngles;

        oldPosition = transform.position;
        oldRotation = transform.rotation.eulerAngles;
    }

    public void UpdateData(Vector3 position, Vector3 rotation)
    {
        newPosition = position;
        newRotation = rotation;
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
            if (Vector3.Distance(transform.position, oldPosition) <= 0.1f)
            {
                Debug.Log("1");
                oldPosition = transform.position;
                oldRotation = transform.rotation.eulerAngles;
                NetworkManager.SendLocalCharacterData(this, transform.position, transform.eulerAngles);
            }
        }
    }
}
