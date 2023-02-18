using System;
using System.Collections.Generic;
using Xunit;

namespace SimpDAG.Tests
{
	public partial class TopologicalSortTests
	{
		[Fact]
		public void VertexDataIsAcessible()
		{
			var graph = new AcyclicGraph<int, int>();

			var v0 = graph.AddVertex(0);
			var v1 = graph.AddVertex(1);
			var v2 = graph.AddVertex(2);

			Assert.Equal(0, graph[v0]);
			Assert.Equal(1, graph[v1]);
			Assert.Equal(2, graph[v2]);

			graph[v0] = 2;
			graph[v1] = 1;
			graph[v2] = 0;

			Assert.Equal(2, graph[v0]);
			Assert.Equal(1, graph[v1]);
			Assert.Equal(0, graph[v2]);
		}

		[Fact]
		public void EdgeDataIsAcessible()
		{
			var graph = new AcyclicGraph<int, int>();

			var v0 = graph.AddVertex();
			var v1 = graph.AddVertex();
			var v2 = graph.AddVertex();

			var e0 = graph.AddEdge(v0, v1, 0);
			var e1 = graph.AddEdge(v1, v2, 1);
			var e2 = graph.AddEdge(v2, v0, 2);

			Assert.Equal(0, graph[e0]);
			Assert.Equal(1, graph[e1]);
			Assert.Equal(2, graph[e2]);

			graph[e0] = 2;
			graph[e1] = 1;
			graph[e2] = 0;

			Assert.Equal(2, graph[e0]);
			Assert.Equal(1, graph[e1]);
			Assert.Equal(0, graph[e2]);
		}

		[Fact]
		public void EdgesAreUnique()
		{
			var graph = new AcyclicGraph<int, int>();

			var v0 = graph.AddVertex();
			var v1 = graph.AddVertex();

			var lhs = graph.AddEdge(v0, v1);
			var rhs = graph.AddEdge(v0, v1);

			Assert.Equal(lhs, rhs);
		}

		[Fact]
		public void EdgeDirectionIsStable()
		{
			var graph = new AcyclicGraph<int, int>();

			var v0 = graph.AddVertex();
			var v1 = graph.AddVertex();
			var v2 = graph.AddVertex();
			var v3 = graph.AddVertex();

			graph.AddEdge(v0, v1);
			graph.AddEdge(v0, v2);
			graph.AddEdge(v0, v3);

			var edges  = new Edge[graph.GetOutDegree(v0)];
			var count  = graph.GetOutgoingEdges(v0, edges);

			Assert.Equal(3, count);

			// = {v2, v3, v1} TODO
			
			Assert.Equal(v1, edges[2].to);
			Assert.Equal(v2, edges[0].to);
			Assert.Equal(v3, edges[1].to);
		}

		[Fact]
		public void LinearIsSortable()
		{
			var graph = new AcyclicGraph<int, int>();

			var v0 = graph.AddVertex();
			var v1 = graph.AddVertex();
			var v2 = graph.AddVertex();
			var v3 = graph.AddVertex();
			
			graph.AddEdge(v0, v1);
			graph.AddEdge(v1, v2);
			graph.AddEdge(v2, v3);

			var vertices = new Vertex[graph.vertexCount];

			Assert.True(graph.TopologicalSort(vertices));
			
			Assert.Equal(vertices[3], v0);
			Assert.Equal(vertices[2], v1);
			Assert.Equal(vertices[1], v2);
			Assert.Equal(vertices[0], v3);
		}

		[Fact]
		public void CycleIsNotSortable()
		{
			var graph = new AcyclicGraph<int, int>();

			var v0 = graph.AddVertex();
			var v1 = graph.AddVertex();
			var v2 = graph.AddVertex();
			
			graph.AddEdge(v0, v1);
			graph.AddEdge(v1, v2);
			graph.AddEdge(v2, v0);

			var vertices = new Vertex[graph.vertexCount];

			Assert.False(graph.TopologicalSort(vertices));
		}
	}
}

