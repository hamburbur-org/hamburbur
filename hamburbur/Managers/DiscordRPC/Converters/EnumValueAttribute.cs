using System;

namespace hamburbur.Managers.DiscordRPC.Converters;

internal class EnumValueAttribute : Attribute
{
    public EnumValueAttribute(string value) => Value = value;
    public string Value { get; set; }
}