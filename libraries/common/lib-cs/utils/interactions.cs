using System;
using System.Reflection;
using System.Resources;
using System.Threading;

namespace SpringCard.LibCs
{
	public enum InteractionType
	{
		Fatal,
		Error,
		Warning,
		Info
	};
	
	public class Interaction
	{
        public static bool ToConsole;

        InteractionType type;
        string message;
        object[] args;

        public Interaction(InteractionType type, string message, params object[] args)
        {
            this.type = type;
            this.message = message;
            this.args = args;

            if (ToConsole)
                WriteToConsole();
        }

        public Interaction(string message, params object[] args)
        {
            this.type = InteractionType.Info;
            this.message = message;
            this.args = args;

            if (ToConsole)
                WriteToConsole();
        }

        public Interaction(InteractionType type, string message)
        {
            this.type = type;
            this.message = message;
            this.args = null;

            if (ToConsole)
                WriteToConsole();
        }

        public Interaction(string message)
        {
            this.type = InteractionType.Info;
            this.message = message;
            this.args = null;

            if (ToConsole)
                WriteToConsole();
        }

        public string Message
        {
            get
            {
                if (args != null)
                    return string.Format(message, args);
                return message;
            }
        }

        public void WriteToConsole()
        {
            Console.WriteLine("[{0}] {1}", type.ToString(), Message);
        }

        public static string GetMessage(Interaction interaction)
        {
            if (interaction == null)
                return "Internal error";

            return interaction.Message;
        }
    }
}
