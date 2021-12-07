using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class FighterAgent : Agent
{
    [SerializeField]
    private Transform m_attackArea;
    [SerializeField]
    private Vector3 m_attackRange;

    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        base.OnActionReceived(actions);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        base.Heuristic(actionsOut);
    }

    private void Attack()
    {
        Collider[] hitEnemies = Physics.OverlapBox(m_attackArea.position, m_attackRange, Quaternion.identity, 8);

        foreach(Collider target in hitEnemies)
        {
            if(target.tag != "Corpse")
            {
            }
        }
    }
    

    //Editor methods
    private void OnDrawGizmosSelected() 
    {
        if(m_attackArea != null)
        {
            Gizmos.DrawWireCube(m_attackArea.position, m_attackRange);    
        }
    }
}
