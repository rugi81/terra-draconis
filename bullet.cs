using Godot;
using System;

public partial class bullet : RigidBody2D
{
	private Vector2 _thrust = new Vector2(0, -5.0f);
	private Boolean initialFire = false;
	public Vector2 _ownerVelocity;
	public Node _owner;
	public Polygon2D poly;
	
	[Signal]
	public delegate void DeformTerrainEventHandler();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//GD.Print("Bullet!");
		poly = GetNode<Polygon2D>("Explosion");
		
		//GD.Print(poly);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
	
	public override void _IntegrateForces( PhysicsDirectBodyState2D state ){
		if ( !initialFire ){
			initialFire = true;
			//state.Transform = new Transform2D(Rotation, Position);
			state.LinearVelocity = _ownerVelocity;
			//state.AngularVelocity = 0;
			state.ApplyImpulse( _thrust.Rotated(Rotation) );
		}
	}
		
	private void _on_life_timer_timeout()
	{
		removeBullet();
		// Replace with function body.
	}
	
	private void removeBullet(){
		QueueFree();
	}
	
	private void _on_body_entered(Node body)
	{
		//GD.Print("BULLET HIT");
		//GD.Print( (Terrain) body.GetParent() );
		
		if (body.GetType().Name == "StaticBody2D"){
			Terrain t = (Terrain) body.GetParent();
			t.DoDeformTerrain( poly.Polygon, GlobalPosition );
			removeBullet();
		}
		else if (body.GetType().Name == "Debris"){
			Debris d = (Debris) body;
			d.DoDeformDebris( poly.Polygon, GlobalPosition );
			removeBullet();
		}
		// Replace with function body.
	}
}




