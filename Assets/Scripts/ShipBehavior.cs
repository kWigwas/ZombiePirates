using UnityEngine;

/// <summary>
/// EnemyBehavior — Top-Down Pirate Game
/// -------------------------------------------------------
/// Attach to an enemy ship GameObject.
///
/// States:
///   Idle    — enemy drifts slowly on its own heading.
///   Flee    — player has entered fleeRadius; enemy moves
///             directly away at fleeSpeed.
///   Combat  — player has entered combatRadius; enemy stops
///             fleeing and signals the CombatManager to begin
///             an encounter (implement OnCombatEngage as needed).
///
/// Requirements:
///   • Rigidbody2D on this GameObject (Gravity Scale = 0).
///   • A GameObject tagged "Player" in the scene.
///   • (Optional) A CombatManager in the scene with a static
///     StartCombat(GameObject enemy) method.
/// </summary>

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBehavior : MonoBehaviour
{
    // -------------------------------------------------------
    //  Inspector-tunable variables
    // -------------------------------------------------------

    [Header("Detection Radii")]
    [Tooltip("Distance at which the enemy begins to flee the player.")]
    public float fleeRadius = 8f;

    [Tooltip("Distance at which the enemy stops fleeing and combat begins.")]
    public float combatRadius = 2f;

    [Header("Movement Speeds")]
    [Tooltip("Same unit as player moveForce (default 6). At 6 the enemy matches the player exactly.")]
    public float fleeSpeed = 4f;        // slightly slower than player so player can always catch up

    [Tooltip("Same unit as player moveForce (default 6). Gentle drift while idle.")]
    public float idleDriftSpeed = 1.5f;

    [Header("Idle Drift")]
    [Tooltip("How often (seconds) the enemy picks a new idle drift direction.")]
    public float idleDirectionChangeInterval = 3f;

    [Tooltip("Max random rotation per direction-change tick (degrees).")]
    public float idleMaxTurnDegrees = 45f;

    [Header("Visuals / Rotation")]
    [Tooltip("How quickly the ship rotates to face its movement direction (degrees/sec).")]
    public float rotationSpeed = 120f;

    // -------------------------------------------------------
    //  Private state
    // -------------------------------------------------------

    private enum State { Idle, Flee, Combat }
    private State currentState = State.Idle;

    private Rigidbody2D rb;
    private Transform playerTransform;

    private Vector2 idleDriftDirection;
    private float idleDirectionTimer;

    private bool combatTriggered = false;

    // -------------------------------------------------------
    //  Unity lifecycle
    // -------------------------------------------------------

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;          // No drag — velocity is set directly, same as player

        // Pick a random initial drift direction
        idleDriftDirection = Random.insideUnitCircle.normalized;
        idleDirectionTimer = idleDirectionChangeInterval;
    }

    private void Start()
    {
        // Find player by tag — make sure your player GameObject is tagged "Player"
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;
        else
            Debug.LogWarning("[EnemyBehavior] No GameObject with tag 'Player' found in scene.");
    }

    private void FixedUpdate()
    {
        if (playerTransform == null) return;

        float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        UpdateState(distToPlayer);

        switch (currentState)
        {
            case State.Idle: HandleIdle(); break;
            case State.Flee: HandleFlee(); break;
            case State.Combat: HandleCombat(); break;
        }
    }

    // -------------------------------------------------------
    //  State machine
    // -------------------------------------------------------

    /// <summary>Transitions between states based on player distance.</summary>
    private void UpdateState(float distToPlayer)
    {
        if (distToPlayer <= combatRadius)
        {
            currentState = State.Combat;
        }
        else if (distToPlayer <= fleeRadius)
        {
            currentState = State.Flee;
            combatTriggered = false; // reset so re-entry can trigger again
        }
        else
        {
            currentState = State.Idle;
            combatTriggered = false;
        }
    }

    // -------------------------------------------------------
    //  State handlers
    // -------------------------------------------------------

    /// <summary>Gentle random drift while the player is far away.</summary>
    private void HandleIdle()
    {
        // Periodically change drift heading
        idleDirectionTimer -= Time.fixedDeltaTime;
        if (idleDirectionTimer <= 0f)
        {
            float randomAngle = Random.Range(-idleMaxTurnDegrees, idleMaxTurnDegrees);
            idleDriftDirection = RotateVector(idleDriftDirection, randomAngle).normalized;
            idleDirectionTimer = idleDirectionChangeInterval;
        }

        MoveInDirection(idleDriftDirection, idleDriftSpeed);
    }

    /// <summary>Move directly away from the player at fleeSpeed.</summary>
    private void HandleFlee()
    {
        Vector2 awayFromPlayer = ((Vector2)transform.position - (Vector2)playerTransform.position).normalized;
        MoveInDirection(awayFromPlayer, fleeSpeed);
    }

    /// <summary>Stop moving; fire the combat engagement event once.</summary>
    private void HandleCombat()
    {
        rb.linearVelocity = Vector2.zero;

        if (!combatTriggered)
        {
            combatTriggered = true;
            OnCombatEngage();
        }
    }

    // -------------------------------------------------------
    //  Movement helpers
    // -------------------------------------------------------

    /// <summary>
    /// Applies velocity and smoothly rotates the ship sprite
    /// to face its direction of travel.
    /// </summary>
    private void MoveInDirection(Vector2 direction, float speed)
    {
        rb.linearVelocity = direction * speed;

        if (direction.sqrMagnitude > 0.001f)
        {
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            float currentAngle = rb.rotation;
            float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle,
                                                     rotationSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(newAngle);
        }
    }

    /// <summary>Rotates a 2D vector by angleDegrees.</summary>
    private Vector2 RotateVector(Vector2 v, float angleDegrees)
    {
        float rad = angleDegrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(cos * v.x - sin * v.y,
                           sin * v.x + cos * v.y);
    }

    // -------------------------------------------------------
    //  Combat hook — wire up to your CombatManager here
    // -------------------------------------------------------

    /// <summary>
    /// Called once when the player enters combatRadius.
    /// Replace / extend this with your CombatManager call.
    /// </summary>
    private void OnCombatEngage()
    {
        Debug.Log($"[EnemyBehavior] Combat engaged with {gameObject.name}!");

        // Example: CombatManager.Instance.StartCombat(this.gameObject);
        // Example: GameEvents.OnCombatStart?.Invoke(this.gameObject);
    }

    // -------------------------------------------------------
    //  Editor gizmos — visible in the Scene view
    // -------------------------------------------------------

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Flee radius — yellow
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, fleeRadius);

        // Combat radius — red
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, combatRadius);
    }
#endif
}