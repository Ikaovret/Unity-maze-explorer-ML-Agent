using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MazeConstructor))]

public class GameController : MonoBehaviour
{
    [SerializeField] private MazeAgent agent;
    [SerializeField] private int m_sizeRows = 13, m_sizeCols = 15;
    public int sizeRows
    { 
        get { return m_sizeRows; } private set { m_sizeRows = value; }
    }

    public int sizeCols
    {
        get { return m_sizeCols; } private set { m_sizeCols = value; }
    }

    private MazeConstructor generator;

    private DateTime startTime;
    private int timeLimit;
    private int reduceLimitBy;

    private int score;
    private bool goalReached;

    void Start() {
        generator = GetComponent<MazeConstructor>();
        StartNewGame();
    }

    private void StartNewGame()
    {
        timeLimit = 80;
        reduceLimitBy = 5;
        startTime = DateTime.Now;

        score = 0;

        StartNewMaze();
    }

    private void StartNewMaze()
    {
        generator.GenerateNewMaze(sizeRows, sizeCols);

        // restart timer
        timeLimit -= reduceLimitBy;
        startTime = DateTime.Now;
    }

    void FixedUpdate()
    {
        if (!agent.enabled)
        {
            return;
        }

        // int timeUsed = (int)(DateTime.Now - startTime).TotalSeconds;
        // int timeLeft = timeLimit - timeUsed;

        // if (timeLeft > 0)
        // {
        //     timeLabel.text = timeLeft.ToString();
        // }
        // else
        // {
        //     timeLabel.text = "TIME UP";
        //     agent.enabled = false;

        //     Invoke("StartNewGame", 4);
        // }
    }

    private void OnGoalTrigger(GameObject trigger, GameObject other)
    {
        Debug.Log("Goal!");
        goalReached = true;

        score += 1;

        Destroy(trigger);
    }

    private void OnStartTrigger(GameObject trigger, GameObject other)
    {
        if (goalReached)
        {
            Debug.Log("Finish!");
            agent.enabled = false;

            Invoke("StartNewMaze", 4);
        }
    }
}
