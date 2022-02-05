using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;

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

    private int completedMazeCount, completionsToMazeChange = 5;
    private bool goalReached;
    private EnvironmentParameters env;
    float time;

    void Start() 
    {
        env = Academy.Instance.EnvironmentParameters;
        generator = GetComponent<MazeConstructor>();
        sizeRows = (int)env.GetWithDefault("MazeSize", 21);
        sizeCols = sizeRows + 2;
        StartNewGame();
    }

    /// <summary>
    /// 
    /// </summary>
    public void StartNewGame()
    {
        generator.GenerateAllMazes(sizeRows, sizeCols);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newRowSize"></param>
    /// <param name="newColSize"></param>
    public void ChangeMazeSize(int newRowSize, int newColSize)
    {
        sizeRows = newRowSize;
        sizeCols = newColSize;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="environment"></param>
    public void CreateNewMaze(int environment)
    {
        sizeRows = (int)env.GetWithDefault("MazeSize", 21);
        sizeCols = sizeRows + 2;
        generator.GenerateSingleMaze(sizeRows, sizeCols, environment);
    }
}
