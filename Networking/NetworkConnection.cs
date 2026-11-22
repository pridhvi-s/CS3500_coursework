// <copyright file="NetworkConnection.cs" company="UofU-CS3500">
// Copyright (c) 2025 UofU-CS3500. All rights reserved.
// </copyright>

using System.Net.Sockets;
using System.Text;
namespace Networking;

/// <summary>
///   Wraps the StreamReader/Writer/TcpClient together so we
///   don't have to keep creating all three for network actions.
/// </summary>
public sealed class NetworkConnection : IDisposable
{
    /// <summary>
    ///   The connection/socket abstraction
    /// </summary>
    private TcpClient _tcpClient;

    /// <summary>
    ///   Reading end of the connection
    /// </summary>
    private StreamReader? _reader;

    /// <summary>
    ///   Writing end of the connection
    /// </summary>
    private StreamWriter? _writer;

    /// <summary>
    /// Send lock for thread safety when sending
    /// messages. This prevents multiple threads
    /// from writing to the same StreamWriter
    /// simultaneously.
    /// </summary>
    private readonly object _sendLock = new();

    /// <summary>
    ///   Initializes a new instance of the <see cref="NetworkConnection"/> class.
    ///   <para>
    ///     Create a network connection object.
    ///   </para>
    /// </summary>
    /// <param name="tcpClient">
    ///   An already existing TcpClient
    /// </param>
    public NetworkConnection( TcpClient tcpClient )
    {
        _tcpClient = tcpClient;
        if ( IsConnected )
        {
            // Only establish the reader/writer if the provided TcpClient is already connected.
            _reader = new StreamReader( _tcpClient.GetStream(), Encoding.UTF8 );
            _writer = new StreamWriter( _tcpClient.GetStream() ) { AutoFlush = true }; // AutoFlush ensures data is sent immediately
        }
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="NetworkConnection"/> class.
    ///   <para>
    ///     Create a network connection object.  The tcpClient will be unconnected at the start.
    ///   </para>
    /// </summary>
    public NetworkConnection( )
        : this( new TcpClient( ) )
    {
    }

    /// <summary>
    /// Gets a value indicating whether the socket is connected.
    /// </summary>
    public bool IsConnected
    {
        get
        {
            return _tcpClient.Connected;
        }
    }


    private const int ConnectTimeoutMs = 5000;

    /// <summary>
    ///   Milliseconds a read may wait before ReadLine fails; 0 means wait forever.
    /// </summary>
    public int ReceiveTimeout
    {
        get => _tcpClient.ReceiveTimeout;
        set => _tcpClient.ReceiveTimeout = value;
    }

    /// <summary>
    ///   Try to connect to the given host:port.
    /// </summary>
    /// <param name="host"> The URL or IP address, e.g., www.cs.utah.edu, or  127.0.0.1. </param>
    /// <param name="port"> The port, e.g., 11000. </param>
    public void Connect( string host, int port )
    {
        // Without a timeout an unreachable IP blocks for minutes.
        try
        {
            _tcpClient.ConnectAsync(host, port).WaitAsync(TimeSpan.FromMilliseconds(ConnectTimeoutMs)).GetAwaiter().GetResult();
        }
        catch (TimeoutException)
        {
            throw new InvalidOperationException("Connection timed out");
        }

        _reader = new StreamReader(_tcpClient.GetStream(), Encoding.UTF8);
        _writer = new StreamWriter(_tcpClient.GetStream()) {AutoFlush = true};
    }


    /// <summary>
    ///   Send a message to the remote server.  If the <paramref name="message"/> contains
    ///   new lines, these will be treated on the receiving side as multiple messages.
    ///   This method should attach a newline to the end of the <paramref name="message"/>
    ///   (by using WriteLine).
    ///   If this operation can not be completed (e.g. because this NetworkConnection is not
    ///   connected), throw an InvalidOperationException.
    /// </summary>
    /// <param name="message"> The string of characters to send. </param>
    public void Send( string message )
    {
        StreamWriter? writer = _writer;

        if (!IsConnected || writer is null)
        {
            throw new InvalidOperationException("Connection failed");
        }

        try
        {
            lock (_sendLock)
            {
                writer.WriteLine(message);
            }
        }
        catch (Exception ex) when (ex is IOException or ObjectDisposedException)
        {
            throw new InvalidOperationException("Connection failed", ex);
        }
    }


    /// <summary>
    ///   Read a message from the remote side of the connection.  The message will contain
    ///   all characters up to the first new line. See <see cref="Send"/>.
    ///   If this operation can not be completed (e.g. because this NetworkConnection is not
    ///   connected), throw an InvalidOperationException.
    /// </summary>
    /// <returns> The contents of the message. </returns>
    public string ReadLine( )
    {
        StreamReader? reader = _reader;

        if (!IsConnected || reader is null)
        {
            throw new InvalidOperationException("Connection failed");
        }

        string? line;
        try
        {
            line = reader.ReadLine();
        } 
        catch (Exception ex) when 
            (ex is IOException or ObjectDisposedException)
        {
            // If the connection is closed or disposed
            throw new InvalidOperationException("Connection closed", ex);
        }  

        // null if remote side closes connection
        if (line is null)
        {
            throw new InvalidOperationException("Connection closed");
        }

        return line;
    }

    /// <summary>
    ///   If connected, disconnect the connection and clean 
    ///   up (dispose) any streams.
    ///   This method is safe to call multiple times, and it is safe to call it from any thread.
    ///   After calling this method, the NetworkConnection instance is no longer usable.
    /// </summary>
    public void Disconnect( )
    {
        // Atomically take ownership
        StreamWriter? writer = Interlocked.Exchange(ref _writer, null);
        StreamReader? reader = Interlocked.Exchange(ref _reader, null);

        try { writer?.Dispose(); }
        catch (Exception ex) when (ex is IOException or ObjectDisposedException) { }

        try { reader?.Dispose(); }
        catch (Exception ex) when (ex is IOException or ObjectDisposedException) { }

        try { _tcpClient.Dispose(); }
        catch (Exception ex) when (ex is IOException or ObjectDisposedException) { }
    }

    /// <summary>
    ///   Automatically called with a using statement (see IDisposable)
    /// </summary>
    public void Dispose( )
    {
        Disconnect();
    }
}
