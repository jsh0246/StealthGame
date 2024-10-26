using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using Extra;
using Objects;

public class Evaluator : MonoBehaviour {

    Brushfire bf;
    Graph graph;
    MaximumFlow mf;
    Homotopy ht;

    List<int>[] capaList;

    void ComputePathFarness(List<List<int>[]> guardPaths) {
        string str = null;

        //for (int i=0; i<guardPaths.Count; i++) {
        for (int i=0; i<2; i++)
        {
            Vector3[] center = new Vector3[guardPaths[i].Length];
            float dist;

            for (int j=0; j<guardPaths[i].Length; j++)
            {
                for (int k=0; k<guardPaths[i][j].Count; k++)
                {
                    center[j] += bf.CrdntTransform(new Vector3(bf.vertices[guardPaths[i][j][k]].xPos, 0f, bf.vertices[guardPaths[i][j][k]].yPos));
                }

                center[j] /= guardPaths[i][j].Count;
            }

            /*
            for (int j=0; j<2; j++)
                Debug.Log(center [j]);
            */

            str += dist = (float)Extra.Distance.L2Dist(center[0], center[1]);
            str += '\n';
        }

        Debug.Log(str);
    }

    public void TraceGuard(List<List<int>[]> guardPaths) {
        int timestamps = 1000;
        float stepSize = 10f;
        int layer = LayerMask.NameToLayer ("Homotopy");
        string str = null;

        Extra.Timer.Start();

        //graph.TestPath();
        graph.RandomTwoGuardPath();
        StorePositions();

        GameObject[] en = GameObject.FindGameObjectsWithTag ("Enemy") as GameObject[];
        Enemy[] enemies = new Enemy[en.Length];
        for (int i = 0; i < en.Length; i++) {
            enemies [i] = en [i].GetComponent<Enemy> ();
            enemies [i].positions = new Vector3[timestamps];
            enemies [i].forwards = new Vector3[timestamps];
            enemies [i].rotations = new Quaternion[timestamps];
            enemies [i].cells = new Vector2[timestamps][];
        }
        
        // Foreach period time, we advance a stepsize into the future and compute the map for it
        for (int counter = 0; counter < timestamps; counter++) {
            // Simulate and store the values for future use
            foreach (Enemy e in enemies) {
                e.Simulate (stepSize);
                e.positions [counter] = e.GetSimulationPosition ();
                e.forwards [counter] = e.GetSimulatedForward ();
                e.rotations [counter] = e.GetSimulatedRotation ();

                if(Physics.CheckSphere(e.positions[counter], 5f, 1 << layer))
                    Debug.Log("yy");

                //Debug.DrawLine(new Vector3(0f, 0f, 0f), e.positions[counter], Color.blue);
                //str += e.positions[counter] + "\n";
            }
        }



        //Debug.Log(str);

        /*
        string str = null;
        for(int i=0; i<en.Length; i++) {
            for(int j=0; j<1000; j=j+10) {
                str += enemies[i].positions[j];
                str += '\n';
            }
            str += "\n\n";
        }

        Debug.Log(str);
        */
        Debug.Log(Extra.Timer.End());

        //ResetAI();



        /*
        Enemy[] enemies = new Enemy[guardPaths[0].Length];
        
        for(int i=0; i<guardPaths[0].Length; i++) {
            enemies[i].moveSpeed = 1f;
            enemies[i].rotationSpeed = 30;
            enemies[i].fovDistance = 6;
        }
        
        for(int i=0; i<guardPaths[0].Length; i++) {

        }
        */

        
        /*
        for (int counter = 0; counter < timestamps; counter++) {
            // Simulate and store the values for future use
            foreach (Enemy e in enemies) {
                e.Simulate (stepSize);
                e.positions [counter] = e.GetSimulationPosition ();
                e.forwards [counter] = e.GetSimulatedForward ();
                e.rotations [counter] = e.GetSimulatedRotation ();
            }
        }
        */
    }

    private void StorePositions () {
        GameObject[] objs = GameObject.FindGameObjectsWithTag ("Enemy") as GameObject[];
        for (int i = 0; i < objs.Length; i++) {
            objs [i].GetComponent<Enemy> ().SetInitialPosition ();
        }
        objs = GameObject.FindGameObjectsWithTag ("AI") as GameObject[];
        for (int i = 0; i < objs.Length; i++) {
            objs [i].GetComponent<Player> ().SetInitialPosition ();
        }
    }
    
    private void ResetAI () {
        GameObject[] objs = GameObject.FindGameObjectsWithTag ("AI") as GameObject[];
        foreach (GameObject ob in objs)
            ob.GetComponent<Player> ().ResetSimulation ();
        
        objs = GameObject.FindGameObjectsWithTag ("Enemy") as GameObject[];
        foreach (GameObject ob in objs) 
            ob.GetComponent<Enemy> ().ResetSimulation ();
        
        //timeSlice = 0;
    }

    void EvaluateByMaximumFlowWithHomotopy(List<List<int>[]> guardPaths) {
        string str = null;

        mf.SetList(MyClone(ht.weight));
        Debug.Log("Homotopy Maximumflow : " + mf.MaxFlow());

        // 뭔가 경로의 길이에 비례하지 않게 처리가 필요 할 듯
        // Guard 2
        /*
        for(int i=0; i<2; i++) {
            weight = MyClone(ht.weight);
            
            for(int j=0; j<graph.twoPaths[i].Length; j++) {
                for(int k=0; k<graph.twoPaths[i][j].Count-1; k++) {
                    //ht.weight[graph.twoPaths[i][j][k]][mf.vertexList[graph.twoPaths[i][j][k]].IndexOf(graph.twoPaths[i][j][k+1])] /= 2;
                    
                    weight[graph.twoPaths[i][j][k]][mf.vertexList[graph.twoPaths[i][j][k]].IndexOf(graph.twoPaths[i][j][k+1])] /= 2;
                    //;
                }
            }
            
            mf.SetList(weight);
            str += i + " : " + mf.MaxFlow() + '\n';
        }

        Debug.Log(str);
        */

        // Arbiturary Guard Number
        string str2 = null;
        for(int i=0; i<0; i++) {
            capaList = MyClone(ht.weight);
            
            for(int j=0; j<guardPaths[i].Length; j++) {
                for(int k=0; k<guardPaths[i][j].Count-1; k++) {
                    capaList[guardPaths[i][j][k]][mf.vertexList[guardPaths[i][j][k]].IndexOf(guardPaths[i][j][k+1])] = 0;
                }
            }

            // MaximumFlow Class는 길이 weight를 썼는데 새로 homotopy class number의 wegiht를 새로 준다
            mf.SetList(capaList);
            str2 += i + " : " + mf.MaxFlow() + '\n';
        }

        Debug.Log(str2);
        //ht.PrintEdgeWeight();
    }

    List<int>[] MyClone(List<int>[] arr) {
        List<int>[] ret = new List<int>[arr.Length];

        for(int i=0; i<arr.Length; i++) {
            ret[i] = new List<int>();

            for(int j=0; j<arr[i].Count; j++) {
                ret[i].Add(arr[i][j]);
            }
        }

        return ret;
    }

    // 막고 있는 호모토피 갯수를 세어보자
    // 막고 있는 호모토피에도 가중치를??
    void EvaluateByHomotopyNubmer() {
        string str = null;

        //for (int i=0; i<graph.twoPaths.Count; i++) {
        for (int i=0; i<30; i++) {
            bool[] blocked = new bool[ht.homotopy.Count];
            int score = 0;

            for(int j=0; j<graph.twoPaths[i].Length; j++) {
                for(int k=0; k<graph.twoPaths[i][j].Count-1; k++) {
                    string piece1 = " " + graph.twoPaths[i][j][k] + " " + graph.twoPaths[i][j][k+1] + " ";
                    string piece2 = " " + graph.twoPaths[i][j][k+1] + " " + graph.twoPaths[i][j][k] + " ";

                    for(int l=0; l<ht.homotopy.Count; l++) {
                        if(ht.homotopy[l].Contains(piece1) || ht.homotopy[l].Contains(piece2)) {
                            blocked[l] = true;

                            //Debug.Log("1 : " + piece1);
                            //Debug.Log("2 : " + piece2);
                        }
                    }
                }
            }

            for(int j=0; j<blocked.Length; j++) {
                if(blocked[j] == true)
                    score++;
            }

            str += score + "\n";
        }

        Debug.Log(str);
        /*
        string str = null;
        for (int i=0; i<blocked.Length; i++)
            str += blocked [i] + "\n";
        Debug.Log(str);
        */
    }
    
    void EvaluatePathByHomotopy() {
        StreamWriter sw = new StreamWriter("Value.txt");
        string str = null;
        string fileOut = null;
        string selectedGrade = null;
        int count = 0;
        
        // 조합을 이루어야 할 것 같지만 일단 그냥 하겠음
        // 이것은 몇개의 길목을 막고 있는가를 제대로 모델링한게 맞는가? 생각해봐야 함

        // 경로 평균 2 Guard
        Extra.Timer.Start();
        //for(int i=3; i<4; i++) {
        for(int i=0; i<graph.twoPaths.Count; i++) {
            float score = 0f;
            
            for(int j=0; j<graph.twoPaths[i].Length; j++) {
                /*
                string piece;
                
                for(int k=0; k<graph.twoPaths[i][j].Count-1; k++) {
                    piece = " " + graph.twoPaths[i][j][k] + " " + graph.twoPaths[i][j][k+1] + " ";
                    
                    for(int l=0; l<ht.homotopy.Count; l++) {
                        if(ht.homotopy[l].Contains(piece))
                            score++;
                    }
                    
                    //Debug.Log(piece);
                }
                
                for(int k=graph.twoPaths[i][j].Count-1; k>0; k--) {
                    piece = " " + graph.twoPaths[i][j][k] + " " + graph.twoPaths[i][j][k-1] + " ";
                    
                    for(int l=0; l<ht.homotopy.Count; l++) {
                        if(ht.homotopy[l].Contains(piece))
                            score++;
                    }
                    
                    //Debug.Log(piece);
                }
                */

                for(int k=0; k<graph.twoPaths[i][j].Count-1; k++) {
                    int secondIdx = mf.vertexList[graph.twoPaths[i][j][k]].IndexOf(graph.twoPaths[i][j][k+1]);

                    score += ht.weight[graph.twoPaths[i][j][k]][secondIdx];
                }
            }

            score /= graph.twoPaths[i][0].Count + graph.twoPaths[i][1].Count - 2;
            //score /= (graph.twoPaths[i][0].Count-1) * 2 + (graph.twoPaths[i][1].Count-1) * 2;
            //str += (i+1) + " : " + score + '\n';
            fileOut += score + "\r\n";

            if(score == 5) {
                graph.testPaths.Add(new List<int>[2]);

                for(int j=0; j<graph.twoPaths[i].Length; j++) {
                    for(int k=0; k<graph.twoPaths[i][j].Count; k++) {
                        graph.testPaths[count] = graph.twoPaths[i];
                    }
                }

                count++;
                selectedGrade += score;
                selectedGrade += '\n';
            }

            //Debug.Log(score);
        }
        
        //Debug.Log(str);
        sw.Write(fileOut);
        sw.Flush();
        sw.Close();
        Debug.Log(Extra.Timer.End());
        Debug.Log("COUNT : " + count);
        Debug.Log(graph.testPaths.Count);
        Debug.Log(selectedGrade);
        PrintPath(graph.testPaths);

        // 경로 평균 3 Guard
        /*
        Extra.Timer.Start();
        for(int i=0; i<2000; i++) {
            float score = 0f;
            
            for(int j=0; j<graph.threePaths[i].Length; j++) {
                string piece;
                
                for(int k=0; k<graph.threePaths[i][j].Count-1; k++) {
                    piece = graph.threePaths[i][j][k] + " " + graph.threePaths[i][j][k+1];
                    
                    for(int l=0; l<ht.homotopy.Count; l++) {
                        if(ht.homotopy[l].Contains(piece))
                            score++;
                    }
                    
                    //Debug.Log(piece);
                }
                
                for(int k=graph.threePaths[i][j].Count-1; k>0; k--) {
                    piece = graph.threePaths[i][j][k] + " " + graph.threePaths[i][j][k-1];
                    
                    for(int l=0; l<ht.homotopy.Count; l++) {
                        if(ht.homotopy[l].Contains(piece))
                            score++;
                    }
                    
                    //Debug.Log(piece);
                }
            }
            
            //score /= graph.threePaths[i][0].Count + graph.threePaths[i][1].Count + graph.threePaths[i][2].Count - 3;
            score /= (graph.threePaths[i][0].Count-1)*2 + (graph.threePaths[i][1].Count-1)*2 + (graph.threePaths[i][2].Count-1)*2;
            //str += (i+1) + " : " + score + '\n';
            fileOut += score + "\r\n";
            //Debug.Log(score);
        }
        
        //Debug.Log(str);
        sw.Write(fileOut);
        sw.Flush();
        sw.Close();
        Debug.Log(Extra.Timer.End());
        */

        // 경로 평균 Test Guard
        /*
        Extra.Timer.Start();
        for(int i=0; i<2; i++) {
            float score = 0f;
            
            for(int j=0; j<graph.testPaths[i].Length; j++) {
                string piece;
                
                for(int k=0; k<graph.testPaths[i][j].Count-1; k++) {
                    piece = graph.testPaths[i][j][k] + " " + graph.testPaths[i][j][k+1];
                    
                    for(int l=0; l<ht.homotopy.Count; l++) {
                        if(ht.homotopy[l].Contains(piece))
                            score++;
                    }
                    
                    //Debug.Log(piece);
                }
                
                for(int k=graph.testPaths[i][j].Count-1; k>0; k--) {
                    piece = graph.testPaths[i][j][k] + " " + graph.testPaths[i][j][k-1];
                    
                    for(int l=0; l<ht.homotopy.Count; l++) {
                        if(ht.homotopy[l].Contains(piece))
                            score++;
                    }
                    
                    //Debug.Log(piece);
                }
            }
            
            //score /= graph.testPaths[i][0].Count + graph.testPaths[i][1].Count - 2;
            score /= (graph.testPaths[i][0].Count-1) * 2 + (graph.testPaths[i][1].Count-1) * 2;
            //str += (i+1) + " : " + score + '\n';
            fileOut += score + "\r\n";
            //Debug.Log(score);
        }
        
        //Debug.Log(str);
        sw.Write(fileOut);
        sw.Flush();
        sw.Close();
        Debug.Log(Extra.Timer.End());
        */

        // 경로 조합
        /*
        Extra.Timer.Start();
        for(int i=0; i<2703; i++) {
            float score = 0f;
            int count = 0;

            for(int j=0; j<graph.twoPaths[i][0].Count-1; j++) {
                string piece;
                
                piece = graph.twoPaths[i][0][j] + " " + graph.twoPaths[i][0][j+1];

                for(int l=0; l<ht.homotopy.Count; l++) {
                    if(ht.homotopy[l].Contains(piece)) {
                        score++;
                    }
                }

                for(int k=0; k<graph.twoPaths[i][1].Count-1; k++) {
                    string piece2;

                    piece2 = graph.twoPaths[i][1][k] + " " + graph.twoPaths[i][1][k+1];

                    for(int l=0; l<ht.homotopy.Count; l++) {
                        if(ht.homotopy[l].Contains(piece2)) {
                            score++;
                        }
                    }

                    count++;
                }
            }

            //Debug.Log("COUNT : " + count);
            score /= count;
            //Debug.Log(score);
            fileOut += score + "\r\n";
        }

        sw.Write(fileOut);
        sw.Flush();
        sw.Close();
        Debug.Log(Extra.Timer.End());
        */
        
        /*
        for(int i=0; i<1; i++) {
            for(int j=0; j<graph.onePaths[i].Length; j++) {
                float score = 0f;

                // Cycle 고려해야함
                // Cycle 아닌 것도 cycle을 이루어야 함
                for(int k=0; k<graph.onePaths[i][j].Count-1; k++) {
                    string piece = graph.onePaths[i][j][k] + " " + graph.onePaths[i][j][k+1];

                    for(int l=0; l<homotopy.Count; l++) {
                        if(homotopy[l].Contains(piece))
                            score++;
                    }

                    Debug.Log(score);
                    Debug.Log(piece);
                }

                score /= graph.onePaths[i][j].Count - 1;
                Debug.Log("Score : " + score);
            }
        }
        */
    }

    void PathCombinationFor2() {
        string str = null;

        for(int i=0; i<1; i++) {
            for(int j=0; j<graph.twoPaths[i][0].Count-1; j++) {
                for(int k=0; k<graph.twoPaths[i][1].Count-1; k++) {
                    str += graph.twoPaths[i][0][j] + " " + graph.twoPaths[i][0][j+1] + " ";
                    str += graph.twoPaths[i][1][k] + " " + graph.twoPaths[i][1][k+1] + "\n";
                }
            }
        }

        Debug.Log(str);
    }

    void PathCombinationFor3(List<List<int>[]> guardPaths) {
        string str = null;

        /*
        for(int i=0; i<1; i++) {
            for(int j=0; j<guardPaths[i].Length; j++) {
                for(int k=0; k<guardPaths[i][j].Count-1; k++) {
                    str += guardPaths[i][j][k] + " " + guardPaths[i][j][k+1] + "\n";

                    for(int l=0; l<guardPaths[i][j+1].Count-1; l++) {

                        for(int m=0; 
                    }
                }
            }
        }
        */

        /*
        for(int i=0; i<1; i++) {
            for(int j=0; j<guardPaths[i][0].Count-1; j++) {
                //str += guardPaths[i][0][j] + " " + guardPaths[i][0][j+1] + " ";

                for(int k=0; k<guardPaths[i][1].Count-1; k++) {
                    //str += guardPaths[i][0][j] + " " + guardPaths[i][0][j+1] + " ";
                    //str += guardPaths[i][1][k] + " " + guardPaths[i][1][k+1] + " ";

                    for(int l=0; l<guardPaths[i][2].Count-1; l++) {
                        str += guardPaths[i][0][j] + " " + guardPaths[i][0][j+1] + " ";
                        str += guardPaths[i][1][k] + " " + guardPaths[i][1][k+1] + " ";
                        str += guardPaths[i][2][l] + " " + guardPaths[i][2][l+1] + "\n";
                    }
                }
            }
        }
        */

        // 뭔가 4명 이상의 경비에 대해서 깔끔하게 for문을 돌리는 방법이 있을 거 같은데
        for(int i=0; i<1; i++) {
            for(int j=0; j<guardPaths[i][0].Count-1; j++) {
                for(int k=0; k<guardPaths[i][1].Count-1; k++) {
                    for(int l=0; l<guardPaths[i][2].Count-1; l++) {
                        str += guardPaths[i][0][j] + " " + guardPaths[i][0][j+1] + " ";
                        str += guardPaths[i][1][k] + " " + guardPaths[i][1][k+1] + " ";
                        str += guardPaths[i][2][l] + " " + guardPaths[i][2][l+1] + "\n";
                    }
                }
            }
        }
        
        Debug.Log(str);
    }

    void PrintPath(List<List<int>[]> guardPaths) {
        StreamWriter sw = new StreamWriter("TestPaths.txt");
        string str = null;

        for(int i=0; i<guardPaths.Count; i++) {
            //str += (i+1) + " : ";
            for(int j=0; j<guardPaths[i].Length; j++) {
                for(int k=0; k<guardPaths[i][j].Count; k++) {
                    str += guardPaths[i][j][k] + " ";
                }
                str += " \t";
            }
            str += "\r\n";
        }

        sw.Write(str);
        sw.Flush();
        sw.Close();
        Debug.Log(str);
    }

    void Initialize() {
        bf = GetComponent<Brushfire>();
        graph = GetComponent<Graph>();
        mf = GetComponent<MaximumFlow>();
        ht = GetComponent<Homotopy>();
    }

	void Start () {
        Initialize();

        //ComputePathFarness(graph.testPaths);
        //TraceGuard(graph.rTwoPaths);

        //Test();

        //EvaluateByHomotopyNubmer();
        //EvaluatePathByHomotopy();
        //EvaluateByMaximumFlowWithHomotopy(graph.testPaths);

        //PathCombinationFor2();
        //PathCombinationFor3(graph.threePaths);
	}

    void Test() {
        Vector3 s = new Vector3(8, 0, 2);
        Vector3 t = new Vector3(13, 0, 2);
        int layer = LayerMask.NameToLayer("Homotopy");

        Debug.Log(Physics.Linecast(s, t, 1 << layer));
    }

    void Update() {
        /*
        Vector3 s = new Vector3(8, 0, 2);
        Vector3 t = new Vector3(13, 0, 2);

        Debug.DrawLine(s, t, Color.blue);
        */
    }
}
