using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
//[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour
{
    [Header("Hover Settings")]
    [SerializeField] private float hoverHeight = 3f;
    [SerializeField] private float hoverStrength = 40f;
    [SerializeField] private float hoverDamping = 6f;
    
    [Header("Boosters Settings")]
    [SerializeField] private float accelerationSpeed = 20f;
    [SerializeField] private float maxSpeed = 15f;
    
    [Header("Camera Movement")]
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private float mouseSensitivity = 5f;
    
    [Header("Visuals")]
    [SerializeField] private Renderer[] boostersBack;
    [SerializeField] private Renderer[] boostersLeft;
    [SerializeField] private Renderer[] boostersRight;
    
    [Header("Sound")]
    [SerializeField] private AudioSource SoundClip;
    

    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 lookInput;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        //savety stuff
        if (cameraTarget == null)
        {
            Debug.LogWarning("No camera target assigned to this game object.");
            return;
        }
        if (boostersBack == null)
        {
            Debug.LogWarning("No boosters assigned to this game object.");
            return;
        }
        if (boostersLeft == null)
        {
            Debug.LogWarning("No boosters assigned to this game object.");
            return;
        }
        if (boostersRight == null)
        {
            Debug.LogWarning("No boosters assigned to this game object.");
            return;
        }   
        
        //Sound
        SoundClip.loop = true;
        SoundClip.volume = 0f;
        SoundClip.Play();
        if(SoundClip == null)
        {
            Debug.LogWarning("No sound assigned to this game object.");
            return;
        }
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    private void FixedUpdate()
    {
        Hover();
        FireBoosters();
        UpdateBoostersVisualsAndSound();
        Turn();
    }


    private void FireBoosters()
    {
        // saw that in a Youtube video. Works fine ^^
        // fatten the cameras look direction so the ship moves with W in look direction
        Vector3 camForward = Vector3.Scale(cameraTarget.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = cameraTarget.right;
        
        // -moveInput, because it flips W and S for Farward and Backwards Movement. It works this way after placing the minus :D
        // The Input gives us 1 and -1 we use this in a vector to calculate the direction
        // We get from W = 1, S = -1, A = -1 and D is 1
        // We could use the input.z for up and down, but we dont want this, because we have a hover state.
        Vector3 flydirection = camForward * moveInput.y + camRight * moveInput.x;
        
        rb.AddForce(flydirection * accelerationSpeed, ForceMode.Acceleration);
        
        // we had that in class, with velocity and linearvelocity. Here is the new one :D 
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
            float heightGoal = hoverHeight - hit.distance;
            
            float upVelocity = rb.linearVelocity.y;

            // - upvelocity * damping prevents, that the smooth high adjustment
            float lift = heightGoal * hoverStrength - upVelocity * hoverDamping;
            rb.AddForce(Vector3.up * lift, ForceMode.Acceleration);
        }
    }

    private void Turn()
    {
        // moves camera left and right. Very self explantory.
        float yawDelta = lookInput.x * mouseSensitivity * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, yawDelta, 0f));
    }

    private void UpdateBoostersVisualsAndSound()
    {
        if (moveInput.y != 0 || moveInput.x != 0)
        {
            SoundClip.volume = Mathf.MoveTowards(SoundClip.volume, 1f, 10f * Time.deltaTime);
        }
        else
        {
            SoundClip.volume = Mathf.MoveTowards(SoundClip.volume, 0f, 10f * Time.deltaTime);
        }
        if (moveInput.x > 0.01f)
        {
            SetBoosters(boostersLeft, true);
        }
        else
        {
            SetBoosters(boostersLeft, false);
        }

        if (moveInput.y > 0.01f)
        {
            SetBoosters(boostersBack, true);
        }
        else
        {
            SetBoosters(boostersBack, false);
        }

        if (moveInput.x < -0.01f)
        {
            SetBoosters(boostersRight, true);
        }
        else
        {
            SetBoosters(boostersRight, false);
        }
    }

    private void SetBoosters(Renderer[] boosters, bool active)
    {
        foreach (Renderer r in boosters)
        {
            if (active)
            {
                r.material.SetInt("_Activate", 1);
            }
            else
            {
                r.material.SetInt("_Activate", 0);
            }
        }
    }
}
