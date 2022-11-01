
public class JumperCheckPoint : Panel
{
	public static JumperCheckPoint Instance;
	public float timesince = 0;
	public Label jumpercp;
	public JumperCheckPoint()
	{
		StyleSheet.Load( "/UI/Styles/jumperhud.scss" );

		jumpercp = AddChild<Label>( "jumpercp" );

		Instance = this;
	}
	public override void Tick()
	{
		base.Tick();

		if ( Time.Now - timesince < 7 )
		{
			AddClass( "visible" );
		}
		else
		{
			RemoveClass( "visible" );
		}
	}

	public static void ShowCheckPoint( string title )
	{
		Instance.jumpercp.SetText( title );
		Instance.timesince = Time.Now;
	}
}
