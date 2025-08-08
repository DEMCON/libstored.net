// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

using NetMQ;
using NetMQ.Sockets;

{
    using RequestSocket socket = new();
    socket.Connect("tcp://localhost:5556");

    string message = "?";
    Console.WriteLine("<" + message);
    socket.SendFrame(message);
    string received = socket.ReceiveFrameString();
    Console.WriteLine(">" + received);

    foreach (int i in Enumerable.Range(0, 10))
    {
        message = $"eHello {i:D2}";
        Console.WriteLine("<" + message);
        socket.SendFrame(message);
        received = socket.ReceiveFrameString();
        Console.WriteLine("> " + received);
    }
}

{
    using DealerSocket socket = new();
    socket.Connect("tcp://localhost:5555");

    Console.WriteLine("Sending hello");
    // Assume we connect to the store in the 8_sync example with this hash.
    socket.SendFrame("h681a3ece584568efcf5879a64b688fc19e620577\0\x01\0"u8.ToArray());
    Console.WriteLine(socket.TryReceiveFrameString(TimeSpan.FromSeconds(2), out string? reply, out bool more) ? reply : "No reply received (timeout).");
    while (more)
    {
        if (socket.TryReceiveFrameBytes(TimeSpan.Zero, out byte[]? bytes, out more))
        {
            string hex = BitConverter.ToString(bytes);
            Console.WriteLine(hex);
        }
    }
}
