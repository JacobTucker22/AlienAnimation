using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
     public Transform target; // Reference to the player's Transform
     public float distance = 5f;
     public float heightOffset = 2f;

     void LateUpdate()
     {
          Vector3 targetPosition = target.position + Vector3.up * heightOffset;
          Vector3 newPosition = targetPosition - transform.forward * distance;

          transform.position = newPosition;
          transform.LookAt(targetPosition);
     }
}