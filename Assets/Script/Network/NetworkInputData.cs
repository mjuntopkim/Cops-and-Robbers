using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public struct NetworkInputData : INetworkInput
{
    public Vector3 direction;
    public NetworkButtons button;
    public float cameraYaw;
}
