// <copyright file="Server.cs" company="UofU-CS3500">
// Copyright (c) 2025 UofU-CS3500. All rights reserved.
// </copyright>


using System.Net;
using System.Net.Sockets;

namespace Networking;

/// <summary>
///   Represents a server task that waits for connections on a given
///   port and calls the provided delegate when a connection is made.
/// </summary>
public static class Server
{
    /// <summary>
    ///   Wait on a TcpListener for new connections. Alert the main program
    ///   via a callback (delegate) mechanism.
    /// </summary>
    /// <param name="handleConnect">
    ///   Handler for what the user wants to do when a connection is made.
    ///   This should be run asynchronously via a new thread.
    /// </param>
    /// <param name="port"> The port (e.g., 11000) to listen on. </param>
    /// <returns> The listener; call Stop() on it to end the accept loop. </returns>
    public static TcpListener StartServer( Action<NetworkConnection> handleConnect, int port )
    {
        TcpListener listener = new TcpListener(IPAddress.Any, port);
        listener.Start();

        Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        TcpClient tcpClient = listener.AcceptTcpClient();
                        NetworkConnection connection = new NetworkConnection(tcpClient);
                        new Thread( () => RunHandler(handleConnect, connection)) {IsBackground = true}.Start();
                    }
                    catch (ObjectDisposedException)
                    {
                        // Listener was stopped: exit instead of spinning.
                        break;
                    }
                    catch (SocketException e) when (e.SocketErrorCode is SocketError.Interrupted or SocketError.OperationAborted)
                    {
                        // Stop() cancels a blocked accept this way.
                        break;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error accepting connection: " + e.Message);
                        Thread.Sleep(100); // a persistent error (e.g. out of handles) must not spin the CPU
                    }
                }
            }
        );

        return listener;
    }

    /// <summary>
    ///   Runs the handler; a handler that throws (e.g. its client disconnected) must not crash the process.
    /// </summary>
    private static void RunHandler( Action<NetworkConnection> handleConnect, NetworkConnection connection )
    {
        try
        {
            handleConnect(connection);
        }
        catch (Exception e)
        {
            Console.WriteLine("Connection handler ended: " + e.Message);
            connection.Dispose();
        }
    }
}
