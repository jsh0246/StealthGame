using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Common;
using Extra;
using Objects;
using System;
using System.IO;
using KDTreeDLL;
using Random = UnityEngine.Random;

using PseudoRandom;
using System.Runtime.InteropServices;

//using Meisui.Random;
//using MIConvexHull;

public class GraphGenerator : MonoBehaviour {
    
    public class Node
    {
        public int num { get; private set; }
        public double x { get; private set; }
        public double y { get; private set; }
        public int xPos { get; private set; }
        public int yPos { get; private set; }
        
        public Node(int num, double x, double y) {
            this.num = num;
            this.x = x;
            this.y = y;

            xPos = (int)(x / 0.5 + 60 / 2f);
            yPos = (int)(y / 0.5 + 60 / 2f);
        }
    }
    
    private Cell[][] map;
    private GameObject floor = null;
    private Mapper mapper;
    private Brushfire bf;
    private MersenneTwister xRng;
    private MersenneTwister yRng;

    private List<Vector3> vertices = new List<Vector3>();
    public List<Node>[] g;
    public List<float>[] w;
    public int startIndex, endIndex;
    private float minDist = 8f;
    private float delDist = 2.5f;
    private int degree = 3;
    private StreamReader samples;

    //[DllImport("/Assets/Plugins/libgsl.dll")]
    //private static extern double gsl_pow_2 (double x);
    //[DllImport("libgslcblas.dll")]
    //private static extern float gsl_rng_uniform(gsl_rng *r);


    //private static extern 
    
    // Needs Collision Chcek Function

    void Test() {
        //Debug.Log(gsl_pow_2(2));
    }

    void ReadSamples() {
        string str = null;
        samples = new StreamReader("Samples.txt");

        str = samples.ReadToEnd();
        string[] line = str.Split('\n');

        for(int i=0; i<line.Length-1; i++) {
            string[] s = line[i].Split(' ');

            //int x = Convert.ToInt32("555");
            //Debug.Log(int.TryParse(s[0]));

            int x = Convert.ToInt32(s[0]);
            int y = Convert.ToInt32(s[1]);
            //Debug.Log(x + " " + y);

            //int x = Int32.Parse(s[0]);
            //int y = Int32.Parse(s[1]);

            //Debug.Log(x + " " + y);

            Debug.Log(x + " " + y);
        }

        samples.Close();
        //Debug.Log(str);
    }
    
    void GenerateRandomVertices() {
        /*
        samples = new StreamReader("Samples.txt");
        string str = samples.ReadToEnd();
        string[] line = str.Split('\n');
        
        for(int i=0; i<line.Length-1; i++) {
            string[] s = line[i].Split(' ');
            
            int x = Convert.ToInt32(s[0]);
            int y = Convert.ToInt32(s[1]);

            if(!map[x][y].blocked)
                vertices.Add(bf.CrdntTransform(new Vector3(x, 0, y)));
        }
        
        samples.Close();
        */

        for (int i=0; i<200; i++)
        {
            //int x = xRng.genrand_N(60);
            //int y = yRng.genrand_N(60);
            
            int x = Random.Range(0, 60);
            int y = Random.Range(0, 60);

            if(!map[x][y].blocked)
                vertices.Add(bf.CrdntTransform(new Vector3(x, 0, y)));
            else
                i--;
        }
        
        // 가까운 버텍스 삭제
        List<Vector3> del = new List<Vector3>();
        for (int i=0; i<vertices.Count; i++)
        {
            for(int j=0; j<vertices.Count; j++) {
                if(i == j)
                    continue;
                
                if(Vector3.Distance(vertices[i], vertices[j]) < delDist) {
                    if(!del.Contains(vertices[j])) {
                        del.Add(vertices[i]);
                    }
                }
            }
        }
        
        for (int i=0; i<del.Count; i++)
        {
            vertices.Remove(del[i]);
        }
        
        //Debug.Log(vertices.Count);
    }
    
    public void SetRandomSeed() {
        DateTime now = DateTime.Now;
        UnityEngine.Random.seed = now.Millisecond + now.Second + now.Minute + now.Hour + now.Day + now.Month+ now.Year;
    }
    
    void Initialize() {
        xRng = new MersenneTwister();
        yRng = new MersenneTwister();
        
        /*
        ulong[] xInit = new ulong[] { 0x123, 0x234, 0x345, 0x456 };
        ulong[] yInit = new ulong[] { 0x567, 0x678, 0x789, 0x89a };
        xRng.init_by_array(xInit);
        yRng.init_by_array(yInit);
        */
        
        DateTime now = DateTime.Now;
        UnityEngine.Random.seed = now.Millisecond + now.Second + now.Minute + now.Hour + now.Day + now.Month+ now.Year;
        UnityEngine.Random.seed = 2861;
        Debug.Log("Unity Seed : " + UnityEngine.Random.seed);

        ulong x = (ulong)(now.Hour + now.Day + now.Month + now.Year + now.Second);
        ulong y = (ulong)(now.Millisecond + now.Second + now.Minute + now.Millisecond);

        xRng.init_genrand(x);
        yRng.init_genrand(y);
        
        //xRng.init_genrand(2829);
        //yRng.init_genrand(779);
        
        Debug.Log(x);
        Debug.Log(y);
    }
    
    void ComputeMap() {
        int gridSize = 60;
        
        bf = GetComponent<Brushfire>();
        floor = GameObject.Find("Floor");
        mapper = floor.GetComponent<Mapper>();
        mapper.ComputeTileSize (SpaceState.Editor, floor.GetComponent<Collider>().bounds.min, floor.GetComponent<Collider>().bounds.max, gridSize, gridSize);
        map = mapper.ComputeObstacles();
    }
    
    void FindStartEnd() {
        Vector3 s = bf.CrdntTransform(new Vector3(bf.vertices [13].xPos, 0, bf.vertices [13].yPos));
        Vector3 t = bf.CrdntTransform(new Vector3(bf.vertices [1].xPos, 0, bf.vertices [1].yPos));
        float sMin = 9999f;
        float tMin = 9999f;
        //int start = -1, end = -1;
        
        for (int i=0; i<vertices.Count; i++)
        {
            float ta = Vector3.Distance(vertices[i], s);
            float tb = Vector3.Distance(vertices[i], t); 
            if(ta < sMin) {
                sMin = ta;
                startIndex = i;
            }
            if(tb < tMin) {
                tMin = tb;
                endIndex = i;
            }
        }
    }
    
    void MakeGraph() {
        g = new List<Node>[vertices.Count];
        w = new List<float>[vertices.Count];
        for (int i=0; i<g.Length; i++)
        {
            g[i] = new List<Node>();
            g[i].Add(new Node(i, vertices[i].x, vertices[i].z));

            w[i] = new List<float>();
            w[i].Add(0f);
        }
        
        //Vector3 s = vertices [50];
        int layer = LayerMask.NameToLayer ("Obstacles");

        for(int i=0; i<vertices.Count; i++) {
            for (int j=0; j<vertices.Count; j++)
            {
                if(i == j)
                    continue;
                float dist = Vector3.Distance(vertices[i], vertices[j]);

                if(dist < minDist && !Physics.Linecast(vertices[i], vertices[j], 1<<layer) && g[i].Count <= degree) {
                    g[i].Add(new Node(j, vertices[j].x, vertices[j].z));
                    w[i].Add(dist);
                    //g[j].Add(new Node(i, vertices[i].x, vertices[i].z));
                }
            }
        }
    }
                
    void PrintGraph() {
        string str = null;
        
        for (int i=0; i<g.Length; i++)
        {
            str += i + " : ";
            for(int j=0; j<g[i].Count; j++) {
                str += g[i][j].num + " ";
            }
            
            str += '\n';
        }
        
        Debug.Log(str);
    }

    void PrintWeight() {
        string str = null;
        
        for (int i=0; i<g.Length; i++)
        {
            str += i + " : ";
            for(int j=0; j<g[i].Count; j++) {
                str += w[i][j] + " ";
            }
            
            str += '\n';
        }
        
        Debug.Log(str);
    }
    
    void Start () {
        Initialize();
        ComputeMap();

        Test();
        //ReadSamples();
        GenerateRandomVertices();
        MakeGraph();
        FindStartEnd();
        PrintGraph();
        PrintWeight();
        PrintVertexNumber();
    }
    
    void Update() {
        int layer = LayerMask.NameToLayer ("Obstacles");
        
        for(int i=0; i<vertices.Count; i++) {
            for(int j=0; j<vertices.Count; j++) {
                if(i == j)
                    continue;
                if(!Physics.Linecast(vertices[i], vertices[j], 1 << layer) && Vector3.Distance(vertices[i], vertices[j]) < minDist) {
                    Debug.DrawLine(vertices[i], vertices[j], Color.magenta);
                }
            }
        }
    }

    void PrintVertexNumber() {
        Camera camera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        GUIText[] text = new GUIText[vertices.Count];
        
        for(int i=0; i<vertices.Count; i++) {
            GameObject go = new GameObject("GUIText " + i);
            text[i] = (GUIText)go.AddComponent(typeof(GUIText));
            text[i].transform.position = camera.WorldToViewportPoint(vertices[i]);
            text[i].text = g[i][0].num.ToString();
            //text[i].text = depth[i].ToString();
            //text[i].text = pred[i].ToString();
            text[i].color = Color.magenta;
            text[i].fontSize = 15;
        }
    }
    
    void OnDrawGizmos() {
        /*
        Gizmos.color = Color.red;
        for(int i=0; i<vertices.Count; i++) {
            Gizmos.DrawSphere(vertices[i], 1);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(vertices [startIndex], 1);
        Gizmos.DrawSphere(vertices [endIndex], 1);
        */
        
        //Vector3 s = bf.CrdntTransform(new Vector3(bf.vertices [13].xPos, 0, bf.vertices [13].yPos));
        //Vector3 t = bf.CrdntTransform(new Vector3(bf.vertices [1].xPos, 0, bf.vertices [1].yPos));

        /*
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(vertices [50], 1);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(vertices[3], 1);
        Gizmos.DrawSphere(vertices[19], 1);
        Gizmos.DrawSphere(vertices[43], 1);
        Gizmos.DrawSphere(vertices[45], 1);
        */
    }
}
