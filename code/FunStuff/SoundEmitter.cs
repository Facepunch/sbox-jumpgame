using Sandbox;

public sealed class SoundEmitter : Component
{
	[Property] public string SoundString { get; set; }

	protected override void OnStart()
	{
		base.OnStart();

		Sound.Play( SoundString, Transform.Position );
	}
	protected override void OnUpdate()
	{

	}
}
