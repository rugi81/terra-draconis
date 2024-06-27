using Godot;
using System;

public partial class player : RigidBody2D
{
	private Vector2 _thrust = new Vector2(0, -250);
	private float _torque = 1000;
	private AnimatedSprite2D boosterAnim;
	public Boolean resetState = false;
	public Vector2 moveTo;
	public float speed;
	private Boolean stopY = false;
		
	[Export]
	private float rateOfFire = .1f;
	private Boolean readyToFire = true;
	private Boolean weaponFired = false;
	private float fireTimer = 0;
	
	[Signal]
	public delegate void CreateProjectileEventHandler( Vector2 direction );
		
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		boosterAnim = GetNode<AnimatedSprite2D>("BoosterFlame");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if ( Input.IsActionPressed("fire") && readyToFire ){
			fireBullet();
		}
		if ( Input.IsActionPressed("mouse_click_left") && readyToFire  ){
			//GD.Print("Fire at: ", GetViewport().GetMousePosition());
			
			var x = GetGlobalTransformWithCanvas();
			//GD.Print(x.Origin);
			//GD.Print(GetViewport().GetMousePosition());
			fireBullet( GetViewport().GetMousePosition() );//( x.Origin - GetViewport().GetMousePosition()).Normalized()  );			
		}
		
		if ( weaponFired ){
			fireTimer += (float)delta;
		}
		
		if ( fireTimer > rateOfFire ){
			weaponFired = false;
			readyToFire = true;
			fireTimer = 0;
		}
	}
	
	public void moveBodyTo( Vector2 position ){
		resetState = true;
		moveTo = position;
	}
	
	public void stopYForce(){
		stopY = true;
	}
	
	public override void _IntegrateForces( PhysicsDirectBodyState2D state ){
		//GD.Print(state.LinearVelocity.Length());
		speed = state.LinearVelocity.Length();
		
		if (resetState){
			state.Transform = new Transform2D(Rotation, moveTo);	
			resetState = false;
		}
		if (stopY){
			state.LinearVelocity = new Vector2(state.LinearVelocity.X,0);
			stopY = false;
		}
		
		if (Input.IsActionPressed("booster")){
			boosterAnim.Visible = true;
			boosterAnim.Play("boost");
			state.ApplyForce(_thrust.Rotated(Rotation));
		}else{
			boosterAnim.Visible = false;
			state.ApplyForce(new Vector2());
		}
		
		var rotationDir = 0;
		if (Input.IsActionPressed("rotate_right"))
			rotationDir += 1;
		if (Input.IsActionPressed("rotate_left"))
			rotationDir -= 1;
		state.ApplyTorque(rotationDir * _torque);
	}

			
	private void fireBullet( Vector2 direction ){
		readyToFire = false;
		weaponFired = true;
		fireTimer = 0;
	
		EmitSignal(SignalName.CreateProjectile, direction );	
	}
	
	private void fireBullet(){
		//GD.Print("FIre!");
		//GD.Print(Position);
		//GD.Print(Rotation);
		
		readyToFire = false;
		weaponFired = true;
		fireTimer = 0;
	
		EmitSignal(SignalName.CreateProjectile, new Vector2(-1000,-1000) );
	}
	
}


