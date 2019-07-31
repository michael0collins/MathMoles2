using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepParticle : MonoBehaviour
{
    public float playerVelocity = 0f;
    public ParticleSystem particleSystem = null;

    private void Start()
    {
        particleSystem = GetComponentInChildren<ParticleSystem>();
    }

    private void Update()
    {
        if(playerVelocity >= .1f) 
        {
            if (!particleSystem.isPlaying)
                particleSystem.Play();
        }
        else
            if (particleSystem.isPlaying)
                particleSystem.Stop();
    }
}
