using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepParticle : MonoBehaviour
{
    private ParticleSystem ps;
    private PlayerController pc;

    private void Start()
    {
        ps = GetComponentInChildren<ParticleSystem>();
        pc = GetComponentInParent<PlayerController>();
    }

    private void Update()
    {
        if(pc.AnimationSpeed >= .1f) 
        {
            if (!ps.isPlaying)
                ps.Play();
        }
        else
            if (ps.isPlaying)
                ps.Stop();
    }
}
