using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class BossAI : MonoBehaviour
{
    [SerializeField]
    private Transform   m_target, m_startPos;
    [SerializeField]
    private float       m_nextWaypointDistance = 3f;

    Path                m_path;
    int                 m_currentWaypoint = 0;
    bool                m_endOfPath = false;
    bool                m_dummy = false;
    float               m_attackWaitTimer = 0f;
    float               m_attackTimerCap = 0.5f;

    Seeker              m_seeker;
    Rigidbody2D         m_rb2D;

    private void Start() 
    {
        Initialize();
    }

    public void Initialize()
    {
        transform.localPosition = m_startPos.localPosition;
        if(m_seeker == null) m_seeker = GetComponent<Seeker>();
        if(m_rb2D == null) m_rb2D = GetComponentInChildren<Rigidbody2D>();
        
        CancelInvoke();


        InvokeRepeating("UpdatePath", 0f, 0.5f);
    }

    void UpdatePath()
    {
        if(m_seeker.IsDone()) m_seeker.StartPath(m_rb2D.position, m_target.position, OnPathComplete);
    }

    void OnPathComplete(Path p)
    {
        if(!p.error)
        {
            m_path = p;
            m_currentWaypoint = 0;
        }
    }


    void FixedUpdate() 
    {
        if(m_path == null) return;

        if(m_currentWaypoint >= m_path.vectorPath.Count)
        {
            m_endOfPath = true;
            return;
        }
        else m_endOfPath = false;

        m_attackWaitTimer += Time.fixedDeltaTime;

        if(!m_dummy)
        {

            Vector2 direction = ((Vector2)(m_path.vectorPath[m_currentWaypoint]) - m_rb2D.position).normalized;

            float distance = Vector2.Distance(m_rb2D.position, m_path.vectorPath[m_currentWaypoint]);

            if(distance < m_nextWaypointDistance)
            {
                m_currentWaypoint++;
            }
        }
    }
}
