// SPDX-FileCopyrightText: 2025 Guus Kuiper
//
// SPDX-License-Identifier: MIT

using LibStored.Net.DebugClient;
using LibStored.Net.ZeroMQ;
using NetMQ;
using NetMQ.Sockets;

Console.WriteLine("Make sure the 8_sync example is running and uses the 5555 port for sync and 5556 for debugging. For example using the Dockerfile in the AppHost example:");
Console.WriteLine("podman run -p 5555:5555 -p 5556:5556 <name>");
{
    using RequestSocket socket = new();
    socket.Connect("tcp://localhost:5556");
    DebugClient client = new(new ZeroMQLayer(socket));

    string capabilities = client.Capabilities();
    Console.WriteLine(capabilities);

    string[] registers = client.List();
    Console.WriteLine(string.Join("\n", registers));

    string path = registers.Length == 0 ? "" : registers[0].Substring(registers[0].IndexOf('/'));
    string value = client.Read(path);
    Console.WriteLine($"{path} = {value}");

    foreach (int i in Enumerable.Range(0, 10))
    {
        string message = $"Hello {i:D2}";
        Console.WriteLine("<" + message);
        string received = client.Echo(message);
        Console.WriteLine(">" + received);
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
