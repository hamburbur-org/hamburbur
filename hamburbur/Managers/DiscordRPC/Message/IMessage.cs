using System;

namespace hamburbur.Managers.DiscordRPC.Message;

/// <summary>
///     Messages received from discord.
/// </summary>
public abstract class IMessage
{
	/// <summary>
	///     Creates a new instance of the message
	/// </summary>
	public IMessage() => TimeCreated = DateTime.Now;

	/// <summary>
	///     The type of message received from discord
	/// </summary>
	public abstract MessageType Type { get; }

	/// <summary>
	///     The time the message was created
	/// </summary>
	public DateTime TimeCreated { get; }
}