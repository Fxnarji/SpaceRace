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
    [SerializeField] private float rollAcceleration = 15f;

    
    [Header("Camera Movement")]
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private float mouseSensitivity = 5f;
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 3f, -8f);
    [SerializeField] private float cameraDamping = 6f;
    
    [Header("Visuals")]
    [SerializeField] private Renderer[] boostersBack;
    [SerializeField] private Renderer[] boostersLeft;
    [SerializeField] private Renderer[] boostersRight;
    
    [Header("Sound")]
    [SerializeField] private AudioSource SoundClip;
    

    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float elevation;
    private float roll;


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        //safety stuff
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

    public void OnElevation(InputValue value)
    {
        elevation = value.Get<float>();
    }


    public void OnLook(InputValue value)
    {
        Vector2 value_raw = value.Get<Vector2>();
        //lookInput = new Vector2(Mathf.Clamp(value_raw.x, -1f, 1f), Mathf.Clamp(value_raw.y, -1f, 1f));
        lookInput = value_raw;
    }

    public void OnRoll(InputValue value)
    {
        roll = value.Get<float>();
    }

    private void FixedUpdate()
    {
        FireBoosters();
        UpdateBoostersVisualsAndSound();
        UpdateCamera();
    }


    private void FireBoosters()
    {
        // add local forward backward
        rb.AddRelativeForce(Vector3.forward * moveInput.x * accelerationSpeed, ForceMode.Acceleration);

        // add local left and right
        rb.AddRelativeForce(Vector3.right * -moveInput.y * accelerationSpeed, ForceMode.Acceleration);

        // add local up and down
        rb.AddRelativeForce(Vector3.up * elevation * accelerationSpeed, ForceMode.Acceleration);



        // add local roll
        rb.AddRelativeTorque(Vector3.right * -roll * rollAcceleration, ForceMode.Acceleration);

        // add local yaw
        rb.AddRelativeTorque(Vector3.up * lookInput.x * mouseSensitivity, ForceMode.Acceleration);

        // add local pitch
        rb.AddRelativeTorque(Vector3.forward * -lookInput.y * mouseSensitivity, ForceMode.Acceleration);
        
        // we had that in class, with velocity and linearvelocity. Here is the new one :D 
        Vector3 flatVelocity = rb.linearVelocity;
        flatVelocity.y = 0f;
        if (flatVelocity.magnitude > maxSpeed)
        {
            Vector3 clamped = flatVelocity.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(clamped.x, rb.linearVelocity.y, clamped.z);
        }
    }

    private void UpdateCamera()
    {
        Vector3 desiredposition = transform.TransformPoint(cameraOffset);
        cameraTarget.position = Vector3.Lerp(cameraTarget.position, desiredposition, cameraDamping * Time.deltaTime);
        cameraTarget.LookAt(transform.position);
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
