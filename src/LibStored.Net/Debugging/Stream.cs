// SPDX-FileCopyrightText: 2025 Guus Kuiper
//
// SPDX-License-Identifier: MIT

using LibStored.Net.Protocol;

namespace LibStored.Net.Debugging;

/// <summary>
///
/// </summary>
public class Stream : ProtocolLayer
{
    private readonly int _maxSize;

    private List<byte> _buffer = [];

    private bool _blocked;

    /// <inheritdoc />
    public Stream(int maxSize = 1024)
    {
        _maxSize = maxSize;
    }

    /// <inheritdoc />
    public override void Decode(Span<byte> buffer) {}

    /// <inheritdoc />
    public override void Encode(ReadOnlySpan<byte> buffer, bool last)
    {
        if (_blocked)
        {
            return;
        }

        _buffer.AddRange(buffer);
    }

    /// <summary>
    /// Swap the internal buffer for another buffer
    /// </summary>
    /// <param name="buffer"></param>
    public void Swap(ref List<byte> buffer) => (_buffer, buffer) = (buffer, _buffer);

    /// <inheritdoc />
    public override bool Flush()
    {
        Block();
        return true;
    }

    /// <summary>
    ///
    /// </summary>
    public void Clear()
    {
        _buffer.Clear();
        Unblock();
    }

    /// <summary>
    ///
    /// </summary>
    public bool Empty => _buffer.Count == 0;

    /// <summary>
    ///
    /// </summary>
    public void Unblock() => _blocked = false;


    /// <summary>
    ///
    /// </summary>
    public void Block() => _blocked = true;

    /// <summary>
    ///
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    public int Fits(int size)
    {
        if (_buffer.Count >= _maxSize)
        {
            return 0;
        }
        return Math.Min(size, _maxSize - _buffer.Count);
    }

    /// <summary>
    ///
    /// </summary>
    public bool IsFull => _buffer.Count >= _maxSize;
}
