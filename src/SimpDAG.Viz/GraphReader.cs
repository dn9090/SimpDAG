using System;
using System.Collections.Generic;
using System.IO;

namespace SimpDAG
{
	public static class GraphReader
	{
		public static AcyclicGraph<string, int> Read(TextReader reader)
		{
			var graph = new AcyclicGraph<string, int>();
			var map = new Dictionary<string, Vertex>();
			var neighbors = new List<Vertex>();
			
			string line = null;

			while((line = reader.ReadLine()) != null)
			{
				var vertices = line.Split(',');

				for(int i = 0; i < vertices.Length; ++i)
				{
					if(!map.TryGetValue(vertices[i], out var vertex))
					{
						vertex = graph.AddVertex(vertices[i]);
						map.Add(vertices[i], vertex);
					}

					if(neighbors.Count > 0)
						graph.AddEdge(neighbors[neighbors.Count - 1], vertex);

					neighbors.Add(vertex);
				}

				neighbors.Clear();
			}

			return graph;
		}
	}
}