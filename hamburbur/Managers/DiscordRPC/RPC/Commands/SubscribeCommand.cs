using hamburbur.Managers.DiscordRPC.RPC.Payload;

namespace hamburbur.Managers.DiscordRPC.RPC.Commands;

internal class SubscribeCommand : ICommand
{
    public ServerEvent Event         { get; set; }
    public bool        IsUnsubscribe { get; set; }

    public IPayload PreparePayload(long nonce) =>
            new EventPayload(nonce)
            {
                    Command = IsUnsubscribe ? Command.Unsubscribe : Command.Subscribe,
                    Event   = Event,
            };
}