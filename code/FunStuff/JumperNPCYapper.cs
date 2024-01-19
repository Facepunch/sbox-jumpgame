[GameResource( "NPC TEXT", "npct", "Npc_text", Icon = "sentiment_very_satisfied", IconBgColor = "#fcba03", IconFgColor = "#363324" )]
public class NPCTextGameResource : GameResource
{
	[Property]
	[Title( "Name" )]
	[Category( "Name" )]
	public String NPCName { get; set; }

	[Property]
	[Title( "Text" )]
	[Category( "Text" )]
	public List<String> NPCText { get; set; } = new();

	[Property, ResourceType( "sound" )]
	[Title( "Voice" )]
	[Category( "Voice" )]
	public string NPCVoice { get; set; }
}
