using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepParticle : MonoBehaviour
{
    private ParticleSystem ps;
    private Rigidbody rb;
    private PlayerController pc;
    private float groundHeight = 1000;

    private void Start()
    {
        ps = GetComponentInChildren<ParticleSystem>();
        pc = GetComponentInParent<PlayerController>();
        rb = GetComponentInParent<Rigidbody>();
    }

    private void Update()
    {
        if (groundHeight == 1000 && pc.isGrounded)
            groundHeight = pc.transform.position.y;

        transform.position = new Vector3(transform.position.x, groundHeight, transform.position.z);

        if(rb.velocity.magnitude > .1f && pc.isGrounded)
        {
            if (!ps.isPlaying)
            {
                ps.Play();
            }
        } else
        {
            if (ps.isPlaying)
            {
                ps.Stop();
            }
        }
    }
}
