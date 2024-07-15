using Godot;
using System;

public partial class Terrain : Godot.Node2D
{
	
	private Polygon2D poly;
	private Polygon2D bgpoly;
	private CollisionPolygon2D cpoly;
	public float width;
	public int[] leftEdge;
	public int[] rightEdge;

	public float firstY; // Y value for first point
	public float lastY; // Y value for last point
	private Debris[] debris;
	private MapGenerator parentNode;
	private float segmentDepth = 300.0f;
	private Boolean hasDebugTerrain = false;
	private LightOccluder2D shadows;

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
		bgpoly =  GetNode<Polygon2D>("StaticBody2D/Background");
		cpoly = GetNode<CollisionPolygon2D>("StaticBody2D/CollisionPolygon2D");
		shadows = GetNode<LightOccluder2D>("StaticBody2D/Shadow");
		
//		GD.Print( GetNode<Node>("..") );
		parentNode = GetNode<MapGenerator>("..");
		//GD.Print( parentNode.map_size );
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if ( Input.IsActionPressed("test_feature") ){
			SimplifyPolygon( poly.Polygon, new Vector2(0,0) );
		}
	}
		
	public void BuildTerrain( Vector2 start_position, string terrain_type = "flat" ){
		//GD.Print( "BuildTerrain - "+start_position.X +", "+start_position.Y+" -- "+terrain_type);
		Vector2[] polyArr = GenerateGround( start_position, terrain_type );
		
		firstY = start_position.Y;
		poly.Polygon = polyArr;
		cpoly.Polygon = polyArr;
		bgpoly.Polygon = polyArr;
		shadows.Occluder.Polygon = polyArr;
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

		return polyArr;
	}
	
	private void Debug_CreatePolygon( Vector2[] inPoly, Color inCol, Boolean isTerrain = false){
		if ( hasDebugTerrain && isTerrain ){
			return;
		}
		Polygon2D p = new Polygon2D();
		p.Polygon = inPoly;
		p.Color = inCol;
		AddChild(p);
	}

	public void DoDeformTerrain( Vector2[] inPoly, Vector2 polyPosition ){
		Vector2 polyOffset = new Vector2(0,498);
		Vector2 colPosition = polyPosition - GlobalPosition - polyOffset;

		for ( var i = 0; i<inPoly.Length; i++ ){
			inPoly[i] += colPosition;
		}
	 	
		// DEBUG
		/*
		float ppSize = 5f;
		Vector2[] pinPointPoly = new Vector2[4];
		pinPointPoly[0] = colPosition - new Vector2(-ppSize,-ppSize);
		pinPointPoly[1] = colPosition - new Vector2(ppSize,-ppSize);
		pinPointPoly[2] = colPosition - new Vector2(ppSize,ppSize);
		pinPointPoly[3] = colPosition - new Vector2(-ppSize,ppSize);

		Color Red = new Color(1f,0f,0f,0.4f);
		Color White = new Color(1f,1f,1f);
		Debug_CreatePolygon(poly.Polygon, White, true);
		Debug_CreatePolygon(inPoly, Red);
		Debug_CreatePolygon(pinPointPoly, Red);
*/
		// END DEBUG

		var p = Geometry2D.ClipPolygons(poly.Polygon, inPoly);


		var minSize = 200;
		
		poly.Polygon = SimplifyPolygon( p[0], colPosition ); //p[0];
		
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
	
	private Vector2[] SimplifyPolygon ( Vector2[] inPoly, Vector2 colPosition ){		
		Vector2[] newPoly = new Vector2[0];
		Vector2[] debugPoly = new Vector2[0];

		Vector2 cPoint = Vector2.Zero;
		Vector2 nPoint, pPoint;
		Vector2 estPoint = Vector2.Zero;
		int pointDist = 0;
		int initPoint = GetFirstPoint( inPoly );
		int maxSimpDistance = 7; // max simplify distance
		int simpAngle = 10; // directional angle that we simplify
		float pDirection, nDirection;

		cPoint = inPoly[initPoint];
		newPoly = AddPoint( newPoly, cPoint );

		//GD.Print( cPoint );

		int loopCount = 0;
		for ( var i=initPoint; loopCount < 1; i++ ){
			
			if ( i == inPoly.Length ){
				i = 0;
			}
			if ( i == initPoint-1 || ( initPoint == 0 && i == inPoly.Length-1 ) ){
				loopCount++;
			}

			//GD.Print(i);
			// once a main point is found;
			// build next main point;
				// get next point
				// if distance < minsimpdistance OR ( cdirection > 15 && cdirection < 75 )
				//		add to estimated point
				// else
				// 		build main point
			// loop until we wrap around to mainpointindex

			pPoint = inPoly[ ( i == 0 ) ? inPoly.Length-1 : i-1 ];
			nPoint = inPoly[ ( i == inPoly.Length-1 ) ? 0 : i+1 ];
			var ndist = cPoint.DistanceTo(nPoint);

			pDirection = Mathf.RadToDeg(cPoint.AngleToPoint(pPoint));
			pDirection = (pDirection < 0)?360+pDirection:pDirection;

			nDirection = Mathf.RadToDeg(cPoint.AngleToPoint(nPoint));
			nDirection = (nDirection < 0)?360+nDirection:nDirection;

			var cdirection = 180 + (simpAngle/2) - Mathf.Abs( nDirection - pDirection );

//			GD.Print( nPoint + " DIR: "+cdirection+" DIST:"+ndist);
//			GD.Print(pDirection + " -- "+nDirection);
//			GD.Print(ndist);

			if ( ndist < maxSimpDistance || cdirection > 0 && cdirection < simpAngle ){
				estPoint += nPoint;
				pointDist++;
			}else{
				if ( estPoint != Vector2.Zero ){
					estPoint /= pointDist;
					newPoly = AddPoint( newPoly, estPoint );
					//GD.Print( "Add Point"+estPoint+" - "+pointDist);
				}
				newPoly = AddPoint( newPoly, nPoint );
				estPoint = Vector2.Zero;
				pointDist = 0;
			}

			//newPoly = AddPoint( newPoly, cPoint );
			cPoint = nPoint;		
		}

		GD.Print( "npLength: "+newPoly.Length );
		var npString = "";
		for(int i=0; i<newPoly.Length; i++ ){
			npString += newPoly[i];
		}
		//GD.Print(npString);

		return newPoly;
	}

	private int GetFirstPoint( Vector2[] inPoly ){
		// need to find first main point.
		// check previous point's angle.
		// check next point's angle.
		// if compared direction hits a threshold over a distance, this isn't a main point.
			// if distance < minsimpdistance OR ( cdirection > 15 && cdirection < 75 )
				// not a main point
			// else 
				// is a main point - get mainpointindex
		Vector2 nPoint, pPoint;
		Vector2 cPoint = Vector2.Zero;

		for ( var i=0; i<inPoly.Length; i++ ){
			// set current point
			if ( cPoint == Vector2.Zero ){
				cPoint = inPoly[i];
			}

			// get previous point
			pPoint = inPoly[ ( i == 0 ) ? inPoly.Length-1 : i-1 ];
			nPoint = inPoly[ ( i == inPoly.Length-1 ) ? 0 : i+1 ];

			// get distance and direction to previous point.
			var pdist = cPoint.DistanceTo(pPoint);
			var ndist = cPoint.DistanceTo(nPoint);

			var pdirection = Mathf.RadToDeg(cPoint.AngleToPoint(pPoint));
			var ndirection = Mathf.RadToDeg(cPoint.AngleToPoint(nPoint));
			pdirection = (pdirection < 0)?360+pdirection:pdirection;
			ndirection = (ndirection < 0)?360+ndirection:ndirection;

			var cdirection = 185 - Mathf.Abs( ndirection - pdirection );

			if ( cdirection < 0 || cdirection > 10 ){
				return i;
			}
		}
		return 0;
	}

	private Vector2[] AddPoint( Vector2[] inPoly, Vector2 inPoint ) {
		int points = inPoly.Length+1;
		Array.Resize<Vector2>(ref inPoly, points);
		inPoly[ points-1 ] = inPoint;
		return inPoly;
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
		shadows.Occluder.Polygon = inPoly;
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





