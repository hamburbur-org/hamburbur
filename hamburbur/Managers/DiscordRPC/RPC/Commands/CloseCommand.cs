using hamburbur.Managers.DiscordRPC.RPC.Payload;
using Newtonsoft.Json;

namespace hamburbur.Managers.DiscordRPC.RPC.Commands;

internal class CloseCommand : ICommand
{
	/// <summary>
	///     The rich presence to be set. Can be null.
	/// </summary>
	[JsonProperty("close_reason")]
    public string value = "Unity 5.5 doesn't handle thread aborts. Can you please close me discord?";

	/// <summary>
	///     The process ID
	/// </summary>
	[JsonProperty("pid")]
    public int PID { get; set; }

    public IPayload PreparePayload(long nonce) =>
            new ArgumentPayload
            {
                    Command   = Command.Dispatch,
                    Nonce     = null,
                    Arguments = null,
            };
}