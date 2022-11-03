
[Library( "js_start", Description = "Start Level" )]
[EditorModel( "models/editor/startfinish/start.vmdl" )]
[Display( Name = "Start Level", GroupName = "Jumper", Description = "Start Level" ), Category( "Gameplay" ), Icon( "vertical_align_bottom" )]
[HammerEntity]
partial class StartPoint : Entity
{
	public override void ClientSpawn()
	{
		base.ClientSpawn();

		Transmit = TransmitType.Always;
	}
}
