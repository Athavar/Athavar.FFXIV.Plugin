namespace Athavar.FFXIV.Plugin.Manager.Interface;

internal interface ILocalizerManager
{
    void ChangeLanguage(Language language);
    string Localize(string? message);
}