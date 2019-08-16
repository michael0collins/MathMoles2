using UnityEngine;
using System.Collections;

// src https://gist.github.com/ftvs/5822103
public class CameraShake : MonoBehaviour
{
    private static CameraShake Instance;
	// Transform of the camera to shake. Grabs the gameObject's transform
	// if null.
	public Transform camTransform;
	
	// How long the object should shake for.
	public float shakeDuration = 0f;
	
	// Amplitude of the shake. A larger value shakes the camera harder.
	public float shakeAmount = 0.7f;
	public float decreaseFactor = 1.0f;
	
	Vector3 originalPos;
	
    public static void ShakeCamera(float duration, float amount, float decreaseFactor) {
        Instance.shakeAmount = amount;
        Instance.decreaseFactor = decreaseFactor;
        Instance.shakeDuration = duration;
    }

	void Awake()
	{
        Instance = this;

		if (camTransform == null)
			camTransform = GetComponent(typeof(Transform)) as Transform;
	}
	
	void OnEnable()
	{
		originalPos = camTransform.localPosition;
	}

	void Update()
	{
		if (shakeDuration > 0)
		{
			camTransform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;
			
			shakeDuration -= Time.deltaTime * decreaseFactor;
		}
		else
		{
			shakeDuration = 0f;
			camTransform.localPosition = originalPos;
		}
	}
}