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

    private int score;
    private bool goalReached;
    float time;

    void Start() {
        generator = GetComponent<MazeConstructor>();
        StartNewGame();
    }

    private void StartNewGame()
    {

        score = 0;

        StartNewMaze();
    }

    private void StartNewMaze()
    {
        generator.GenerateNewMaze(sizeRows, sizeCols);
    }

    void FixedUpdate()
    {
        if (!agent.enabled)
        {
            return;
        }
        // time += Time.fixedDeltaTime;
        // if(time > 10)
        // {
        //     StartNewMaze();
        //     time = 0;
        // }

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
