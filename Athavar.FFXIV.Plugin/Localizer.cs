namespace Athavar.FFXIV.Plugin
{
    internal class Localizer
    {
        public Language Language = Language.EN;

        public Localizer(Language language = Language.EN)
        {
            this.Language = language;
        }

        public string Localize(string? message)
        {
            if (message is null)
            {
                return "NullText";
            }

            return message;
        }
    }
}
