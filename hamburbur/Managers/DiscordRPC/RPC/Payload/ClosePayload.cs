using Newtonsoft.Json;

namespace hamburbur.Managers.DiscordRPC.RPC.Payload;

internal class ClosePayload : IPayload
{
    [JsonConstructor]
    public ClosePayload()
    {
        Code   = -1;
        Reason = "";
    }

	/// <summary>
	///     The close code the discord gave us
	/// </summary>
	[JsonProperty("code")]
    public int Code { get; set; }

	/// <summary>
	///     The close reason discord gave us
	/// </summary>
	[JsonProperty("message")]
    public string Reason { get; set; }
}