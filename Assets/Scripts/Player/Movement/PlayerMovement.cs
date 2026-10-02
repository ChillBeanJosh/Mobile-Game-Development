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
    [SerializeField] private Collider playerCollider;
    [Space]


    [Header("Global Physics State")]
    private Vector3 previousPosition;
    private Vector3 currentPosition;
    private Vector3 currentVelocity;
    private Vector3 totalAcceleration;
    Bounds bounds;


    [Header("Collision Parameters: ")]
    [SerializeField] private LayerMask collisionLayers = Physics.DefaultRaycastLayers;
    [Space]
    [SerializeField] private int maxBounces = 5;

    [SerializeField] float skinWidth = 0.015f;
    [SerializeField] float maxSlopeAngle = 45f;
    [Space]
    [SerializeField] private float collisionRadius = 0.3f;
    [SerializeField] private float groundCheckDistance = 0.05f;


    private bool isGrounded;
    private Vector3 groundNormal = Vector3.up;


    [Header("Physics Settings")]
    [SerializeField] private float mass = 80f;

    [SerializeField] private float gravity = 9.8f;
    [SerializeField] private float dragCoefficient = 8f;
    [SerializeField] private float meshRotationSpeed = 15f;
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


    private struct CollisionDebugHit
    {
        public Vector3 point;
        public Vector3 normal;

        public CollisionDebugHit(Vector3 point, Vector3 normal)
        {
            this.point = point;
            this.normal = normal;
        }
    }

    private readonly List<CollisionDebugHit> _collisionDebugHits = new List<CollisionDebugHit>();

    //List to capture individual acceleration vectors for gizmo drawing each frame
    List<Vector3> _activeAccelerations = new List<Vector3>();
    private const float Epsilon = 0.0001f;


    void Awake()
    {
        if (playerCollider == null)
        {
            Debug.LogError("NEEDS COLLIDER COMPONENT~!!!!");

            enabled = false;
            return;
        }

        //Initialize Position State:
        currentPosition = transform.position;
        previousPosition = currentPosition;

        //Initialize Bounds:
        bounds = playerCollider.bounds;

        //Initialize Rotation State:
        _requestedRotation = transform.rotation;
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
        Vector3 gravityAcceleration = Vector3.down * gravity;
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
            Vector3 impulseDirection = _requestedRotation * Vector3.forward;
            Vector3 impulse = impulseDirection * impulseStrength;
            AddImpulse(impulse);

            _requestedImpulse = false;  
        }
    }

    public void ApplyMovement()
    {
        float dt = Time.fixedDeltaTime;
        bounds = playerCollider.bounds;
        _collisionDebugHits.Clear();

        //Store Previous Position For Interpolation:
        previousPosition = currentPosition;

        //Semi Implicit Euler Integration:
        currentVelocity += totalAcceleration * dt;
        Vector3 desiredDisplacement = currentVelocity * dt;
        UpdateGroundedState();

        //Collision Detection + Resolution:
        Vector3 resolvedDisplacement = CollideAndSlide(desiredDisplacement, currentPosition, 0, false, currentVelocity);
        currentPosition += resolvedDisplacement;

        //Update Velocity Based On Actual Displacement To Fix Numerical Errors:
        if (dt > Epsilon) currentVelocity = resolvedDisplacement / dt;
        
        //Apply Position Update:
        transform.position = currentPosition;
        UpdateGroundedState();
    }

    public void UpdateBody(float deltaTime)
    {
        //Player Position Update Interpolation:
        float interpolationFactor = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
        root.position = Vector3.Lerp(previousPosition, currentPosition, interpolationFactor);

        //Player Rotation Update:
        Vector3 facingDirection = currentVelocity;
        //facingDirection.y = 0;

        if (facingDirection.sqrMagnitude > Epsilon * Epsilon)
        {
            Quaternion targetRotation = Quaternion.LookRotation(facingDirection.normalized, Vector3.up);
            root.rotation = Quaternion.Slerp(root.rotation, targetRotation, meshRotationSpeed * deltaTime);
        }
    }

    private Vector3 CollideAndSlide(Vector3 velocity, Vector3 position, int depth, bool gravityPass, Vector3 initialVelocity)
    {
        //Prevent Infinite Recursion:
        if (depth > maxBounces) return Vector3.zero;

        //Nothing To Move:
        if (velocity.sqrMagnitude <= Epsilon * Epsilon) return Vector3.zero;

        Vector3 direction = velocity.normalized;
        float distance = velocity.magnitude + skinWidth;

        //Collision Detection:
        RaycastHit hit;
        bool hasHit = Physics.SphereCast(position, collisionRadius, direction, out hit, distance,collisionLayers, QueryTriggerInteraction.Ignore);
        if (hasHit) _collisionDebugHits.Add(new CollisionDebugHit(hit.point, hit.normal));

        //No Collision:
        if (!hasHit) return velocity;
    
        //Move Up To The Surface, Stopping Slightly Before It:
        float travelDistance = Mathf.Max(0f, hit.distance - skinWidth);

        Vector3 snapToSurface = direction * travelDistance;
        Vector3 leftOver = velocity - snapToSurface;

        float angle = Vector3.Angle(Vector3.up, hit.normal);
        if (angle <= maxSlopeAngle)
        {
            //Walkable Surface / Gentle Slope:
            isGrounded = true;
            groundNormal = hit.normal;

            if (gravityPass)
            {
                //Gravity Pass Can Be Treated Specially Later:
                return snapToSurface;
            }

            //Remove Velocity Component Pushing Into The Surface:
            leftOver = ProjectAndScale(leftOver, hit.normal);
        }
        else
        {
            //Wall / Steep Slope:
            Vector3 horizontalWallNormal = new Vector3(hit.normal.x, 0f, hit.normal.z);
            Vector3 horizontalInitialVelocity = new Vector3(initialVelocity.x, 0f, initialVelocity.z);
            float scale = 1f;

            //Calculate Horizontal Wall Sliding Factor Only When Both Vectors Are Meaningful:
            if (horizontalWallNormal.sqrMagnitude > Epsilon && horizontalInitialVelocity.sqrMagnitude > Epsilon)
            {
                horizontalWallNormal.Normalize();
                horizontalInitialVelocity.Normalize();

                scale = 1f - Vector3.Dot(horizontalWallNormal, -horizontalInitialVelocity);
                scale = Mathf.Clamp01(scale);
            }

            if (isGrounded && !gravityPass)
            {
                //Preserve Horizontal Movement While Removing The Component Pushing Into The Wall:
                Vector3 horizontalLeftOver = new Vector3(leftOver.x, 0f, leftOver.z);

                if (horizontalWallNormal.sqrMagnitude > Epsilon)
                {
                    horizontalLeftOver = ProjectAndScale(horizontalLeftOver, horizontalWallNormal);
                    horizontalLeftOver *= scale;
                }

                leftOver = new Vector3(horizontalLeftOver.x, leftOver.y, horizontalLeftOver.z);
            }

            //Finally Project Remaining Movement Onto The Actual Collision Plane:
            leftOver = ProjectAndScale(leftOver, hit.normal);
        }

        //Prevent Tiny Collision Movements From Producing Numerical Noise:
        if (snapToSurface.sqrMagnitude <= Epsilon * Epsilon) snapToSurface = Vector3.zero;
        if (leftOver.sqrMagnitude <= Epsilon * Epsilon) leftOver = Vector3.zero;

        //Recursive Collision Handling:
        Vector3 recursiveResult = CollideAndSlide(leftOver, position + snapToSurface, depth + 1, gravityPass, initialVelocity);
        return snapToSurface + recursiveResult;
    }

    private Vector3 ProjectAndScale(Vector3 vector, Vector3 normal)
    {
        float magnitude = vector.magnitude;

        //Prevent Division By Zero:
        if (magnitude <= Epsilon) return Vector3.zero;
        Vector3 projected = Vector3.ProjectOnPlane(vector,normal);

        //Prevent Division By Zero:
        if (projected.sqrMagnitude <= Epsilon * Epsilon) return Vector3.zero;       
        return projected.normalized * magnitude;
    }

    private void UpdateGroundedState()
    {
        isGrounded = false;
        groundNormal = Vector3.up;

        float castRadius = Mathf.Max(0.001f, collisionRadius - skinWidth);
        float castDistance = groundCheckDistance + skinWidth + 0.05f;

        //Touching Ground Check:
        RaycastHit hit;
        bool hasGround = Physics.SphereCast(currentPosition, castRadius, Vector3.down, out hit,castDistance, collisionLayers, QueryTriggerInteraction.Ignore);
        if (!hasGround) return;

        //Check Slope Angle:
        float angle = Vector3.Angle(Vector3.up,hit.normal);
        if (angle <= maxSlopeAngle)
        {
            isGrounded = true;
            groundNormal = hit.normal;
        }
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

        //PLAYER COLLISION SPHERE:
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(currentPosition, collisionRadius);

        //COLLISION DEBUG HITS:
        Gizmos.color = Color.cyan;
        foreach (CollisionDebugHit collision in _collisionDebugHits)
        {
            //Hit point sphere:
            Gizmos.DrawSphere(collision.point,0.2f);

            //Surface normal:
            Gizmos.DrawRay(collision.point,collision.normal);
        }
    }
}
