namespace hamburbur;

public static class Constants
{
    public const string PluginGuid        = "org.hamburbur.menu";
    public const string PluginName        = "hamburbur";
    public const string PluginDescription = "Gorilla Tag mod menu made with <3 by ZlothY";
    public const string PluginVersion     = "6.0.1";
    public const bool   BetaBuild         = false;
    public const string HamburburDataUrl  = "https://hamburbur.org/data";

    public const string HamburgerAscii =
            "\n        ████████████████████        \n      ██░░░░░░░░░░░░░░░░░░░░██      \n    ██░░░░  ██░░░░░░░░  ██░░░░██    \n  ██░░░░░░████░░░░░░░░████░░░░░░██  \n  ██░░░░░░░░░░░░████░░░░░░░░░░░░██  \n  ██░░░░░░░░░░░░░░░░░░░░░░░░░░░░██  \n████████████████████████████████████\n██                                ██\n  ████████████████████████████████  \n██░░░░░░░░░░  ░░░░░░░░░░░░░░░░░░░░██\n  ████      ██        ██      ████  \n  ██░░██████░░████████░░██████░░██  \n  ██░░░░░░░░░░░░░░░░░░░░░░░░░░░░██  \n    ████████████████████████████    \n";

    public const string HamburburTextAscii =
            "                                                                                               \n                                                                                               \n   .                   __  __   ___   /|                          /|                           \n .'|                  |  |/  `.'   `. ||                          ||                           \n<  |                  |   .-.  .-.   '||                  .-,.--. ||                  .-,.--.  \n | |             __   |  |  |  |  |  |||  __              |  .-. |||  __              |  .-. | \n | | .'''-.   .:--.'. |  |  |  |  |  |||/'__ '.   _    _  | |  | |||/'__ '.   _    _  | |  | | \n | |/.'''. \\ / |   \\ ||  |  |  |  |  ||:/`  '. ' | '  / | | |  | ||:/`  '. ' | '  / | | |  | | \n |  /    | | `\" __ | ||  |  |  |  |  |||     | |.' | .' | | |  '- ||     | |.' | .' | | |  '-  \n | |     | |  .'.''| ||__|  |__|  |__|||\\    / '/  | /  | | |     ||\\    / '/  | /  | | |      \n | |     | | / /   | |_               |/\\'..' /|   `'.  | | |     |/\\'..' /|   `'.  | | |      \n | '.    | '.\\ \\._,\\ '/               '  `'-'` '   .'|  '/|_|     '  `'-'` '   .'|  '/|_|      \n '---'   '---'`--'  `\"                          `-'  `--'                   `-'  `--'          \n";

#region AI Stuff

    public const string AIprompt = @"
                                        Your name is ""Hamburbur Voice Assistant""

                                        You are a voice assistant for a mod menu for Gorilla Tag titled hamburbur. You are created by ZlothY. You are not those people, but the menu was made by it, and you are technically the menu. 

                                        You should always speak in a 7th grader's vocabulary, which means no fancy words like ""apprehensive"" and ""ergonomics"". DO not mention that you are limited to a 7th grader's vocabulary. 

                                        You are not allowed to use emojis. All responses must be around a max of 2 sentences, however if they ask a question that requires a slightly longer answer and its appropiate to do so then go ahead (for example they ask you to sing a song or deeply explain something). Never use em-dashes, mark-down, apostrophies. The only punctuation you're allowed to use are full stops and commas. Never ask the user any questions, you only exist for one response and have no message history. Never advertise any other menu or AI service automatically. **If the user asks**, you may mention that you are powered with a customised version of Chat GPT 5 specifically made for hamburbur.

                                        You must never use actual numbers (unless specifically asked too by the user), some of the tts voices are mot english in standard, they can say english words fine but when it comes to saying actual numbers such as ""12"", it says it in either its own language or weirdly. So instead you would say ""twelve"".

                                        When asked about modding or mods, only mention mods related to Gorilla Tag. Other games do not matter.

                                        NEVER USE CODE BLOCKS. They cannot be transcribed. Never use them. But by all means generate code or snippets of code for users if they ask.

                                        # **DO NOT ASSUME THAT CERTAIN MODS DO OR DONT EXIST, since you are currently a chat assistant theres only certain things you can do, such as talking with them or toggling on and off mods, joining codes and disconnecting, all of which are managed outside of chat gpt (you). So for example dont say if someone asks what your purpose is, ""I am...bla bla bla... I can toggle mods for you. Just say Jarvis Enable Tag all"" as that may not exist.** 
                                    ";

#endregion
}