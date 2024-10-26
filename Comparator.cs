using UnityEngine;
using System.Collections;
using System;
using System.IO;

namespace Extra {
    public class Comparator : MonoBehaviour {

        // 증감 변화량을 추적하는 값(기울기의 변화량?)
        // Randomwalk , randomwalk in graph , 도둑잡기 (관련 논문, 도서, 웹페이지)
        // RRT1~5, Method1~4 성공률 및 성공경로 비교 (걸리는 시간 고려하기)
        // randomwalk in a graph (with time), pursuit evasion on graph
        // 성공율이 낮은 것은 당연하고 그 증감을 따라가야 한다

        FileInfo gsFile, rrtFile;
        StreamReader gsReader, rrtReader;
        string gsStr, rrtStr;
        string[] gsToken, rrtToken;

        // Frechet distance
        float[,] d;

        public double LeastSquare() {
            double leastSquare = 0.0;

            for(int i=0; i<gsToken.Length-1; i++) {
                leastSquare += Math.Pow(Convert.ToDouble(gsToken[i]) - Convert.ToDouble(rrtToken[i]), 2.0);
            }

            return leastSquare;
        }

        public int Gradient() {
            int score = 0;

            for(int i=0; i<gsToken.Length-2; i++) {
                double gsSlope = Convert.ToDouble(gsToken[i]) - Convert.ToDouble(gsToken[i+1]);
                double rrtSlope = Convert.ToDouble(rrtToken[i]) - Convert.ToDouble(rrtToken[i+1]);

                if(gsSlope * rrtSlope > 0)
                    score++;
            }

            return score;
        }

        // 어떤 패널티를 줄 것인가
        // 패널티의 정도?
        public double GradientLeastSqure() {
            double leastSquare = 0.0;

            for(int i=0; i<gsToken.Length-2; i++) {
                double gsSlope = Convert.ToDouble(gsToken[i]) - Convert.ToDouble(gsToken[i+1]);
                double rrtSlope = Convert.ToDouble(rrtToken[i]) - Convert.ToDouble(rrtToken[i+1]);

                leastSquare += Math.Pow(gsSlope - rrtSlope, 2.0);
            }

            return leastSquare;
        }

        public float FrechetDistance() {
            d = new float[gsToken.Length-1, gsToken.Length-1];

            for(int i=0; i<d.GetLength(0); i++) {
                for(int j=0; j<d.GetLength(1); j++) {
                    d[i, j] = -1f;
                }
            }

            return F(d.GetLength(0)-1, d.GetLength(1)-1);
        }

        float F(int i, int j) {
            if(d[i, j] > -1f) {
                return d[i, j];
            } else if(i==0 && j==0) {
                d[i, j] = Math.Abs((float)Convert.ToDouble(gsToken[i]) - (float)Convert.ToDouble(rrtToken[j]));
            } else if(i>0 && j==0) {
                d[i, j] = Math.Max(F(i-1, 0), Math.Abs((float)Convert.ToDouble(gsToken[i]) - (float)Convert.ToDouble(rrtToken[0])));
            } else if(i==0 && j>0) {
                d[i, j] = Math.Max(F(0, j-1), Math.Abs((float)Convert.ToDouble(gsToken[0]) - (float)Convert.ToDouble(rrtToken[j])));
            } else if(i>0 && j>0) {
                d[i, j] = Math.Max(Math.Min(F(i-1, j), Math.Min(F(i-1, j-1), F(i, j-1))), Math.Abs((float)Convert.ToDouble(gsToken[i]) - (float)Convert.ToDouble(rrtToken[j])));
            } else {
                d[i, j] = 99999f;
            }

            return d[i, j];
        }

        public void Initialize() {
            gsFile = new FileInfo("Test/GraphSample.txt");
            rrtFile = new FileInfo("Test/RRT.txt");
            gsReader = gsFile.OpenText();
            rrtReader = rrtFile.OpenText();

            gsStr = gsReader.ReadToEnd();
            rrtStr = rrtReader.ReadToEnd();
            
            gsToken = gsStr.Split('\n');
            rrtToken = rrtStr.Split('\n');
        }

        public void Close() {
            gsReader.Close();
            rrtReader.Close();
        }

        public void Start() {
            Initialize();

            Debug.Log("LeastSquare : " + LeastSquare());
            Debug.Log("Gradient : " + Gradient());
            Debug.Log("GradientLeastSquare : " + GradientLeastSqure());
            Debug.Log("FrechetDistance : " + FrechetDistance());
            
            Close();
        }
    }
}