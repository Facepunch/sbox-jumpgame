

[Library( "js_ui_overlay", Description = "UI Overlay" )]
[Display( Name = "UI Overlay", GroupName = "Jumper", Description = "UI Overlay" ), Category( "Gameplay" )]
[HammerEntity]
public partial class UiOverlayTrigger : BaseTrigger
{

	[Net, Property]
	public string TopEntity { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;

		EnableTouchPersists = true;
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( !Game.IsClient ) return;
		if ( other is not JumperPawn p ) return;
		if ( !p.IsLocalPawn ) return;

		if ( !p.ReachedEnd )
		{
			p.AtEnding();
		}

		p.EnableDrawing = false;

		foreach ( var child in p.Children )
		{
			if ( child is not ModelEntity m || !child.IsValid() ) continue;
			m.EnableDrawing = false;
		}
		
		var overlayui = Game.RootPanel.ChildrenOfType<OverlayUI>()?.FirstOrDefault();
		if ( overlayui != null )
		{
			overlayui.Open = true;
		}
	}

	public override void EndTouch( Entity other )
	{
		base.EndTouch( other );

		if ( !Game.IsClient ) return;
		if ( other is not JumperPawn p ) return;

		p.EnableDrawing = true;

		foreach ( var child in p.Children )
		{
			if ( child is not ModelEntity m || !child.IsValid() ) continue;
			m.EnableDrawing = true;
		}

		var overlayui = Game.RootPanel.ChildrenOfType<OverlayUI>()?.FirstOrDefault();
		if ( overlayui != null )
		{
			overlayui.Open = false;
		}
	}

}
