// <copyright file="ChatServer.cs" company="UofU-CS3500">
// Copyright (c) 2025 UofU-CS3500. All rights reserved.
// </copyright>

using Networking;

// ReSharper disable once CheckNamespace
namespace Chatting;

/// <summary>
///   A simple ChatServer that handles clients separately and replies with a static message.
/// </summary>
public abstract class ChatServer
{
    /// <summary>
    /// Stops two threads from writing to the same StreamWriter simultaneously
    /// </summary>
    private static object sync = new();
    
    /// <summary>
    /// List of the clients that have sent a name
    /// </summary>
    private static List<NetworkConnection> clients = new();

    /// <summary>
    ///   The main program.
    /// </summary>
    private static void Main( string[] _ )
    {
        Server.StartServer( HandleConnect, 11_000 );
        Console.Read(); // don't stop the program.
    }

    /// <summary>
    ///   <pre>
    ///     When a new connection is established, enter a loop that receives from and
    ///     replies to a client.
    ///   </pre>
    /// </summary>
    ///
    private static void HandleConnect(NetworkConnection connection)
    {
        try
        {
            string name = connection.ReadLine();

            lock (sync)
            {
                clients.Add(connection);
            }

            while (true)
            {
                string message = connection.ReadLine();
                BroadcastMessage(name + ": " + message);
            }
        }
        catch (Exception)
        {
            // client disconnected, or read failed, now to clean up 
        }
        finally
        {
            lock (sync)
            {
                clients.Remove(connection);
            }
            connection.Dispose();
        }
    }
    
    /// <summary>
    /// Sends a message to every connected client, including the sender. Also handles failed
    /// sends by removing them .
    /// </summary>
    /// <param name="message">The string of characters to send.</param>
    private static void BroadcastMessage(string message)
    {
        lock (sync)
        {
            foreach (NetworkConnection client in clients)
            {
                try
                {
                    client.Send(message);
                }
                catch (Exception)
                {
                    // The client's thread handles its own cleanup
                }
            }
        }
    }
    
}