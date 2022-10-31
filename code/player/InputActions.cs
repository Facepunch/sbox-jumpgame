using Sandbox;

public enum InputActions
{
	None,
	Forward,
	Back,
	Left,
	Right,
	//Pedal,
	Move,
	Walk,
	Jump,
	Duck,
	Look,
	JumpHigher,
	Kill,
	Use,
	LeftClick,
	RightClick,
	Spray,
	Menu
}

public static class InputActionsExtensions
{

	public static bool Pressed( this InputActions action ) => Input.Pressed( GetInputButton( action ) );
	public static bool Released( this InputActions action ) => Input.Released( GetInputButton( action ) );
	public static bool Down( this InputActions action ) => Input.Down( GetInputButton( action ) );
	public static string GetButtonOrigin( this InputActions action ) => Input.GetButtonOrigin( GetInputButton( action ) );
	public static InputButton Button( this InputActions action ) => GetInputButton( action );

	public static InputButton GetInputButton( InputActions action )
	{
		if ( Input.UsingController )
		{
			return action switch
			{
				InputActions.Forward => InputButton.Forward,
				InputActions.Back => InputButton.Back,
				InputActions.Left => InputButton.Left,
				InputActions.Right => InputButton.Right,
				InputActions.Walk => InputButton.Reload,
				InputActions.Jump => InputButton.Jump,
				InputActions.Duck => InputButton.SecondaryAttack,
				InputActions.Kill => InputButton.Menu,
				InputActions.LeftClick => InputButton.Reload,
				InputActions.RightClick => InputButton.SlotNext,
				InputActions.Spray => InputButton.Flashlight,
				InputActions.Menu => InputButton.SlotNext,
				InputActions.Use => InputButton.Use,
				_ => default
			};
		}

		return action switch
		{
			InputActions.Forward => InputButton.Forward,
			InputActions.Back => InputButton.Back,
			InputActions.Left => InputButton.Left,
			InputActions.Right => InputButton.Right,
			InputActions.Walk => InputButton.Run,
			InputActions.Jump => InputButton.Jump,
			InputActions.Duck => InputButton.Duck,
			InputActions.Kill => InputButton.Reload,
			InputActions.LeftClick => InputButton.PrimaryAttack,
			InputActions.RightClick => InputButton.SecondaryAttack,
			InputActions.Spray => InputButton.Flashlight,
			InputActions.Menu => InputButton.Menu,
			InputActions.Use => InputButton.Use,
			_ => default
		};
	}

}
