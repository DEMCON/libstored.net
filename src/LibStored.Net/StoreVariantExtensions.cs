// SPDX-FileCopyrightText: 2025 Guus Kuiper
//
// SPDX-License-Identifier: MIT

using System.Text;

namespace LibStored.Net;

/// <summary>
/// Extension methods for converting between <see cref="StoreVariant{T}"/> and string or byte[] types.
/// </summary>
public static class StoreVariantExtensions
{
    /// <summary>
    /// Gets the string value from a StoreVariant of type string using UTF-8 decoding.
    /// </summary>
    /// <param name="variant">The string variant.</param>
    /// <returns>The decoded string value.</returns>
    public static string Get(this StoreVariant<string> variant)
    {
        ReadOnlySpan<byte> bytes = variant.Get();
        return Encoding.UTF8.GetString(bytes.TrimEnd((byte)'\0'));
    }

    /// <summary>
    /// Sets the value of a StoreVariant of type string using UTF-8 encoding.
    /// </summary>
    /// <param name="variant">The string variant.</param>
    /// <param name="text">The string value to set.</param>
    /// <exception cref="ArgumentException">Thrown if the string is too long for the variant size.</exception>
    public static void Set(this StoreVariant<string> variant, string text)
    {
        ReadOnlySpan<byte> bytes = Encoding.UTF8.GetBytes(text);
        if (bytes.Length > variant.Size)
        {
            throw new ArgumentException($"String length exceeds the size of the variant: {variant.Size} bytes.", nameof(text));
        }

        variant.Set(bytes);
    }

    /// <summary>
    /// Gets the value from a StoreVariant of type byte[] as a byte array.
    /// </summary>
    /// <param name="variant">The byte array variant.</param>
    /// <returns>The value as a byte array.</returns>
    public static byte[] Get(this StoreVariant<byte[]> variant)
    {
        byte[] bytes = new byte[variant.Size];
        variant.CopyTo(bytes);
        return bytes;
    }

    /// <summary>
    /// Sets the value of a StoreVariant of type byte[] from a byte array.
    /// </summary>
    /// <param name="variant">The byte array variant.</param>
    /// <param name="bytes">The byte array to set.</param>
    /// <exception cref="ArgumentException">Thrown if the byte array is too long for the variant size.</exception>
    public static void Set(this StoreVariant<byte[]> variant, byte[] bytes)
    {
        if (bytes.Length > variant.Size)
        {
            throw new ArgumentException($"bytes array length exceeds the size of the variant: {variant.Size} bytes.", nameof(bytes));
        }

        variant.Set(bytes);
    }
}
