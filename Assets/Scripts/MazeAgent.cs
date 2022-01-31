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
    [SerializeField]
    private int mazeCountToChange = 5;
    public Transform startPos;
    public List<Transform> goals;
    public bool stayPut;
    private Transform hitGoal;
    LayerMask mask;
    int width, height, counter, succesfulMazeCount;


    public override void OnEpisodeBegin()
    {
        counter = MaxStep;
        SendMessageUpwards("ResetGoal", transform.parent.GetSiblingIndex());
        GameController gc = transform.GetComponentInParent<GameController>();
        width = gc.sizeCols;
        height = gc.sizeRows;
        mask = ~LayerMask.GetMask("Object");
        if(startPos != null) transform.localPosition = startPos.localPosition;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        counter--;

        if(IsCloseToGoal())
        {
            if(hitGoal != null) 
            {
                succesfulMazeCount++;
                if(succesfulMazeCount >= mazeCountToChange)
                {
                    succesfulMazeCount = 0;
                    SendMessageUpwards("CreateNewMaze", transform.parent.GetSiblingIndex());
                }

                AddReward((float)counter/MaxStep);
                goals.RemoveAt(hitGoal.GetSiblingIndex());
                SendMessageUpwards("StartNewGame");
                EndEpisode();
            }
            hitGoal = null;
        }
        ActionSegment<int> disc = actions.DiscreteActions;
        Vector3 movement = Vector3.zero;
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
        if(!stayPut) transform.localPosition += movement;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        
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

    public void ResetStayPut()
    {
        stayPut = false;
    }
}
