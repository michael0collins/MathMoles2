using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudioController : MonoBehaviour
{
    [Header("Movement Sounds")]
    public AudioClip[] runLoop;
    public AudioClip jump;
    public AudioClip jumpLand;
    public AudioSource feet;
    public AudioSource body;

    [Header("Player interaction.")]
    public AudioClip swing;
    public AudioClip swingHitPlayer;
    public AudioClip swingHitObjective;
    public AudioClip playerLandOnHead;
    public AudioSource item;

    private int runLoopIndex;
    private CharacterController characterController;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        runLoopIndex = Random.Range(0, runLoop.Length);
        feet.clip = runLoop[runLoopIndex];
    }

    private void Update()
    {
        RunLoop();
    }

    private void RunLoop()
    {
        if (characterController.velocity.magnitude > .25f && !feet.isPlaying)
            feet.Play();
        else if (characterController.velocity.magnitude < .25f && feet.isPlaying)
            feet.Stop();
    }

    public void PlaySound(AudioSource source, AudioClip clip)
    {
        if (source.isPlaying)
            source.Stop();

        source.clip = clip;
        source.Play();
    }
}
