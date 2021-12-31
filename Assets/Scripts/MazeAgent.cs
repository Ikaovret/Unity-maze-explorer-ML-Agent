using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class MazeAgent : Agent
{
    [SerializeField]
    private float step = 3.75f;
    public Transform startPos;
    public List<Transform> goals;
    private Transform hitGoal;


    public override void OnEpisodeBegin()
    {
        // if(startPos != null) transform.localPosition = startPos.localPosition;
        if(hitGoal != null) 
        {
            goals.RemoveAt(hitGoal.GetSiblingIndex());
            SendMessageUpwards("ResetGoal", new System.Tuple<int, int>(transform.parent.GetSiblingIndex(), hitGoal.GetSiblingIndex()));
        }
        hitGoal = null;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if(IsCloseToGoal())
        {
            AddReward(1f);
            EndEpisode();
        }
        AddReward(-0.01f);
        ActionSegment<int> disc = actions.DiscreteActions;
        Vector3 movement = Vector3.zero;
        LayerMask mask = LayerMask.GetMask("Object");
        mask = ~mask;
        switch(disc[0])
        {
            case 1:
                if(!Physics.Raycast(transform.position, Vector3.forward, step, mask)) movement.z += step;
                break;
            case 2:
                if(!Physics.Raycast(transform.position, Vector3.back, step, mask)) movement.z -= step;
                break;
            case 3:
                if(!Physics.Raycast(transform.position, Vector3.right, step, mask)) movement.x += step;
                break;
            case 4:
                if(!Physics.Raycast(transform.position, Vector3.left, step, mask)) movement.x -= step;
                break;
        }
        transform.localPosition += movement;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> disc = actionsOut.DiscreteActions;
        if(Input.GetKey(KeyCode.W)) disc[0] = 1;
        if(Input.GetKey(KeyCode.S)) disc[0] = 2;
        if(Input.GetKey(KeyCode.D)) disc[0] = 3;
        if(Input.GetKey(KeyCode.A)) disc[0] = 4;
    }

    private void OnDrawGizmos() 
    {
        Gizmos.DrawRay(transform.position, Vector3.forward * step);
    }

    private bool IsCloseToGoal()
    {
        foreach(Transform tran in goals)
        {
            if(Vector3.Distance(transform.localPosition, tran.localPosition) < 3)
            {
                hitGoal = tran;
                return true;
            }
        }
        return false;
    }
}
