namespace Athavar.FFXIV.Plugin.Common.Extension;

using System.Runtime.CompilerServices;

/// <summary>
///     Extension methods for dealing with enums.
///     Source:
///     https://github.com/Reloaded-Project/Reloaded.Memory/blob/0ed11ef7043dea23664a0d20ab05cadd46ab8a5a/src/Reloaded.Memory/Extensions/EnumExtensions.cs#L18
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    ///     Determines if the given enum has a specified flag.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="flag">The flag to check.</param>
    /// <typeparam name="T">The type to check the flag of.</typeparam>
    /// <exception cref="NotSupportedException">This type of enum is not supported.</exception>
    /// <returns>True if the enum is contained in the value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool HasFlagFast<T>(this T value, T flag)
        where T : unmanaged, Enum
        => sizeof(T) switch
        {
            sizeof(byte) => (Unsafe.As<T, byte>(ref value) & Unsafe.As<T, byte>(ref flag)) == Unsafe.As<T, byte>(ref flag),
            sizeof(short) => (Unsafe.As<T, short>(ref value) & Unsafe.As<T, short>(ref flag)) == Unsafe.As<T, short>(ref flag),
            sizeof(int) => (Unsafe.As<T, int>(ref value) & Unsafe.As<T, int>(ref flag)) == Unsafe.As<T, int>(ref flag),
            sizeof(long) => (Unsafe.As<T, long>(ref value) & Unsafe.As<T, long>(ref flag)) == Unsafe.As<T, long>(ref flag),
            _ => value.HasFlag(flag),
        };
}