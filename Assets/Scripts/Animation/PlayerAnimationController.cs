using System.Collections;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    Animator anim;
    Rigidbody rb;
    PlayerController pc;

    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody>();
        anim = GetComponentInParent<Animator>();
        pc = GetComponentInParent<PlayerController>();
    }

    private void Update()
    {
        float moveSpeed = pc.AnimationSpeed;
        if (moveSpeed < 0) moveSpeed *= -1;
        anim.SetFloat("MovementSpeed", moveSpeed);
        anim.SetBool("Grounded", pc.isGrounded);
    }

    public void AttackTrigger()
    {
        StartCoroutine(SetAttackTrigger());
    }

    public void SetDigTrigger()
    {
        anim.SetTrigger("Dig");
    }

    private IEnumerator SetAttackTrigger()
    {
        anim.SetTrigger("Attack");
        yield return 0;
        anim.ResetTrigger("Attack");
    }
}
