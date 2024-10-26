using UnityEngine;
using System.Collections.Generic;
using Delaunay;
using Delaunay.Geo;
using System;

using Common;

public class DelaunayMesh : MonoBehaviour {
    [SerializeField]
    public int
        m_pointCount = 300;
    
    private List<Vector2> m_points;

    private float m_mapWidth = 30;
    private float m_mapHeight = 30;
    private List<LineSegment> m_edges = null;
    private List<LineSegment> m_spanningTree;
    private List<LineSegment> m_delaunayTriangulation;

    private Brushfire bf;
    private GameObject floor = null;
    private Mapper mapper;
    private Graph graph;
    private Cell[][] map;
    public List<Vector3> m_points3;
    public List<Vector3> vertices = new List<Vector3>();
    public List<List<int>> vertexList = new List<List<int>>();
    public List<List<int>> weightList = new List<List<int>>();
    public List<List<float>> weightListF = new List<List<float>>();
    public int startIndex, endIndex;

    
    void Start () {
        ComputeMap();
        SetRandomSeed();
        Demo ();
        GenerateGraph();
        FindStartEnd();
    }
    
    void Update () {
        /*
        if (Input.anyKeyDown) {
            Demo ();
        }
        */
    }
    
    private void Demo () {
        List<uint> colors = new List<uint> ();
        m_points = new List<Vector2> ();
        m_points3 = new List<Vector3>();
        
        for (int i = 0; i < m_pointCount; i++) {
            colors.Add (0);

            Vector3 point = new Vector3(UnityEngine.Random.Range (0, 60), 0f, UnityEngine.Random.Range(0, 60));
            if(map[(int)point.x][(int)point.z].blocked) {
                i--;
                continue;
            }

            vertices.Add(point);
            point = bf.CrdntTransform(point);
            m_points.Add(new Vector2(point.x, point.z));
            m_points3.Add(new Vector3(point.x, 0f, point.z));

            /*
            m_points.Add (new Vector2 (
                UnityEngine.Random.Range (-m_mapWidth/2f, m_mapWidth/2f),
                UnityEngine.Random.Range (-m_mapWidth/2f, m_mapHeight/2f))
                          );
                          */
        }

        Delaunay.Voronoi v = new Delaunay.Voronoi (m_points, colors, new Rect (0, 0, m_mapWidth, m_mapHeight));
        //m_edges = v.VoronoiDiagram ();
        
        //m_spanningTree = v.SpanningTree (KruskalType.MINIMUM);
        m_delaunayTriangulation = v.DelaunayTriangulation ();
    }

    void GenerateGraph() {
        for(int i=0; i<m_points3.Count; i++) {
            vertexList.Add(new List<int>());
            vertexList[i].Add(i);

            weightList.Add(new List<int>());
            weightList[i].Add(0);

            weightListF.Add(new List<float>());
            weightListF[i].Add(0f);
        }

        int layer = LayerMask.NameToLayer ("Obstacles");
        for (int i=0; i<m_delaunayTriangulation.Count; i++) {
            Vector2 left = (Vector2)m_delaunayTriangulation[i].p0;
            Vector2 right = (Vector2)m_delaunayTriangulation[i].p1;
            Vector3 l = new Vector3(left.x, 0f, left.y);
            Vector3 r = new Vector3(right.x, 0f, right.y);

            if(!Physics.Linecast(l, r, 1 << layer)) {
                int leftIdx = m_points3.IndexOf(l);
                int rightIdx = m_points3.IndexOf(r);
                vertexList[leftIdx].Add(rightIdx);
                vertexList[rightIdx].Add(leftIdx);

                int dist = (int)Math.Ceiling(Vector3.Distance(l, r));
                weightList[leftIdx].Add(dist);
                weightList[rightIdx].Add(dist);

                float fDist = Vector3.Distance(l, r);
                weightListF[leftIdx].Add(fDist);
                weightListF[rightIdx].Add(fDist);
            }
        }

        // 洹몃옒??由ъ뒪??異쒕젰
        // print graph
        /*
        string str = null;
        for(int i=0; i<vertexList.Count; i++) {
            str += vertexList[i][0] + " : ";
            for(int j=1; j<vertexList[i].Count; j++) {
                str += vertexList[i][j] + " ";
            }
            str += '\n';
        }
        Debug.Log(str);
        */

        /*
        for (int i=0; i<vertices.Count; i++)
        {
            Debug.Log(vertices[i]);
        }
        */

        // ?⑥씠??異쒕젰
        /*
        string str2 = null;
        for(int i=0; i<weightList.Count; i++) {
            str2 += weightList[i][0] + " : ";
            for(int j=1; j<weightList[i].Count; j++) {
                str2 += weightList[i][j] + " ";
            }
            str2 += '\n';
        }
        Debug.Log(str2);
        */
    }

    void FindStartEnd() {
        //Vector3 s = new Vector3(bf.vertices [13].xPos, 0, bf.vertices [13].yPos);
        //Vector3 t = new Vector3(bf.vertices [1].xPos, 0, bf.vertices [1].yPos);

        Vector3 s = GameObject.Find("Start").transform.position;
        Vector3 t = GameObject.Find("End").transform.position;

        s = bf.InvCrdntTransform(s);
        t = bf.InvCrdntTransform(t);

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

    void ComputeMap() {
        int gridSize = 60;

        bf = GetComponent<Brushfire>();
        graph = GetComponent<Graph>();
        floor = GameObject.Find("Floor");
        mapper = floor.GetComponent<Mapper>();
        mapper.ComputeTileSize (SpaceState.Editor, floor.GetComponent<Collider>().bounds.min, floor.GetComponent<Collider>().bounds.max, gridSize, gridSize);
        map = mapper.ComputeObstacles();
    }

    public void SetRandomSeed() {
        DateTime now = DateTime.Now;
        UnityEngine.Random.seed = now.Millisecond + now.Second + now.Minute + now.Hour + now.Day + now.Month+ now.Year;
        //UnityEngine.Random.seed = 2120;
        //UnityEngine.Random.seed = 2355;
        //UnityEngine.Random.seed = 2143;

        if (graph.scene == 4)
            UnityEngine.Random.seed = 2143;
        else if(graph.scene == 8)
            UnityEngine.Random.seed = 2920;

        Debug.Log(UnityEngine.Random.seed);
    }
    
    void OnDrawGizmoss () {
        Gizmos.color = Color.red;
        if (m_points != null) {
            for (int i = 0; i < m_points.Count; i++) {
                Gizmos.DrawSphere (new Vector3(m_points [i].x, 0f, m_points[i].y), 0.2f);
            }
        }
        
        if (m_edges != null) {
            Gizmos.color = Color.white;
            for (int i = 0; i< m_edges.Count; i++) {
                Vector2 left = (Vector2)m_edges [i].p0;
                Vector2 right = (Vector2)m_edges [i].p1;
                Gizmos.DrawLine ((Vector3)left, (Vector3)right);
            }
        }

        int layer = LayerMask.NameToLayer ("Obstacles");
        Gizmos.color = Color.magenta;
        if (m_delaunayTriangulation != null) {
            for (int i = 0; i< m_delaunayTriangulation.Count; i++) {
                Vector2 left = (Vector2)m_delaunayTriangulation [i].p0;
                Vector2 right = (Vector2)m_delaunayTriangulation [i].p1;
                //Gizmos.DrawLine ((Vector3)left, (Vector3)right);

                Vector3 l = new Vector3(left.x, 0f, left.y);
                Vector3 r = new Vector3(right.x, 0f, right.y);
                if(!Physics.Linecast(l, r, 1 << layer))
                    Gizmos.DrawLine(l, r);
            }
        }
        
        if (m_spanningTree != null) {
            Gizmos.color = Color.green;
            for (int i = 0; i< m_spanningTree.Count; i++) {
                LineSegment seg = m_spanningTree [i];               
                Vector2 left = (Vector2)seg.p0;
                Vector2 right = (Vector2)seg.p1;
                Gizmos.DrawLine ((Vector3)left, (Vector3)right);
            }
        }

        /*
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine (new Vector2 (0, 0), new Vector2 (0, m_mapHeight));
        Gizmos.DrawLine (new Vector2 (0, 0), new Vector2 (m_mapWidth, 0));
        Gizmos.DrawLine (new Vector2 (m_mapWidth, 0), new Vector2 (m_mapWidth, m_mapHeight));
        Gizmos.DrawLine (new Vector2 (0, m_mapHeight), new Vector2 (m_mapWidth, m_mapHeight));
        */

        //Gizmos.DrawSphere(m_points3 [startIndex], 1);
        //Gizmos.DrawSphere(m_points3 [endIndex], 1);
    }
}