using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class player : MonoBehaviour
{
    public float x;
    public float y;

    public int numOfFrames;
    public int numOfRobots;

    public GameObject robot;
    public GameObject robot2;
    public GameObject particle;
    public GameObject[] particles;
    public GameObject[] robots;
    public int currAnimFrame = 0;

    public Color[] colors;

    public int ScaleMultiplier = 7;
    Vector2[][] frames;
    public bool isPlaying = false;

    public string gameDataFileName = "data1.csv";
    public string goalsFileName = "goals1.csv";

    // Use this for initialization
    void Start ()
    {
        LoadGameData();
        LoadGoals();
    }

    private void LoadGoals()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, goalsFileName);
        if (File.Exists(filePath))
        {
            string goalsAsCSV = File.ReadAllText(filePath);
            StringReader strReader = new StringReader(goalsAsCSV);
            string[] s = goalsAsCSV.Split('\n');
            int numOfGoals = Int32.Parse(s[0]);
            Debug.Log(numOfGoals);
            for(int i = 0; i < numOfGoals; i++)
            {
                string[] coords = s[1].Split(' ');

                particles[i].transform.position = new Vector3(float.Parse(coords[2 * i]) * ScaleMultiplier, 0, float.Parse(coords[2 * i + 1]) * ScaleMultiplier);
            }
        }
    }

    private void calculateFramesAndRobots()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, gameDataFileName);

        if (File.Exists(filePath))
        {
            string dataAsCSV = File.ReadAllText(filePath);
            StringReader strReader = new StringReader(dataAsCSV);
            string[] s = dataAsCSV.Split('\n');
            numOfFrames = s.Length - 1;
            numOfRobots = Int32.Parse(s[0]);
        }
    }

    private void LoadGameData()
    {
        // Path.Combine combines strings into a file path
        // Application.StreamingAssets points to Assets/StreamingAssets in the Editor, and the StreamingAssets folder in a build
        string filePath = Path.Combine(Application.streamingAssetsPath, gameDataFileName);

        calculateFramesAndRobots();
        System.Random r = new System.Random();
        robots = new GameObject[numOfRobots];
        frames = new Vector2[numOfFrames][];
        for (int i = 0; i < numOfFrames; i++)
            frames[i] = new Vector2[numOfRobots];

        for (int i = 0; i < numOfRobots; i++)
        {
            robots[i] = (GameObject)Instantiate(robot2, new Vector3(0, 0, 0), Quaternion.identity);
            robots[i].transform.Find("Object004").GetComponent<Renderer>().material.color = colors[i];
        }

        int currentFrameNumber=0;

        if (File.Exists(filePath))
        {
            // Read the json from the file into a string
            string dataAsCSV = File.ReadAllText(filePath);
            StringReader strReader = new StringReader(dataAsCSV);
            string aLine = strReader.ReadLine();    // preskacemo prvi red jer broj robota
            while (true)
            {
                aLine = strReader.ReadLine();
                if(aLine != null)
                {
                    string[] coords = aLine.Split(' ');

                    for(int i = 0; i < coords.Length; i++)
                    {
                        if (i % 2 == 0)
                            frames[currentFrameNumber][i / 2].x = ScaleMultiplier * float.Parse(coords[i]);
                        else
                        {
                            frames[currentFrameNumber][i / 2].y = ScaleMultiplier * float.Parse(coords[i]);
                    //        Debug.Log(currentFrameNumber + ". Robot[" + i / 2 + "].x = " + frames[currentFrameNumber][i / 2].x + " a y = " + frames[currentFrameNumber][i / 2].y);
                        }
                    }
                    currentFrameNumber++;
                }
                else
                {
                    break;
                }
            }
        //    Debug.Log(dataAsCSV);
        }
        else
        {
            Debug.LogError("Cannot load game data!");
        }
    }
    public int substep = 0;
    public int number_of_substeps = 100;
    // Update is called once per frame
    void Update ()
    {
        if (isPlaying && currAnimFrame < numOfFrames-2)
        {
            for(int i = 0; i < numOfRobots; i++)
            {
                Vector3 forwardVec = new Vector3((frames[currAnimFrame + 1][i].x - frames[currAnimFrame][i].x), 0, frames[currAnimFrame + 1][i].y - frames[currAnimFrame][i].y);
                forwardVec = new Vector3((forwardVec.x * substep) / number_of_substeps, 0, (forwardVec.z * substep ) / number_of_substeps);
                robots[i].transform.position = new Vector3(frames[currAnimFrame][i].x + forwardVec.x, 0, frames[currAnimFrame][i].y + forwardVec.z);
                robots[i].transform.forward = forwardVec;
                if(currAnimFrame < numOfFrames - 2)
                {
                    forwardVec = Vector3.Normalize(forwardVec);
                    Vector3 nextForwardVec = new Vector3((frames[currAnimFrame + 2][i].x - frames[currAnimFrame + 1][i].x), 0, frames[currAnimFrame + 2][i].y - frames[currAnimFrame + 1][i].y);
                    nextForwardVec = Vector3.Normalize(nextForwardVec);
                    Vector3 newForwardVec = forwardVec / number_of_substeps + (forwardVec / number_of_substeps) * substep;
                    robots[i].transform.rotation = Quaternion.Slerp(Quaternion.LookRotation(forwardVec, new Vector3(0, 1, 0)), Quaternion.LookRotation(nextForwardVec, new Vector3(0, 1, 0)), (float)(1.0 * substep) / number_of_substeps);
                   // if(i == 0)
                  //      Debug.Log(forwardVec + ", " + nextForwardVec + ", " + (float)((1.0 * substep) / number_of_substeps));
                }
            }
            if (substep == number_of_substeps + 1)
            {
                if (currAnimFrame < numOfFrames - 1)
                    currAnimFrame++;
                substep = 0;
            }
            else
            {
             //   Debug.Log("Substep je: " + substep);
                substep++;
            }
        }
	}
}
