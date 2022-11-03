
[Library( "js_end", Description = "End Level" )]
[EditorSprite( "materials/editor/assault_rally.vmat" )]
[Display( Name = "End Level", GroupName = "Jumper", Description = "End Level" ), Category( "Gameplay" ), Icon( "grass" )]
[HammerEntity]
partial class EndPoint : Entity
{
	public override void ClientSpawn()
	{
		base.ClientSpawn();

		Transmit = TransmitType.Always;
	}
}
