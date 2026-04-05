using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PrisonManager : NetworkBehaviour
{
    public static PrisonManager Instance;

    [SerializeField] private Transform[] prisonPoint;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    public Vector3 GetPrisonPosition(int copIndex)
    {
        if(copIndex >= 0 && copIndex < prisonPoint.Length)
        {
            return prisonPoint[copIndex].position;
        }

        return Vector3.zero;
    }
}
