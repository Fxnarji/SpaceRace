using UnityEngine;

public class LaunchPad : MonoBehaviour
{
    [SerializeField] private float launchForce = 20f;
    [SerializeField] private Vector3 launchDirection = Vector3.up;
    
    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(launchDirection * launchForce, ForceMode.Impulse);
        }
    }
}
