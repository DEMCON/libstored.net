// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

using System.Text;

namespace LibStored.Net;

/// <summary>
/// Helper method to convert between StoreVariant and string or byte[] types.
/// </summary>
public static class StoreVariantExtensions
{
    public static string Get(this StoreVariant<string> variant)
    {
        ReadOnlySpan<byte> bytes = variant.Get();
        return Encoding.UTF8.GetString(bytes);
    }

    public static void Set(this StoreVariant<string> variant, string text)
    {
        ReadOnlySpan<byte> bytes = Encoding.UTF8.GetBytes(text);
        if (bytes.Length > variant.Size)
        {
            throw new ArgumentException($"String length exceeds the size of the variant: {variant.Size} bytes.", nameof(text));
        }

        variant.Set(bytes.Slice(0, variant.Size));
    }

    public static byte[] Get(this StoreVariant<byte[]> variant)
    {
        byte[] bytes = new byte[variant.Size];
        variant.CopyTo(bytes);
        return bytes;
    }

    public static void Set(this StoreVariant<byte[]> variant, byte[] bytes)
    {
        if (bytes.Length > variant.Size)
        {
            throw new ArgumentException($"bytes array length exceeds the size of the variant: {variant.Size} bytes.", nameof(bytes));
        }

        variant.Set(bytes.AsSpan(0, variant.Size));
    }
}