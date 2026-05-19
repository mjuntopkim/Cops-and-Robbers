using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.AI;

public class AIManager : NetworkBehaviour
{
    public static AIManager Instance;

    public List<CitizenAI> ActiveCitizens = new List<CitizenAI>();

    [SerializeField] private List<NetworkPrefabRef> AIPrefabs;

    [SerializeField] private int AICount = 10;
    [SerializeField] private float spawnRadius = 30f;

    private Vector3 spawnAreaCenter;

    public override void Spawned()
    {
        if(Instance == null)
        {
            Instance = this;
        }

        if (HasStateAuthority)
        {
            spawnAreaCenter = transform.position;

            SpawnAI();
        }
    }

    private void SpawnAI()
    {
        for(int i = 0; i < AICount; i++)
        {
            Vector3 randomPos = GetRandomPosition();

            NetworkPrefabRef prefabToSpawn = AIPrefabs[Random.Range(0, AIPrefabs.Count)];
            Runner.Spawn(prefabToSpawn, randomPos, Quaternion.identity);
        }
    }

    private Vector3 GetRandomPosition()
    {
        Vector3 randomDirection = Random.insideUnitSphere * spawnRadius;
        randomDirection += spawnAreaCenter;

        NavMeshHit hit;
        if(NavMesh.SamplePosition(randomDirection, out hit, spawnRadius, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return spawnAreaCenter;
    }
}
