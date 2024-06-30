using Godot;
using System;

public partial class PolygonTest : Node2D
{
	Polygon2D poly1;
	Polygon2D poly2;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if ( Input.IsActionPressed("input_action") ){
			GD.Print( Geometry2D.ClipPolygons(poly1.Polygon, poly2.Polygon) );
			GD.Print("test");
		}
	}
}
