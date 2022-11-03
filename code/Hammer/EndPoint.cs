
[Library( "js_end", Description = "End Level" )]
[EditorModel( "models/editor/startfinish/finish.vmdl" )]
[Display( Name = "End Level", GroupName = "Jumper", Description = "End Level" ), Category( "Gameplay" ), Icon( "vertical_align_top" )]
[HammerEntity]
partial class EndPoint : Entity
{
	public override void ClientSpawn()
	{
		base.ClientSpawn();

		Transmit = TransmitType.Always;
	}
}
