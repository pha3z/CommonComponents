using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public enum UserCmd : byte
    {
        GOTO_ROOM,
        FONT_SIZE,
        UNRECOGNIZED,
        BAD_ARGUMENT
    }

    public struct ParsedCommand
    {
        public UserCmd Cmd;
        public IEnumerable<string> args;

        /// <summary>
        /// If Cmd indicates a bad argument was given, this string will contain information about it.
        /// </summary>
        public string BadArgMessage;
    }

    /// <summary>
    /// This parser was originally thrown together quickly for an early prototype of the MUD Editor
    /// It really isn't very flexible because of the use of hardcoded switch/enum pattern
    /// A better system would probably be a dynamic system
    /// Command handler methods could be registered with the parser along with the command that should invoke it
    /// This would allow different modules to define their own commands and handlers
    /// What IS nice about this implementation is that it demonstrates argument tokens.
    /// You could use the same thing in a dynamic parser.
    /// What I also don't like is the fact that it depends on IEnumerable and passes a ParsedCommand struct
    /// It would be simpler just to pass all the tokens as an array
    /// Command handlers can ignore the first token (the command token) internally.
    /// This gives them the option of actually READING the command token if they want
    /// That would allow one handler to manage multiple commands
    /// Its just more flexible and elegant
    /// </summary>
    public class SimpleCmdParser
    {
        public ParsedCommand Parse(string text)
        {
            text = text.ToUpper();
            var tokens = text.Split(' ');

            ParsedCommand cmd = new ParsedCommand();
            cmd.args = tokens.Skip(1);

            bool check;

            switch (tokens[0])
            {
                case "GOTO":
                    UInt32 rmId;
                    check = UInt32.TryParse(tokens[1], out rmId);
                    if (check)
                        cmd.Cmd = UserCmd.GOTO_ROOM;
                    else
                    {
                        cmd.Cmd = UserCmd.BAD_ARGUMENT;
                        cmd.BadArgMessage = "GOTO requires valid RoomId (arg1 must be UInt32)";
                    }
                    break;

                case "FONTSIZE":
                    UInt32 fSize;
                    check = UInt32.TryParse(tokens[1], out fSize);
                    if (check)
                        cmd.Cmd = UserCmd.FONT_SIZE;
                    else
                    {
                        cmd.Cmd = UserCmd.BAD_ARGUMENT;
                        cmd.BadArgMessage = "FONTSIZE requires positive number (arg1 must be UInt32)";
                    }
                    break;


                default:
                    cmd.Cmd = UserCmd.UNRECOGNIZED;
                    break;
            }

            return cmd;
        }
    }
}
