using UnityEngine;

public class PlayerWrapTrigger : MonoBehaviour
{
    public Transform ReturnLocation;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var cc = other.GetComponent<CharacterController>();

            if (cc == null)
            {
                return;
            }

            float newZ = ReturnLocation.position.z - (transform.position.z - other.transform.position.z);
            Vector3 newPosition = new Vector3(other.transform.position.x, other.transform.position.y, newZ);

            cc.enabled = false;
            other.transform.position = newPosition;
            cc.enabled = true;
        }
    }
}