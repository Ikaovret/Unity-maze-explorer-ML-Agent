using System.Collections.Generic;
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

    private MazeDataGenerator dataGenerator;
    private MazeMeshGenerator meshGenerator;

    public int[,] data
    {
        get; private set;
    }
    public float hallWidth
    {
        get; private set;
    }
    public float hallHeight
    {
        get; private set;
    }

    public int startRow
    {
        get; private set;
    }
    public int startCol
    {
        get; private set;
    }

    public int goalRow
    {
        get; private set;
    }
    public int goalCol
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
    
    public void GenerateNewMaze(int sizeRows, int sizeCols)
    {
        if (sizeRows % 2 == 0 && sizeCols % 2 == 0)
        {
            Debug.LogError("Odd numbers work better for dungeon size.");
        }

        DisposeOldMaze();

        data = dataGenerator.FromDimensions(sizeRows, sizeCols);

        FindStartPosition();
        FindGoalPosition();

        // store values used to generate this mesh
        hallWidth = meshGenerator.width;
        hallHeight = meshGenerator.height;

        DisplayMaze();

        foreach(Transform envi in environments)
        {
            PlaceStartTrigger(envi);
            ResetGoal(envi.GetSiblingIndex());
        }
    }

    private void DisplayMaze()
    {
        GameObject go = new GameObject();
        go.transform.SetParent(transform);
        go.name = "Procedural Maze";
        go.tag = "Wall";
        go.layer = 9;

        go.transform.SetParent(environments[0]);
        go.transform.localPosition = Vector3.zero;

        float x = startCol * hallWidth;
        float y = environments[0].GetChild(0).localScale.y / 2f;
        float z = startRow * hallWidth;
        environments[0].GetChild(0).localPosition = new Vector3(x, y, z);

        MeshFilter mf = go.AddComponent<MeshFilter>();
        mf.mesh = meshGenerator.FromData(data);
        
        MeshCollider mc = go.AddComponent<MeshCollider>();
        mc.sharedMesh = mf.mesh;

        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.materials = new Material[2] {mazeMat1, mazeMat2};

        for(int i = 1; i < environments.Length; i++)
        {
            GameObject g = GameObject.Instantiate(go, environments[i].position, Quaternion.identity);
            g.transform.SetParent(environments[i]);
            g.transform.localPosition = Vector3.zero;

            x = startCol * hallWidth;
            y = environments[i].GetChild(0).localScale.y / 2f;
            z = startRow * hallWidth;
            environments[i].GetChild(0).localPosition = new Vector3(x, y, z);
        }        
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

    public void DisposeOldMaze()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Wall");
        foreach (GameObject go in objects) 
        {
            Destroy(go);
        }
    }

    private void FindStartPosition()
    {
        int[,] maze = data;
        int rMax = maze.GetUpperBound(0);
        int cMax = maze.GetUpperBound(1);

        for (int i = 0; i <= rMax; i++)
        {
            for (int j = 0; j <= cMax; j++)
            {
                if (maze[i, j] == 0)
                {
                    startRow = i;
                    startCol = j;
                    return;
                }
            }
        }
    }

    private void FindGoalPosition()
    {
        int[,] maze = data;
        int rMax = maze.GetUpperBound(0);
        int cMax = maze.GetUpperBound(1);

        // loop top to bottom, right to left
        for (int i = rMax; i >= 0; i--)
        {
            for (int j = cMax; j >= 0; j--)
            {
                if (maze[i, j] == 0)
                {
                    goalRow = i;
                    goalCol = j;
                    return;
                }
            }
        }
    }

    private void PlaceStartTrigger(Transform location = null)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        if(location != null) 
        {
            go.transform.SetParent(location);
            location.GetComponentInChildren<MazeAgent>().startPos = go.transform;
        }
        go.transform.localPosition = new Vector3(startCol * hallWidth, .5f, startRow * hallWidth);
        go.name = "Start Trigger";
        go.tag = "Untagged";
        go.layer = 8;

        go.GetComponent<BoxCollider>().isTrigger = true;
        go.GetComponent<MeshRenderer>().sharedMaterial = startMat;
        go.SetActive(false);
    }

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
        else
        {
            go.transform.localPosition = new Vector3(goalCol * hallWidth, 1.5f, goalRow * hallWidth);
        }
        go.name = "Treasure";
        go.tag = "Goal";
        go.layer = 8;

        go.GetComponent<BoxCollider>().isTrigger = true;
        go.GetComponent<MeshRenderer>().sharedMaterial = treasureMat;
    }

    private void PlaceGoalTrigger(Vector3 position)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.localScale = new Vector3(3, 3, 3);
        go.transform.position = position;
        go.name = "Treasure";
        go.tag = "Goal";
        go.layer = 8;

        go.GetComponent<BoxCollider>().isTrigger = true;
        go.GetComponent<MeshRenderer>().sharedMaterial = treasureMat;
    }

    public void ResetGoal(int environment)
    {
        for(int i = 0; i < environments[environment].GetChild(2).childCount; i++)
        {
            Destroy(environments[environment].GetChild(2).GetChild(i).gameObject);
        }

        for(int i = 0; i < goalCount; i++)
        {
            PlaceGoalTrigger(environments[environment]);
        }
    }

    public void ResetGoal(System.Tuple<int, int> data)
    {
        Destroy(environments[data.Item1].GetChild(2).GetChild(data.Item2).gameObject);
        PlaceGoalTrigger(environments[data.Item1]);
    }

    public Vector3 RandomizeGoalPlacement(Transform environment)
    {
        List<Vector3> goalLocations = CollectPossibleGoalLocations(environment.position);
        return goalLocations[Random.Range(0, goalLocations.Count)];
    }

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
}