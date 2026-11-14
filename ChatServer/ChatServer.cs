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
    private static readonly object sync = new();
    
    /// <summary>
    /// List of the clients that have sent a name
    /// </summary>
    private static readonly List<NetworkConnection> clients = new();

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
            if (string.IsNullOrWhiteSpace(name))
            {
                connection.Send("Invalid name. Connection closed.");
                return;
            }
            
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
        catch (Exception ex) when 
            (ex is IOException or ObjectDisposedException or InvalidOperationException)
        {
            // Connection closed or disposed
            Console.WriteLine("Connection closed: " + ex.Message);
        }
        catch (Exception ex)
        {
            // Handle other exceptions
            Console.WriteLine("Error: " + ex.Message);
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
    /// Sends a message to every connected client, including the sender.
    /// Also handles failed sends by removing them .
    /// </summary>
    /// <param name="message">The string of characters to send.</param>
    private static void BroadcastMessage(string message)
    {
        
        List<NetworkConnection> clientsCopy;
        lock (sync)
        {
            // Copy the iterator
            clientsCopy = new List<NetworkConnection>(clients);
        }

        foreach (NetworkConnection client in clientsCopy)
        {
            try
            {
                client.Send(message);
            }
            catch (Exception)
            {
                // Remove the client from the list if sending fails
                lock (sync)
                {
                    clients.Remove(client);
                }
                client.Dispose();
            }
        }

    }
}