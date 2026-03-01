using hamburbur.Managers.DiscordRPC.RPC.Payload;

namespace hamburbur.Managers.DiscordRPC.RPC.Commands;

internal interface ICommand
{
    IPayload PreparePayload(long nonce);
}