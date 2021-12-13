using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class MazeAgent : Agent
{
    [SerializeField]
    private float m_speed;
    [SerializeField]
    private Rigidbody m_rb;
    [SerializeField]
    private Transform m_startPos;


    public override void OnEpisodeBegin()
    {
        transform.localPosition = m_startPos.localPosition;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        ActionSegment<int> disc = actions.DiscreteActions;
        ActionSegment<float> cont = actions.ContinuousActions;
        
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = cont[0];
        controlSignal.z = cont[1];
        m_rb.AddForce(controlSignal * m_speed);

        Vector3 vel = m_rb.velocity;
        if(m_rb.velocity.z > 30f) vel.z = 30f;
        else if(m_rb.velocity.z < -30f) vel.z = -30f;
        if(m_rb.velocity.x > 30f) vel.x = 30f;
        else if(m_rb.velocity.x < -30f) vel.x = -30f;
        m_rb.velocity = vel;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> disc = actionsOut.DiscreteActions;
        ActionSegment<float> cont = actionsOut.ContinuousActions;

        cont[1] = Input.GetAxis("Vertical");
        cont[0] = Input.GetAxis("Horizontal");
    }

    private void Move(Vector3 direction)
    {
        m_rb.velocity = direction * m_speed;
    }

    private void OnTriggerEnter(Collider other) 
    {
        if(other.tag == "Goal")
        {
            AddReward(1f);
            EndEpisode();
        }
    }
}
