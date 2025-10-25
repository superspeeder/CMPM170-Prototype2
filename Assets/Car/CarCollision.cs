using UnityEngine;

public class CarCollision : MonoBehaviour
{
    public int score = 0;

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Player"))
        {
            Destroy(hit.gameObject);
            score++;
            Debug.Log("Score: " + score);
        }
    }
}