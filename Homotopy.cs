using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class Homotopy : MonoBehaviour {

    private Brushfire bf;
    private MaximumFlow mf;
    public List<string> homotopy = new List<string>();
    public List<int>[] weight;
    public List<int>[] weight2;

    // DFS related variables
    // DFS시 d, f 배열은 사용안하면 지우자
    int[] color, pred, d, f;

    int time = 0;
    int startIndex, endIndex;

    void MakeHomotopyLayer() {
        //Line line = new Line(new Vector3(0f, 0f, 0f), new Vector3(5f, 0f, 5f));

        LineRenderer[] lr = new LineRenderer[homotopy.Count];

        //lr[0] = gameObject.AddComponent<LineRenderer>();
        //lr[0].gameObject.layer = LayerMask.NameToLayer("Obstacles");

        for(int i=0; i<homotopy.Count; i++) {
            string[] parsed = homotopy[i].Split(' ');

            GameObject go = new GameObject("Line Renderer " + i);
            lr[i] = (LineRenderer)go.AddComponent(typeof(LineRenderer));
            //lr[i].material = new Material(Shader.Find("FOV"));
            lr[i].SetColors(Color.white, Color.red);
            lr[i].SetWidth(1f, 1f);
            lr[i].SetVertexCount(parsed.Length-3);
            lr[i].gameObject.layer = LayerMask.NameToLayer("Homotopy");

            for(int j=1; j<parsed.Length-2; j++) {
                int f = Convert.ToInt32(parsed[j]);

                lr[i].SetPosition(j-1, bf.CrdntTransform(new Vector3(bf.vertices[f].xPos, 2f, bf.vertices[f].yPos)));
            }
        }




        // Linecast or another casting(Spherecast, Checksphere)




        //((GameObject)line).layer = LayerMask.NameToLayer("Homotopy");




        /*
        Camera camera = GameObject.FindWithTag("MainCamera").camera;
        GUIText[] text = new GUIText[vNum];

        for(int i=0; i<vNum; i++) {
            GameObject go = new GameObject("GUIText " + i);
            text[i] = (GUIText)go.AddComponent(typeof(GUIText));
            text[i].transform.position = camera.WorldToViewportPoint(bf.CrdntTransform(new Vector3(bf.vertices[i].xPos, 0.5f, bf.vertices[i].yPos)));
            text[i].text = i.ToString();
            //text[i].text = depth[i].ToString();
            //text[i].text = pred[i].ToString();
            text[i].color = Color.magenta;
            text[i].fontSize = 15;
        }
        */
    }

    void OperateOnWeight() {
        for(int i=0; i<weight.Length; i++) {
            for(int j=1; j<weight[i].Count; j++) {
                weight[i][j] = (int)Math.Pow(1.7, weight[i][j]);
            }
        }
    }

    void ComputeEdgeWeight() {
        for(int i=0; i<homotopy.Count; i++) {
            string[] parsed = homotopy[i].Split(' ');

            // 무방향
            for(int j=0; j<parsed.Length-2; j++) {
                int revFirstIdx, revSecondParsed;
                int secondParsed = Convert.ToInt32(parsed[j+1]);

                int firstIdx = Convert.ToInt32(parsed[j]);
                int secondIdx = mf.vertexList[firstIdx].IndexOf(secondParsed);

                // if, else 로 나눌 필요없는듯, 두 분기문 중 아무거나 하나만 하면 됨, 일단 그냥 둠
                if(firstIdx < secondParsed) {
                    revFirstIdx = firstIdx;
                    revSecondParsed = secondParsed;

                    secondIdx = mf.vertexList[secondParsed].IndexOf(firstIdx);
                    firstIdx = secondParsed;
                } else {
                    revFirstIdx = secondParsed;
                    revSecondParsed = firstIdx;
                }

                weight[firstIdx][secondIdx]++;
                weight[revFirstIdx][mf.vertexList[revFirstIdx].IndexOf(revSecondParsed)]++;

                weight2[firstIdx][secondIdx]++;
                weight2[revFirstIdx][mf.vertexList[revFirstIdx].IndexOf(revSecondParsed)]++;
            }

            // 방향
            /*
            for(int j=0; j<parsed.Length-1; j++) {
                int firstIdx = Convert.ToInt32(parsed[j]);
                int secondIdx = mf.vertexList[firstIdx].IndexOf(Convert.ToInt32(parsed[j+1]));
                
                weight[firstIdx][secondIdx]++;
            }
            */
        }

        //OperateOnWeight();
        //AddSpaceToHomotopy();
        //PostProcess();
    }

    void PostProcess() {
        for(int i=0; i<weight.Length; i++) {
            for(int j=1; j<weight[i].Count; j++) {
                if(weight[i][j] == 0)
                    weight[i][j] = 10;
            }
        }
    }

    void AddSpaceToHomotopy() {
        for (int i=0; i<homotopy.Count; i++)
        {
            homotopy[i] += " ";
        }
    }

    void GenerateHomotopyClass() {
        for(int i=0; i<1000; i++) {
            DFS();

            InitDFS();
        }

        homotopy.Sort();

        string str = null;
        for(int i=0; i<homotopy.Count; i++) {
            //str += (i+1) + " : " + homotopy[i] + '\n';
            str += (i+1) + " : " + homotopy[i] + '\n';
        }

        Debug.Log(str);
    }

    void InitDFS() {
        for(int i=0; i<color.Length; i++) {
            color[i] = 0;
            pred[i] = -1;
            d[i] = 0;
            f[i] = 0;
        }
    }

    void DFS() {
        startIndex = mf.startIndex;
        endIndex = mf.endIndex;

        DFSVisit(startIndex);
    }

    void DFSVisit(int u) {
        List<int> rndList = new List<int>();

        color[u] = 1;
        d[u] = ++time;

        if(u == endIndex) {
            TraversePath(u);
        }

        /*
        if(u == 5 || u == 2 || u == 1 || u == 14 || u == 10 || u == 9 || u == 0) {
            TraversePath(u);
            return ;
        }
        */

        for(int i=1; i<mf.vertexList[u].Count; i++) {
            int rnd = Random.Range(1, mf.vertexList[u].Count);

            if(!rndList.Contains(rnd)) {
                rndList.Add(rnd);

                if(color[mf.vertexList[u][rnd]] == 0) {
                    pred[mf.vertexList[u][rnd]] = u;
                    DFSVisit(mf.vertexList[u][rnd]);
                }
            } else {
                i--;
                continue;
            }

            color[u] = 2;
            f[u] = ++time;
        }
    }

    void TraversePath(int u) {
        string str = null;
        int length = 0;

        str = u.ToString();
        while(u != startIndex) {
            u = pred[u];
            str = u + " " + str;
            length++;
        }

        str += ' ';

        // 12
        if(!homotopy.Contains(str) && length < 12)
            homotopy.Add(str);
    }

    void Initialize() {
        bf = GetComponent<Brushfire>();
        mf = GetComponent<MaximumFlow>();
        weight = new List<int>[mf.vertexList.Length];
        weight2 = new List<int>[mf.vertexList.Length];

        color = new int[mf.vertexList.Length];
        pred = new int[mf.vertexList.Length];
        d = new int[mf.vertexList.Length];
        f = new int[mf.vertexList.Length];

        // weight array initialization
        for(int i=0; i<mf.vertexList.Length; i++) {
            weight[i] = new List<int>();
            weight[i].Add(i);

            weight2[i] = new List<int>();
            weight2[i].Add(i);

            for(int j=1; j<mf.vertexList[i].Count; j++) {
                weight[i].Add(0);

                weight2[i].Add(0);
            }
        }

        // dfs related array initialization
        for(int i=0; i<pred.Length; i++) {
            pred[i] = -1;
            color[i] = 0;
        }
    }

    void Print() {
        string str = null;

        for(int i=0; i<pred.Length; i++) {
            str += i + " : ";
            str += pred[i] + "\n";
        }

        Debug.Log(str);
    }

    public void PrintWeight() {
        string str = null;

        for(int i=0; i<weight.Length; i++) {
            for(int j=0; j<weight[i].Count; j++) {
                str += weight[i][j] + " ";
            }
            str += '\n';
        }

        str += '\n';
        for(int i=0; i<mf.vertexList.Length; i++) {
            for(int j=0; j<mf.vertexList[i].Count; j++) {
                str += mf.vertexList[i][j] + " ";
            }
            str += '\n';
        }

        Debug.Log(str);
    }

    public void PrintEdgeWeight() {
        Brushfire bf = GetComponent<Brushfire>();
        Camera camera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        int count = 0;
        
        for(int i=0; i<weight.Length; i++) {
            for(int j=1; j<weight[i].Count; j++) {
                count++;
            }
        }
        
        GUIText[] text = new GUIText[count];
        count = 1;
        
        for(int i=0; i<weight.Length; i++) {
            if(mf.vertexList[i][0] == endIndex || mf.vertexList[i][0] == startIndex)
                continue;

            Vector3 l = camera.WorldToViewportPoint(bf.CrdntTransform(new Vector3(bf.vertices[mf.vertexList[i][0]].xPos, 0.5f, bf.vertices[mf.vertexList[i][0]].yPos)));
            
            for(int j=1; j<weight[i].Count; j++) {
                if(mf.vertexList[i][j] == endIndex || mf.vertexList[i][j] == startIndex)
                    continue;

                if(mf.vertexList[i][0] > mf.vertexList[i][j]) {
                    Vector3 r = camera.WorldToViewportPoint(bf.CrdntTransform(new Vector3(bf.vertices[mf.vertexList[i][j]].xPos, 0.5f, bf.vertices[mf.vertexList[i][j]].yPos)));
                    Vector3 c = (l + r) / 2;
                    
                    GameObject go = new GameObject("GUIText " + count++);
                    text[i] = (GUIText)go.AddComponent(typeof(GUIText));
                    text[i].transform.position = c;
                    //text[i].text = mf.flow[i][j].ToString();
                    //text[i].text = mf.capaList[i][j].ToString();
                    text[i].text = weight[i][j].ToString();
                    text[i].color = Color.blue;
                    text[i].fontSize = 15;
                }
            }
        }
    }

	void Start () {
        Initialize();

        GenerateHomotopyClass();
        ComputeEdgeWeight();

        PrintEdgeWeight();

        MakeHomotopyLayer();

        PrintWeight();
    }
}
