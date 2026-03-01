using System;
using System.Text;
using hamburbur.Managers.DiscordRPC.Exceptions;
using Newtonsoft.Json;

namespace hamburbur.Managers.DiscordRPC;

/// <summary>
///     A Rich Presence button.
/// </summary>
public class Button
{
    private string _label;
    private string _url;

    /// <summary>
    ///     Text shown on the button
    ///     <para>Max 31 bytes.</para>
    /// </summary>
    [JsonProperty("label")]
    public string Label
    {
        get => _label;

        set
        {
            if (!BaseRichPresence.ValidateString(value, out _label, true, 31, Encoding.UTF8))
                throw new StringOutOfRangeException(31);
        }
    }

    /// <summary>
    ///     The URL opened when clicking the button.
    ///     <para>Max 512 characters.</para>
    /// </summary>
    [JsonProperty("url")]
    public string Url
    {
        get => _url;

        set
        {
            if (!BaseRichPresence.ValidateString(value, out _url, false, 512))
                throw new StringOutOfRangeException(512);

            if (!BaseRichPresence.ValidateUrl(_url))
                throw new ArgumentException("Url must be a valid URI");
        }
    }
}