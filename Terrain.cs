using Godot;
using System;

public partial class Terrain : Godot.Node2D
{
	
	private Polygon2D poly;
	private CollisionPolygon2D cpoly;
	public float width;
	public float firstY; // Y value for first point
	public float lastY; // Y value for last point
	private Debris[] debris;
	private MapGenerator parentNode;
	private float segmentDepth = 300.0f;
	private Vector2[] originalPoly;
	
	[Signal]
	public delegate void DeformTerrainEventHandler();	
	[Signal]
	public delegate void CreateDebrisEventHandler( Vector2[] p, Vector2 position );
	[Signal]
	public delegate void CreateTerrainEventHandler( Vector2[] p );
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		poly = GetNode<Polygon2D>("StaticBody2D/Polygon2D");
		cpoly = GetNode<CollisionPolygon2D>("StaticBody2D/CollisionPolygon2D");
		
//		GD.Print( GetNode<Node>("..") );
		parentNode = GetNode<MapGenerator>("..");
		//GD.Print( parentNode.map_size );
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if ( Input.IsActionPressed("test_feature") ){
			SimplifyPolygon( poly.Polygon );
		}
	}
		
	public void BuildTerrain( Vector2 start_position, string terrain_type = "flat" ){
		//GD.Print( "BuildTerrain - "+start_position.X +", "+start_position.Y+" -- "+terrain_type);
		Vector2[] polyArr = GenerateGround( start_position, terrain_type );
		
		firstY = start_position.Y;
		poly.Polygon = polyArr;
		cpoly.Polygon = polyArr;
		//poly.Color = new Color((float)GD.RandRange(0.5,1.0),(float)GD.RandRange(0.5,1.0),(float)GD.RandRange(0.5,1.0));
	}
	
	private Vector2[] GenerateGround( Vector2 start_position, String ground_type = "" ){	
		Vector2[] polyArr = new Vector2[1]{ start_position };
		Vector2[] p2Arr;
		Vector2 oldPoint;
		Vector2 point;
		Vector2 direction = new Vector2( 1,0 );
		width = 0;
		
		int numPoints = 100;
		float segmentDistance = 20.0f;
		float oceanDecline;

		if ( ground_type == "ocean_left" ){
			polyArr[0].Y = segmentDepth-1;
		}
				
		for ( int i=0; i<numPoints; i++){
			
			oldPoint = polyArr[i];
			point = (segmentDistance * direction) + oldPoint;
			width += segmentDistance;

			//GD.Print( i+ "-- " +point );
			Vector2 altitudeAdjust = new Vector2(0,0);
			
			if ( i > 0 && i < numPoints-1 ){
				switch ( ground_type ){
					case ("mountain_incline"):
						altitudeAdjust.Y = GD.RandRange(-50,10);
						break;
					case ("mountain_decline"):
						altitudeAdjust.Y = GD.RandRange(-10,50);
						break;
					case ("rocky"):
						altitudeAdjust.Y = GD.RandRange(-10,10);
						break;
					case ("hill"):
						//add altitude;
						altitudeAdjust.Y = GD.RandRange(-5,5);
						break;
					case ("ocean_left"):
						oceanDecline = (segmentDepth-point.Y)/(numPoints-i) + (i/2); 
						//GD.Print("ocean: depth:"+segmentDepth+" - point.Y:"+point.Y+" - i:"+i+" - numPoints:"+numPoints+" = "+oceanDecline);
						if ( oceanDecline > 20 ){
							oceanDecline = 20;
						}
						altitudeAdjust.Y = (float) GD.RandRange(5, -oceanDecline);
						if ( i == 1 ){
							point.Y = segmentDepth - 1;
						}
						if ( point.Y < 0 ){
							point.Y = 0;
						}
						break;					
					case ("ocean_right"):
						oceanDecline = (segmentDepth-point.Y)/(numPoints-i) + (i/2); 
						//GD.Print("ocean:"+oceanDecline);
						altitudeAdjust.Y = (float) GD.RandRange(-5, oceanDecline);//(i/2));
						if ( i == numPoints-1 ){
							point.Y = segmentDepth - 1;
						}

						break;
					default:
						break;
				}
				point = point + altitudeAdjust;
			}
			
			if ( point.Y > segmentDepth ){
				point.Y = segmentDepth - 1;
			}
			
			p2Arr = new Vector2[polyArr.Length+1];
			for ( int j=0; j<polyArr.Length; j++ ){
				p2Arr[j] = polyArr[j];
			}
			p2Arr[p2Arr.Length-1] = point;
			polyArr = p2Arr;
			lastY = point.Y;
		}
		
		// add underground
		p2Arr = new Vector2[polyArr.Length+2];
		for ( int j=0; j<polyArr.Length; j++ ){
			p2Arr[j] = new Vector2( MathF.Round(polyArr[j].X, 2), MathF.Round(polyArr[j].Y, 2) );
		}
		p2Arr[p2Arr.Length-1] = new Vector2 (start_position.X, segmentDepth);
		p2Arr[p2Arr.Length-2] = new Vector2 (p2Arr[p2Arr.Length-3].X, segmentDepth);		
		polyArr = p2Arr;

		//GD.Print(polyArr);
		originalPoly = polyArr;

		//GD.Print("lastX:"+width+", lastY:"+lastY);
		return polyArr;
	}
	
	public void DoDeformTerrain( Vector2[] inPoly, Vector2 polyPosition ){

		for ( var i = 0; i<inPoly.Length; i++ ){
			inPoly[i] = new Vector2( inPoly[i].X + polyPosition.X - GlobalPosition.X -4, inPoly[i].Y + polyPosition.Y - GlobalPosition.Y - 501 );
		}
		var p = Geometry2D.ClipPolygons(poly.Polygon, inPoly);


		var minSize = 200;
		
		poly.Polygon = SimplifyPolygon( p[0] );//p[0];
		
		// for each other value in p, create a new enviro object
		//GD.Print( GetPolygonSize( p[0] ) );
		
		for(var i=1; i<p.Count; i++){
			// any polys that touch the base ground are new Terrain objects.
			// in the event the new poly is the "hole", don't make Debris.
			// if the poly is below a certain size, make nothing (or SmallDebris TODO)			
			//GD.Print("MORE POLY: "+i+" Normals:"+Geometry2D.IsPolygonClockwise(p[i]));
			
			float pSize = GetPolygonSize( p[i] );
			//GD.Print( pSize );
			//GD.Print("touching floor? "+IsTouchingFloor(p[i]) );
			
			
			if ( !Geometry2D.IsPolygonClockwise(p[i]) ){
				if ( !IsTouchingFloor(p[i]) ){
					// create Debris
					if ( pSize > minSize ){
						
//					EmitSignal(SignalName.CreateTerrain, p[i]);
						EmitSignal(SignalName.CreateDebris, p[i], GlobalPosition);
					}
				}else{
					// create Terrain
					EmitSignal(SignalName.CreateTerrain, p[i]);
				}
			}
		}
		
		CallDeferred("SetCPolygon", poly.Polygon);
	}
	
	private Boolean comparePoint( Vector2 point ){
		for ( var i=0; i < originalPoly.Length; i++ ){
			if ( originalPoly[i].DistanceTo(point) < 2 ){
				GD.Print( originalPoly[i] + " " + point );
				return true;
			}
		}
		return false;
	}

	private Vector2[] SimplifyPolygon ( Vector2[] inPoly ){		
		Vector2[] newPoly = inPoly;
		float minDistance = 7;
		Boolean cPointSet = false;
		Vector2 cPoint = Vector2.Zero;
		int points = 0;
		
	//	GD.Print( "============\nSimplify Polygon\n==========");
	//	GD.Print( "inPoly.Length = "+inPoly.Length );
	//	GD.Print( "minDistance = "+minDistance );
		
		
		Vector2 estPoint = Vector2.Zero;
		int pointDist = 0;
		String inPolyString = "";
		
		for ( var i=0; i<inPoly.Length; i++ ){

			inPolyString += inPoly[i];
			if ( !cPointSet ){
				cPoint = inPoly[i];
				cPointSet = true;
				points++;
				pointDist = 1;
				
			}else{
				float d = inPoly[i].DistanceTo(cPoint);
				
				estPoint += inPoly[i];			
				if ( d > minDistance || inPoly[i].Y == segmentDepth ){
					points++;
					Array.Resize<Vector2>(ref newPoly, points);
					newPoly[ points-1 ] = cPoint;

	//				GD.Print( " - cPoint: "+cPoint );
					cPoint = estPoint / pointDist;
					estPoint = new Vector2(0,0);
					pointDist = 1;
				}else{
	//				GD.Print( " - distance: "+d+" - estPoint: "+estPoint );	
					pointDist++;
				}				
				if ( i == inPoly.Length-1 ){
					points++;
					Array.Resize<Vector2>(ref newPoly, points);
					newPoly[ points-1 ] = cPoint;
				}
					
			}			
		}
		Vector2[] nPOutput = new Vector2[points];
		String outputString = "";
		//GD.Print( "numPoints: "+points+"\nPoly:" );
		for ( var i = 0; i < nPOutput.Length; i++ ){
			nPOutput[i] = newPoly[i];
			outputString += nPOutput[i];
			//GD.Print( newPoly[i] );
		}
		return nPOutput;
	}
	
	private Boolean IsTouchingFloor( Vector2[] inPoly ){
		for ( var i=inPoly.Length-1; i>-1; i-- ){
			if ( inPoly[i].Y == segmentDepth ){
				return true;
			}
		}
		return false;
	}
	
	private float GetPolygonSize( Vector2[] inPoly ){
		//https://www.wikihow.com/Calculate-the-Area-of-a-Polygon
		float a = 0f;
		float b = 0f;
		float A;
		float B;
		
		for ( var i = inPoly.Length-1; i > -1; i-- ){
			A = inPoly[i].X;
			if ( i == 0 ){
				B = inPoly[inPoly.Length-1].Y;
			}else{
				B = inPoly[i-1].Y;
			}
			a += A*B;
			
			A = inPoly[i].Y;
			if ( i == 0 ){
				B = inPoly[inPoly.Length-1].X;
			}else{
				B = inPoly[i-1].X;
			}
			b += A*B;
		}
		
		a = Mathf.Abs( (a-b)/2 );
		
		return a;
	}
	
	public Polygon2D GetPolygon(){
		return poly;
	}

	public void SetPolygon(Vector2[] inPoly){
		poly.Polygon = inPoly;
		CallDeferred("SetCPolygon", inPoly);
	}
	
	private void SetCPolygon(Vector2[] inPoly){
		cpoly.Polygon = inPoly;
	}
	
	public void MergeTerrain( Terrain inTerrain ){
		MergePolygon( inTerrain.GetPolygon() );
		inTerrain.DestroyTerrain();
	}
	
	public void MergePolygon( Polygon2D inPoly ){
		var p = Geometry2D.MergePolygons( poly.Polygon, inPoly.Polygon );
		poly.Polygon = p[0];
		cpoly.Polygon = poly.Polygon;		
	}

	public void DestroyTerrain(){
		QueueFree();
	}

}





