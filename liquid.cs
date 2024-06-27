using Godot;
using System;

public partial class liquid : Area2D
{
	[Export]
	private float waterDepth {get; set;} = 50;

	private Polygon2D poly;
	private CollisionPolygon2D cpoly;
	public float width;
	public float firstY; // Y value for first point
	public float lastY; // Y value for last point
	
	private Timer waveTimer;
	private float waveTime = 0;
	private float waterLevel;
	private int testCount = 0;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		poly = GetNode<Polygon2D>("Polygon2D");
		cpoly = GetNode<CollisionPolygon2D>("Shape");
		waveTimer = GetNode<Timer>("WaveTimer");
	}
		 
	public override void _Process(double delta)
	{
		AnimateLiquid();
	}	
		
	public void BuildLiquid( Vector2 start_position, float width_offset ){
		GD.Print( "BuildLiquid" + start_position );
		Vector2[] polyArr = GenerateWaterBody( start_position, width_offset );
		
		firstY = start_position.Y;
		poly.Polygon = polyArr;
		cpoly.Polygon = polyArr;
	}
	
	// water starts from outside the map... so. 0 and map.width, until it hits land.
	
	private Vector2[] GenerateWaterBody( Vector2 start_position, float width_offset = 500 ){
		Vector2[] polyArr = new Vector2[1]{ start_position };
		Vector2[] p2Arr;
		Vector2 oldPoint;
		Vector2 point;
		Vector2 direction = new Vector2( 1,0 );
		width = 0;
		string water_type = "";
		
		float segmentDistance = 50.0f;
		float segmentDepth = waterDepth;
		int numPoints = (int)(width_offset / segmentDistance);
		GD.Print( "liquid: "+width_offset+" / "+segmentDistance+ " = "+numPoints);
		
		for ( int i=0; i<numPoints; i++){
			
			oldPoint = polyArr[i];
			point = (segmentDistance * direction) + oldPoint;
			width += segmentDistance;

			//GD.Print( i+ "-- " +point );
			Vector2 altitudeAdjust = new Vector2(0,0);
			
			if ( i > 0 && i < numPoints-1 ){
				switch ( water_type ){
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
						altitudeAdjust.Y = GD.RandRange(-1,-1-i);//-5-(i/2));
						if ( i < numPoints/2 ){
							point.Y = segmentDepth - 1;
						}
						if ( point.Y < 0 ){
							point.Y = 0;
						}
						break;					
					case ("ocean_right"):
						var x = (segmentDepth-point.Y)/(numPoints-i) + (i/2); 
						GD.Print("ocean:"+x);
						altitudeAdjust.Y = (float) GD.RandRange(-5, x);//(i/2));
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
			p2Arr[j] = polyArr[j];
		}
		p2Arr[p2Arr.Length-1] = new Vector2 (start_position.X, segmentDepth);
		p2Arr[p2Arr.Length-2] = new Vector2 (p2Arr[p2Arr.Length-3].X, segmentDepth);
		polyArr = p2Arr;
		
		waterLevel = polyArr[0].Y - 10;
		
		return polyArr;
	}	
	
	private void AnimateLiquid(){
		Vector2[] polyArr = poly.Polygon;
		Vector2 point;
		float wave;
		float speed = 1;
		float height  = 5;
		String waveOutput = "";
		
		for  ( int i = 0; i < polyArr.Length-2; i++ ){
			//GD.Print( i + "-- " + polyArr[i] );
			point = polyArr[i];
			
			wave = MathF.Round( MathF.Sin( ((i%5)*waveTime*speed)*MathF.PI/180 ) * height, 1 );
			
			polyArr[i] = new Vector2( point.X, waterLevel - wave );
			waveOutput += wave+", ";
		}
		
		if ( testCount < 10 ){
			//GD.Print( waveOutput );
			testCount++;
		}
		poly.Polygon = polyArr;
	}
	
	private void _on_wave_timer_timeout()
	{
		waveTime++;
		// Replace with function body.
	}
}



