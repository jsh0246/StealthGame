using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
using System;
using Random = UnityEngine.Random;

public class GraphGeneratorB : MonoBehaviour {

    private Graph g;
    private Brushfire bf;
    private MaximumFlow mf;

    private Cell[][] map;
    public List<Cell> vertices = new List<Cell>();
    public List<List<int>> vertexList;
    public List<List<float>> weightList;
    private int startIndex, endIndex;

    private GameObject floor = null;
    private Mapper mapper;

    void AddRandomVertices() {
        float maxDist = 5f;

        // Add random vertices
        int nV = 60;
        for(int i=0; i<nV; i++) {
            int x = Random.Range(0, 60);
            int y = Random.Range(0, 60);

            if(!map[x][y].blocked) {
                Cell c = new Cell(x, y);
                vertices.Add(c);

                vertexList.Add(new List<int>());
                vertexList[vertexList.Count-1].Add(vertexList.Count-1);

                weightList.Add(new List<float>());
            } else 
                i--;
        }

        // Add vertices like grid
        /*
        int nV = 0;
        for (int i=0; i<60; i+=5)
        {
            for(int j=0; j<60; j+=5) {
                int x = i;
                int y = j;

                if(!map[x][y].blocked) {
                    Cell c = new Cell(x, y);
                    vertices.Add(c);

                    vertexList.Add(new List<int>());
                    vertexList[vertexList.Count-1].Add(vertexList.Count-1);
                    
                    weightList.Add(new List<float>());
                    nV++;
                }
            }
        }
        */

        // Add edge for new vertices
        int layer = LayerMask.NameToLayer("Obstacles");
        for(int i=vertices.Count-1; i>=vertices.Count-1-nV; i--) {
            Vector3 s = bf.CrdntTransform(new Vector3(vertices[i].xPos, 0f, vertices[i].yPos));

            // Add dummy weight value for each weightList[i]
            weightList[i].Add(0f);

            for(int j=vertices.Count-1; j>=0; j--) {
                if(i == j)
                    continue;

                Vector3 t = bf.CrdntTransform(new Vector3(vertices[j].xPos, 0f, vertices[j].yPos));
                float dist = Vector3.Distance(s, t);

                if(!Physics.Linecast(s, t, 1<<layer) && dist < maxDist && vertexList[i].Count < 4) {
                    vertexList[i].Add(j);
                    vertexList[j].Add(i);

                    weightList[i].Add(dist);
                    weightList[j].Add(dist);
                }
            }
        }
    }

    void Initialize() {
        g = GetComponent<Graph>();
        bf = GetComponent<Brushfire>();
        mf = GetComponent<MaximumFlow>();

        vertexList = new List<List<int>>();
        weightList = new List<List<float>>();

        CopyData();
        ComputeMap();
    }

    void CopyData() {
        for(int i=0; i<g.vertexList.Length; i++)
            vertexList.Add(g.vertexList[i].ToList());

        for(int i=0; i<bf.vertices.Count; i++)
            vertices.Add(bf.vertices[i].Copy());

        for(int i=0; i<g.weight.Length; i++)
            weightList.Add(g.weight[i].ToList());
    }
    
    void ComputeMap() {
        int gridSize = 60;
        
        floor = GameObject.Find("Floor");
        mapper = floor.GetComponent<Mapper>();
        mapper.ComputeTileSize (SpaceState.Editor, floor.GetComponent<Collider>().bounds.min, floor.GetComponent<Collider>().bounds.max, gridSize, gridSize);
        map = mapper.ComputeObstacles();
    }

    public void SetRandomSeed() {
        DateTime now = DateTime.Now;
        UnityEngine.Random.seed = now.Millisecond + now.Second + now.Minute + now.Hour + now.Day + now.Month+ now.Year;
    }
    
    void PrintVertexList() {
        string str = null;

        for(int i=0; i<vertexList.Count; i++) {
            str += vertexList[i][0] + " : ";

            for(int j=1; j<vertexList[i].Count; j++) {
                str += vertexList[i][j] + " ";
            }

            str += '\n';
        }

        Debug.Log(str);
    }

    void PrintWeight() {
        string str = null;
        
        for(int i=0; i<weightList.Count; i++) {
            str += i + " : ";
            
            for(int j=0; j<weightList[i].Count; j++) {
                str += weightList[i][j] + " ";
            }
            
            str += '\n';
        }
        
        Debug.Log(str);
    }

    void DrawGraph() {
        for(int i=0; i<vertexList.Count; i++) {
            Vector3 start = bf.CrdntTransform(new Vector3(vertices[vertexList[i][0]].xPos, 0, vertices[vertexList[i][0]].yPos));
            
            for(int j=1; j<vertexList[i].Count; j++) {
                Vector3 end = bf.CrdntTransform(new Vector3(vertices[vertexList[i][j]].xPos, 0, vertices[vertexList[i][j]].yPos));
                
                Debug.DrawLine(start, end, Color.blue);
            }
        }
    }

	void Start () {
        Initialize();

        //SetRandomSeed();
        AddRandomVertices();
        PrintVertexList();
        PrintWeight();
	}

    void Update() {
        DrawGraph();
    }
}
