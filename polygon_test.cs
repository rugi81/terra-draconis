using Godot;
using System;

public partial class polygon_test : Node2D
{
	private RigidBody2D test;
	private Polygon2D p; 
	private Timer t;
	private Terrain terrain;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		test = GetNode<RigidBody2D>("TestRigid");
		p = GetNode<Polygon2D>("TestRigid/Poly2");
		t = GetNode<Timer>("TestRigid/Timer");
		terrain = GetNode<Terrain>("Terrain");
		Vector2[] poly = p.Polygon;
		
		for ( var i=0; i<poly.Length; i++ ){
			GD.Print( poly[i] );
		}
			GD.Print( "\nPos: "+ GlobalPosition );
		terrain.BuildTerrain(new Vector2(-100,0),"flat");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//test.Rotation += 0.01f;
		//p.Rotation += 0.01f;
		
		if ( Input.IsActionJustPressed("mouse_click_left") ){
			GD.Print("Mouse Click/Unclick at: ", GetViewport().GetMousePosition() );
			CloneRotatedPolygon();
		}
	}
	
	private void CloneRotatedPolygon(){
		Polygon2D pnew = new Polygon2D();
		Vector2[] dpoly = p.Polygon;
		Vector2 origin = test.Position;
		GD.Print(origin);
		for ( var i = 0; i<dpoly.Length; i++ ){
			dpoly[i] = CalculateRotatedPoint( dpoly[i] );
			dpoly[i] += origin;
		}	
		pnew.Polygon = dpoly;
		AddChild( pnew );
	}
	
	private void PunchHole(){
		Polygon2D pnew = new Polygon2D();
		Vector2[] newpoly = new Vector2[3];
		Vector2 newpos = new Vector2(50,55);
		
		newpoly[0] = new Vector2(0,0) + newpos;
		newpoly[1] = new Vector2(50,0) + newpos;
		newpoly[2] = new Vector2(25,50) + newpos;
		
		pnew.Polygon = newpoly;
		var x = Geometry2D.ClipPolygons( p.Polygon, pnew.Polygon );
		p.Polygon = x[0];
		GD.Print(x.Count);
		AddChild( pnew );
	}	
		
	private void PrintPolyPoints(){
		Vector2[] poly = p.Polygon;
		Vector2 point = new Vector2(0,0);
		
		GD.Print( "\nRotation: "+ test.Rotation );
		for ( var i=0; i<poly.Length; i++ ){
			point = poly[i];
			GD.Print( CalculateRotatedPoint(point) );
			poly[i] = CalculateRotatedPoint(point);
		}		
		GD.Print( "\nPos: "+ test.GlobalPosition );
	}
	
	private Vector2 CalculateRotatedPoint( Vector2 point ){
		Vector2 a = new Vector2(0,0);
		a.X = (point.X * Mathf.Cos(test.Rotation)) - (point.Y * Mathf.Sin(test.Rotation));
		a.Y = (point.Y * Mathf.Cos(test.Rotation)) + (point.X * Mathf.Sin(test.Rotation));
		return a;
	}
		
	private int CloneCount = 0;
	private void _on_timer_timeout()
	{
		//PrintPolyPoints();
		GD.Print( CloneCount );
		if ( CloneCount < 5 ){
			CloneRotatedPolygon();
			CloneCount++;
		}
	}
}


