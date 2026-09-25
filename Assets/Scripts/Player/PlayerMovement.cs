using System.Collections.Generic;
using UnityEngine;

public struct CharacterInput
{
    public Quaternion Rotation;
    public Vector2 Move;
    public bool ImpulsePressed;
}

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Setup: ")]
    [SerializeField] private Transform root;
    [SerializeField] private Transform cameraTarget;
    [Space]


    [Header("Global Physics State")]
    private Vector3 previousPosition;
    private Vector3 currentPosition;
    private Vector3 currentVelocity;
    private Vector3 totalAcceleration;
    [Space]


    [Header("Physics Settings")]
    [SerializeField] private float mass = 80f;
    [SerializeField] private float dragCoefficient = 8f;
    public float meshRotationSpeed = 15f;
    [Space]

    [Header("Test Fields: ")]
    [SerializeField] private bool activateTestFields = false;
    [Space]
    [SerializeField] private Vector3 windForce = new Vector3(200f, 0f, 0f);

    [SerializeField] private float impulseStrength = 10f;
    [Space]


    [Header("Vector Visualization: ")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private Color individualAccelerationColor = Color.blue;
    [SerializeField] private Color totalAccelerationColor = Color.red;
    [Space]
    [SerializeField] private Color velocityColor = Color.green;
    [Space]
    [SerializeField] private Color trajectoryColor = Color.yellow;
    [SerializeField] private int trajectorySteps = 30;
    [Space]


    private Quaternion _requestedRotation;
    private Vector3 _requestedMovement;
    private bool _requestedImpulse;


    // List to capture individual acceleration vectors for gizmo drawing each frame
    List<Vector3> _activeAccelerations = new List<Vector3>();

    void Awake()
    {
        //Initialize Position State:
        currentPosition = transform.position;
        previousPosition = currentPosition;
    }

    public void UpdateInput(CharacterInput input)
    {
        _requestedRotation = input.Rotation;

        //Remap 2D Vector -> Y-Plane For Proper Input Direction:
        _requestedMovement = new Vector3(input.Move.x, 0f, input.Move.y);
        _requestedMovement = Vector3.ClampMagnitude(_requestedMovement, 1f);

        //Ensure the Y-Plane Rotates Correlated To The Look Direction:
        _requestedMovement = _requestedRotation * _requestedMovement;

        //Store Button Request:
        _requestedImpulse |= input.ImpulsePressed;
    }

    public void AddAcceleration(Vector3 acceleration)
    {
        totalAcceleration += acceleration;
        _activeAccelerations.Add(acceleration);
    }

    public void AddForce(Vector3 force)
    {
        AddAcceleration(force / mass);
    }

    public void AddImpulse(Vector3 impulse)
    {
        currentVelocity += impulse / mass;
    }

    public void GatherIntrinsicAccelerations(float moveAcceleration)
    {
        //Clear Last TimeStep Data:
        _activeAccelerations.Clear();
        totalAcceleration = Vector3.zero;

        //Player Controlled Acceleration:
        Vector3 appliedAcceleration = _requestedMovement * moveAcceleration;
        AddAcceleration(appliedAcceleration);

        //Resisting Force:
        Vector3 dragAcceleration = -currentVelocity * dragCoefficient;
        AddAcceleration(dragAcceleration);

        //Gravitational Force:
        Vector3 gravityAcceleration = new Vector3(0, -9.8f, 0);
        AddAcceleration(gravityAcceleration);
    }

    public void GatherIntrinsicPhysics()
    {
        //Mass Based Interaction:
        if (activateTestFields)
        {
            AddForce(windForce);
        }

        if (_requestedImpulse)
        {
            /*
            Vector3 impulseDirection = _requestedRotation * Vector3.forward;
            Vector3 impulse = impulseDirection * impulseStrength;
            AddImpulse(impulse);

            _requestedImpulse = false;
            */
        }
    }

    public void ApplyMovement()
    {
        float dt = Time.fixedDeltaTime;

        //Store Previous Position For Interpolation:
        previousPosition = currentPosition;

        //Semi Implicit Euler Integration:
        currentVelocity += totalAcceleration * dt;
        currentPosition += currentVelocity * dt;

        //Apply Position Update:
        transform.position = currentPosition;
    }

    public void UpdateBody(float deltaTime)
    {
        //Player Position Update Interpolation:
        float interpolationFactor = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
        root.position = Vector3.Lerp(previousPosition, currentPosition, interpolationFactor);

        //Player Rotation Update:
        //Quaternion targetYaw = Quaternion.Euler(0, _requestedRotation.eulerAngles.y, 0);
        root.rotation = Quaternion.Slerp(root.rotation, _requestedRotation, meshRotationSpeed * deltaTime);
    }

    public Transform GetCameraTarget() => cameraTarget;
    public Vector3 GetTotalAcceleration() => totalAcceleration;

    void OnDrawGizmos()
    {
        if (!Application.isPlaying || !showDebugGizmos) return;

        //INDEPENDENT ACCELERATION VECTORS:
        Gizmos.color = individualAccelerationColor;
        foreach (var acc in _activeAccelerations)
        {
            Gizmos.DrawRay(currentPosition, acc);
        }

        //TOTAL ACCELERATION VECTOR:
        Gizmos.color = totalAccelerationColor;
        Gizmos.DrawRay(currentPosition, totalAcceleration);

        //CURRENT VELOCITY VECTOR:
        Gizmos.color = velocityColor;
        Gizmos.DrawRay(currentPosition, currentVelocity);

        //POSITION TRAJECTORY PATH:
        Gizmos.color = trajectoryColor;
        Vector3 simPosition = currentPosition;
        Vector3 simVelocity = currentVelocity;
        float dt = Time.fixedDeltaTime;

        for (int i = 0; i < trajectorySteps; i++)
        {
            // Simulate steps ahead assuming total acceleration remains constant
            simVelocity += totalAcceleration * dt;
            Vector3 nextSimPosition = simPosition + simVelocity * dt;

            Gizmos.DrawLine(simPosition, nextSimPosition);
            simPosition = nextSimPosition;
        }
    }
}
