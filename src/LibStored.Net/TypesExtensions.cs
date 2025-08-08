// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

namespace LibStored.Net;

public static class TypesExtensions
{
    public static bool IsFixed(this Types type) => (type & Types.FlagFixed) != 0;
    public static bool IsFunction(this Types type) => (type & Types.FlagFunction) != 0;
    public static bool IsInt(this Types type) => (type & Types.FlagInt) != 0;
    public static bool IsSigned(this Types type) => (type & Types.FlagSigned) != 0;
    public static bool IsSpecial(this Types type) => (type & Types.MaskFlags) == 0;
    public static int Size(this Types type) => !type.IsFixed() ? 0 : (int)(type & Types.MaskSize) + 1;

    public static Types GetType<T>()
    {
        Type t = typeof(T);
        return t switch
        {
            _ when t == typeof(byte) => Types.Uint8,
            _ when t == typeof(sbyte) => Types.Int8,
            _ when t == typeof(short) => Types.Int16,
            _ when t == typeof(ushort) => Types.Uint16,
            _ when t == typeof(int) => Types.Int32,
            _ when t == typeof(uint) => Types.Uint32,
            _ when t == typeof(long) => Types.Int64,
            _ when t == typeof(ulong) => Types.Uint64,
            _ when t == typeof(float) => Types.Float,
            _ when t == typeof(double) => Types.Double,
            _ when t == typeof(bool) => Types.Bool,
            _ when t == typeof(IntPtr) && IntPtr.Size <= 4 => Types.Pointer32,
            _ when t == typeof(IntPtr) && IntPtr.Size > 4 => Types.Pointer64,
            _ when t == typeof(string) => Types.String,
            _ when t == typeof(byte[]) => Types.Blob,
            _ when t == typeof(void) => Types.Void,
            _ => throw new ArgumentException($"Unsupported type: {t.Name}"),
        };
    }
}