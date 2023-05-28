namespace Athavar.FFXIV.Plugin.Config.Extensions;

internal static class DictionaryExtensions
{
    public static TKey? KeyByValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, TValue val)
        where TKey : notnull
    {
        TKey? key = default;
        foreach (var pair in dict)
        {
            if (EqualityComparer<TValue>.Default.Equals(pair.Value, val))
            {
                key = pair.Key;
                break;
            }
        }

        return key;
    }
}