using System;
using System.Collections.Generic;

namespace SimpDAG
{
	public readonly struct Edge : IComparable<Edge>
	{
		public readonly int index;

		public readonly Vertex from;

		public readonly Vertex to;

		public Edge(int index, Vertex from, Vertex to)
		{
			this.index = index;
			this.from  = from;
			this.to    = to;
		}

		public int CompareTo(Edge other)
		{
			return this.index.CompareTo(other.index);
		}

		public override string ToString()
		{
			return $"Edge({this.index}: {from.index} -> {to.index})";
		}
	}

	public readonly struct Vertex : IComparable<Vertex>
	{
		public readonly int index;

		internal Vertex(int index)
		{
			this.index = index;
		}

		public int CompareTo(Vertex other)
		{
			return this.index.CompareTo(other.index);
		}

		public override string ToString()
		{
			return "Vertex(" + this.index +")";
		}
	}

	internal struct VertexData<V>
	{
		public int incoming;

		public int outgoing;

		public V value;
	}

	internal struct EdgeData<E>
	{
		public Vertex from;

		public Vertex to;

		public E value;
	}

	internal struct DirectedEdge
	{
		public int index;
	}

	internal struct DirectedEdgeComparer<V, E> : IComparer<DirectedEdge>
	{
		public AcyclicGraph<V, E> graph;

		public DirectedEdgeComparer(AcyclicGraph<V, E> graph)
		{
			this.graph = graph;
		}

		public int Compare(DirectedEdge lhs, DirectedEdge rhs)
		{
			ref var lhsEdge = ref this.graph.edges[lhs.index];
			ref var rhsEdge = ref this.graph.edges[rhs.index];

			var fComp = lhsEdge.from.CompareTo(rhsEdge.from);
			var tComp = lhsEdge.to.CompareTo(rhsEdge.to);

			return fComp == 0 ? tComp : fComp;
		}
	}
	
	internal enum Marker : byte
	{
		None,
		Temporary,
		Permanent
	}

	public partial class AcyclicGraph<V, E>
	{
		public V this[Vertex vertex]
		{
			get => this.vertices[vertex.index].value;
			set => this.vertices[vertex.index].value = value;
		}

		public E this[Edge edge]
		{
			get => this.edges[edge.index].value;
			set => this.edges[edge.index].value = value;
		}

		public int edgeCount;

		public int vertexCount;

		internal VertexData<V>[] vertices;

		internal EdgeData<E>[] edges;

		internal DirectedEdge[] sortedEdges;

		internal DirectedEdgeComparer<V, E> comparer;

		public AcyclicGraph()
		{
			this.vertices    = new VertexData<V>[16];
			this.edges       = new EdgeData<E>[16];
			this.sortedEdges = new DirectedEdge[16];
			this.comparer    = new DirectedEdgeComparer<V, E>(this);
		}

		public Vertex AddVertex(V value = default)
		{
			if(this.vertices.Length == this.vertexCount)
				Array.Resize(ref this.vertices, this.vertices.Length * 2);

			var index = this.vertexCount++;
			this.vertices[index] = new VertexData<V> { value = value };

			return new Vertex(index);
		}

		public Edge AddEdge(Vertex from, Vertex to, E value = default)
		{
			if(from.index == to.index)
				throw new ArgumentException(nameof(to));

			var index = SearchEdgeIndex(from, to);

			if(index >= 0)
				return new Edge(index, from, to);

			if(this.edges.Length == this.edgeCount)
			{
				Array.Resize(ref this.edges, this.edges.Length * 2);
				Array.Resize(ref this.sortedEdges, this.sortedEdges.Length * 2);
			}

			index = this.edgeCount++;

			++this.vertices[from.index].outgoing;
			++this.vertices[to.index].incoming;

			this.edges[index]       = new EdgeData<E> { from = from, to = to, value = value };
			this.sortedEdges[index] = new DirectedEdge { index = index };
			
			this.sortedEdges.AsSpan(0, this.edgeCount).Sort(comparer);

			return new Edge(index, from, to);
		}

		public Edge GetEdge(int index)
		{
			if((uint)index >= (uint)edgeCount)
				throw new ArgumentOutOfRangeException(nameof(index));

			return new Edge(index, this.edges[index].from, this.edges[index].to);
		}

		public void GetEdges(Span<Edge> edges)
		{
			for(int i = 0; i < edgeCount; ++i)
				edges[i] = new Edge(i, this.edges[i].from, this.edges[i].to);
		}

		public int GetInDegree(Vertex vertex)
		{
			return this.vertices[vertex.index].incoming;
		}

		public int GetOutDegree(Vertex vertex)
		{
			return this.vertices[vertex.index].outgoing;
		}

		public int GetOutgoingEdges(Vertex vertex, Span<Edge> edges)
		{
			var directedEdges = GetOutgoingEdges(vertex);

			if(edges.Length < directedEdges.Length)
				throw new ArgumentOutOfRangeException(nameof(edges));

			for(int i = 0; i < directedEdges.Length; ++i)
			{
				var index = directedEdges[i].index;
				edges[i] = new Edge(index, this.edges[index].from, this.edges[index].to);
			}

			return directedEdges.Length;
		}

		internal ReadOnlySpan<DirectedEdge> GetOutgoingEdges(Vertex vertex)
		{
			var index = SearchEdgeStartIndex(vertex);
			
			if(index >= 0)
				return this.sortedEdges.AsSpan(index, this.vertices[vertex.index].outgoing);

			return Span<DirectedEdge>.Empty;
		}
		
		public bool TopologicalSort(Span<Vertex> vertices)
		{
			if(this.vertexCount < vertices.Length)
				throw new ArgumentOutOfRangeException(nameof(vertices));
			
			var markers = new Marker[this.vertexCount];
			var index   = 0;

			for(int i = 0; i < this.vertexCount; ++i)
			{
				if(!Visit(new Vertex(i), markers, vertices, ref index))
					return false;
			}
			
			return true;
		}

		internal bool Visit(Vertex vertex, Marker[] markers, Span<Vertex> vertices, ref int index)
		{
			if(markers[vertex.index] == Marker.Permanent)
				return true;
					
			if(markers[vertex.index] == Marker.Temporary)
				return false;

			markers[vertex.index] = Marker.Temporary;

			var directedEdges = GetOutgoingEdges(vertex);

			for(int i = 0; i < directedEdges.Length; ++i)
			{
				if(!Visit(this.edges[directedEdges[i].index].to, markers, vertices, ref index))
					return false;
			}
			
			markers[vertex.index] = Marker.Permanent;
			vertices[index++] = vertex;

			return true;
		}

		public bool TopologicalSort2(Span<Vertex> vertices)
		{
			if(this.vertexCount < vertices.Length)
				throw new ArgumentOutOfRangeException(nameof(vertices));
			
			var markers = new Marker[this.vertexCount];
			var index   = 0;

			var stack = new Stack<Vertex>();

			for(int i = 0; i < this.vertexCount; ++i)
			{
				if(markers[i] != Marker.None)
					continue;

				stack.Push(new Vertex(i));

				while(stack.Count > 0)
				{
					var vertex = stack.Peek();

					if(markers[vertex.index] == Marker.None)
					{
						markers[vertex.index] = Marker.Temporary;
					} else {
						markers[vertex.index] = Marker.Permanent;
						vertices[index++] = stack.Pop();
					}
					
					var directedEdges = GetOutgoingEdges(vertex);

					for(int j = directedEdges.Length - 1; j >= 0; --j)
					{
						var neighbor = this.edges[directedEdges[j].index].to;

						if(markers[neighbor.index] == Marker.Temporary)
							return false;
						
						if(markers[neighbor.index] == Marker.None)
							stack.Push(neighbor);
					}
				}
			}
			
			return true;
		}
	}
}
