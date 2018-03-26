using bkbot.Core;

namespace bkbot
{
    public class BotSettings : Settings<BotSettings>
    {
        public string url = "http://bkvousecoute.fr/";
        public string reference = ""; // must be valid, 5 digits
        public string postalCode = ""; // must be vaid as well
        public int amount = 1;
    }
}
