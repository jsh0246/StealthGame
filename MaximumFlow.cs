using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MaximumFlow : MonoBehaviour {

    Graph graph;
    Brushfire bf;

    public List<int>[] capaList;
    public List<int>[] vertexList;
    public List<int>[] flow;

    [HideInInspector]
    public int startIndex, endIndex;
    private int INF = 99999;

    void EvaluateGuardPath() {
        string str = null;

        for(int i=0; i<1; i++) {
            //for (int i=0; i<graph.onePaths.Count; i++) 
            for(int j=0; j<1; j++) {
                for(int k=0; k<graph.onePaths[i][j].Count-1; k++) {
                    //int idx = vertexList[graph.onePaths[i][j][k]][vertexList[graph.onePaths[i][j][k]].IndexOf(graph.onePaths[i][j][k+1])];
                    str += flow[graph.onePaths[i][j][k]][vertexList[graph.onePaths[i][j][k]].IndexOf(graph.onePaths[i][j][k+1])] + " ";
                    
                    //str += flow[graph.onePaths[i][j][k]][idx].ToString() + " ";
                }
            }
        }

        Debug.Log(str);

        //Debug.Log(LCM(7, 12, 20));
    }

    void EvaluateGuardPath2() {
        string str = null;
        int[] grade = new int[graph.onePaths.Count];
        
        for(int i=0; i<1; i++) {
            for(int j=0; j<1; j++) {
                for(int k=0; k<graph.onePaths[i][j].Count-1; k++) {
                    str += graph.onePaths[i][j][k] + " " + graph.onePaths[i][j][k+1];
                    
                    str += " Adj : ";
                    for(int l=1; l<vertexList[graph.onePaths[i][j][k+1]].Count; l++) {
                        if(vertexList[graph.onePaths[i][j][k+1]][l] != graph.onePaths[i][j][k])
                            str += vertexList[graph.onePaths[i][j][k+1]][l] + " ";
                    }
                    
                    str += '\n';
                }
                
                str += '\n';
                for(int k=graph.onePaths[i][j].Count-1; k>0; k--) {
                    str += graph.onePaths[i][j][k] + " " + graph.onePaths[i][j][k-1];
                    
                    str += " Adj : ";
                    for(int l=1; l<vertexList[graph.onePaths[i][j][k-1]].Count; l++) {
                        if(vertexList[graph.onePaths[i][j][k-1]][l] != graph.onePaths[i][j][k]) {
                            str += vertexList[graph.onePaths[i][j][k-1]][l] + " ";
                        }
                    }
                    
                    str += '\n';
                }
            }
        }
        
        Debug.Log(str);
    }

    void Initialize() {
        bf = GetComponent<Brushfire>();
        graph = GetComponent<Graph>();

        capaList = new List<int>[graph.weight.Length+2];
        vertexList = new List<int>[graph.weight.Length+2];
        //flow = new List<List<int>>();
        flow = new List<int>[graph.weight.Length+2];

        MakeList();

        //FindStartEnd();
        MakeStartEnd();

        // flow를 위한 메모리 할당, vertexList과 같은 크기로 만들되 값은 0
        for(int i=0; i<vertexList.Length; i++) {
            flow[i] = new List<int>();
            //flow.Add(new List<int>());
            
            for(int j=0; j<vertexList[i].Count; j++) {
                flow[i].Add(0);
            }
        }
    }

    public void SetList(List<int>[] capaList) {
        //this.capaList = (List<int>[]) capaList.Clone();
        //this.capaList.Initialize();
        this.capaList = capaList;
        capaList[startIndex][1] = INF;
        capaList[endIndex][1] = INF;
        capaList[vertexList[startIndex][1]][vertexList[vertexList[startIndex][1]].IndexOf(startIndex)] = INF;
        capaList[vertexList[endIndex][1]][vertexList[vertexList[endIndex][1]].IndexOf(endIndex)] = INF;

        for(int i=0; i<flow.Length; i++) {
            for(int j=1; j<flow[i].Count; j++) {
                flow[i][j] = 0;
            }
        }
    }

    public int MaxFlow() {
        int pathCapacity;
        int maximumFlow = 0;
        int count = 0;

        while(true) {            
            pathCapacity = BFS();

            if(pathCapacity == INF)
                break;
            else
                maximumFlow += pathCapacity;

            count++;
        }

        //Debug.Log("LOOPS : " + count.ToString());

        return maximumFlow;
    }

    int BFS() {
        int pathCapacity = INF, node;

        Queue<int> q = new Queue<int>();
        bool[] visited = new bool[capaList.Length];
        int[] prev = new int[capaList.Length];
        bool endArrived = false;

        prev[startIndex] = -1;
        q.Enqueue(startIndex);
        visited[startIndex] = true;

        while(q.Count != 0) {
            int pop = q.Dequeue();

            for(int i=1; i<capaList[pop].Count; i++) {
                int t = vertexList[pop][i];

                if(!visited[t] && capaList[pop][i] > 0) {
                    q.Enqueue(t);
                    visited[t] = true;
                    prev[t] = pop;

                    if(t == endIndex) {
                        endArrived = true;
                        break;
                    }
                }

                if(endArrived)
                    break;
            }
        }

        if(!endArrived)
            return INF;

        node = endIndex;
        while(prev[node] != -1) {
            int t = prev[node];
            pathCapacity = Math.Min(pathCapacity, capaList[t][vertexList[t].IndexOf(node)]);
            node = t;
        }

        node = endIndex;
        while(prev[node] != -1) {
            int t = prev[node];
            flow[t][vertexList[t].IndexOf(node)] += pathCapacity;
            flow[node][vertexList[node].IndexOf(t)] += pathCapacity;
            node = t;
        }

        if(pathCapacity != INF) {
            node = endIndex;

            while(prev[node] != -1) {
                int t = prev[node];
                capaList[t][vertexList[t].IndexOf(node)] -= pathCapacity;
                capaList[node][vertexList[node].IndexOf(t)] += pathCapacity;
                node = t;
            }
        }

        return pathCapacity;
    }

    int PFS() {

        return 0;
    }

    void MakeList() {
        for(int i=0; i<graph.weight.Length; i++) {
            capaList[i] = new List<int>();
            vertexList[i] = graph.vertexList[i].ToList();

            // List<float> -> List<int>로 ToList() 할 수 없었음, 결국 각각 반올림 하였음
            for(int j=0; j<graph.weight[i].Length; j++) {
                capaList[i].Add((int)Math.Round(graph.weight[i][j]));
            }
        }
    }

    void MakeStartEnd() {
        Vector3 start = GameObject.Find("Start").transform.position;
        Vector3 end = GameObject.Find("End").transform.position;
        double min = 9999.0;
        int vertex = -1;

        // vertexList, capaList 각각 출발 인덱스와 도착 인덱스의 리스트 메모리 할당과 0번째 인덱스 요소 삽입
        for(int i=2; i>=1; i--) {
            vertexList[vertexList.Length - i] = new List<int>();
            vertexList[vertexList.Length - i].Add(vertexList.Length - i);

            capaList[capaList.Length - i] = new List<int>();
            capaList[capaList.Length - i].Add(0);
        }

        startIndex = vertexList.Length - 2;
        endIndex = vertexList.Length - 1;

        // OPTION 1
        // 각각의 정점과 비교하여 출발지점과 도착지점의 거리가 상수보다 가까우면 리스트에 삽입, 무방향 그래프이므로 반대방향도 삽입해 주어야 함
        /*
        for(int i=0; i<bf.vertices.Count; i++) {
            double t = Extra.Distance.L2Dist(bf.CrdntTransform(new Vector3(bf.vertices[i].xPos, -0.4f, bf.vertices[i].yPos)), start);
            if(t < 10.0) {
                vertexList[vertexList.Length - 2].Add(i);
                vertexList[i].Add(vertexList.Length - 2);

                //capaList[capaList.Length - 2].Add((int)Math.Round(t));
                //capaList[i].Add((int)Math.Round(t));

                capaList[capaList.Length - 2].Add(INF);
                capaList[i].Add(INF);
            }

            t = Extra.Distance.L2Dist(bf.CrdntTransform(new Vector3(bf.vertices[i].xPos, -0.4f, bf.vertices[i].yPos)), end); 
            if(t < 10.0) {
                vertexList[vertexList.Length - 1].Add(i);
                vertexList[i].Add(vertexList.Length - 1);

                //capaList[capaList.Length - 1].Add((int)Math.Round(t));
                //capaList[i].Add((int)Math.Round(t));

                capaList[capaList.Length - 1].Add(INF);
                capaList[i].Add(INF);
            }
        }
        */

        // OPTION 2 이게 더 적합한 듯
        // 출발지점과 도착지점과 가장 가까운 정점을 골라서 간선 추가
        for(int i=0; i<bf.vertices.Count; i++) {
            double t = Extra.Distance.L2Dist(bf.CrdntTransform(new Vector3(bf.vertices[i].xPos, -0.4f, bf.vertices[i].yPos)), start);
            if(t < min) {
                min = t;
                vertex = i;
            }
        }

        vertexList [vertexList.Length - 2].Add(vertex);
        vertexList [vertex].Add(vertexList.Length - 2);

        capaList [capaList.Length - 2].Add(INF);
        capaList [vertex].Add(INF);

        /*
        for(int i=1; i<capaList[vertex].Count; i++) {
            capaList[vertex][i] = 50;
        }
        */
        
        min = 9999.0;
        for(int i=0; i<bf.vertices.Count; i++) {
            double t = Extra.Distance.L2Dist(bf.CrdntTransform(new Vector3(bf.vertices[i].xPos, -0.4f, bf.vertices[i].yPos)), end);
            if(t < min) {
                min = t;
                vertex = i;
            }
        }

        vertexList [vertexList.Length - 1].Add(vertex);
        vertexList [vertex].Add(vertexList.Length - 1);
        
        capaList [capaList.Length - 1].Add(INF);
        capaList [vertex].Add(INF);

        /*
        for(int i=1; i<capaList[vertex].Count; i++) {
            capaList[vertex][i] = 50;
        }
        */
    }

    // Find nearest vertex from start and end point (Linear search)
    void FindStartEnd() {
        Vector3 start = GameObject.Find("Start").transform.position;
        Vector3 end = GameObject.Find("End").transform.position;
        double min = 9999.0;

        for(int i=0; i<bf.vertices.Count; i++) {
            double t = Extra.Distance.L2Dist(bf.CrdntTransform(new Vector3(bf.vertices[i].xPos, -0.4f, bf.vertices[i].yPos)), start);
            if(t < min) {
                min = t;
                startIndex = i;
            }
        }
        
        min = 9999.0;
        for(int i=0; i<bf.vertices.Count; i++) {
            double t = Extra.Distance.L2Dist(bf.CrdntTransform(new Vector3(bf.vertices[i].xPos, -0.4f, bf.vertices[i].yPos)), end);
            if(t < min) {
                min = t;
                endIndex = i;
            }
        }
    }

    public void Print() {
        string str = null;

        str += "CAPACITY LIST\n";
        for(int i=0; i<capaList.Length; i++) {
            str += i + " : ";

            for(int j=0; j<capaList[i].Count; j++) {
                str += capaList[i][j].ToString() + " ";
            }
            str += '\n';
        }

        str += "\nADJ LIST\n";
        for (int i=0; i<vertexList.Length; i++) {
            str += i + " : ";

            for(int j=0; j<vertexList[i].Count; j++) {
                str += vertexList[i][j].ToString() + " ";
            }
            str += '\n';
        }

        str += "\nFLOW LIST\n";
        //for(int i=0; i<flow.Count; i++) {
        for(int i=0; i<flow.Length; i++) {
            str += i + " : ";

            for(int j=0; j<flow[i].Count; j++) {
                str += flow[i][j].ToString() + " ";
            }
            str += '\n';
        }
        
        Debug.Log(str);
    }

	void Start () {
        Initialize();

        //Debug.Log(startIndex);
        //Debug.Log(endIndex);

        //Print();
        //Debug.Log(MaxFlow());
	}
}
