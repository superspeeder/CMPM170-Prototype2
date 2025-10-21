using System;
using UnityEngine;

public class PlayerWrapTrigger : MonoBehaviour {
    public Transform ReturnLocation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
    }

    // Update is called once per frame
    void Update() {
    }

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            var cc = other.GetComponent<CharacterController>();
            cc.enabled = false;
            other.transform.position = new Vector3(transform.position.x, transform.position.y,
                ReturnLocation.position.z + (transform.position.z - other.transform.position.z));
            cc.enabled = true;
        }
    }
}