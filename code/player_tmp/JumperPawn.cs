
internal partial class JumperPawn : Sandbox.Player
{

	public const float MaxRenderDistance = 128f;
	public float Height { get; set; }

	[Net]
	public PropCarriable HeldBody { get; set; }

	public override void Respawn()
	{
		base.Respawn();

		SetModel( "models/citizen/citizen.vmdl" );

		Controller = new WalkController();
		Animator = new JumperAnimator();
		CameraMode = new JumperCamera();

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		if ( Client != null )
		{
			var clothing = new ClothingContainer();
			clothing.LoadFromClient( Client );
			clothing.DressEntity( this );
		}
	}

	public override void OnKilled()
	{
		base.OnKilled();

		Controller = null;
		EnableAllCollisions = false;
		EnableDrawing = false;

		CameraMode = new RagdollCamera();
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if ( Input.Pressed( InputButton.Drop ) || Input.Pressed( InputButton.Reload ) )
		{
			Game.Current.DoPlayerSuicide( cl );
		}
	}

	[Event.Frame]
	private void UpdateRenderAlpha()
	{
		if ( !Local.Pawn.IsValid() || Local.Pawn == this )
			return;

		var dist = Local.Pawn.Position.Distance( Position );
		var a = 1f - dist.LerpInverse( MaxRenderDistance, MaxRenderDistance * .1f );
		a = Math.Max( a, .15f );
		a = Easing.EaseOut( a );

		RenderColor = RenderColor.WithAlpha( a );

		foreach ( var child in Children )
		{
			if ( child is not ModelEntity m || !child.IsValid() ) continue;
			m.RenderColor = m.RenderColor.WithAlpha( a );
		}
	}

}
