using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class CopRole : NetworkBehaviour
{
    [SerializeField] private float attackDistance = 2.5f;
    [SerializeField] private float attackRadius = 0.5f;
    [SerializeField] private LayerMask targetLayer;

    [Networked] public int CopIndex { get; set; }

    public override void FixedUpdateNetwork()
    {
        if(GetInput(out NetworkInputData data))
        {
            if (data.button.IsSet(2))
            {
                TryAttack();
            }
        }
    }

    private void TryAttack()
    {
        Vector3 position = transform.position + (Vector3.up * 1f);
        Vector3 direction = transform.forward;

        if(Physics.SphereCast(position, attackRadius, direction, out RaycastHit hit, attackDistance, targetLayer))
        {
            RobberRole robber = hit.collider.GetComponentInParent<RobberRole>();
            
            if(robber != null)
            {
                if (Object.HasStateAuthority)
                {
                    ExecuteArrest(robber);
                }
                else
                {
                    RPC_RequestArrest(robber);
                }
            }
        }
    }

    private void ExecuteArrest(RobberRole robber)
    {
        Vector3 prisonPos = PrisonManager.Instance.GetPrisonPosition(CopIndex);

        robber.Arrest(prisonPos);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestArrest(RobberRole robber)
    {
        if(robber != null)
        {
            float distance = Vector3.Distance(transform.position, robber.transform.position);
            if(distance <= attackDistance)
            {
                ExecuteArrest(robber);
            }
        }
    }
}
