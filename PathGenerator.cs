using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Objects;

public class PathGenerator : MonoBehaviour {
    public Waypoint waypoint;
    public Enemy enemy;

    private Waypoint[][] wp;
    private Enemy[] en;

    private Graph graph;
    private int[] wpNum;
    private int[] realWPNum;
    private int enemyNum;

    // 인자로 List<List<int>[]> 같은 것을 넘기는 무거운 행위인가?
    // graph.guardIdx 같은 통신은 올바른 것인가?

    // 많은 반복을 위한 초기화 함수
    public void Initialize (List<List<int>[]> guardPath) {
        graph = GetComponent<Graph>();
        enemyNum = graph.pathVertices.Count;
        
        wpNum = new int[enemyNum];
        realWPNum = new int[enemyNum];
        wp = new Waypoint[enemyNum][];

        en = new Enemy[enemyNum];
        for (int i=0; i<enemyNum; i++) {
            // 이렇게 밖에 할 수 없나? sendmessage?
            //wpNum[i] = graph.twoPaths[graph.guardIdx][i].Count;
            //wpNum[i] = graph.onePaths[graph.guardIdx][i].Count;
            wpNum[i] = guardPath[graph.guardIdx][i].Count;
            realWPNum[i] = wpNum[i] * 2 - 2;

            if(graph.pathVertices[i][0] != graph.pathVertices[i][wpNum[i]-1]) // not cycle path
                wp [i] = new Waypoint[realWPNum[i]];
            else // cycle path
                wp [i] = new Waypoint[wpNum[i]-1];
        }
    }

    // 커스텀 경로를 위한 초기화 함수
    public void Initialize (int[] waypoint) {
        graph = GetComponent<Graph>();
        enemyNum = graph.pathVertices.Count;
        
        wpNum = new int[enemyNum];
        realWPNum = new int[enemyNum];
        wp = new Waypoint[enemyNum][];
        
        en = new Enemy[enemyNum];
        for (int i=0; i<enemyNum; i++) {
            // 이렇게 밖에 할 수 없나? sendmessage?
            //wpNum[i] = graph.twoPaths[graph.guardIdx][i].Count;
            //wpNum[i] = graph.onePaths[graph.guardIdx][i].Count;
            wpNum[i] = waypoint[i];
            realWPNum[i] = wpNum[i] * 2 - 2;
            
            if(graph.pathVertices[i][0] != graph.pathVertices[i][wpNum[i]-1]) // not cycle path
                wp [i] = new Waypoint[realWPNum[i]];
            else // cycle path 
                wp [i] = new Waypoint[wpNum[i]-1];
        }
    }

    // 웨이포인트 생성
    public void GenerateWaypoint() {
        for(int i=0; i<enemyNum; i++) {

            // if it does not cycle path
            if(graph.pathVertices[i][0] != graph.pathVertices[i][wpNum[i]-1]) {
                for(int j=0; j<wpNum[i]; j++) {
                    wp[i][j] = Instantiate(waypoint, graph.pathVertices[i][j], Quaternion.identity) as Waypoint;
                }

                for(int j=wpNum[i]; j<realWPNum[i]; j++) {
                    wp[i][j] = Instantiate(waypoint, graph.pathVertices[i][realWPNum[i]-j], Quaternion.identity) as Waypoint;
                }

                for(int j=0; j<realWPNum[i]; j++) {
                    wp[i][j].next = wp[i][(j+1)%realWPNum[i]];
                }
            }

            // cycle path
            else {
                for(int j=0; j<wpNum[i]-1; j++) {
                    wp[i][j] = Instantiate(waypoint, graph.pathVertices[i][j], Quaternion.identity) as Waypoint;
                }

                for(int j=0; j<wpNum[i]-1; j++) {
                    wp[i][j].next = wp[i][(j+1)%(wpNum[i]-1)];
                }
            }
        }
    }

    // 웨이포인트를 따라서 왕복운동할 경비병 생성
    public void GenerateEnemy() {
        for (int i=0; i<enemyNum; i++) {
            en [i] = Instantiate(enemy, graph.pathVertices [i][0], Quaternion.identity) as Enemy;

            en [i].moveSpeed = 1f;
            en [i].rotationSpeed = 30;
            en [i].target = wp [i][0];
            en [i].fovDistance = 6;
        }
    }

    public void Destroy() {
        if(wp != null) {
            for(int i=0; i<wp.Length; i++) {
                for(int j=0; j<wp[i].Length; j++) {
                    DestroyImmediate(wp[i][j].gameObject);
                }
            }
        }
        
        if(en != null) {
            for(int i=0; i<en.Length; i++) {
                DestroyImmediate(en[i].gameObject);
            }
        }
    }
}
