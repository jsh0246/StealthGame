using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using Extra;
using Priority_Queue;
using System.Linq;
using CyclesInUndirectedGraphs;
using System.IO;

using System.Diagnostics;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class Graph : MonoBehaviour {

    // 스크립트 실행 순서?? dependancy 처리(노랑책)
    // 네임 스페이스 필요성?
    // 그래프 표현 방법? 배열 or 리스트
    // 프로퍼티는 엄청나게 자주 쓰이는 것인가?
    // 객체 생성 / 참조 / 해제 / 객체 생성 최소화

    private Brushfire bf;
    [HideInInspector]
    public List<Vector3[]> pathVertices { get; private set; } // 프로퍼티에 대해 더 생각해보기, 밑에 INT는 private set 하면 접근 불가인데 이거는 private set 해도 접근 가능함
	private int[][] path4Cycle;
    private float[] dist;
    private int[] prev;

    public int[][] vertexList { get; private set; }
    public float[][] weight { get; private set; }
    public PathGenerator pg { get; private set; }
    public List<List<int>[]> onePaths = new List<List<int>[]>();
    public List<List<int>[]> twoPaths = new List<List<int>[]>();
    public List<List<int>[]> threePaths = new List<List<int>[]>();
    public List<List<int>[]> testPaths = new List<List<int>[]>();
    public List<List<int>[]> cyclePaths = new List<List<int>[]>();

    public List<List<int>[]> rOnePaths = new List<List<int>[]>();
    public List<List<int>[]> rTwoPaths = new List<List<int>[]>();
    public List<List<int>[]> rThreePaths = new List<List<int>[]>();

    public List<List<int>[]> restricted = new List<List<int>[]>();

    public int vNum { get; private set; }
    public int wpNum { get; private set; }
    private static float M = 99999f;
    public int tS { get; private set; }
    public int tT { get; private set; }
    //private int cycleNum = 0;
    private int cycleNum = 0;
    [HideInInspector]
    public int guardIdx = 0;
    public int enemyNumber;
    public int randomSeed = -1;
    public int scene;

    private List<Vector3> st = new List<Vector3>();
    private List<Vector3> en = new List<Vector3>();
    private Stopwatch sw = new Stopwatch();

    void MakeGraphList() {
        MemoryAllocation();
		int n = 0;
        for(int i=0; i<bf.adjList.Count; i++) {
            Vector3 start = new Vector3(bf.adjList[i][0].xPos, 0f, bf.adjList[i][0].yPos);
            start = bf.CrdntTransform(start);
            st.Add(start);

            for(int j=1; j<bf.adjList[i].Count; j++) {
                Vector3 end = new Vector3(bf.adjList[i][j].xPos, 0f, bf.adjList[i][j].yPos);
                end = bf.CrdntTransform(end);
                en.Add(end);

                weight[i][j] = (float) Extra.Distance.L2Dist(start, end);

                int x = 0, y = 0;
                for(int k=0; k<vNum; k++) {
                    if(bf.vertices[k] == bf.adjList[i][0])
                        x = k;
                    if(bf.vertices[k] == bf.adjList[i][j])
                        y = k;
                }

                path4Cycle[n] = new int[2];
                path4Cycle[n][0] = x;
                path4Cycle[n][1] = y;
                n++;

                vertexList[i][0] = x;
                vertexList[i][j] = y;
            }
        }

        // resize?
		Array.Resize (ref path4Cycle, n);

        //PrintWeight();
		//PrintPath ();
        //PrintVertexList();

        //FindCycles ();

        /*
        sw.Start();
		FindCycles ();
        sw.Stop();
        Debug.Log((sw.ElapsedMilliseconds / 1000.0f).ToString() + " sec");
        */

        //ComputeImportanceNDegree();
        GenerateOneGuardPath();
        //GenerateTwoGuardPath();

        /*
        sw.Start();
        //GenerateTwoGuardPath();
        sw.Stop();
        Debug.Log((sw.ElapsedMilliseconds / 1000.0f).ToString() + " sec");
        */

        //GenerateThreeGuardPath();

        //GenerateCycleGuardPath();
        GenerateNonoverlappedTwoGuardPath();
        GenerateNonoverlappedThreeGuardPath();

        //GenerateTextGuardPath();
        //GenerateTestGuardPath();

        //sw.Reset();
        //sw.Start();
        //GenerateNonoverlappedTwoGuardPath();
        //GenerateNonoverlappedThreeGuardPath();
        //sw.Stop();
        //Debug.Log((sw.ElapsedMilliseconds / 1000.0f).ToString() + " sec");
    }

    // Compute each vertex's importance value and degree
    void ComputeImportanceNDegree() {
        string str = null;

        // Compute degree
        for(int i=0; i<bf.vertices.Count; i++) {
            bf.vertices[i].degree = vertexList[i].Length - 1;
        }

        // Compute Importance
        // Importance가 겹칠 때는? ex)4는 포켓이면서 출발지점에 가까움
        Vector3 start = GameObject.Find("Start").transform.position;
        Vector3 end = GameObject.Find("End").transform.position;

        for(int i=0; i<bf.vertices.Count; i++) {
            if(bf.vertices[i].degree == 1)
                bf.vertices[i].importance = 1;
            if(Extra.Distance.L2Dist(bf.CrdntTransform(new Vector3(bf.vertices[i].xPos, -0.4f, bf.vertices[i].yPos)), start) < 8)
                bf.vertices[i].importance = 3;
            if(Extra.Distance.L2Dist(bf.CrdntTransform(new Vector3(bf.vertices[i].xPos, -0.4f, bf.vertices[i].yPos)), end) < 8)
                bf.vertices[i].importance = 3;
            
            //Debug.Log(L2Dist(new Vector3(v.xPos, -0.4f, v.yPos), start));
        }

        /*
        for (int i=0; i<bf.vertices.Count; i++)
            str += i.ToString() + " : " +
                "Imp : " + bf.vertices[i].importance.ToString() + "  " +
                "Deg : " + bf.vertices[i].degree.ToString() + "  " +
                Extra.Distance.L2Dist(bf.CrdntTransform(new Vector3(bf.vertices[i].xPos, -0.4f, bf.vertices[i].yPos)), start).ToString("N2") + "  " + 
                Extra.Distance.L2Dist(bf.CrdntTransform(new Vector3(bf.vertices[i].xPos, -0.4f, bf.vertices[i].yPos)), end).ToString("N2") + "\n";

        Debug.Log(str);
        */
    }

    // Compute value for all one-guard path 
    // and make one-guard path list
    void GenerateOneGuardPath() {
        int[] p = new int[vNum];
        //float[] pathValue = new float[vNum * (vNum-1) + CycleFinder.cycles.Count];
        
        int idx = 0;
        string str = null;
        
        //str += "One Guard Path Value\n";
        for(int i=0; i<vNum; i++) {
            Dijkstra(i);
            
            switch(scene) {
                //Scene 4
                case 4 : {
                    //if(i == 4 || i == 12 || i == 13 || i == 11)
                    if(i == 4 || i == 12 || i == 13)
                        continue;
                    
                    if(i == 7 || i == 8)
                        continue;

                    break;
                }

                // Scene 5
                case 5 : {

                    break;
                }

                case 6 : {

                    break;
                }

                // Scene 6
                case 7 : {
                    if(i == 14)
                        continue;
                    break;
                }

                case 8 : {
                    if(i == 1)
                        continue;
                    break;
                }
            }
            
            
            for(int j=0; j<vNum; j++) {
                if(i == j)
                    continue;
                
                switch(scene) {
                    //Scene 4
                    case 4 : {
                        // 끝점이 포켓 안인 것 제외(Scene4)
                        if(j == 4 || j == 7 || j == 8)
                            continue;
                        
                        // 시작점와 끝점 근방의 엣지 길이 1 짜리 제외(Scene4)
                        if(i==1 && j==2 || i==2 && j==1 ||
                           i==1 && j==14 || i==14 && j==1 ||
                           i==11 && (j==12 || j==13 || j==16))
                            continue;

                        break;
                    }
                        
                    // Scene 5
                    case 5 : {

                        break;
                    }

                    case 6 : {
                        if(i==4 && j==8 || i==8 && j==4 ||
                           i==3 && j==5 || i==5 && j==3)
                            continue;

                        break;
                    }
                        
                    // Scene 6
                    case 7 : {
                        if(i==0 && j==13 || i==13 && j==0 ||
                           i==2 && (j==14 || j==5))
                            continue;

                        break;
                    }
                    case 8: {
                        if(i==8 && j==3 || i==3 && j==8)
                            continue;
                        break;
                    }
                }
                
                // onePath memory allocation
                onePaths.Add(new List<int>[1]);
                onePaths[idx][0] = new List<int>();
                int end = j;
                
                p[wpNum++] = end;
                while(i != end) {
                    end = prev[end];
                    p[wpNum++] = end;
                }
                
                //str += (idx+1).ToString() + " : ";
                for(int k=wpNum-1; k>=0; k--) {
                    onePaths[idx][0].Add(p[k]);
                    //pathValue[idx] += bf.vertices[p[k]].getValue();
                    
                    //str += p[k].ToString() + " ";
                }
                
                // wpNum-1?
                //pathValue[idx] /= wpNum;
                
                //str += "\t\t\t\tValue : " + pathValue[idx].ToString();
                //str += '\n';
                wpNum = 0;
                idx++;
            }
            
            //str += '\n';
        }

        // Add cycle path
        /*
        for(int i=0; i<CycleFinder.cycles.Count; i++) {
            onePaths.Add(new List<int>[1]);
            onePaths[idx][0] = CycleFinder.cycles[i].ToList();
            onePaths[idx][0].Add(onePaths[idx][0][0]);

            //str += (idx+1).ToString() + " : ";
            //for(int j=0; j<onePaths[idx][0].Count; j++) {
            //    str += onePaths[idx][0][j].ToString() + " ";
            //    pathValue[idx] += bf.vertices[onePaths[idx][0][j]].getValue();
            //}

            //pathValue[idx] /= onePaths[idx][0].Count;

            //str += "\t\t\t\tValue : " + pathValue[idx].ToString();
            //str += '\n';

            idx++;
        }
        */

        // Add cycles path manullay from text file
        StreamReader cycleText = new StreamReader("Cycles"+scene.ToString()+".txt");
        string cycleStr = cycleText.ReadToEnd();
        string[] cycleLine = cycleStr.Split('\n');

        for (int i=0; i<cycleLine.Length-1; i++)
        {
            string[] cycle = cycleLine[i].Split(' ');
            onePaths.Add(new List<int>[1]);
            onePaths[idx][0] = new List<int>();

            for(int j=0; j<cycle.Length; j++)
                onePaths[idx][0].Add(Convert.ToInt32(cycle[j]));
            // my mistake
            onePaths[idx][0].Add(Convert.ToInt32(cycle[0]));
            idx++;
        }

        //Debug.Log(str);

        /*
        for (int i=0; i<onePaths.Count; i++) {
            for(int j=0; j<onePaths[i][0].Count; j++) {
                str += onePaths[i][0][j] + " ";
            }
            str += '\n';
        }
        Debug.Log(str);
        */

        Debug.Log(onePaths.Count);
    }

    void GenerateRestrictedPaths() {
        List<List<int>[]> restricted1 = new List<List<int>[]>();
        List<List<int>[]> restricted2 = new List<List<int>[]>();
        List<List<int>[]> restricted3 = new List<List<int>[]>();

        bool[] any = {false, false, false};

        string str = null;
        for(int i=0; i<onePaths.Count; i++) {
            for(int j=0; j<onePaths[i].Length; j++) {
                for(int k=0; k<onePaths[i][j].Count; k++) {
                    str += onePaths[i][j][k] + " ";
                }

                if(onePaths[i][j].Contains(5) && onePaths[i][j].Contains(16) && onePaths[i][j].Contains(15))
                    //if(onePaths[i][j].Contains(9) && onePaths[i][j].Contains(8) && onePaths[i][j].Contains(3) && onePaths[i][j].Contains(14))
                    restricted1.Add((List<int>[])onePaths[i].Clone());
                
                if(any[1] == true)
                    restricted2.Add((List<int>[])onePaths[i].Clone());
                //else if(onePaths[i][j].Contains(7) && onePaths[i][j].Contains(11) && onePaths[i][j].Contains(10) && onePaths[i][j].Contains(6))
                else if(onePaths[i][j].Contains(6) && onePaths[i][j].Contains(11) && onePaths[i][j].Contains(13))
                    restricted2.Add((List<int>[])onePaths[i].Clone());
                
                //if(onePaths[i][j].Contains(3) && onePaths[i][j].Contains(7) && enemyNumber == 3)
                if(onePaths[i][j].Contains(6) && onePaths[i][j].Contains(9) && onePaths[i][j].Contains(0) && enemyNumber == 3)
                    restricted3.Add((List<int>[])onePaths[i].Clone());

            }
            str += '\n';
        }
        //Debug.Log(str);

        string str2 = null;
        for(int i=0; i<restricted1.Count; i++) {
            for(int j=0; j<restricted1[i].Length; j++) {
                for(int k=0; k<restricted1[i][j].Count; k++) {
                    str2 += restricted1[i][j][k] + " ";
                }
                str2 += '\n';
            }
        }
        str2 += '\n';

        for(int i=0; i<restricted2.Count; i++) {
            for(int j=0; j<restricted2[i].Length; j++) {
                for(int k=0; k<restricted2[i][j].Count; k++) {
                    str2 += restricted2[i][j][k] + " ";
                }
                str2 += '\n';
            }
        }
        str2 += '\n';

        for(int i=0; i<restricted3.Count; i++) {
            for(int j=0; j<restricted3[i].Length; j++) {
                for(int k=0; k<restricted3[i][j].Count; k++) {
                    str2 += restricted3[i][j][k] + " ";
                }
                str2 += '\n';
            }
        }
        Debug.Log("Each Restricted Path : " + "\n" + str2);

        int idx = 0;
        for(int i=0; i<restricted1.Count; i++) {
            for(int j=0; j<restricted2.Count; j++) {
                IEnumerable<int> intersection = restricted1[i][0].Intersect(restricted2[j][0]);

                switch(enemyNumber) {
                    case 2 : {
                        if(intersection.Count() <= 1) {
                            restricted.Add(new List<int>[2]);
                            restricted[idx][0] = new List<int>(restricted1[i][0]);
                            restricted[idx][1] = new List<int>(restricted2[j][0]);
                            idx++;
                        }
                        break;
                    }
                    case 3 : {
                        if(intersection.Count() <= 1) {
                            for(int k=0; k<restricted3.Count; k++) {
                                IEnumerable<int> intersection1 = restricted1[i][0].Intersect(restricted3[k][0]);
                                IEnumerable<int> intersection2 = restricted2[j][0].Intersect(restricted3[k][0]);

                                if(intersection1.Count() <= 1 && intersection2.Count() <= 1) {
                                    restricted.Add(new List<int>[3]);
                                    restricted[idx][0] = new List<int>(restricted1[i][0]);
                                    restricted[idx][1] = new List<int>(restricted2[j][0]);
                                    restricted[idx][2] = new List<int>(restricted3[k][0]);
                                    idx++;
                                }
                            }
                        }
                        break;
                    }
                }
            }
        }

        StreamWriter sw = new StreamWriter("Restricted Paths.txt");
        string str3 = null;
        for(int i=0; i<restricted.Count; i++) {
            str3 += i+1 + " : ";
            for(int j=0; j<restricted[i].Length; j++) {
                for(int k=0; k<restricted[i][j].Count; k++) {
                    str3 += restricted[i][j][k] + " ";
                }
                str3 += "\t";
            }
            str3 += "\r\n";
        }
        Debug.Log("Combined Restricted Path : " + '\n' + str3);
        sw.WriteLine(str3);
        sw.Flush();
        sw.Close();

        Debug.Log("Restricted 1 count : " + restricted1.Count + "\n" +
                  "Restricted 2 count : " + restricted2.Count + "\n" +
                  "Restricted 3 count : " + restricted3.Count + "\n" +
                  "Restricted total count : " + restricted.Count);
    }

    void GenerateNonoverlappedTwoGuardPath() {
        int idx = 0;

        //for(int i=0; i<100; i++) {
        for(int i=0; i<onePaths.Count; i++) {
            for(int j=i+1; j<onePaths.Count; j++) {
                if(i == j)
                    continue;

                IEnumerable<int> intersection = onePaths[i][0].Intersect(onePaths[j][0]);
                if(intersection.Count() <= 1) {
                    twoPaths.Add(new List<int>[2]);
                    twoPaths[idx][0] = new List<int>(onePaths[i][0]);
                    twoPaths[idx][1] = new List<int>(onePaths[j][0]);

                    idx++;
                }
            }
        }

        Debug.Log(twoPaths.Count);

        /*
        string str = null;
        //for(int i=0; i<5; i++) {
        for(int i=0; i<0; i++) {
            for(int j=0; j<2; j++) {
                for(int k=0; k<twoPaths[i][j].Count; k++) {
                    str += twoPaths[i][j][k].ToString() + " ";
                }
                str += "    ";
            }
            str += '\n';
        }

        Debug.Log(str);
        */
    }

    void GenerateNonoverlappedThreeGuardPath() {
        int idx = 0;

        /*
        for(int i=0; i<twoPaths.Count; i++) {
            for(int j=0; j<onePaths.Count; j++) {
                if(twoPaths[i][0].SequenceEqual(onePaths[j][0]) || twoPaths[i][1].SequenceEqual(onePaths[j][0]))
                    continue;

                IEnumerable<int> intersection1 = twoPaths[i][0].Intersect(onePaths[j][0]);
                IEnumerable<int> intersection2 = twoPaths[i][1].Intersect(onePaths[j][0]);
                if(intersection1.Count() <= 1 && intersection2.Count() <= 1) {
                    threePaths.Add(new List<int>[3]);

                    for(int k=0; k<2; k++)
                        threePaths[idx][k] = new List<int>(twoPaths[i][k]);
                    threePaths[idx][2] = new List<int>(onePaths[j][0]);

                    idx++;
                }
            }
        }
        */

        for(int i=0; i<onePaths.Count; i++) {
            for(int j=i+1; j<onePaths.Count; j++) {

                IEnumerable<int> intersection = onePaths[i][0].Intersect(onePaths[j][0]);
                if(intersection.Count() > 1)
                    continue;

                for(int k=j+1; k<onePaths.Count; k++) {
                    IEnumerable<int> intersection1 = onePaths[i][0].Intersect(onePaths[k][0]);
                    IEnumerable<int> intersection2 = onePaths[j][0].Intersect(onePaths[k][0]);

                    if(intersection1.Count() <= 1 && intersection2.Count() <= 1) {
                        threePaths.Add(new List<int>[3]);

                        threePaths[idx][0] = new List<int>(onePaths[i][0]);
                        threePaths[idx][1] = new List<int>(onePaths[j][0]);
                        threePaths[idx][2] = new List<int>(onePaths[k][0]);

                        idx++;
                    }
                }
            }
        }

        Debug.Log(threePaths.Count);

        /*
        string str = null;
        for(int i=0; i<1; i++) {
            for(int j=0; j<3; j++) {
                for(int k=0; k<threePaths[i][j].Count; k++) {
                    str += threePaths[i][j][k].ToString() + " ";
                }
                str += "    ";
            }
            str += '\n';
        }

        Debug.Log(str);
        */
    }

    void GenerateTwoGuardPath() {
        int idx = 0;
        float value = 0f;
        string str = null;

        // Make twoPaths List
        for(int i=0; i<onePaths.Count; i++) {
            for(int j=0; j<onePaths.Count; j++) {
                if(i == j)
                    continue;

                // new or AddRange
                twoPaths.Add(new List<int>[2]);
                twoPaths[idx][0] = new List<int>(onePaths[i][0]);
                twoPaths[idx][1] = new List<int>(onePaths[j][0]);
                //twoPaths[idx][0].AddRange(onePaths[i]);
                //twoPaths[idx][1].AddRange(onePaths[j]);

                idx++;
            }
        }

        float[] pathValue = new float[twoPaths.Count];

        // Not Union
        /*
        string str3 = null;
        str3 += "Two Guard Path Value\n";

        for(int i=0; i<twoPaths.Count; i++) {
        //for(int i=0; i<twoPaths.Count; i++) {
            for(int j=0; j<twoPaths[i][0].Count; j++) {
                value += bf.vertices[twoPaths[i][0][j]].getValue();
                //str3 += twoPaths[i][0][j].ToString() + " ";
            }

            //str3 += "\t\t\t";
            for(int j=0; j<twoPaths[i][1].Count; j++) {
                value += bf.vertices[twoPaths[i][1][j]].getValue();
                //str3 += twoPaths[i][1][j].ToString() + " ";
            }
            
            value /= (float)(twoPaths[i][0].Count + twoPaths[i][1].Count);
            pathValue[i] = value;
            str3 += value.ToString() + "\n";
            value = 0;
        }
        */

        // Union
        string str5 = null;
        for(int i=0; i<twoPaths.Count; i++) {
            IEnumerable<int> union = twoPaths[i][0].Union(twoPaths[i][1]);

            for(int j=0; j<union.Count(); j++) {
                value += bf.vertices[union.ElementAt(j)].getValue();
                str5 += union.ElementAt(j).ToString() + " ";
            }
            
            // 몇으로 나누어야 합당한지
            value /= union.Count();
            pathValue[i] = value;
            str5 += "Value : " + value.ToString() + "\n";
            value = 0;
        }

        //Debug.Log(str5);

        //Debug.Log(twoPaths.Count);
        //Debug.Log(str3);


        int count = 0;
        string str4 = null;
        for(int i=0; i<pathValue.Length; i++) {
            if(pathValue[i] >= 5.5f) {
                for(int j=0; j<2; j++) {
                    for(int k=0; k<twoPaths[i][j].Count; k++) {
                        str4 += twoPaths[i][j][k].ToString() + " ";
                    }
                    str4 += '\t';
                }

                str4 += pathValue[i].ToString() + "\n";
                count++;
            }
        }

        Debug.Log(count);
        //Debug.Log(str4);


        //Debug.Log(count);
        
        /*
        string str2 = null;
        int[] t = new int[15];
        foreach(float n in pathValue) {
            if(n <= 4.2)
                t[0]++;
            else if(n <= 4.4f)
                t[1]++;
            else if(n <= 4.6f)
                t[2]++;
            else if(n <= 4.8f)
                t[3]++;
            else if(n <= 5.0f)
                t[4]++;
            else if(n <= 5.2f)
                t[5]++;
            else if(n <= 5.4f)
                t[6]++;
            else if(n <= 5.6f)
                t[7]++;
            else if(n <= 5.8f)
                t[8]++;
            else if(n <= 6.0f)
                t[9]++;
            else if(n <= 6.2f)
                t[10]++;
            else
                t[11]++;
        }

        for(int i=0; i<15; i++)
            str2 += i.ToString() + " : " + t[i].ToString() + "\n";

        Debug.Log(str2);
        */
        
        /*
        float[] pathValue = new float[twoPaths.Count];

        // Compute value
        // Doesn't consider time
        for(int i=0; i<twoPaths.Count; i++) {
            IEnumerable<int> union = twoPaths[i][0].Union(twoPaths[i][1]);

            foreach(int n in union) {
                str += n.ToString() + " ";
                //value += bf.vertices[n].importance + bf.vertices[n].degree;
                value += bf.vertices[n].getValue();
            }

            // 몇으로 나누어야 합당한지
            value /= union.Count();
            pathValue[i] = value;
            //str += "\t\t\tValue : " + value.ToString("N2") + "\n";
            value = 0;
        }
        */

        // Considering time
        /*
        for(int i=0; i<twoPaths.Count; i++) {
            int sCount = twoPaths[i][0].Count < twoPaths[i][1].Count ? twoPaths[i][0].Count : twoPaths[i][1].Count;

            for(int k=0; k<sCount; k++) {
                if(twoPaths[i][0][k] != twoPaths[i][1][k]) {
                    value += bf.vertices[twoPaths[i][0][k]].getValue();
                    value += bf.vertices[twoPaths[i][1][k]].getValue();
                } else {
                    value += bf.vertices[twoPaths[i][0][k]].getValue();
                }
            }

            int bCount, bIndex;
            if(twoPaths[i][0].Count < twoPaths[i][1].Count) {
                bCount = twoPaths[i][1].Count;
                bIndex = 1;
            } else {
                bCount = twoPaths[i][0].Count;
                bIndex = 0;
            }

            for(int k=sCount; k<bCount; k++) {
                value += bf.vertices[twoPaths[i][bIndex][k]].getValue();
            }

            // 몇으로 나누어야 합당한지
            value /= (twoPaths[i][0].Count + twoPaths[i][1].Count);
            value = 0;
        }
        */

        //Debug.Log(str);
    }

    void GenerateThreeGuardPath() {
        float[] pathValue = new float[twoPaths.Count * (onePaths.Count - 1)];
        int idx = 0;
        string str = null;
        
        // 배열을 만드는 방법 
        // 1 : 투패쓰에다가 원패쓰를 더한다.
        // 2 : 쓰리패쓰를 그냥 만든다.
        // 더 효율적인 방법은?
        for (int i=0; i<twoPaths.Count; i++){ 
            for(int j=0; j<onePaths.Count; j++) {
                if(twoPaths[i][0].SequenceEqual(onePaths[j][0]) || twoPaths[i][1].SequenceEqual(onePaths[j][0]))
                    continue;
                
                threePaths.Add(new List<int>[3]);
                
                for(int k=0; k<2; k++)
                    threePaths[idx][k] = new List<int>(twoPaths[i][k]);
                threePaths[idx][2] = new List<int>(onePaths[j][0]);

                int pathLength = 0;
                // Compute path value
                for(int k=0; k<3; k++) {
                    pathLength += threePaths[idx][k].Count;

                    for(int l=0; l<threePaths[idx][k].Count; l++) {
                        pathValue[idx] += bf.vertices[threePaths[idx][k][l]].getValue();
                    }
                }

                pathValue[idx] /= pathLength;
                idx++;
            }
        }

        /*
        string str2 = null;
        str2 += "Three Guard Path Value\n";
        for (int i=0; i<200; i++) {
            str2 += pathValue[i].ToString() + "\n";
        }
        Debug.Log(str2);
        */


        /*
        int tCount = 0;
        string str3 = null;
        int idx2 = 0;

        str3 += "Test(Selected) Guard Path Value\n";
        for (int i=0; i<idx; i++) {
            if(pathValue[i] > 4.45f) {
                tCount++;

                testPaths.Add(new List<int>[3]);

                for(int j=0; j<3; j++) {
                    testPaths[idx2][j] = new List<int>(threePaths[i][j]);

                    for(int k=0; k<threePaths[i][j].Count; k++) {
                        str3 += threePaths[i][j][k].ToString() + " ";
                    }

                    str3 += '\t';
                }

                idx2++;
                str3 += pathValue[i].ToString();
                str3 += '\n';
            }
        }


        Debug.Log(tCount);
        Debug.Log(str3);
        */
    }

    void GenerateRandomOneGuardPaths() {
        List<int> rNumList = new List<int>();

        for(int i=0; i<80; i++) {
            int r = Random.Range(0, onePaths.Count);

            if(!rNumList.Contains(r)) {
                rNumList.Add(r);
                //rOnePaths.Add(onePaths[r]);
                rOnePaths.Add((List<int>[])onePaths[r].Clone());
            } else {
                i--;
            }
        }

        /*
        string str = null;
        for(int i=0; i<rOnePaths.Count; i++) {
            for(int j=0; j<rOnePaths[i].Length; j++) {
                for(int k=0; k<rOnePaths[i][j].Count; k++) {
                    str += rOnePaths[i][j][k] + " ";
                }
                str += "    ";
            }
            str += '\n';
        }
        Debug.Log(str);
        */
    }

    void GenerateRandomTwoGuardPaths() {
        List<int> rNumList = new List<int>();

        SetSeed();
        
        for(int i=0; i<80; i++) {
            int r = Random.Range(0, twoPaths.Count);
            
            if(!rNumList.Contains(r)) {
                rNumList.Add(r);
                rTwoPaths.Add(twoPaths[r]);
                //rTwoPaths.Add((List<int>[])twoPaths[r].Clone());
            } else {
                i--;
            }
        }

        string str = null;
        int count = 1;
        for(int i=0; i<rTwoPaths.Count; i++) {
            str += count++.ToString() + " : ";
            for(int j=0; j<rTwoPaths[i].Length; j++) {
                for(int k=0; k<rTwoPaths[i][j].Count; k++) {
                    str += rTwoPaths[i][j][k] + " ";
                }
                str += "\t";
            }
            str += "\r\n";
        }

        StreamWriter sw = new StreamWriter("2Guard Paths.txt");
        sw.WriteLine(str);
        sw.Flush();
        sw.Close();
        //Debug.Log(str);
    }

    void GenerateRandomThreeGuardPaths() {
        List<int> rNumList = new List<int>();
        
        for(int i=0; i<80; i++) {
            int r = Random.Range(0, threePaths.Count);
            
            if(!rNumList.Contains(r)) {
                rNumList.Add(r);
                rThreePaths.Add(threePaths[r]);
                //rThreePaths.Add((List<int>[])threePaths[r].Clone());
            } else {
                i--;
            }
        }
        
        string str = null;
        int count = 1;
        for(int i=0; i<rThreePaths.Count; i++) {
            str += count++.ToString() + " : ";
            for(int j=0; j<rThreePaths[i].Length; j++) {
                for(int k=0; k<rThreePaths[i][j].Count; k++) {
                    str += rThreePaths[i][j][k] + " ";
                }
                str += "\t";
            }
            str += "\r\n";
        }
        
        StreamWriter sw = new StreamWriter("3Guard Paths.txt");
        sw.WriteLine(str);
        sw.Flush();
        sw.Close();
        //Debug.Log(str);
    }

    void GenerateTextGuardPath() {
        StreamReader reader = new StreamReader("TextPaths.txt");

        // 여러 줄 읽기 위해서
        /*
        string str = reader.ReadToEnd();
        string[] line = str.Split('\n');
    
        for (int i=0; i<line.Length; i++)
        {
            string[] paths = line[i].Split('\t');
            testPaths.Add(new List<int>[enemyNumber]);

            for(int j=0; j<paths.Length-1; j++) {
                string[] path = paths[j].Split(' ');
                testPaths[i][j] = new List<int>();

                for(int k=0; k<path.Length-1; k++) {
                    testPaths[i][j].Add(Convert.ToInt32(path[k]));
                }
            }
        }

        string str2 = null;
        for (int i=0; i<testPaths.Count; i++)
        {
            for(int j=0; j<testPaths[i].Length; j++) {
                for(int k=0; k<testPaths[i][j].Count; k++) {
                    str2 += testPaths[i][j][k] + " ";
                }
                str2 += '\t';
            }
            str2 += '\n';
        }
        Debug.Log(str2);
        */

        // 한줄만 읽기 위해서 간단하게
        string str = reader.ReadLine();
        string[] line = str.Split('\t');
        string temp = null;

        testPaths.Add(new List<int>[enemyNumber]);

        for (int i=0; i<line.Length-1; i++)
        {
            string[] path = line[i].Split(' ');
            testPaths[0][i] = new List<int>();

            for(int j=0; j<path.Length-1; j++) {
                testPaths[0][i].Add(Convert.ToInt32(path[j]));
                temp += testPaths[0][i][j] + " ";
            }
            temp += '\t';
        }

        Debug.Log(temp);
        reader.Close();
    }

    void GenerateTestGuardPath() {
        // testPaths [] [].Add();

        // 2 Guard
        // Path 1
        testPaths.Add(new List<int>[2]);
        testPaths [0] [0] = new List<int>();
        testPaths [0] [0].Add(5);
        testPaths [0] [0].Add(2);
        
        testPaths [0] [1] = new List<int>();
        testPaths [0] [1].Add(6);
        testPaths [0] [1].Add(9);

        /*
        // Paths 2
        testPaths.Add(new List<int>[2]);
        testPaths [0] [0] = new List<int>();
        testPaths [0] [0].Add(5);
        testPaths [0] [0].Add(2);
        
        testPaths [0] [1] = new List<int>();
        testPaths [0] [1].Add(5);
        testPaths [0] [1].Add(16);
        */
        
        /*
        // Paths 2
        testPaths.Add(new List<int>[2]);
        testPaths [1] [0] = new List<int>();
        testPaths [1] [0].Add(5);
        testPaths [1] [0].Add(2);
        
        testPaths [1] [1] = new List<int>();
        testPaths [1] [1].Add(5);
        testPaths [1] [1].Add(16);
        */

        // 3 Guard
        // Path 1
        /*
        testPaths.Add(new List<int>[3]);
        testPaths [0] [0] = new List<int>();
        testPaths [0] [0].Add(5);
        testPaths [0] [0].Add(16);
        
        testPaths [0] [1] = new List<int>();
        testPaths [0] [1].Add(5);
        testPaths [0] [1].Add(2);

        testPaths [0] [2] = new List<int>();
        testPaths [0] [2].Add(3);
        testPaths [0] [2].Add(15);
        */

        /*
        // Paths 2
        testPaths.Add(new List<int>[3]);
        testPaths [1] [0] = new List<int>();
        testPaths [1] [0].Add(5);
        testPaths [1] [0].Add(16);
        
        testPaths [1] [1] = new List<int>();
        testPaths [1] [1].Add(3);
        testPaths [1] [1].Add(14);
        
        testPaths [1] [2] = new List<int>();
        testPaths [1] [2].Add(15);
        testPaths [1] [2].Add(6);
        */

        /*
        testPaths.Add(new List<int>[3]);
        testPaths [0] [0] = new List<int>();
        testPaths [0] [0].Add(5);
        testPaths [0] [0].Add(16);
        
        testPaths [0] [1] = new List<int>();
        testPaths [0] [1].Add(3);
        testPaths [0] [1].Add(14);
        
        testPaths [0] [2] = new List<int>();
        testPaths [0] [2].Add(15);
        testPaths [0] [2].Add(6);
        */
    }

    public void GenerateCycleGuardPath() {
        for(int i=0; i<CycleFinder.cycles.Count; i++) {
            cyclePaths.Add(new List<int>[1]);
            cyclePaths[i][0] = CycleFinder.cycles[i].ToList();
            
            // 잘 모르겠어서 싸이클 완성을 여기서 완성했는데 CycleFinder 클래스 안에서 하고싶다.
            // 배열의 크기를 1 늘리고 맨 마지막 원소를 처음원소로 할당
            cyclePaths[i][0].Add(cyclePaths[i][0][0]);
        }
    }

    public void OneGuardPath() {
        // 바뀐 경로 생성기 사용
        for (int i=0; i<1; i++) {
            for(int j=0; j<onePaths[guardIdx][0].Count; j++) {
                pathVertices[i][j] = new Vector3(bf.vertices[onePaths[guardIdx][0][j]].xPos, 0f, bf.vertices[onePaths[guardIdx][0][j]].yPos);
                pathVertices[i][j] = bf.CrdntTransform(pathVertices[i][j]);
            }
        }
        
        pg.Destroy();
        pg.Initialize(onePaths);
        pg.GenerateWaypoint();
        pg.GenerateEnemy();
        
        guardIdx = (guardIdx + 1) % onePaths.Count;
        
        // 옛날 경로 생성기 사용
        /*
        for(int i=0; i<onePaths[guardIdx].Count; i++) {
            pathVertices[i] = new Vector3(bf.vertices[onePaths[guardIdx][i]].xPos, 0f, bf.vertices[onePaths[guardIdx][i]].yPos);
            pathVertices[i] = bf.CrdntTransform(pathVertices[i]);
        }
        
        npg.Destroy();
        npg.Initialize(onePaths[guardIdx].Count);
        npg.GenerateWaypoint();
        npg.GenerateEnemy();
        guardIdx = (guardIdx + 1) % onePaths.Count;
        */
    }

    public void TwoGuardPath() {
        for (int i=0; i<2; i++) {
            for(int j=0; j<twoPaths[guardIdx][i].Count; j++) {
                pathVertices[i][j] = new Vector3(bf.vertices[twoPaths[guardIdx][i][j]].xPos, 0f, bf.vertices[twoPaths[guardIdx][i][j]].yPos);
                pathVertices[i][j] = bf.CrdntTransform(pathVertices[i][j]);
            }
        }

        pg.Destroy();
        pg.Initialize(twoPaths);
        pg.GenerateWaypoint();
        pg.GenerateEnemy();

        guardIdx = (guardIdx + 1) % twoPaths.Count;
    }

    public void ThreeGuardPath() {
        for (int i=0; i<3; i++) {
            for(int j=0; j<threePaths[guardIdx][i].Count; j++) {
                pathVertices[i][j] = new Vector3(bf.vertices[threePaths[guardIdx][i][j]].xPos, 0f, bf.vertices[threePaths[guardIdx][i][j]].yPos);
                pathVertices[i][j] = bf.CrdntTransform(pathVertices[i][j]);
            }
        }
        
        pg.Destroy();
        pg.Initialize(threePaths);
        pg.GenerateWaypoint();
        pg.GenerateEnemy();
        
        guardIdx = (guardIdx + 1) % threePaths.Count;
    }

    public void TestPath() {
        for (int i=0; i<enemyNumber; i++) {
            for(int j=0; j<testPaths[guardIdx][i].Count; j++) {
                pathVertices[i][j] = new Vector3(bf.vertices[testPaths[guardIdx][i][j]].xPos, 0f, bf.vertices[testPaths[guardIdx][i][j]].yPos);
                pathVertices[i][j] = bf.CrdntTransform(pathVertices[i][j]);
            }
        }
        
        pg.Destroy();
        pg.Initialize(testPaths);
        pg.GenerateWaypoint();
        pg.GenerateEnemy();

        guardIdx = (guardIdx + 1) % testPaths.Count;
    }

    public void OneGuardCyclePath() {
        for(int i=0; i<1; i++) {
            int j=0;
            
            for(j=0; j<CycleFinder.cycles[guardIdx].Length; j++) {
                pathVertices[i][j] = new Vector3(bf.vertices[CycleFinder.cycles[guardIdx][j]].xPos, 0f, bf.vertices[CycleFinder.cycles[guardIdx][j]].yPos);
                pathVertices[i][j] = bf.CrdntTransform(pathVertices[i][j]);
            }
            
            pathVertices[i][j] = new Vector3(bf.vertices[CycleFinder.cycles[guardIdx][0]].xPos, 0f, bf.vertices[CycleFinder.cycles[guardIdx][0]].yPos);
            pathVertices[i][j] = bf.CrdntTransform(pathVertices[i][j]);
        }
        
        pg.Destroy();
        pg.Initialize(cyclePaths);
        pg.GenerateWaypoint();
        pg.GenerateEnemy();
        
        guardIdx = (guardIdx + 1) % cyclePaths.Count;
    }

    public void RandomTwoGuardPath() {
        for (int i=0; i<2; i++) {
            for(int j=0; j<rTwoPaths[guardIdx][i].Count; j++) {
                pathVertices[i][j] = new Vector3(bf.vertices[rTwoPaths[guardIdx][i][j]].xPos, 0f, bf.vertices[rTwoPaths[guardIdx][i][j]].yPos);
                pathVertices[i][j] = bf.CrdntTransform(pathVertices[i][j]);
            }
        }
        
        pg.Destroy();
        pg.Initialize(rTwoPaths);
        pg.GenerateWaypoint();
        pg.GenerateEnemy();
        
        guardIdx = (guardIdx + 1) % rTwoPaths.Count;
    }
    
    public void RandomThreeGuardPath() {
        for (int i=0; i<3; i++) {
            for(int j=0; j<rThreePaths[guardIdx][i].Count; j++) {
                pathVertices[i][j] = new Vector3(bf.vertices[rThreePaths[guardIdx][i][j]].xPos, 0f, bf.vertices[rThreePaths[guardIdx][i][j]].yPos);
                pathVertices[i][j] = bf.CrdntTransform(pathVertices[i][j]);
            }
        }
        
        pg.Destroy();
        pg.Initialize(rThreePaths);
        pg.GenerateWaypoint();
        pg.GenerateEnemy();
        
        guardIdx = (guardIdx + 1) % rThreePaths.Count;
    }

    public void RestrictedTwoGuardPath() {
        for (int i=0; i<enemyNumber; i++) {
            for(int j=0; j<restricted[guardIdx][i].Count; j++) {
                pathVertices[i][j] = new Vector3(bf.vertices[restricted[guardIdx][i][j]].xPos, 0f, bf.vertices[restricted[guardIdx][i][j]].yPos);
                pathVertices[i][j] = bf.CrdntTransform(pathVertices[i][j]);
            }
        }
        
        pg.Destroy();
        pg.Initialize(restricted);
        pg.GenerateWaypoint();
        pg.GenerateEnemy();
        
        guardIdx = (guardIdx + 1) % restricted.Count;
    }

    void BFS() {
        Queue<int> q = new Queue<int>();
        List<int> visited = new List<int>();
        int[] depth = new int[vNum];
        int[] pred = new int[vNum];
        double min = 9999.0;
        int startIndex = -1, endIndex = -1;

        string str = null;
        string str2 = null;

        // Find nearest vertex from start and end point (Linear search)
        Vector3 start = GameObject.Find("Start").transform.position;
        Vector3 end = GameObject.Find("End").transform.position;
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

        visited.Add(startIndex);
        q.Enqueue(startIndex);
        depth[startIndex] = 0;
        pred[startIndex] = -1;

        while (q.Count != 0)
        {
            int t = q.Dequeue();
            str += t.ToString() + " ";

            // t == end ?
            if(t == endIndex) {
                continue;
            }

            for(int i=1; i<vertexList[t].Length; i++) {
                /*
                if(vertexList[t][i] == endIndex) {
                    int s = vertexList[t][0];

                    str2 += vertexList[t][i].ToString() + " " + s.ToString() + " ";
                    while(s != startIndex) {
                        s = pred[s];
                        str2 += s.ToString() + " ";
                    }

                    str2 += '\n';
                }
                */

                if(!visited.Contains(vertexList[t][i])) {
                    visited.Add(vertexList[t][i]);
                    q.Enqueue(vertexList[t][i]);
                    depth[vertexList[t][i]] = depth[t] + 1;
                    pred[vertexList[t][i]] = t;
                }
            }
        }

        str += "\n";
        for (int i=0; i<vNum; i++)
            str += depth [i].ToString() + " ";

        str += "\n";
        for (int i=0; i<vNum; i++)
            str += pred [i].ToString() + " ";

        Debug.Log(str);
        Debug.Log(str2);
    }

    // find Cycles
    // static하게 하는게 좋은건가 non-static하게 하는게 좋은건가
	void FindCycles() {
        CycleFinder.SetGraph (path4Cycle);
        CycleFinder.FindCycles();
        CycleFinder.PrintCycles ();
	}

    // 내부 클래스?
    private class INT : PriorityQueueNode {
        public int vertex { get; private set; }
        //private int vertex { get; private set; }
        // private , public 의 차이 getter 와 setter와 함께 생각...??
        public INT(int vertex) { this.vertex = vertex; }
    }

    // dijkstra, floyd-warshall, a*, johnson
    void Dijkstra(int start) {
        HeapPriorityQueue<INT> pQ = new HeapPriorityQueue<INT>(100);
        float min = M;
        INT[] t = new INT[vNum];

        // 이렇게 해야되나? 두번 할당하는 느낌...
        for (int i=0; i<vNum; i++)
            t [i] = new INT(i);

        dist [start] = 0;
        prev [start] = 0;
        for (int i=0; i<vNum; i++) {
            if(i != start) {
                dist[i] = M;
                prev[i] = (int)M;
            }

            pQ.Enqueue(t[i], dist[i]);
        }

        while (pQ.Count > 0) {
            int u = pQ.Dequeue().vertex;

            for(int v=1; v<weight[u].Length; v++) {
                min = dist[u] + weight[u][v];

                if(min < dist[vertexList[u][v]]) {
                    dist[vertexList[u][v]] = min;
                    prev[vertexList[u][v]] = u;
                    pQ.UpdatePriority(t[vertexList[u][v]], min);
                }
            }

            min = M;
        }

        //PrintDijkstra();
    }

    public void CustomPath() {
        int s = 2, ss = 16, t = 2, tt = 16, u = 10, uu = 6, v = 10, vv = 6;
        int end1, end2;
        int[] p = new int[vNum];
        int[] waypoint = new int[10];

        // first guard
        Dijkstra(s);
        end1 = ss;

        p[wpNum++] = end1;
        while(s != end1) {
            end1 = prev[end1];
            p[wpNum++] = end1;
        }

        for(int i=wpNum-1, j=0; i>=0; i--, j++) {
            pathVertices[0][j] = new Vector3(bf.vertices[p[i]].xPos, 0f, bf.vertices[p[i]].yPos);
            pathVertices[0][j] = bf.CrdntTransform(pathVertices[0][j]);
        }
        waypoint [0] = wpNum;
        wpNum = 0;

        // second guard
        if(s != t)
            Dijkstra(t);
        end2 = tt;
        
        p[wpNum++] = end2;
        while(t != end2) {
            end2 = prev[end2];
            p[wpNum++] = end2;
        }

        for(int i=wpNum-1, j=0; i>=0; i--, j++) {
            pathVertices[1][j] = new Vector3(bf.vertices[p[i]].xPos, 0f, bf.vertices[p[i]].yPos);
            pathVertices[1][j] = bf.CrdntTransform(pathVertices[1][j]);
        }
        waypoint [1] = wpNum;
        wpNum = 0;

        // third guard
        if(t != u)
            Dijkstra(u);
        end2 = uu;
        
        p[wpNum++] = end2;
        while(u != end2) {
            end2 = prev[end2];
            p[wpNum++] = end2;
        }

        for(int i=wpNum-1, j=0; i>=0; i--, j++) {
            pathVertices[2][j] = new Vector3(bf.vertices[p[i]].xPos, 0f, bf.vertices[p[i]].yPos);
            pathVertices[2][j] = bf.CrdntTransform(pathVertices[2][j]);
        }
        waypoint [2] = wpNum;
        wpNum = 0;

        // fourth guard
        if(u != v)
            Dijkstra(v);
        end2 = vv;

        p[wpNum++] = end2;
        while(v != end2) {
            end2 = prev[end2];
            p[wpNum++] = end2;
        }
        
        for(int i=wpNum-1, j=0; i>=0; i--, j++) {
            pathVertices[3][j] = new Vector3(bf.vertices[p[i]].xPos, 0f, bf.vertices[p[i]].yPos);
            pathVertices[3][j] = bf.CrdntTransform(pathVertices[3][j]);
        }
        waypoint [3] = wpNum;
        wpNum = 0;

        // first cycle guard
        /*
        cycleNum = 0;
        for(int i=0; i<CycleFinder.cycles[cycleNum].Length; i++) {
            pathVertices[2][i] = new Vector3(bf.vertices[CycleFinder.cycles[cycleNum][i]].xPos, 0f, bf.vertices[CycleFinder.cycles[cycleNum][i]].yPos);
            pathVertices[2][i] = bf.CrdntTransform(pathVertices[2][i]);
            wpNum++;
        }
        pathVertices [2] [wpNum] = new Vector3(bf.vertices [CycleFinder.cycles [cycleNum] [0]].xPos, 0f, bf.vertices [CycleFinder.cycles [cycleNum] [0]].yPos);
        pathVertices [2] [wpNum] = bf.CrdntTransform(pathVertices [2] [wpNum]);

        waypoint [2] = wpNum + 1;
        wpNum = 0;
        */

        // second cycle guard
        /*
        cycleNum = 2;
        for(int i=0; i<CycleFinder.cycles[cycleNum].Length; i++) {
            pathVertices[3][i] = new Vector3(bf.vertices[CycleFinder.cycles[cycleNum][i]].xPos, 0f, bf.vertices[CycleFinder.cycles[cycleNum][i]].yPos);
            pathVertices[3][i] = bf.CrdntTransform(pathVertices[3][i]);
            wpNum++;
        }
        pathVertices [3] [wpNum] = new Vector3(bf.vertices [CycleFinder.cycles [cycleNum] [0]].xPos, 0f, bf.vertices [CycleFinder.cycles [cycleNum] [0]].yPos);
        pathVertices [3] [wpNum] = bf.CrdntTransform(pathVertices [3] [wpNum]);
        
        waypoint [3] = wpNum + 1;
        wpNum = 0;
        */

        pg.Destroy();
        pg.Initialize(waypoint);
        pg.GenerateWaypoint();
        pg.GenerateEnemy();
    }

    void CompareSampling() {
        StreamReader r1 = new StreamReader("Test/G1.txt");
        StreamReader r2 = new StreamReader("Test/G2.txt");

        string[] g1 = r1.ReadToEnd().Split('\n');
        string[] g2 = r2.ReadToEnd().Split('\n');
        int count = 0;

        for(int i=0; i<80; i++) {
            string[] p1 = g1[i].Split('\t');
            string[] p2 = g2[i].Split('\t');
            bool same = true;

            for(int j=0; j<p1.Length; j++) {
                if(!p1[j].Equals(p2[j]))
                    same = false;
            }

            if(same)
                count++;
        }

        Debug.Log(count);

        r1.Close();
        r2.Close();
    }

    // 이렇게 동적 메모리 할당 하는 것이 맞나? 메모리 해제는 안해도 되나?
    void MemoryAllocation() {
        bf = GetComponent<Brushfire>();
        pg = GetComponent<PathGenerator>();
        vNum = bf.vertices.Count;

        pathVertices = new List<Vector3[]>();
        vertexList = new int[bf.adjList.Count][];
        weight = new float[bf.adjList.Count][];
		path4Cycle = new int[200][];
        dist = new float[vNum];
        prev = new int[vNum];

        for(int i=0; i<bf.adjList.Count; i++) {
            weight[i] = new float[bf.adjList[i].Count];
            vertexList[i] = new int[bf.adjList[i].Count];
		}

        // vNum + 1은 싸이클 경로 생성시 최대로 vNum+1개가 필요할 수 있기 때문에 그렇게 처리했음
        // 싸이클을 고려하기 전에는 vNum이었음
        for(int i=0; i<enemyNumber; i++)
            pathVertices.Add(new Vector3[vNum+1]);
    }

    void PrintWeight() {
        string str = null;

        for (int i=0; i<vNum; i++)
            str += bf.vertices [i].xPos.ToString() + " " + bf.vertices [i].yPos.ToString() + "\n";
        str += "\n";

        for(int i=0; i<bf.adjList.Count; i++) {
            for(int j=0; j<bf.adjList[i].Count; j++) {
                str += weight[i][j].ToString() + "    ";
            }

            str += "\n";
        }

        str += "\n";
        for(int i=0; i<vertexList.Length; i++) {
            for(int j=0; j<vertexList[i].Length; j++) {
                str += vertexList[i][j].ToString() + " ";
            }
            str += '\n';
        }
        
        Debug.Log(str);
    }

    void PrintDijkstra() {
        string str = null;

        str += "DIST : ";
        for (int i=0; i<vNum; i++)
            str += dist [i].ToString() + " ";

        str += "\nPREV : ";
        for (int i=0; i<vNum; i++)
            str += prev [i].ToString() + " ";

        Debug.Log(str);
    }

	void PrintPath() {
		string str = null;
		str += "Print Path List\n";

		for(int i=0; i<path4Cycle.GetLength(0); i++) {
			for(int j=0; j<2; j++)
				str += path4Cycle[i][j] + " ";
			str += '\n';
		}

		Debug.Log (str);
	}

    void RayCasting() {
        int layer = LayerMask.NameToLayer ("Obstacles");
           
        for(int i=0; i<st.Count; i++) {
            for(int j=0; j<en.Count; j++) {
                if(!Physics.Linecast(st[i], en[j], 1 << layer))
                    Debug.DrawLine(st[i], en[j], Color.magenta);
            }
        }
    }

    void PrintVertexNumber() {
        Camera camera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
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
    }

    void SetSeed() {
        int seed = randomSeed;

        if (randomSeed != -1)
            UnityEngine.Random.seed = randomSeed;
        else {
            DateTime now = DateTime.Now;
            seed = now.Millisecond + now.Second + now.Minute + now.Hour + now.Day + now.Month+ now.Year;
            UnityEngine.Random.seed = seed;
        }
    }

	// Use this for initialization
	void Start () {
        MakeGraphList();
        //PrintVertexNumber();

        GenerateRandomTwoGuardPaths();
        GenerateRandomThreeGuardPaths();

        GenerateRestrictedPaths();

        PrintVertexNumber();
        //CompareSampling();
	}
	
	// Update is called once per frame
	void Update () {
        //RayCasting();
	}

    void OnGUI() {
        if(GUI.Button(new Rect(20, 20, 150, 30), "Custom Path")) {
            CustomPath();
        }

        if(GUI.Button(new Rect(20, 70, 150, 30), "1 Guard")) {
            OneGuardPath();
        }

        if(GUI.Button(new Rect(20, 120, 150, 30), "2 Guard")) {
            TwoGuardPath();
        }

        if(GUI.Button(new Rect(20, 170, 150, 30), "3 Guard")) {
            ThreeGuardPath();
        }

        if(GUI.Button(new Rect(20, 220, 150, 30), "Test Guard")) {
            TestPath();
        }

        if(GUI.Button(new Rect(20, 270, 150, 30), "Cycle Guard")) {
            OneGuardCyclePath();
        }
        if(GUI.Button(new Rect(20, 320, 150, 30), "Restricted Path")) {
            RestrictedTwoGuardPath();
        }
    }
}