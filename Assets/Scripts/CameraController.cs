using UnityEngine;

[DefaultExecutionOrder(1000)] 
public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Rig")]
    public Vector3 offset = new Vector3(0f, 3f, -6f);
    public Vector3 rotationOffset = new Vector3(0f, 0f, 0f);

    [Header("Damping")]
    public float positionDamping = 0.05f;
    [Range(0f, 1f)] public float aimAtTarget = 0.8f;



    Vector3 _velocity;

    void LateUpdate()
    {

        Vector3 desiredPos = target.position + target.rotation * offset;
        
        transform.position = Vector3.SmoothDamp(
            transform.position, desiredPos, ref _velocity, positionDamping);


        // for some reason rigid Rot needs an offset. Quick fix, but good enough - camera is not the main focus anyways
        Quaternion rigidRot = target.rotation * Quaternion.Euler(rotationOffset);
        Quaternion aimRot   = Quaternion.LookRotation(target.position - transform.position, target.up);

        transform.rotation = Quaternion.Slerp(rigidRot, aimRot, aimAtTarget);
    }
}