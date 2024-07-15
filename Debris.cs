using Godot;
using System;

public partial class Debris : RigidBody2D
{
	private Polygon2D poly;
	private CollisionPolygon2D cpoly;
	private Boolean moveBody = false;
	private Vector2 moveTarget;
	private LightOccluder2D shadows;
	
	
	[Export]
	public PackedScene PolyScene {get;set;}
	
	[Signal]
	public delegate void CreateDebrisEventHandler( Vector2[] p, Vector2 position );
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//GD.Print("Debris made");
		poly = GetNode<Polygon2D>("Polygon2D");
		cpoly = GetNode<CollisionPolygon2D>("CollisionPolygon2D");
		shadows = GetNode<LightOccluder2D>("Shadow");
	}
	
	
	public override void _IntegrateForces( PhysicsDirectBodyState2D state ){
		if ( moveBody ){
			state.Transform = new Transform2D(0.0f, moveTarget);
			moveBody = false;
		}
	}
	
	public void MoveTo( Vector2 inPos ){
		moveBody = true;
		moveTarget = inPos;
	}
	
	public void setPolygon(Vector2[] inPoly){
		//GD.Print("Poly set?");
		poly.Polygon = inPoly;
		var offsetP = ( Geometry2D.OffsetPolygon( inPoly, -2 ) );
		Vector2[] p;
		if ( offsetP.Count > 0 ){
			p = offsetP[0];
		}else{
			p = new Vector2[1];
			p[0] = Vector2.Zero;
		}
		p = inPoly;
		CallDeferred( "setCPoly", p );
	}
	
	private void setCPoly( Vector2[] inPoly ){
		cpoly.Polygon = inPoly;
		shadows.Occluder.Polygon = inPoly;
	}
	
	public void DoDeformDebris( Vector2[] inPoly, Vector2 polyPosition ){
		
		GD.Print("==================\ndeform debris\n=================\n\n");
		Vector2 bulletOffset = new Vector2( 4, 4 );
		String inpolyStr = "";
		for ( var i = 0; i<inPoly.Length; i++ ){
			inPoly[i] = inPoly[i] + polyPosition - GlobalPosition - bulletOffset;
			inpolyStr += inPoly[i]+",";
		}
		
		Vector2[] dpoly = poly.Polygon;
		Vector2 origin = dpoly[0];

		String dpolyStr = "";
		//bulletOffset.Y = GlobalPosition
		
//		GD.Print( CalculateMidPoint( dpoly ) );
//		GD.Print( "COM:" + ToLocal( CenterOfMass ) );
		origin = Position;// ToLocal( CenterOfMass );
		for ( var i = 0; i<dpoly.Length; i++ ){
			dpolyStr += dpoly[i]+",";
			dpoly[i] = CalculateRotatedPoint( dpoly[i] );
		}
		
		var p = Geometry2D.ClipPolygons(dpoly, inPoly);
		var minSize = 300;
			
		//GD.Print( p.Count );
		//GD.Print( "D: "+dpolyStr+"\n" );
		//GD.Print( "I: "+inpolyStr+"\n" );
		GD.Print( "GPos: "+GlobalPosition );	
		GD.Print( "LPos: "+Position );	
		
		//GD.Print( dpoly[0] );
		//GD.Print( inPoly[0] );
		// for each other value in p, create a new enviro object
		//GD.Print( GetPolygonSize( p[0] ) );
		
		Boolean primePolySet = false;
		int DebrisCount = 0;
		
		for(var i=0; i<p.Count; i++){
			// in the event the new poly is the "hole", don't make Debris.
			// if the poly is below a certain size, make nothing (or SmallDebris TODO)						
			float pSize = GetPolygonSize( p[i] );
						
			if ( !Geometry2D.IsPolygonClockwise(p[i]) ){
				// create Debris
				if ( pSize > minSize ){
					for ( var j=0; j<p[i].Length; j++ ){
						//p[i][j] = CalculateRotatedPoint( p[i][j], true );
					}
					if ( primePolySet ){	
						EmitSignal(SignalName.CreateDebris, SimplifyPolygon(p[i]), GlobalPosition + new Vector2(0,-500));
					}else{
						dpoly = SimplifyPolygon(p[i]);
						primePolySet = true;
					}
					DebrisCount++;
				} else {
					// Destroy debris
					
				}
			}
		}
		
		if ( DebrisCount == 0 ){
			QueueFree();
		}
		
		dpolyStr = "";
		for ( var i = 0; i<dpoly.Length; i++ ){
			//dpoly[i] += new Vector2(0,600);
			dpoly[i] = CalculateRotatedPoint( dpoly[i], true );
			dpolyStr += dpoly[i]+",";
		}
		GD.Print("dpoly: "+dpoly[0]);
		//GD.Print( dpolyStr );
		setPolygon(dpoly);
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

	private Vector2 CalculateRotatedPoint( Vector2 point, Boolean reverse = false){
		Vector2 a = new Vector2(0,0);
		float r = (reverse) ? -Rotation : Rotation;
		a.X = (point.X * Mathf.Cos(r)) - (point.Y * Mathf.Sin(r));
		a.Y = (point.Y * Mathf.Cos(r)) + (point.X * Mathf.Sin(r));
		return a;
	}
	
	private Vector2 CalculateMidPoint( Vector2[] poly ){
		Vector2 origin = new Vector2(0,0);
		for ( var i=0; i<poly.Length; i++ ){
			origin += poly[i];
		}
		return origin/poly.Length;
	}
	
	// EVENT HANDLERS
	
	private void _on_body_entered(Node body)
	{
		//GD.Print( body.Name );
		//GD.Print( "break" );
		// Replace with function body.
	}
	
	private Vector2[] SimplifyPolygon ( Vector2[] inPoly ){		
		Vector2[] newPoly = inPoly;
		float minDistance = 5;
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
	//		GD.Print("inPoly["+i+"]: "+inPoly[i]);
			inPolyString += inPoly[i];
			if ( !cPointSet ){
				//GD.Print("Set CPOINT");
				cPoint = inPoly[i];
				cPointSet = true;
				points++;
				pointDist = 1;
				
				//GD.Print( "cPoint: "+cPoint );
			}else{
				float d = inPoly[i].DistanceTo(cPoint);

				estPoint += inPoly[i];			
				if ( d > minDistance || inPoly[i].Y >= 300.0f ){
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
				if ( i == inPoly.Length-1 || inPoly[i].Y > 295.0f ){
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
		//GD.Print( "original: "+inPolyString );
		//GD.Print( "simplified: "+outputString );
		//GD.Print("------------");
		return nPOutput;
	}	
}


