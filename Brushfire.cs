 using UnityEngine;
using Common;
using Extra;
using System;
using System.Collections;
using System.Collections.Generic;

public class Brushfire : MonoBehaviour {

	// Data holders
	private Cell[][] bfMap;
    private List<Cell> medialAxis = new List<Cell>();
    public List<Cell> vertices { get; private set; }
    public List<List<Cell>> adjList { get; private set; }

	// Parameters with default values
	private static int gridSize = 60;
	private static GameObject floor = null;
    private static bool ray = false;
    private static int NEAR_VALUE = 4;

	// Helping stuff
    private Mapper mapper;
	private List<Queue<Cell>> qList = new List<Queue<Cell>>();
    private List<MidPoint> midPoint = new List<MidPoint>();

	void ComputeMap() {
		if (floor == null) {
			floor = (GameObject)GameObject.Find ("Floor");

			if (mapper == null) {
				mapper = floor.GetComponent<Mapper>();

				if (mapper == null)
					mapper = floor.AddComponent<Mapper>();

				mapper.hideFlags = HideFlags.None;
			}
		}

		//mapper.ComputeTileSize (SpaceState.Editor, floor.collider.bounds.min, floor.collider.bounds.max, MapperWindowEditor.gridSize, MapperWindowEditor.gridSize);
        //mapper.ComputeTileSize (SpaceState.Editor, mapper.collider.bounds.min, mapper.collider.bounds.max, gridSize, gridSize);
        // editor 참조 해서 조금 더 확장성 있게 해야 하나? 이게 전부인가?
        mapper.ComputeTileSize (SpaceState.Editor, floor.GetComponent<Collider>().bounds.min, floor.GetComponent<Collider>().bounds.max, gridSize, gridSize);
		bfMap = mapper.ComputeObstacles ();

        vertices = new List<Cell>();
        adjList = new List<List<Cell>>();
	}

    void MedialAxis() {
        Cell[] nb = new Cell[4];
        
        // Initialization, make L0
        qList.Add (new Queue<Cell> ());
        for (int i=1; i<bfMap.Length-1; i++) {
            for (int j=1; j<bfMap[i].Length-1; j++) {
                if (bfMap [i] [j].blocked) { // find obstacles
                    nb = OneNeighbor (i, j);
                    
                    for (int k=0; k<4; k++) {
                        if (!nb [k].blocked) { // find non-obstacles among neighbors
                            nb[k].dist = 0;
                            nb[k].obs = bfMap[i][j];
                            qList[0].Enqueue(nb[k]);
                        }
                    }
                }
            }
        }
        
        // Next step, loops
        for(int i=0; qList[i].Count > 0; i++) {
            qList.Add (new Queue<Cell> ());
            
            do {
                Cell c = new Cell();
                c = qList[i].Dequeue();
                nb = OneNeighbor(c.xPos, c.yPos);

                for(int j=0; j<4; j++) {
                    if(nb[j].dist == Cell.M) {
                        nb[j].dist = i+1;
                        nb[j].obs = c.obs;
                        qList[i+1].Enqueue(nb[j]);
                    } else if(!nb[j].blocked && Extra.Distance.L1Dist(nb[j].obs, c.obs) > NEAR_VALUE) {
                        if(!medialAxis.Contains(c)) {
                            medialAxis.Add(nb[j]);
                        }
                    }
                }
            } while (qList[i].Count > 0);
        }
    }

    // select vertices which have more than 3 neighbors
    void findMedialAxisVertices() {
        foreach (Cell c in medialAxis)
            if (TwoNeighborNum(c.xPos, c.yPos) >= 3)
                if(!vertices.Contains(c))
                    vertices.Add(c);
    }

	void MakeMidPointList() {
        List<Queue<Cell>> wave = new List<Queue<Cell>>();
        Cell cell = new Cell();
        int idx = 0;

		foreach (Cell c in medialAxis)
        {
            c.dist = Cell.M;
            //c.vDist = Cell.M;
            c.obs = null;
        }

        wave.Add(new Queue<Cell>());
               
        foreach (Cell c in vertices)
        { 
            c.dist = 0;
            //c.vDist = 0;
            c.obs = c;
            wave[0].Enqueue(c);

            adjList.Add(new List<Cell>());
            adjList[idx++].Add(c);  
        }

        for (int i=0; wave[i].Count>0; i++)
        {
            wave.Add(new Queue<Cell> ());

            do 
            {
                Cell[] nb  = new Cell[8];
                cell = wave[i].Dequeue();
                nb = TwoNeighbor(cell.xPos, cell.yPos);

                for(int j=0; j<8; j++)
                {
                    //if(nb[j].vDist == Cell.M)
                    if(nb[j].dist == Cell.M)
                    {
                        nb[j].dist = cell.dist + 1;
                        //nb[j].vDist = cell.vDist + 1;
                        nb[j].obs = cell.obs;
                        wave[i+1].Enqueue(nb[j]);
                    }
                    else if(medialAxis.Contains(nb[j]))
                    {
                        foreach(List<Cell> list in adjList) {
                            if(list[0] == cell.obs) {
                                if(!list.Contains(nb[j].obs)) {
                                    list.Add(nb[j].obs);

                                    //if(!list.Contains(nb[j]))
                                        //list.Add(nb[j]);

                                    midPoint.Add(new MidPoint(cell.obs, nb[j].obs, cell));
                                    break;
                                }
                            }
                        }
                    }
                }
            } while (wave[i].Count > 0);
        }

        /*
        string str = null;
        foreach(MidPoint mPoint in midPoint) {
            str += mPoint.PrintMidPoint() + "\n";
        }
        Debug.Log(str);
        */

        // midPoint postprocessing(delete inverse case, (a-b) == (b-a))
        /*
        int count = midPoint.Count;
        for (int i=0; i<count; i++)
        {
            for(int j=0; j<count; j++)
            {
                if(i == j)
                    continue;

                if(midPoint[i].c1.xPos == midPoint[j].c2.xPos && midPoint[i].c1.yPos == midPoint[j].c2.yPos) {
                    midPoint.Remove(midPoint[j]);
                    count--;
                    break;
                }
            }
        }
        */

        /*
        string str3 = null;
        foreach(MidPoint mPoint in midPoint) {
            str3 += mPoint.PrintMidPoint() + "\n";
        }
        Debug.Log(str3);
        */

        /*
        Debug.Log("adjList\n");
        foreach(List<Cell> list in adjList) {
            string str2 = null;
            foreach(Cell c in list) {
                str2 += c.printCoordinate() + "\n";
            }
            Debug.Log(str2);
        }
        */
    }

    void TraverseAndMakeAdjList()
    {
        int layer = LayerMask.NameToLayer ("Obstacles");
        int count = adjList.Count;

        for(int i=0; i<count; i++)
        {
            Vector3 start = new Vector3(adjList[i][0].xPos, 0f, adjList[i][0].yPos);
            start = CrdntTransform(start);

            int count2 = adjList[i].Count;
            for(int j=1; j<count2; j++)
            {
                Vector3 end = new Vector3(adjList[i][j].xPos, 0f, adjList[i][j].yPos);
                end = CrdntTransform(end);
                
                if(Physics.Linecast(start, end, 1 << layer))
                {
                    for(int k=0; k<midPoint.Count; k++) {
                        if(midPoint[k].CompareCells(adjList[i][0], adjList[i][j])) {
                            vertices.Add(midPoint[k].midPoint);

                            Cell one = adjList[i][0];
                            Cell theOther = adjList[i][j];

                            for(int l=0; l<adjList.Count; l++) {
                                if(adjList[l][0] == one) {
                                    adjList[l].Add(midPoint[k].midPoint);
                                    adjList[l].Remove(theOther);
                                }

                                if(adjList[l][0] == theOther) {
                                    adjList[l].Add(midPoint[k].midPoint);
                                    adjList[l].Remove(one);
                                }
                            }

                            List<Cell> midPointList = new List<Cell>();
                            midPointList.Add(midPoint[k].midPoint);
                            midPointList.Add(one);
                            midPointList.Add(theOther);
                            adjList.Add(midPointList);
                            
                            break;
                        }
                    }
                }
            }
        }
    }

    // comebine close vertices(delete close vertices but leaves only one)
    void DelSimilarVertices() {
        List<Cell> del = new List<Cell>();
        int count = vertices.Count;

        for (int i=0; i<vertices.Count; i++) {
            for(int j=0; j<vertices.Count; j++) {
                if(i == j)
                    continue;
                
                if(Extra.Distance.L1Dist(vertices[i], vertices[j]) < 5) {
                    if(!del.Contains(vertices[j]))
                        del.Add(vertices[i]);

                    //vertices.Remove(vertices[j]);
                    //count--;

                    //i--;
                    //j--;
                    //break;
                }
            }
        }

        for (int i=0; i<del.Count; i++)
        {
            vertices.Remove(del[i]);
        }
    }

    // return one neighborhood of cell[x][y]
	Cell []OneNeighbor(int x, int y) {
		Cell[] nb = new Cell[4];

		nb [0] = bfMap [x + 1] [y];
		nb [1] = bfMap [x] [y + 1];
		nb [2] = bfMap [x - 1] [y];
		nb [3] = bfMap [x] [y - 1];

		return nb;
	}

    // return two neighborhood of cell[x][y]
    Cell []TwoNeighbor(int x, int y) {
        Cell[] nb = new Cell[8];
        
        nb [0] = bfMap [x + 1] [y + 1];
        nb [1] = bfMap [x + 1] [y - 1];
        nb [2] = bfMap [x - 1] [y + 1];
        nb [3] = bfMap [x - 1] [y - 1];
        nb [4] = bfMap [x + 1] [y];
        nb [5] = bfMap [x] [y + 1];
        nb [6] = bfMap [x - 1] [y];
        nb [7] = bfMap [x] [y - 1];
        
        return nb;
    }

    // return number of two neighborhood of cell[x][y]
    int TwoNeighborNum(int x, int y) {
        int count = 0;

        for (int i=-1; i<=1; i++) {
            for (int j=-1; j<=1; j++) {
                if (i == 0 && j == 0)
                    continue;
                if (medialAxis.Contains(bfMap [x+i] [y+j]))
                    count++;
            }
        }

        return count;
    }

    // crdntTransform 입력 변수와 리턴변수를 무엇으로 하는게 편할까 L2Dist와 makeLinearGraph와 같이 고려
    // grid coordinate -> world coordinate
    public Vector3 CrdntTransform(Vector3 v) {
        //Vector3 u = new Vector3();
        // 이거 선언해서 리턴 하는게 안전한 방법인가?
        
        v.x = v.x * mapper.tileSizeX - mapper.cellsX * mapper.tileSizeX / 2f;
        v.z = v.z * mapper.tileSizeZ - mapper.cellsZ * mapper.tileSizeZ / 2f;

        return v;
    }

    public float CrdntTransform(float pos) {
        return pos * mapper.tileSizeX - mapper.cellsX * mapper.tileSizeX / 2f;
    }

    // grid coordinate <- world coordinate
    public Vector3 InvCrdntTransform(Vector3 v) {
        v.x = v.x / mapper.tileSizeX + mapper.cellsX / 2f;
        v.z = v.z / mapper.tileSizeZ + mapper.cellsZ / 2f;
        
        return v;
    }

    public float InvCrdntTransform(float pos) {
        return pos / mapper.tileSizeX + mapper.cellsX / 2f;
    }
	
	void PrintMap() {
		string str = null;

		for (int i=0; i<bfMap.Length; i++) {
			for (int j=0; j<bfMap[i].Length; j++) {
				//str += (bfMap [i] [j].blocked ? "X " : "   ");

				string cc = null;
				cc = bfMap[i][j].dist.ToString();
				if(cc == "-1")
                    cc = "B";
				if(cc.Length == 1)
					cc += "  ";
				str += cc + "";
			}
			
			str += "\n";
		}
		
		Debug.Log (str);
	}

	void DrawMedialAxis() {
		string str = null;

		for (int i=0; i<bfMap.Length; i++) {
			for (int j=0; j<bfMap[i].Length; j++) {
				if (medialAxis.Contains (bfMap [i] [j]))
                    //str += (++count).ToString() + " ";
                    str += "X";
                else if(bfMap[i][j].blocked)
                    str += "B";
				else
					str += "  ";
			}

			str += "\n";
		}

		Debug.Log (str);
	}

	void DrawVerticesBeforeDeletion() {
        string str = null;

        for (int i=0; i<bfMap.Length; i++)
        {
            for (int j=0; j<bfMap[i].Length; j++)
            {
                if (vertices.Contains(bfMap [i] [j]))
                    str += "X";
                else if (bfMap [i] [j].blocked)
                    str += "B";
                else
                    str += "  ";
            }
            str += "\n";
        }

        Debug.Log(str);
    }

    void DrawVerticesAfterDeletion() {
        string str = null;

       	for (int i=0; i<bfMap.Length; i++) {
            for (int j=0; j<bfMap[i].Length; j++) {
                if(vertices.Contains(bfMap[i][j]))
                    str += "X";
                else if(bfMap[i][j].blocked)
                    str += "B";
                else
                    str += "  ";
            }
            str += "\n";
        }

        Debug.Log(str);
	}

	void DrawOnlyMA() {
		string str = null;

		str += "Draw only MA\n";
        for (int i=0; i<bfMap.Length; i++)
        {
            for(int j=0; j<bfMap.Length; j++)
            {
                if(medialAxis.Contains(bfMap[i][j]))
                    str += bfMap[i][j].dist.ToString();
                else 
                    str += "  ";
            }

            str += '\n';
        }
		
		Debug.Log (str);
	}

    void DrawFinalVertices() {
        string str = null;
        
        for (int i=0; i<bfMap.Length; i++)
        {
            for(int j=0; j<bfMap.Length; j++)
            {
                if(vertices.Contains(bfMap[i][j]))
                    str += "X";
                else if(bfMap[i][j].blocked)
                    str += "B";
                else
                    str += "  ";
            }
            
            str += '\n';
        }
        
        Debug.Log (str);
    }

    void DelPocketVertices() {
        List<Cell> del = new List<Cell>();
        List<Cell> del2 = new List<Cell>();
        int idx = 0;

        for(int i=0; i<adjList.Count; i++) {
            if(adjList[i].Count <= 2) {
                adjList.RemoveAt(i);
                del.Add(adjList[i][0]);
                del2.Add(adjList[i][1]);
            }
        }

        for (int i=0; i<adjList.Count; i++)
        {
            if(del2.Contains(adjList[i][0]))
                adjList[i].Remove(del[idx++]);
        }
    }

    void RayCasting() {
        for(int i=0; i<adjList.Count; i++) {
            Vector3 start = new Vector3(adjList[i][0].xPos, 0f, adjList[i][0].yPos);
            start = CrdntTransform(start);

            for(int j=0; j<adjList[i].Count; j++) {
                Vector3 end = new Vector3(adjList[i][j].xPos, 0f, adjList[i][j].yPos);
                end = CrdntTransform(end);

                Debug.DrawLine(start, end, Color.red);
            }
        }
    }

	// Use this for initialization
	void Start () {
        ComputeMap();
        MedialAxis();
        //PrintMap ();

        findMedialAxisVertices();
        //DrawMedialAxis();
       
        //DrawVerticesBeforeDeletion();
        // DelSimilarVertices() is not debugging function.
        DelSimilarVertices();
        //DrawVerticesAfterDeletion();

        MakeMidPointList();
        //DrawOnlyMA();
        TraverseAndMakeAdjList();
        //DrawFinalVertices();

        //DelPocketVertices();
	}

    void Update() {
        if (ray)
        {
            RayCasting();
        }
    }

    void OnGUI() {
        if(GUI.Button(new Rect(2000, 20, 150, 30), "RayCasting")) {
            ray = !ray;
        }
    }
}
