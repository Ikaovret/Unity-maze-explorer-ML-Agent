using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class MazeAgent : Agent
{
    [SerializeField, Tooltip("Remember to set vector observations to 0")]
    private bool useVectorObs;
    [SerializeField]
    private float step = 3.75f;
    [SerializeField]
    private int mazeCountToChange = 5;

    private Transform hitGoal;
    private LayerMask mask;
    private int width, height, counter, mazesTillChange = -1;

    public Transform startPos;
    public List<Transform> goals;
    private bool pausedActions;

    public override void OnEpisodeBegin()
    {
        mazesTillChange++;
        if(mazesTillChange >= mazeCountToChange)
        {
            mazesTillChange = 0;
            SendMessageUpwards("CreateNewMaze", transform.parent.GetSiblingIndex());
        }
        counter = MaxStep;
        GameController gc = transform.GetComponentInParent<GameController>();
        width = gc.sizeCols;
        height = gc.sizeRows;
        mask = ~LayerMask.GetMask("Object");
        if(startPos != null) 
        {
            Vector3 pos = Vector3.zero;
            pos.x = startPos.localPosition.x;
            pos.y = transform.localPosition.y;
            pos.z = startPos.localPosition.z;

            transform.localPosition = pos;
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if(pausedActions)
        {
           return;
        }

        counter--;

        if(IsCloseToGoal())
        {
            if(hitGoal != null) 
            {
                SendMessageUpwards("IncreaseScore", transform.parent.GetSiblingIndex(), SendMessageOptions.DontRequireReceiver);
                if(MaxStep != 0) AddReward((float)counter/MaxStep);
                SendMessageUpwards("ResetGoal", new System.Tuple<int, int>(transform.parent.GetSiblingIndex(),hitGoal.GetSiblingIndex()));
                goals.RemoveAt(hitGoal.GetSiblingIndex());
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

        transform.localPosition += movement;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);

        if(useVectorObs)
        {
            sensor.AddObservation(DirectionBitMaskCreator.CollectNormalisedDirection(this.transform));
            sensor.AddObservation(this.transform.localPosition);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> disc = actionsOut.DiscreteActions;
        if(Input.GetKey(KeyCode.W)) disc[0] = 1;
        if(Input.GetKey(KeyCode.S)) disc[0] = 2;
        if(Input.GetKey(KeyCode.D)) disc[0] = 3;
        if(Input.GetKey(KeyCode.A)) disc[0] = 4;
    }

    /// <summary>
    /// Checks if the agent is in the same "square" as the goal
    /// </summary>
    /// <returns></returns>
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

    public void SetPausedActions(bool value){ pausedActions = value; }

    /// <summary>
    /// Internal class that handles the creation of a normalised bit mask
    /// </summary>
    internal static class DirectionBitMaskCreator
    {
        public enum States
        {
            ST_EMPTY        = 0,        // 0        00000000
            ST_NORTH        = 1 << 0,   // 1        00000001
            ST_EAST         = 1 << 1,   // 2        00000010
            ST_SOUTH        = 1 << 2,   // 4        00000100
            ST_WEST         = 1 << 3,   // 8        00001000
            ST_GOAL_NORTH   = 1 << 4,   // 16       00010000
            ST_GOAL_EAST    = 1 << 5,   // 32       00100000
            ST_GOAL_SOUTH   = 1 << 6,   // 64       01000000
            ST_GOAL_WEST    = 1 << 7,   // 128      10000000

            ST_ALL = (ST_NORTH | ST_EAST | ST_SOUTH | ST_WEST | ST_GOAL_NORTH | ST_GOAL_EAST | ST_GOAL_SOUTH | ST_GOAL_WEST)
        }

        /// <summary>
        /// Handles the normalisation of the bit mask
        /// </summary>
        /// <param name="startPosition"></param>
        /// <returns></returns>
        public static float CollectNormalisedDirection(Transform startPosition)
        {
            return (float)CreateDirectionBitMask(startPosition)/(float)States.ST_ALL;
        }

        /// <summary>
        /// Creates the bit mask by checking the directions with Raycasts
        /// </summary>
        /// <param name="startPosition"></param>
        /// <returns></returns>
        private static int CreateDirectionBitMask(Transform startPosition)
        {
            int mask = 0;

            RaycastHit hit;

            // NORTH
            if(Physics.Raycast(startPosition.position, Vector3.forward, out hit, 5f))
            {
                if(hit.collider.tag == "Wall") mask += (int)States.ST_NORTH;
                else if(hit.collider.tag == "Goal") mask += (int)States.ST_GOAL_NORTH;
            }

            // EAST
            if(Physics.Raycast(startPosition.position, Vector3.right, out hit, 5f))
            {
                if(hit.collider.tag == "Wall") mask += (int)States.ST_EAST;
                else if(hit.collider.tag == "Goal") mask += (int)States.ST_GOAL_EAST;
            }

            // SOUTH
            if(Physics.Raycast(startPosition.position, Vector3.back, out hit, 5f))
            {
                if(hit.collider.tag == "Wall") mask += (int)States.ST_SOUTH;
                else if(hit.collider.tag == "Goal") mask += (int)States.ST_GOAL_SOUTH;
            }

            // WEST
            if(Physics.Raycast(startPosition.position, Vector3.left, out hit, 5f))
            {
                if(hit.collider.tag == "Wall") mask += (int)States.ST_WEST;
                else if(hit.collider.tag == "Goal") mask += (int)States.ST_GOAL_WEST;
            }

            return mask;
        }
    }

}
