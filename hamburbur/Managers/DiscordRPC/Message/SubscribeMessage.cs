using hamburbur.Managers.DiscordRPC.RPC.Payload;

namespace hamburbur.Managers.DiscordRPC.Message;

/// <summary>
///     Called as validation of a subscribe
/// </summary>
public class SubscribeMessage : IMessage
{
    internal SubscribeMessage(ServerEvent evt)
    {
        switch (evt)
        {
            default:
            case ServerEvent.ActivityJoin:
                Event = EventType.Join;

                break;

            case ServerEvent.ActivityJoinRequest:
                Event = EventType.JoinRequest;

                break;
        }
    }

	/// <summary>
	///     The type of message received from discord
	/// </summary>
	public override MessageType Type => MessageType.Subscribe;

	/// <summary>
	///     The event that was subscribed too.
	/// </summary>
	public EventType Event { get; internal set; }
}