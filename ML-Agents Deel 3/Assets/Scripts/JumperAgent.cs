using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class JumperAgent : Agent
{
    private Rigidbody rb;
    private Vector3 startPos;
    private bool isGrounded;
    private GameObject closestObstacle;
    private JumperEnvironment environment;

    public float jumpForce = 5f;
    public Transform obstacleSpawnPoint;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPos = transform.localPosition;
        environment = GetComponentInParent<JumperEnvironment>();
    }

    public override void OnEpisodeBegin()
    {
        // Reset agent position and velocity
        transform.localPosition = startPos;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        isGrounded = true;

        // Request environment to reset
        environment.ResetEnvironment();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Observe agent's position (X, Y) and velocity (X, Y)
        sensor.AddObservation(new Vector2(transform.localPosition.x, transform.localPosition.y));
        sensor.AddObservation(new Vector2(rb.linearVelocity.x, rb.linearVelocity.y));

        // Observe the closest obstacle
        closestObstacle = environment.GetClosestObstacle();

        if (closestObstacle != null)
        {
            sensor.AddObservation(new Vector2(closestObstacle.transform.localPosition.x, closestObstacle.transform.localPosition.y));
            sensor.AddObservation(new Vector2(closestObstacle.GetComponent<Rigidbody>().linearVelocity.x, closestObstacle.GetComponent<Rigidbody>().linearVelocity.y));
        }
        else
        {
            sensor.AddObservation(Vector2.zero);
            sensor.AddObservation(Vector2.zero);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Discrete actions: 0 = do nothing, 1 = jump
        int action = actions.DiscreteActions[0];

        if (action == 1 && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }

        // Small negative reward for each step to encourage efficiency
        AddReward(-0.001f);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            // Colliding with an obstacle ends the episode with a penalty
            AddReward(-1f);
            EndEpisode();
        }
        else if (collision.gameObject.CompareTag("Floor"))
        {
            isGrounded = true;
        }
    }
}
