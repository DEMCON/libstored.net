// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

namespace LibStored.Net;

/// <summary>
/// Represents the supported data types, flags, and masks for variables in the store.
/// </summary>
[Flags]
public enum Types : byte
{
    /// <summary>
    /// Mask for extracting the size bits from a type value.
    /// </summary>
    MaskSize = 0x7,

    /// <summary>
    /// Mask for extracting the flag bits from a type value.
    /// </summary>
    MaskFlags = 0x78,

    /// <summary>
    /// Indicates the type is signed.
    /// </summary>
    FlagSigned = 0x08,

    /// <summary>
    /// Indicates the type is an integer.
    /// </summary>
    FlagInt = 0x10,

    /// <summary>
    /// Indicates the type has a fixed size.
    /// </summary>
    FlagFixed = 0x20,

    /// <summary>
    /// Indicates the type is a function.
    /// </summary>
    FlagFunction = 0x40,

    /// <summary>
    /// 8-bit signed integer type.
    /// </summary>
    Int8 = Types.FlagFixed | Types.FlagInt | Types.FlagSigned | 0,

    /// <summary>
    /// 8-bit unsigned integer type.
    /// </summary>
    Uint8 = Types.FlagFixed | Types.FlagInt | 0,

    /// <summary>
    /// 16-bit signed integer type.
    /// </summary>
    Int16 = Types.FlagFixed | Types.FlagInt | Types.FlagSigned | 1,

    /// <summary>
    /// 16-bit unsigned integer type.
    /// </summary>
    Uint16 = Types.FlagFixed | Types.FlagInt | 1,

    /// <summary>
    /// 32-bit signed integer type.
    /// </summary>
    Int32 = Types.FlagFixed | Types.FlagInt | Types.FlagSigned | 3,

    /// <summary>
    /// 32-bit unsigned integer type.
    /// </summary>
    Uint32 = Types.FlagFixed | Types.FlagInt | 3,

    /// <summary>
    /// 64-bit signed integer type.
    /// </summary>
    Int64 = Types.FlagFixed | Types.FlagInt | Types.FlagSigned | 7,

    /// <summary>
    /// 64-bit unsigned integer type.
    /// </summary>
    Uint64 = Types.FlagFixed | Types.FlagInt | 7,

    /// <summary>
    /// Native signed integer type (platform size).
    /// </summary>
    Int = Types.FlagFixed | Types.FlagInt | (sizeof(int) - 1),

    /// <summary>
    /// Native unsigned integer type (platform size).
    /// </summary>
    Uint = Types.FlagFixed | (sizeof(uint) - 1),

    /// <summary>
    /// 32-bit floating point type.
    /// </summary>
    Float = Types.FlagFixed | Types.FlagSigned | 3,

    /// <summary>
    /// 64-bit floating point type.
    /// </summary>
    Double = Types.FlagFixed | Types.FlagSigned | 7,

    /// <summary>
    /// Boolean type.
    /// </summary>
    Bool = Types.FlagFixed | 0,

    /// <summary>
    /// 32-bit pointer type.
    /// </summary>
    Pointer32 = Types.FlagFixed | 3,

    /// <summary>
    /// 64-bit pointer type.
    /// </summary>
    Pointer64 = Types.FlagFixed | 7,

    // Cant find a constant that can used to determine the pointer size at compile time.
    //Pointer = (IntPtr.Size <= 4 ? Pointer32 : Pointer64),
    /// <summary>
    /// Native pointer type (defaults to 64-bit).
    /// </summary>
    Pointer = Types.Pointer64,

    // (special) things with undefined length
    /// <summary>
    /// Void type (special, undefined length).
    /// </summary>
    Void = 0,

    /// <summary>
    /// Blob type (special, undefined length).
    /// </summary>
    Blob = 1,

    /// <summary>
    /// String type (special, undefined length).
    /// </summary>
    String = 2,

    /// <summary>
    /// Invalid type value.
    /// </summary>
    Invalid = 0xff,
}
