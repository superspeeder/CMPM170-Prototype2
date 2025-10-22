using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private Rigidbody rb;

    public float FollowSpeed = 31;
    public GameObject Player;
    public float StartWaitTime = 3.0f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        StartCoroutine(toggler());
    }

    private bool run = false;

    IEnumerator toggler() {
        yield return new WaitForSeconds(StartWaitTime);
        run = true;
    }

    // Update is called once per frame
    void Update()
    {
    }

    void FixedUpdate() {
        if (!run) return;

        rb.maxLinearVelocity = FollowSpeed;
        
        var desiredVec = Player.transform.position - transform.position;
        desiredVec.y = 0;
        transform.rotation = Quaternion.LookRotation(desiredVec);
        rb.AddRelativeForce(Vector3.forward * FollowSpeed, ForceMode.Force);
    }

    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.CompareTag("Player")) {
            // TODO: death
        }
    }
}
