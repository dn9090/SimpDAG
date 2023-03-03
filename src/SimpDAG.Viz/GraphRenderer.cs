using System;
using System.Collections.Generic;
using System.Numerics;
using SFML.Graphics;
using SFML.System;

namespace SimpDAG
{
	public struct VertexGrid
	{
		public int[] map;

		public Vertex[] cells;

		public VertexGrid(int vertexCount)
		{
			map   = new int[vertexCount];
			cells = new Vertex[vertexCount * vertexCount]; 
		}

		public void Insert(int x, int y, Vertex vertex)
		{
			var index = GetIndex(x, y, map.Length);
			map[vertex.index] = index + 1;
			cells[index]      = vertex;
		}

		public bool Contains(Vertex vertex)
		{
			var index = map[vertex.index] - 1;
			return index >= 0;
		}

		public bool HasVertex(int x, int y)
		{
			var index  = GetIndex(x, y, map.Length);
			var vertex = cells[index];
			return map[vertex.index] - 1 == index;
		}

		public Vertex GetVertex(int x, int y)
		{
			var index = GetIndex(x, y, map.Length);
			return cells[index];
		}

		public bool TryGetPosition(Vertex vertex, out int x, out int y)
		{
			var index = map[vertex.index] - 1;

			if(index >= 0)
			{
				GetPosition(index, map.Length, out x, out y);
				return true;
			}

			x = y = 0;

			return false;
		}

		public static void GetPosition(int index, int size, out int x, out int y)
		{
			x = index % size;
			y = index / size;
		}

		public static int GetIndex(int x, int y, int size)
		{
			return x + (y * size);
		}
	}

	internal static class GraphRenderer
	{
		internal static CircleShape circle = new CircleShape();

		internal static SFML.Graphics.Vertex[] line = new SFML.Graphics.Vertex[2];

		public static VertexGrid CreateGridLayout<V, E>(AcyclicGraph<V, E> graph, ReadOnlySpan<Vertex> vertices)
		{
			var edges = new Edge[graph.edgeCount];
			var grid  = new VertexGrid(vertices.Length);

			var row = 0;

			for(int v = 0; v < vertices.Length; ++v)
			{
				if(!grid.TryGetPosition(vertices[v], out var x, out var y))
				{
					x = 0;
					y = row++;
					grid.Insert(x, y, vertices[v]);
				}

				var col = x + 1;
				var count = graph.GetOutgoingEdges(vertices[v], edges);

				for(int e = 0; e < count; ++e)
				{
					if(grid.Contains(edges[e].to))
						continue;

					var targetX = col;
					var targetY = y;

					if(grid.HasVertex(targetX, targetY))
						targetY = row++;

					grid.Insert(targetX, targetY, edges[e].to);
				}
			}

			return grid;
		}

		public static void Render<V, E>(AcyclicGraph<V, E> graph, VertexGrid grid, RenderTarget renderTarget)
		{
			var size = 80f;

			for(int i = 0; i < graph.edgeCount; ++i)
			{
				var edge = graph.GetEdge(i);

				var from = grid.TryGetPosition(edge.from, out var fromX, out var fromY);
				var to   = grid.TryGetPosition(edge.to, out var toX, out var toY);

				if(from && to)
					DrawEdge(renderTarget, new Vector2(fromX, fromY) * size, new Vector2(toX, toY) * size);
			}

			for(int i = 0; i < grid.map.Length; ++i)
			{
				var index = grid.map[i] - 1;

				if(index < 0)
					continue;
				
				VertexGrid.GetPosition(index, grid.map.Length, out var x, out var y);
				DrawVertex(renderTarget, new Vector2(x, y) * size);
			}
		}

		internal static void DrawVertex(RenderTarget renderTarget, Vector2 position)
		{
			circle.Position         = new Vector2f(position.X - 20f, position.Y - 20f);
			circle.Radius           = 20f;
			circle.FillColor        = Color.White;
			circle.OutlineColor     = Color.Black;
			circle.OutlineThickness = 3f;
		
			renderTarget.Draw(circle);
		}

		internal static void DrawEdge(RenderTarget renderTarget, Vector2 from, Vector2 to, float offset = 25f)
		{
			line[0].Position = new Vector2f(from.X, from.Y);
			line[0].Color    = Color.Black;

			line[1].Position = new Vector2f(to.X, to.Y);
			line[1].Color    = Color.Black;

			renderTarget.Draw(line, PrimitiveType.Lines);

			var dir = to - Vector2.Normalize(to - from) * offset;

			circle.Position         = new Vector2f(dir.X - 5f, dir.Y - 5f);
			circle.Radius           = 5f;
			circle.FillColor        = Color.Black;
			circle.OutlineThickness = 0f;

			renderTarget.Draw(circle);
		}
	}
}
