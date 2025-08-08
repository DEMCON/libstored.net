// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

namespace LibStored.Net;

[Flags]
public enum Types : byte
{
    MaskSize = 0x7,
    MaskFlags = 0x78,
    FlagSigned = 0x08,
    FlagInt = 0x10,
    FlagFixed = 0x20,
    FlagFunction = 0x40,

    Int8 = Types.FlagFixed | Types.FlagInt | Types.FlagSigned | 0,
    Uint8 = Types.FlagFixed | Types.FlagInt | 0,
    Int16 = Types.FlagFixed | Types.FlagInt | Types.FlagSigned | 1,
    Uint16 = Types.FlagFixed | Types.FlagInt | 1,
    Int32 = Types.FlagFixed | Types.FlagInt | Types.FlagSigned | 3,
    Uint32 = Types.FlagFixed | Types.FlagInt | 3,
    Int64 = Types.FlagFixed | Types.FlagInt | Types.FlagSigned | 7,
    Uint64 = Types.FlagFixed | Types.FlagInt | 7,
    Int = Types.FlagFixed | Types.FlagInt | (sizeof(int) - 1),
    Uint = Types.FlagFixed | (sizeof(uint) - 1),

    Float = Types.FlagFixed | Types.FlagSigned | 3,
    Double = Types.FlagFixed | Types.FlagSigned | 7,

    Bool = Types.FlagFixed | 0,
    Pointer32 = Types.FlagFixed | 3,
    Pointer64 = Types.FlagFixed | 7,

    // Cant find a constant that can used to determine the pointer size at compile time.
    //Pointer = (IntPtr.Size <= 4 ? Pointer32 : Pointer64),
    Pointer = Types.Pointer64,

    // (special) things with undefined length
    Void = 0,
    Blob = 1,
    String = 2,

    Invalid = 0xff,
}