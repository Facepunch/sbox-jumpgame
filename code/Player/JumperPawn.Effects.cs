using Sandbox;

partial class JumperPawn
{
	TimeSince timeSinceLastFootstep = 0;
	Particles FakeShadowParticle;

	[Event.Client.Frame]
	private void UpdateRenderAlpha()
	{
		var dist = Camera.Position.Distance( Position );
		var a = 1f - dist.LerpInverse( MaxRenderDistanceSelf, MaxRenderDistanceSelf * .1f );
		a = Math.Max( a, .15f );
		a = Sandbox.Utility.Easing.EaseOut( a );

		RenderColor = RenderColor.WithAlpha( a );

		foreach ( var child in Children )
		{
			if ( child is not ModelEntity m || !child.IsValid() ) continue;
			m.RenderColor = m.RenderColor.WithAlpha( a );
		}
	}

	[Event.Client.Frame]
	private void UpdateRenderAlphaOthers()
	{
		if ( !Game.LocalPawn.IsValid() || Game.LocalPawn == this )
			return;

		var dist = Game.LocalPawn.Position.Distance( Position );
		var a = 1f - dist.LerpInverse( MaxRenderDistanceOther, MaxRenderDistanceOther * .1f );
		a = Math.Max( a, .15f );
		a = Sandbox.Utility.Easing.EaseOut( a );

		RenderColor = RenderColor.WithAlpha( a );

		foreach ( var child in Children )
		{
			if ( child is not ModelEntity m || !child.IsValid() ) continue;
			m.RenderColor = m.RenderColor.WithAlpha( a );
		}
	}
	
	[Event.Client.Frame]
	void UpdatePlayerShadow()
	{
		FakeShadowParticle ??= Particles.Create( "particles/player/fake_shadow/fake_shadow.vpcf" );

		var tr = Trace.Ray( Position, Position + Vector3.Down * 2000 )
			.WorldOnly()
			.Run();

		FakeShadowParticle.SetPosition( 0, tr.EndPosition );
	}
	
	public override void OnAnimEventFootstep( Vector3 pos, int foot, float volume )
	{
		if ( LifeState != LifeState.Alive ) return;
		if ( !Game.IsClient ) return;
		if ( timeSinceLastFootstep < 0.2f ) return;

		timeSinceLastFootstep = 0;

		var footletter = foot == 0 ? "l" : "r";
		var particle = Particles.Create( $"particles/player/footsteps/footstep_{footletter}.vpcf", pos );
		particle.SetOrientation( 0, Transform.Rotation );

		var tr = Trace.Ray( pos, pos + Vector3.Down * 20 )
			.Radius( 1 )
			.Ignore( this )
			.Run();

		if ( !tr.Hit ) return;

		tr.Surface.DoFootstep( this, tr, foot, volume * 2 );
	}
}
