// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace LibStored.Net;

/// <summary>
/// Contains metadata about a debug variant, including its type, offset, and size.
/// </summary>
public record DebugVariantInfo(Types Type, int Offset, int Size);

/// <summary>
/// Represents a base class for a store containing variables and supporting hooks and debug operations.
/// </summary>
public abstract class Store
{
    private readonly Lock _lock = new();

    /// <summary>
    /// Gets the name of the store.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets the hash of the store.
    /// </summary>
    public abstract string Hash { get; }
    
    /// <summary>
    /// Gets the number of variables in the store.
    /// </summary>
    public abstract int VariableCount { get; }

    /// <summary>
    /// Add hooks to allow customization.
    /// </summary>
    public IHooks? Hooks { get; set; }

    /// <summary>
    /// Retrieves a span representing the underlying buffer of the current object.
    /// Do not call yourself, since this will by-pass thread-safety.
    /// </summary>
    /// <remarks>The returned <see cref="Span{T}"/> provides direct access to the underlying data. 
    /// Modifications to the span will affect the underlying buffer.</remarks>
    /// <returns>A <see cref="Span{T}"/> of bytes representing the underlying buffer.</returns>
    public abstract Span<byte> GetBuffer();

    /// <summary>
    /// Notifies that a change has occurred at the specified offset.
    /// </summary>
    /// <remarks>This method is intended to be overridden in a derived class to handle changes at the
    /// specified offset.</remarks>
    /// <param name="offset">The zero-based position where the change occurred. Must be a non-negative integer.</param>
    public abstract void Changed(int offset);

    /// <summary>
    /// Retrieves a read-only dictionary containing debug variants and their associated information.
    /// </summary>
    /// <remarks>The returned dictionary provides a mapping of debug variant names to their corresponding 
    /// metadata, which can be used for debugging or diagnostic purposes.</remarks>
    /// <returns>A read-only dictionary where the keys are the names of debug variants and the values are  <see
    /// cref="DebugVariantInfo"/> objects containing detailed information about each variant.</returns>
    public abstract IReadOnlyDictionary<string, DebugVariantInfo> GetDebugVariants();

    /// <summary>
    /// Find a variant by its path.
    /// </summary>
    /// <param name="path">The path to the variant.</param>
    /// <returns>The <see cref="DebugVariant"/> if found; otherwise, null.</returns>
    public DebugVariant? Find(string path)
    {
        // Skip until '/'
        int index = path.IndexOf('/');
        if (index == -1)
        {
            return null;
        }
        path = path.Substring(index);

        // Only support full matches from here.
        if (GetDebugVariants().TryGetValue(path, out DebugVariantInfo? info))
        {
            return new DebugVariant(this, info.Offset, info.Size, info.Type);
        }

        return null;
    }

    internal DebugVariant? Find(ReadOnlySpan<byte> path)
    {
        string pathStr = Encoding.UTF8.GetString(path);
        return Find(pathStr);
    }

    internal void List(Action<string, DebugVariant> action, string? prefix = null )
    {
        foreach ((string? name, DebugVariantInfo? info) in GetDebugVariants())
        {
            DebugVariant dv = new(this, info.Offset, info.Size, info.Type);
            if (prefix != null)
            {
                string prefixedName = prefix + name;
                action(prefixedName, dv);
            }
            else
            {
                action(name, dv);
            }
        }
    }

    internal Variable<T> GetVariable<T>(int offset) where T : struct
        => new(GetBuffer().Slice(offset, typeof(T) == typeof(bool) ? 1 : Marshal.SizeOf<T>()), this);

    internal Variant GetVariant(Types type, int offset, int size) => new(GetBuffer().Slice(offset, size), type, this);

    internal uint BufferToKey(ReadOnlySpan<byte> buffer)
    {
        ReadOnlySpan<byte> entireBuffer = GetBuffer();

        ref byte entireRef = ref MemoryMarshal.GetReference(entireBuffer);
        ref byte bufferRef = ref MemoryMarshal.GetReference(buffer);

        nint offset = Unsafe.ByteOffset(ref entireRef, ref bufferRef);

        if (offset < 0 || offset + buffer.Length > entireBuffer.Length)
        {
            throw new ArgumentException("Buffer is not a valid slice of the store's buffer.", nameof(buffer));
        }

        return (uint)offset;
    }

    /// <summary>
    /// Hook when exclusive access to a given variable is to be acquired.
    /// Not sure if these are actually needed in C#.
    /// These are called by the Variant / Variable's.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="buffer"></param>
    internal void HookEntryX(Types type, ReadOnlySpan<byte> buffer)
    {
        _lock.Enter();
        Hooks?.EntryX(type, buffer);
    }

    internal void HookExitX(Types type, ReadOnlySpan<byte> buffer, bool changed)
    {
        try
        {
            if (changed)
            {
                uint key = BufferToKey(buffer);
                Changed((int)key);
            }
            Hooks?.ExitX(type, buffer, changed);
        }
        finally
        {
            _lock.Exit();
        }
    }

    internal void HookEntryRO(Types type, ReadOnlySpan<byte> buffer)
    {
        _lock.Enter();
        Hooks?.EntryRO(type, buffer);
    }

    internal void HookExitRO(Types type, ReadOnlySpan<byte> buffer)
    {
        try
        {
            Hooks?.ExitRO(type, buffer);
        }
        finally
        {
            _lock.Exit();
        }
    }

    internal void HookChanged(Types type, ReadOnlySpan<byte> buffer) => Hooks?.Changed(type, buffer);

    internal void ListHookChanged() => List((s, variant) => HookChanged(variant.Type, variant.Buffer()));
}
