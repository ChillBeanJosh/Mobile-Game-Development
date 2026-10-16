using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

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

    [Header("Arena Bounding Box")]
    [SerializeField] private bool useBoundingBox = true;
    [SerializeField] private Vector3 boxMin = new Vector3(-15f, 0f, -15f);
    [SerializeField] private Vector3 boxMax = new Vector3(15f, 20f, 15f);
    [SerializeField] private float boxSpringConstant = 300f;
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


    [Header("Movement Burst & Cooldown Settings: ")]
    [SerializeField] private float movementDuration = 1.5f;   // How long the movement burst lasts (X seconds)
    [SerializeField] private float cooldownTime = 2.0f;       // Cooldown before the burst can be triggered again
    [SerializeField] private float lineVisualizationLength = 3f; // Length of the preview line
    [SerializeField] private LineRenderer inputLineRenderer;   // Reference to the Line Renderer component

    private float movementTimer = 0f;
    private float cooldownTimer = 0f;
    private bool isMovementActive = false;

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

    void Update()
    {
        UpdateInputVisualization();
    }

    public void UpdateInput(CharacterInput input)
    {
        _requestedRotation = input.Rotation;

        // 1. Get the camera's forward and right vectors, then flatten them on the Y-plane
        Vector3 cameraForward = _requestedRotation * Vector3.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        Vector3 cameraRight = _requestedRotation * Vector3.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();

        // 2. Map the 2D input (Move.x / Move.y) to the camera's horizontal axes
        _requestedMovement = (cameraRight * input.Move.x) + (cameraForward * input.Move.y);
        _requestedMovement = Vector3.ClampMagnitude(_requestedMovement, 1f);

        // Store Button Request:
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
        if (isMovementActive)
        {
            Vector3 appliedAcceleration = _requestedMovement * moveAcceleration;
            AddAcceleration(appliedAcceleration);
        }

        //Resisting Force:
        Vector3 dragAcceleration = -currentVelocity * dragCoefficient;
        AddAcceleration(dragAcceleration);

        // Gravitational Force & Ground Management:
        if (!isGrounded)
        {
            Vector3 gravityAcceleration = Vector3.down * gravity;
            AddAcceleration(gravityAcceleration);
        }
        else
        {
            if (currentVelocity.y < 0f)
            {
                currentVelocity.y = 0f;
            }
        }
    }

    public void GatherIntrinsicPhysics()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.fixedDeltaTime;
        }

        if (isMovementActive)
        {
            movementTimer -= Time.fixedDeltaTime;
            if (movementTimer <= 0f)
            {
                isMovementActive = false;
            }
        }

        //Mass Based Interaction:
        if (activateTestFields)
        {
            AddForce(windForce);
        }

        //Arena Bounding Box Soft Walls:
        ApplyBoundingBoxForces();

        if (_requestedImpulse)
        {
            // Only trigger if cooldown is ready and player is actually pressing movement keys
            if (cooldownTimer <= 0f && _requestedMovement.sqrMagnitude > Epsilon)
            {
                isMovementActive = true;
                movementTimer = movementDuration;
                cooldownTimer = cooldownTime;
            }

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

    private void ApplyBoundingBoxForces()
    {
        if (!useBoundingBox) return;

        float radius = collisionRadius;
        Vector3 boxForce = Vector3.zero;

        // --- X-Axis Boundaries ---
        if (currentPosition.x < boxMin.x + radius)
        {
            float overlap = (boxMin.x + radius) - currentPosition.x;
            boxForce.x += boxSpringConstant * overlap;
        }
        else if (currentPosition.x > boxMax.x - radius)
        {
            float overlap = currentPosition.x - (boxMax.x - radius);
            boxForce.x -= boxSpringConstant * overlap;
        }

        // --- Y-Axis Boundaries (Floor / Ceiling) ---
        if (currentPosition.y < boxMin.y + radius)
        {
            float overlap = (boxMin.y + radius) - currentPosition.y;
            boxForce.y += boxSpringConstant * overlap;
        }
        else if (currentPosition.y > boxMax.y - radius)
        {
            float overlap = currentPosition.y - (boxMax.y - radius);
            boxForce.y -= boxSpringConstant * overlap;
        }

        // --- Z-Axis Boundaries ---
        if (currentPosition.z < boxMin.z + radius)
        {
            float overlap = (boxMin.z + radius) - currentPosition.z;
            boxForce.z += boxSpringConstant * overlap;
        }
        else if (currentPosition.z > boxMax.z - radius)
        {
            float overlap = currentPosition.z - (boxMax.z - radius);
            boxForce.z -= boxSpringConstant * overlap;
        }

        // Apply the spring repulsion force using your existing mass-based force handler
        if (boxForce.sqrMagnitude > Epsilon)
        {
            AddForce(boxForce);
        }
    }

    public Transform GetCameraTarget() => cameraTarget;
    public Vector3 GetTotalAcceleration() => totalAcceleration;
    public Vector3 GetCurrentVelocity() => currentVelocity;
    public Vector3 GetCurrentPosition() => currentPosition;

    public bool GetGroundedStatus() => isGrounded;

    private void UpdateInputVisualization()
    {
        if (inputLineRenderer == null) return;

        // Show line when movement keys are pressed and we are not currently in an active movement burst
        if (_requestedMovement.sqrMagnitude > Epsilon && !isMovementActive)
        {
            inputLineRenderer.enabled = true;
            inputLineRenderer.positionCount = 2;
            inputLineRenderer.SetPosition(0, currentPosition);
            inputLineRenderer.SetPosition(1, currentPosition + _requestedMovement * lineVisualizationLength);
        }
        else
        {
            inputLineRenderer.enabled = false;
        }
    }
    void OnDrawGizmos()
    {
        //if (!Application.isPlaying || !showDebugGizmos) return;

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

        //BOUNDING BOX:
        Gizmos.color = Color.magenta;
        Vector3 boxCenter = (boxMin + boxMax) * 0.5f;
        Vector3 boxSize = boxMax - boxMin;
        Gizmos.DrawWireCube(boxCenter, boxSize);     
    }
}
