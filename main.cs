using Godot;
using System;

public partial class main : Node2D
{
	[Export]
	private float camYBoundary {get;set;} = -300.0f;
	[Export]
	public PackedScene Bullets {get;set;}
	

	
	private Node2D cam;
	private Camera2D cam2D;
	private Vector2 camMotion;
	private MapGenerator map;
	private player pl;
	
	
	public override void _Ready()
	{ 
		cam = GetNode<Node2D>("View");
		cam2D = GetNode<Camera2D>("View/Camera2D");
		map = GetNode<MapGenerator>("MapGenerator");
		pl = GetNode<player>("Player");
	}
	
	public override void _Process(double delta)
	{	
		checkPlayerBoundary();

		//cameraMouseMove();
		cameraFollowPlayer();

		if ( Input.IsActionJustPressed("restart") ){
			map.GenerateMap();
		}
	}
	
	private void cameraFollowPlayer(){
		// Zoom based off player speed;
		float adjSpeed = pl.speed;
		if (adjSpeed > 200){ adjSpeed = 200; };
		float zoomAmt = 1; //3 - (2*(adjSpeed/200));
		//cam2D.Zoom = new Vector2( zoomAmt, zoomAmt );
		
		// Movement based off player position;
		cam2D.Transform = new Transform2D( cam2D.Transform.Rotation, new Vector2( pl.Position.X, pl.Position.Y )); //+ (camMotion.Y*4)) );
		
		float boundaryY = 1130 - GetViewport().GetVisibleRect().Size.Y;//480;
		//GD.Print(GetViewport().GetVisibleRect().Size.Y);
		float boundaryOffset = GetViewport().GetVisibleRect().Size.Y/2 - (GetViewport().GetVisibleRect().Size.Y/2/zoomAmt);
		boundaryY += boundaryOffset;		
		//GD.Print( boundaryOffset );	
		if (cam2D.Transform.Origin.Y > boundaryY){
			cam2D.Transform = new Transform2D ( cam2D.Transform.Rotation, new Vector2(  cam2D.Transform.Origin.X, boundaryY ) );
		}		
	}
	
	private void checkPlayerBoundary(){
		//GD.Print("Boundary: "+pl.Position.X + " : " + map.fullWidth );
		if ( pl.Position.X > map.fullWidth+2000 ){
			pl.moveBodyTo( new Vector2(-2000, pl.Position.Y));
		}else if ( pl.Position.X < -2000 ){
			pl.moveBodyTo( new Vector2(map.fullWidth+2000, pl.Position.Y));
		}
		
		if ( pl.Position.Y > 800 ){
			pl.moveBodyTo( new Vector2(pl.Position.X, 800) );
			pl.stopYForce();
		}
	}
	
	private void cameraMouseMove(){
		cam.Position += camMotion;
		if ( cam.Position.Y > 100 ){
			cam.Position = new Vector2 ( cam.Position.X, 100 );
		}else if ( cam.Position.Y < camYBoundary ){
			cam.Position = new Vector2 ( cam.Position.X, camYBoundary );
		}
		
		if ( cam.Position.X < 0 ){
			cam.Position = new Vector2 ( 0, cam.Position.Y );
		}else if ( cam.Position.X > map.fullWidth ){
			cam.Position = new Vector2 ( map.fullWidth, cam.Position.Y );
		}		
	}
	
	public override void _Input(InputEvent @event)
	{
		// Mouse in viewport coordinates.
		if (@event is InputEventMouseButton eventMouseButton){
			//GD.Print("Mouse Click/Unclick at: ", eventMouseButton.Position);
		}else if (@event is InputEventMouseMotion eventMouseMotion){
			//GD.Print("Mouse Motion at: ", eventMouseMotion.Position);
			
			camMotion = new Vector2 (0,0);
			if ( eventMouseMotion.Position.X > GetViewport().GetVisibleRect().Size.X - 400 ){
				camMotion.X	+= (400 - (GetViewport().GetVisibleRect().Size.X - eventMouseMotion.Position.X))/10;
			}else if ( eventMouseMotion.Position.X < 400 ){
				camMotion.X -= (400 - eventMouseMotion.Position.X)/10;
			}
			if ( eventMouseMotion.Position.Y > GetViewport().GetVisibleRect().Size.Y - 300 ){
				camMotion.Y	+= (300 - (GetViewport().GetVisibleRect().Size.Y - eventMouseMotion.Position.Y))/10;
			}else if ( eventMouseMotion.Position.Y < 300 ){
				camMotion.Y -= (300 - eventMouseMotion.Position.Y)/10;
			}
			if ( cam.Position.Y > 300 && camMotion.Y > 0 ){
				camMotion.Y = 0;
			}
			//GD.Print( camMotion );
			//cam.Position = eventMouseMotion.Position - (GetViewport().GetVisibleRect().Size/2);
		}
		// Print the size of the viewport.
		//GD.Print("Viewport Resolution is: ", GetViewport().GetVisibleRect().Size);
	}
	
	private void _on_player_create_projectile( Vector2 direction )
	{
		//GD.Print("Bullet! "+direction);
		bullet b = Bullets.Instantiate<bullet>();
		b.Position = pl.Position;
		//b.Rotation = Mathf.Pi/4;
		
		if ( direction.X == -1000 && direction.Y == -1000 ){
			b.Rotation = pl.Rotation;
		}else{
			
			var l = pl.GetGlobalTransformWithCanvas().Origin;
			var d = (direction - l).Normalized();
	//		var h = 0; //Vector2.Right.Angle();
			
			b.Rotation = d.Angle() + (Mathf.Pi/2);// - h;
		}
		b._ownerVelocity = pl.LinearVelocity;
		AddChild(b);
		// Replace with function body.
	}

}
