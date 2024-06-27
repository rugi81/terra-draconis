using Godot;
using System;

public partial class MapGenerator : Node2D
{
	[Export]
	public int map_size { get; set; } = 3;
	[Export]
	public Vector2 start_position { get; set; } = new Vector2(0,0);
	[Export]
	public PackedScene TerrainScene { get; set; }
	[Export]
	public PackedScene LiquidScene { get; set; }
	[Export]
	private string[] map_types;
	[Export]
	public PackedScene Debriss {get;set;}
	 
	private Terrain[] map;
	private Vector2 map_draw_position;
	public float fullWidth;
	private string previous_terrain_type;
		
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GenerateMap();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public void GenerateMap(){
		ClearMap();
		//GD.Print("Generate Map()");
		
				
		map = new Terrain[map_size];
		string terrain_type;
		string previous_terrain_type = "";
		map_draw_position = start_position;
		fullWidth = 0;
		
		for ( int i = 0; i < map_size; i++ ){
			if ( i == 0 ){
				terrain_type = "ocean_left";
			} else if ( i == map_size-1 ){
				terrain_type = "ocean_right";
			} else if ( previous_terrain_type == "mountain_incline") {
				terrain_type = "mountain_decline";
			} else{
				terrain_type = map_types[GD.RandRange(0,map_types.Length-1)];
			}
			map[i] = CreateChunk( map_draw_position, terrain_type );
			map_draw_position.X += map[i].width;
			map_draw_position.Y = map[i].lastY;
			previous_terrain_type = terrain_type;
			
			if ( i < map_size-1 ){ fullWidth = map_draw_position.X; }
		}
		
		//MergeTerrain();
		
		// generate water bodies
		map_draw_position = new Vector2( start_position.X-3000, start_position.Y );
		CreateWaterBody( map_draw_position );
	}
	
	private void MergeTerrain(){
		Terrain mainTerrain = map[0];
		for ( int i = 1; i < map_size; i++ ){
			mainTerrain.MergeTerrain( map[i] );
		}
	}
	
	public void MergeAdjacentChunk( int tIndex, Boolean mergeRight ){
		Terrain mainTerrain = map[tIndex];
		int a = (mergeRight)?1:-1;
		mainTerrain.MergeTerrain( map[tIndex+a] );
	}
	
	private Terrain CreateChunk( Vector2 terrain_position, string terrain_type = "flat" ){
		Terrain chunk = TerrainScene.Instantiate<Terrain>();
		AddChild(chunk);
		chunk.BuildTerrain( terrain_position, terrain_type );
		chunk.CreateDebris += CreateNewDebris;
		chunk.CreateTerrain += CreateNewTerrain;
		return chunk;
	}
	
	private void ClearMap(){
		GetTree().CallGroup("terrain", Node.MethodName.QueueFree);
		GetTree().CallGroup("liquid", Node.MethodName.QueueFree);
	}
	
	private liquid CreateWaterBody( Vector2 body_position ){
		liquid body = LiquidScene.Instantiate<liquid>();
		AddChild(body);
		MoveChild(body, 0);
		body.BuildLiquid( body_position, fullWidth + 8000 );
		return body;
	}
	
	public void CreateNewDebris( Vector2[] inPoly, Vector2 inPos ){
		//GD.Print("DEBRIS");
		
		Debris d = Debriss.Instantiate<Debris>();
		AddChild(d);
		
		GD.Print("Creation Pos: "+d.Position);
		d.MoveTo( inPos + new Vector2(0,500) );
		d.Position = inPos;
		GD.Print("New Pos: "+d.Position);
		d.setPolygon( inPoly );
		d.CreateDebris += CreateNewDebris;
//		d.Position = new Vector2( d.Position.X, 500 );
		
		GD.Print( inPoly[0] +"\n\n");
//		for ( )
	}
	
	public void CreateNewTerrain( Vector2[] inPoly ){
		CallDeferred("DeferredNewTerrain", inPoly);
	}
	
	private void DeferredNewTerrain( Vector2[] inPoly ){
		GD.Print("DeferredNewTerrain:" + inPoly.Length);
		Terrain newChunk = TerrainScene.Instantiate<Terrain>();
		AddChild(newChunk);
		newChunk.SetPolygon( inPoly );
		newChunk.CreateDebris += CreateNewDebris;
		newChunk.CreateTerrain += CreateNewTerrain;
	}	
}


