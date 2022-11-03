
[Library( "js_start", Description = "Start Level" )]
[EditorSprite( "materials/editor/assault_rally.vmat" )]
[Display( Name = "Start Level", GroupName = "Jumper", Description = "Start Level" ), Category( "Gameplay" ), Icon( "grass" )]
[HammerEntity]
partial class StartPoint : Entity
{
	public override void ClientSpawn()
	{
		base.ClientSpawn();

		Transmit = TransmitType.Always;
	}
}
