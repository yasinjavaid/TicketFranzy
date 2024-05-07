using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollManage : MonoBehaviour
{
    public Rigidbody[] ragdollRigidbodies;

    private bool activeRagdoll;

    public Collider[] ragdollColliders;

    public Collider mainCollider;
    
    private Animator animator;

    private Rigidbody rigidbody;

    private void Awake()
    {
        mainCollider = GetComponent<Collider>();
        ragdollColliders = GetComponentsInChildren<Collider>(true);
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>(true);
        animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody>();
        DoRagdoll(false);
    
    }

    private void DoRagdoll(bool isRagdoll)
    {
        foreach (var col in ragdollRigidbodies)
            col.isKinematic = !isRagdoll;
        foreach (var col in ragdollColliders)
            col.enabled = isRagdoll;

        mainCollider.enabled = !isRagdoll;
        rigidbody.isKinematic = !isRagdoll;
        //  animator.enabled = !isRagdoll;
        //  rigidbody.useGravity = !isRagdoll;
    }

    public void ShotPlayer(float force)
    {
        rigidbody.isKinematic = false;
       
    //    rigidbody.constraints = RigidbodyConstraints.None;
        rigidbody.AddRelativeForce (rigidbody.transform.up * force, ForceMode.Impulse);
        transform.parent = null;
        activeRagdoll = true;
        //  foreach (var col in ragdollRigidbodies)
        //  col.isKinematic = false;
        //  foreach (var col in ragdollColliders)
        // col.enabled = true;
        // mainCollider.enabled = false;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (activeRagdoll)
        {
            DoRagdoll(true);
        }
    }
}
