using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private Rigidbody rb;

    public float FollowSpeed = 31;
    public float TurnSpeed = 16;
    public float KP = 1f;
    public GameObject Player;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        StartCoroutine(toggler());
    }

    private bool run = false;

    IEnumerator toggler() {
        yield return new WaitForSeconds(1.5f);
        run = true;
    }

    float angularError() {
        var rot = transform.rotation.eulerAngles.y;
        var desiredVec = Player.transform.position - transform.position;
        var desired = MathF.Atan2(desiredVec.z, desiredVec.x);
        return desired - rot;
    }

    float angularControl() {
        return KP * angularError();
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
