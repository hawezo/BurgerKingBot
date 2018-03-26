using bkbot.Core;

namespace bkbot
{
    public class BotSettings : Settings<BotSettings>
    {
        public string url = "http://bkvousecoute.fr/";
        public string reference = "23143";
        public string postalCode = "13490";
        public int amount = 1;
    }
}
