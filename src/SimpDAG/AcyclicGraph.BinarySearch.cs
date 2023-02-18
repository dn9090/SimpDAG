using System;
using System.Collections.Generic;

namespace SimpDAG
{
	public partial class AcyclicGraph<V, E>
	{
		internal int SearchEdgeIndex(Vertex from, Vertex to)
		{
			var lo = 0;
			var hi = edgeCount - 1;

			while (lo <= hi)
			{
				var i = (int)(((uint)hi + (uint)lo) >> 1);

				var f = from.CompareTo(this.edges[sortedEdges[i].index].from);
				var t = to.CompareTo(this.edges[sortedEdges[i].index].to);
				var c = f == 0 ? t : f;

				if(c == 0)
					return i;
				else if(c > 0)
					lo = i + 1;
				else
					hi = i - 1;
			}

			return -1;
		}

		internal int SearchEdgeStartIndex(Vertex vertex)
		{
			var lo = 0;
			var hi = edgeCount - 1;

			while (lo <= hi)
			{
				var i = (int)(((uint)hi + (uint)lo) >> 1);
				var c = vertex.CompareTo(this.edges[sortedEdges[i].index].from);

				if(c == 0)
					return i;
				else if(c > 0)
					lo = i + 1;
				else
					hi = i - 1;
			}

			return -1;
		}
	}
}
