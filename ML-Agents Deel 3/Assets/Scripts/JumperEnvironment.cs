using System.Collections.Generic;
using UnityEngine;

public class JumperEnvironment : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public Transform obstacleSpawnPoint;
    public float minSpeed = 5f;
    public float maxSpeed = 10f;
    public float spawnInterval = 2f; // Time between obstacle spawns

    private List<GameObject> obstacles = new();
    private float spawnTimer;
    private float currentSpeed;

    void Start()
    {
        // Set a random speed for this episode
        currentSpeed = Random.Range(minSpeed, maxSpeed);
        spawnTimer = 0f;
    }

    public void ResetEnvironment()
    {
        // Destroy existing obstacles
        foreach (var obs in obstacles)
        {
            if (obs != null) Destroy(obs);
        }
        obstacles.Clear();

        // Reset spawn timer and set a new random speed for the episode
        spawnTimer = 0f;
        currentSpeed = Random.Range(minSpeed, maxSpeed);

        // Spawn the first obstacle immediately
        SpawnObstacle();
    }

    void Update()
    {
        // Spawn new obstacles at regular intervals
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            SpawnObstacle();
            spawnTimer = 0f;
        }

        // Remove obstacles that have passed the agent
        obstacles.RemoveAll(obs => obs == null || obs.transform.position.x < -15);

        // Reward agent for surviving
        var agent = GetComponentInChildren<JumperAgent>();
        if (agent != null)
        {
            agent.AddReward(0.01f); // Reward for staying alive
        }
    }

    void SpawnObstacle()
    {
        GameObject newObstacle = Instantiate(obstaclePrefab, obstacleSpawnPoint.position, Quaternion.identity);
        newObstacle.transform.SetParent(transform);
        newObstacle.GetComponent<Rigidbody>().linearVelocity = new Vector3(-currentSpeed, 0, 0);
        obstacles.Add(newObstacle);
    }

    public GameObject GetClosestObstacle()
    {
        GameObject closest = null;
        float minDist = float.MaxValue;
        Vector3 agentPos = transform.Find("Agent").position;

        foreach (var obs in obstacles)
        {
            if (obs == null) continue;
            float dist = Vector3.Distance(agentPos, obs.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = obs;
            }
        }
        return closest;
    }
}
