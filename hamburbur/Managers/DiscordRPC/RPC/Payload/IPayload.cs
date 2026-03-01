using hamburbur.Managers.DiscordRPC.Converters;
using Newtonsoft.Json;

namespace hamburbur.Managers.DiscordRPC.RPC.Payload;

/// <summary>
///     Base Payload that is received by both client and server
/// </summary>
internal abstract class IPayload
{
    protected IPayload() { }
    protected IPayload(long nonce) => Nonce = nonce.ToString();

	/// <summary>
	///     The type of payload
	/// </summary>
	[JsonProperty("cmd")]
    [JsonConverter(typeof(EnumSnakeCaseConverter))]
    public Command Command { get; set; }

	/// <summary>
	///     A incremental value to help identify payloads
	/// </summary>
	[JsonProperty("nonce")]
    public string Nonce { get; set; }

    public override string ToString() => $"Payload || Command: {Command}, Nonce: {Nonce}";
}