using System;
using UnityEngine;

public class PlayerWrapTrigger : MonoBehaviour {
    public Transform ReturnLocation;
    public GameObject Car;

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
            other.transform.position = new Vector3(other.transform.position.x, other.transform.position.y,
                ReturnLocation.position.z - (transform.position.z - other.transform.position.z));
            cc.enabled = true;
            
            var carpos = new Vector3(Car.transform.position.x, Car.transform.position.y, ReturnLocation.position.z - (transform.position.z - Car.transform.position.z));
            Car.transform.position = carpos;
            Car.GetComponent<Rigidbody>().position = carpos;
        }
    }
}