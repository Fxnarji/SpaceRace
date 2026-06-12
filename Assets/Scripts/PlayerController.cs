using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Hover Settings")]
    [SerializeField] private float hoverHeight = 3f;
    [SerializeField] private float hoverStrength = 40f;
    [SerializeField] private float hoverDamping = 6f;
    
    [Header("Boosters Settings")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float maxSpeed = 15f;

    private Rigidbody rb;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void FixedUpdate()
    {
        Hover();
        FireBoosters();
    }


    private void FireBoosters()
    {
        // -moveInput, because it flips W and S for Farward and Backwards Movement. It works this way after placing the minus :D
        // The Input gives us 1 and -1 we use this in a vector to calculate the direction
        // We get from W = 1, S=-1, A = -1 and D is 1
        // We could use the input.z for up and down, but we dont want this, because we have a hover state.
        Vector3 flydirection = new Vector3(-moveInput.y, 0f,moveInput.x );
        
        rb.AddForce(flydirection * speed, ForceMode.Acceleration);
        
        //we had that in class, with velocity and linearvelocity. Here is the new one :D 
        Vector3 flatVelocity = rb.linearVelocity;
        flatVelocity.y = 0f;
        if (flatVelocity.magnitude > maxSpeed)
        {
            Vector3 clamped = flatVelocity.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(clamped.x, rb.linearVelocity.y, clamped.z);
        }
    }
    private void Hover()
    {
        // Checking where the ground is to hover up or down. Helps when we fly something down or up. Only Stairs can be a Problem. Ramps are fine.
        // max Distance is capped on 50. More would be very useless and would look bad, with a 50 or more hover hight.
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 50, LayerMask.GetMask("Ground")))
        {
            float heightError = hoverHeight - hit.distance;
            
            float upVelocity = rb.linearVelocity.y;

            // Spring-damper: push toward the target height, damping stops endless bouncing.
            float lift = heightError * hoverStrength - upVelocity * hoverDamping;
            rb.AddForce(Vector3.up * lift, ForceMode.Acceleration);
        }
        //wenn -
    }
}
