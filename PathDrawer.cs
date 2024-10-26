using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;

public class PathDrawer : MonoBehaviour {

    private Brushfire bf;
    private Graph graph;
    private StreamReader gs, rrt, guard;

    private List<List<Vector3>> gsPaths = new List<List<Vector3>>();
    private List<List<Vector3>> rrtPaths = new List<List<Vector3>>();
    private List<List<Vector3>> guardPaths = new List<List<Vector3>>();
    private List<int> gsColor = new List<int>();

    void DrawGraphSamplePaths() {
        int index = 0;

        // Load paths from file until blank line, blank line means end path of player path for one enemy path
        while (true)
        {
            int i;
            string path = gs.ReadLine();
            //Debug.Log(path);

            if(path.Length == 0)
                break;

            string[] parsed = path.Split(' ');
            gsPaths.Add(new List<Vector3>());

            for (i=0; i<parsed.Length-1; i++)
            {
                int s = Convert.ToInt32(parsed [i]);
                gsPaths[index].Add(bf.CrdntTransform(new Vector3(bf.vertices [s].xPos, 0f, bf.vertices [s].yPos)));
            }
            gsColor.Add(Convert.ToInt32(parsed[i]));

            index++;
        }

        // Draw paths
        for(int i=0; i<gsPaths.Count; i++) {
            Color c = new Color (UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f));
            for(int j=0; j<gsPaths[i].Count-1; j++) {
                DebugLine.DrawLine(gsPaths[i][j], gsPaths[i][j+1], c, 5, gsColor[i]/10f);
            }
        }

        // Remove paths and color data after draw paths and use for next enemy path
        for(int i=gsPaths.Count-1; i>=0; i--) {
            for(int j=gsPaths[i].Count-1; j>=0; j--) {
                gsPaths[i].RemoveAt(j);
            }
            gsPaths.RemoveAt(i);
            gsColor.RemoveAt(i);
        }
    }

    void DrawRRTPaths() {
        int index = 0;

        // Load paths from file
        while (true)
        {
            string path = rrt.ReadLine();
            //Debug.Log(path);

            if(path.Length == 0)
                break;

            string[] parsed = path.Split(' ');
            rrtPaths.Add(new List<Vector3>());

            for(int i=0; i<parsed.Length-1; i=i+2) {
                int s = Convert.ToInt32(parsed[i]);
                int t = Convert.ToInt32(parsed[i+1]);

                rrtPaths[index].Add(bf.CrdntTransform(new Vector3(s, 0f, t)));
            }
            index++;
        }

        // Draw paths
        for(int i=0; i<rrtPaths.Count; i++) {
            //Color c = new Color (UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f));
            for(int j=0; j<rrtPaths[i].Count-1; j++) {
                DebugLine.DrawLine(rrtPaths[i][j], rrtPaths[i][j+1], Color.black, 5, 1);
            }
        }

        // Remove paths data
        for(int i=rrtPaths.Count-1; i>=0; i--) {
            for(int j=rrtPaths[i].Count-1; j>=0; j--) {
                rrtPaths[i].RemoveAt(j);
            }
            rrtPaths.RemoveAt(i);
        }
    }

    void Initialize() {
        bf = GetComponent<Brushfire>();
        graph = GetComponent<Graph>();

        gs = new StreamReader("Test/GSPaths.txt");
        rrt = new StreamReader("Test/RRTPaths.txt");
        guard = new StreamReader("3Guard Paths.txt");

        DateTime now = DateTime.Now;
        UnityEngine.Random.seed = now.Millisecond + now.Second + now.Minute + now.Hour + now.Day + now.Month+ now.Year;
    }

	void Start () {
        Initialize();
    }

    void OnGUI() {
        if(GUI.Button(new Rect(800, 70, 150, 30), "Next GS")) {
            DrawGraphSamplePaths();
        }
        if(GUI.Button(new Rect(800, 120, 150, 30), "Next RRT")) {
            DrawRRTPaths();
        }
        if(GUI.Button(new Rect(800, 170, 150, 30), "Next Guard")) {
            graph.RandomTwoGuardPath();
        }
        if(GUI.Button(new Rect(800, 220, 150, 30), "Close")) {
            gs.Close();
            rrt.Close();
            guard.Close();
        }
    }
    
    void Update () {
        
    }
}
