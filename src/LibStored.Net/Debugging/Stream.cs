// SPDX-FileCopyrightText: 2025 Guus Kuiper
//
// SPDX-License-Identifier: MIT

using LibStored.Net.Protocol;

namespace LibStored.Net.Debugging;

/// <summary>
/// A simple in-memory byte stream used for debugging protocol layers.
/// It buffers encoded bytes until they are swapped out or cleared. The
/// stream can be blocked to prevent additional data from being appended.
/// </summary>
public class Stream : ProtocolLayer
{
    private readonly int _maxSize;

    private List<byte> _buffer = [];
    private bool _blocked;

    /// <summary>
    /// Creates a new <see cref="Stream"/> instance.
    /// </summary>
    /// <param name="maxSize">The maximum number of bytes the internal buffer will hold. When the
    /// buffer reaches this limit, <see cref="Fits(int)"/> will return 0.</param>
    public Stream(int maxSize)
    {
        _maxSize = maxSize;
    }

    /// <summary>
    /// Decoding is a no-op for the debugging <see cref="Stream"/>. This layer
    /// does not transform incoming bytes; it only buffers encoded data.
    /// </summary>
    /// <param name="buffer">The input buffer to decode (ignored).</param>
    public override void Decode(Span<byte> buffer) {}

    /// <summary>
    /// Appends the provided bytes to the internal buffer unless the stream is
    /// currently blocked. This is used by debugging stacks to capture outgoing
    /// encoded frames.
    /// </summary>
    /// <param name="buffer">The bytes to append to the internal buffer.</param>
    /// <param name="last">Indicates whether this is the last segment of a
    /// logical frame. The debugging stream ignores this value but it is kept
    /// for compatibility with the protocol layer signature.</param>
    public override void Encode(ReadOnlySpan<byte> buffer, bool last)
    {
        if (_blocked)
        {
            return;
        }

        _buffer.AddRange(buffer);
    }

    /// <summary>
    /// Swaps the internal buffer with the caller-provided buffer. After this
    /// call the stream's buffer is the previous caller buffer and the caller
    /// receives the previous stream buffer contents.
    /// </summary>
    /// <param name="buffer">The buffer to exchange with the internal buffer.</param>
    public void Swap(ref List<byte> buffer) => (_buffer, buffer) = (buffer, _buffer);

    /// <inheritdoc />
    public override bool Flush()
    {
        Block();
        return true;
    }

    /// <summary>
    /// Clears any buffered bytes and unblocks the stream so it can accept new
    /// data.
    /// </summary>
    public void Clear()
    {
        _buffer.Clear();
        Unblock();
    }

    /// <summary>
    /// Gets a value indicating whether the internal buffer is empty.
    /// </summary>
    public bool Empty => _buffer.Count == 0;

    /// <summary>
    /// Unblocks the stream, allowing <see cref="Encode(ReadOnlySpan{byte}, bool)"/>
    /// to append data again.
    /// </summary>
    public void Unblock() => _blocked = false;

    /// <summary>
    /// Blocks the stream to prevent further data from being appended to the
    /// internal buffer.
    /// </summary>
    public void Block() => _blocked = true;

    /// <summary>
    /// Returns how many bytes of the requested <paramref name="size"/> will
    /// fit into the internal buffer without exceeding <c>_maxSize</c>.
    /// </summary>
    /// <param name="size">Requested number of bytes to fit.</param>
    /// <returns>The number of bytes that can be accepted (0 if the buffer is full).</returns>
    public int Fits(int size) => IsFull ? 0 : Math.Min(size, _maxSize - _buffer.Count);

    /// <summary>
    /// Gets a value indicating whether the internal buffer has reached or
    /// exceeded the configured maximum size.
    /// </summary>
    public bool IsFull => _buffer.Count >= _maxSize;
}
