using System;

namespace hamburbur.Managers.DiscordRPC.Message;

/// <summary>
///     The connection to the discord client was succesfull. This is called before <see cref="MessageType.Ready" />.
/// </summary>
public class ConnectionEstablishedMessage : IMessage
{
	/// <summary>
	///     The type of message received from discord
	/// </summary>
	public override MessageType Type => MessageType.ConnectionEstablished;

	/// <summary>
	///     The pipe we ended up connecting too
	/// </summary>
	[Obsolete("The connected pipe is not neccessary information.")]
    public int ConnectedPipe { get; internal set; }
}