using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

namespace CyclesInUndirectedGraphs
{
	public class CycleFinder
	{
		//  Graph modelled as list of edges
		private static int[][] graph;
		public static List<int[]> cycles = new List<int[]>();

		public static void FindCycles() {
			for (int i = 0; i < graph.GetLength(0); i++) {
				for (int j = 0; j < 2; j++) {
					FindNewCycles(new int[] {graph[i][j]});
				}
			}
		}
		
		static void FindNewCycles(int[] path)
		{
			int n = path[0];
			int x;
			int[] sub = new int[path.Length + 1];
			
			for (int i = 0; i < graph.GetLength(0); i++)
				for (int y = 0; y <= 1; y++)
					if (graph[i][y] == n)
						//  edge referes to our current node
				{
					x = graph[i][(y + 1) % 2];
					if (!Visited(x, path))
						//  neighbor node not on path yet
					{
						sub[0] = x;
						Array.Copy(path, 0, sub, 1, path.Length);
						//  explore extended path
						FindNewCycles(sub);
					}
					else if ((path.Length > 2) && (x == path[path.Length - 1]))
						//  cycle found
					{
						int[] p = Normalize(path);
						int[] inv = Invert(p);
						if (IsNew(p) && IsNew(inv)) {
							cycles.Add(p);
                        }
					}
				}
		}

		public static void SetGraph(int[][] g) {
			graph = new int[g.GetLength(0)][];
			for(int i=0; i<g.GetLength(0); i++)
				graph[i] = new int[2];

			Array.Copy (g, graph, g.GetLength (0));
		}

		public static void PrintCycles() {
            StreamWriter w = new StreamWriter("Cycles.txt");
			foreach (int[] cy in cycles)
			{
				string s = "" + cy[0];
				
				for (int i = 1; i < cy.Length; i++)
                    //s += "," + cy[i];
                    s += " " + cy[i];
				
				//Debug.Log (s);
                w.WriteLine(s);
			}

            w.Flush();
            w.Close();
		}
		
		static bool Equals(int[] a, int[] b)
		{
			bool ret = (a[0] == b[0]) && (a.Length == b.Length);
			
			for (int i = 1; ret && (i < a.Length); i++)
				if (a[i] != b[i])
			{
				ret = false;
			}
			
			return ret;
		}
		
		static int[] Invert(int[] path)
		{
			int[] p = new int[path.Length];
			
			for (int i = 0; i < path.Length; i++)
				p[i] = path[path.Length - 1 - i];
			
			return Normalize(p);
		}
		
		//  rotate cycle path such that it begins with the smallest node
		static int[] Normalize(int[] path)
		{
			int[] p = new int[path.Length];
			int x = Smallest(path);
			int n;
			
			Array.Copy(path, 0, p, 0, path.Length);
			
			while (p[0] != x)
			{
				n = p[0];
				Array.Copy(p, 1, p, 0, p.Length - 1);
				p[p.Length - 1] = n;
			}
			
			return p;
		}
		
		static bool IsNew(int[] path)
		{
			bool ret = true;
			
			foreach(int[] p in cycles)
				if (Equals(p, path))
			{
				ret = false;
				break;
			}
			
			return ret;
		}
		
		static int Smallest(int[] path)
		{
			int min = path[0];
			
			foreach (int p in path)
				if (p < min)
					min = p;
			
			return min;
		}
		
		static bool Visited(int n, int[] path)
		{
			bool ret = false;
			
			foreach (int p in path)
				if (p == n)
			{
				ret = true;
				break;
			}
			
			return ret;
		}
	}
}