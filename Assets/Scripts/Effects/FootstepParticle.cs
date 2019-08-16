using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepParticle : MonoBehaviour
{
    public float playerVelocity = 0f;
    public ParticleSystem pSystem = null;

    private void Start()
    {
        pSystem = GetComponentInChildren<ParticleSystem>();
    }

    private void Update()
    {
        if(playerVelocity >= .1f) 
        {
            if (!pSystem.isPlaying)
                pSystem.Play();
        }
        else
            if (pSystem.isPlaying)
                pSystem.Stop();
    }
}
