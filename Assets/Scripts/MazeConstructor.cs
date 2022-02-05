﻿using System.Collections.Generic;
using UnityEngine;

public class MazeConstructor : MonoBehaviour
{
    public bool showDebug;
    
    [SerializeField] private Material mazeMat1;
    [SerializeField] private Material mazeMat2;
    [SerializeField] private Material startMat;
    [SerializeField] private Material treasureMat;
    [SerializeField] private Transform[] environments;
    [SerializeField] private int goalCount = 4;

    [SerializeField] private float width = 3.75f, height = 3.5f;
    [SerializeField, Tooltip("Toggles whether or not goals are placed at random.\nIf goal count > 1 all goals will be stacked of top of each other")]
    private bool randomizeGoals = false;

    private MazeDataGenerator dataGenerator;
    private MazeMeshGenerator meshGenerator;
    
    private int cols, rows;

    public int[,] data
    {
        get; private set;
    }

    void Awake()
    {
        dataGenerator = new MazeDataGenerator();
        meshGenerator = new MazeMeshGenerator(width, height);
        // default to walls surrounding a single empty cell
        data = new int[,]
        {
            {1, 1, 1},
            {1, 0, 1},
            {1, 1, 1}
        };
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sizeRows"></param>
    /// <param name="sizeCols"></param>
    public void GenerateAllMazes(int sizeRows, int sizeCols)
    {
        if (sizeRows % 2 == 0 && sizeCols % 2 == 0)
        {
            Debug.LogError("Odd numbers work better for dungeon size.");
        }

        rows = sizeRows;
        cols = sizeCols;

        foreach(Transform envi in environments)
        {
            DisposeSingleOldMaze(envi.GetSiblingIndex());

            data = dataGenerator.FromDimensions(sizeRows, sizeCols);

            DisplaySingleMaze(envi.GetSiblingIndex());

            PlaceStartTrigger(envi);
            ResetGoal(envi.GetSiblingIndex());
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sizeRows"></param>
    /// <param name="sizeCols"></param>
    /// <param name="environment"></param>
    public void GenerateSingleMaze(int sizeRows, int sizeCols, int environment)
    {
        if(environment >= environments.Length || environment < 0) return;

        if (sizeRows % 2 == 0 && sizeCols % 2 == 0)
        {
            Debug.LogError("Odd numbers work better for dungeon size.");
        }

        rows = sizeRows;
        cols = sizeCols;

        DisposeSingleOldMaze(environment);

        data = dataGenerator.FromDimensions(sizeRows, sizeCols);

        DisplaySingleMaze(environment);

        PlaceStartTrigger(environments[environment]);
        ResetGoal(environment);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="environment"></param>
    private void DisplaySingleMaze(int environment)
    {
        if(environment >= environments.Length || environment < 0) return;

        GameObject go = new GameObject();
        go.transform.SetParent(environments[environment]);
        go.name = "Procedural Maze";
        go.tag = "Wall";
        go.layer = 9;

        go.transform.localPosition = Vector3.zero;

        float x = width;
        float y = environments[environment].GetChild(0).localScale.y / 2f;
        float z = width;
        environments[environment].GetChild(0).localPosition = new Vector3(x, y, z);

        MeshFilter mf = go.AddComponent<MeshFilter>();
        mf.mesh = meshGenerator.FromData(data);
        
        MeshCollider mc = go.AddComponent<MeshCollider>();
        mc.sharedMesh = mf.mesh;

        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.materials = new Material[2] {mazeMat1, mazeMat2};
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="environment"></param>
    public void DisposeSingleOldMaze(int environment)
    {
        if(environment >= environments.Length || environment < 0) return;
        for(int i = 3; i < environments[environment].childCount; i++)
        {
            Destroy(environments[environment].GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    private void PlaceStartTrigger(Transform location = null)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        if(location != null) 
        {
            go.transform.SetParent(location);
            location.GetComponentInChildren<MazeAgent>().startPos = go.transform;
        }
        go.transform.localPosition = new Vector3(width, .5f, width);
        go.name = "Start Trigger";
        go.tag = "Untagged";
        go.layer = 8;

        go.GetComponent<BoxCollider>().isTrigger = true;
        go.GetComponent<MeshRenderer>().sharedMaterial = startMat;
        go.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    private void PlaceGoalTrigger(Transform location = null)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.localScale = new Vector3(3, 3, 3);
        if(location != null) 
        {
            go.transform.SetParent(location);
            location.GetComponentInChildren<MazeAgent>().goals.Add(go.transform);
            go.transform.SetParent(location.GetChild(2));
            go.transform.localPosition = RandomizeGoalPlacement(location);
        }
        go.name = "Treasure";
        go.tag = "Goal";
        go.layer = 8;

        go.GetComponent<BoxCollider>().isTrigger = true;
        go.GetComponent<MeshRenderer>().sharedMaterial = treasureMat;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="environment"></param>
    public void ResetGoal(int environment)
    {
        environments[environment].GetComponentInChildren<MazeAgent>().goals.Clear();
        for(int i = 0; i < environments[environment].GetChild(2).childCount; i++)
        {
            Destroy(environments[environment].GetChild(2).GetChild(i).gameObject);
        }

        for(int i = 0; i < goalCount; i++)
        {
            PlaceGoalTrigger(environments[environment]);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    public void ResetGoal(System.Tuple<int, int> data)
    {
        Destroy(environments[data.Item1].GetChild(2).GetChild(data.Item2).gameObject);
        PlaceGoalTrigger(environments[data.Item1]);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="environment"></param>
    /// <returns></returns>
    public Vector3 RandomizeGoalPlacement(Transform environment)
    {
        if(randomizeGoals)
        {
            List<Vector3> goalLocations = CollectPossibleGoalLocations(environment.position);
            return goalLocations[Random.Range(0, goalLocations.Count)];
        }
        else return new Vector3(width*rows, 1.5f, width*(cols-4));
        
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="raycastOrigin"></param>
    /// <returns></returns>
    private List<Vector3> CollectPossibleGoalLocations(Vector3 raycastOrigin)
    {
        int sizeCols = GetComponent<GameController>().sizeCols;
        int sizeRows = GetComponent<GameController>().sizeRows;

        List<Vector3> goalLocations = new List<Vector3>();
        RaycastHit hit;

        raycastOrigin.y += 5;
        for(int i = 1; i <= sizeRows; i++)
        {
            raycastOrigin.z += width;
            for(int j = 1; j <= sizeCols; j++)
            {
                raycastOrigin.x += width;
                if(Physics.Raycast(raycastOrigin, Vector3.down, out hit, 6))
                {
                    if(hit.transform.tag == "Wall")
                    {
                        goalLocations.Add(new Vector3(raycastOrigin.x, 1.5f, raycastOrigin.z));
                    }
                }
            }
            raycastOrigin.x -= width*sizeCols;
        }

        return goalLocations;
    }
    
    void OnGUI()
    {
        if (!showDebug)
        {
            return;
        }

        int[,] maze = data;
        int rMax = maze.GetUpperBound(0);
        int cMax = maze.GetUpperBound(1);

        string msg = "";

        for (int i = rMax; i >= 0; i--)
        {
            for (int j = 0; j <= cMax; j++)
            {
                if (maze[i, j] == 0)
                {
                    msg += "....";
                }
                else
                {
                    msg += "==";
                }
            }
            msg += "\n";
        }

        GUI.Label(new Rect(20, 20, 500, 500), msg);
    }
}