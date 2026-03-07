using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public enum PlayerRole
{
    Cop, Robber
}

public class PlayerRoles : NetworkBehaviour
{
    [Networked] public PlayerRole Role { get; set; }

}
