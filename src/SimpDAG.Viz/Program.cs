using System;
using System.IO;
using SFML.System;
using SFML.Window;
using SFML.Graphics;
using SimpDAG;


var basicGraph =
	"a1 -> a2 -> a3 -> a4\n" +
	"b1 -> b2 -> b3 -> b4 -> b5\n" +
	"c1 -> c2 -> b3\n" +
	"d1 -> d2 -> d3 -> c1 -> a2\n" + 
	"a3 -> e1";

using var stream = new StringReader(basicGraph);
var graph = GraphReader.Read(stream);

var vertices = new SimpDAG.Vertex[graph.vertexCount];

if(!graph.TopologicalSort2(vertices))
	throw new InvalidOperationException();

vertices.AsSpan().Reverse();

var grid = GraphRenderer.CreateGridLayout(graph, vertices);


var window = new RenderWindow(new VideoMode(1280, 720), "SimpDAG Viz");

window.SetVerticalSyncEnabled(true);
window.Closed += (_, _) => window.Close();

var sw = new System.Diagnostics.Stopwatch();

sw.Start();

while(window.IsOpen)
{
	window.Clear(Color.White);
	window.DispatchEvents();
	
	Camera.Update(window, (float)sw.Elapsed.TotalSeconds);
	GraphRenderer.Render(graph, grid, window);

	sw.Restart();
	window.Display();
}
