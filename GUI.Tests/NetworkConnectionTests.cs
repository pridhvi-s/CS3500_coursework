using System.Net.Sockets;
using Networking;

namespace GUI.Tests;

public class NetworkConnectionTests
{
    [Test]
    public void SendAndReadLine_RoundTripThroughServer()
    {
        var accepted = new TaskCompletionSource<NetworkConnection>();
        TcpListener listener = Server.StartServer(c => accepted.SetResult(c), 0);
        try
        {
            int port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
            using var client = new NetworkConnection();
            client.Connect("localhost", port);
            using NetworkConnection server = accepted.Task.WaitAsync(TimeSpan.FromSeconds(3)).Result;

            client.Send("hello");
            Assert.That(server.ReadLine(), Is.EqualTo("hello"));

            server.Send("world");
            Assert.That(client.ReadLine(), Is.EqualTo("world"));
        }
        finally
        {
            listener.Stop();
        }
    }

    [Test]
    public void ReadLine_ThrowsWhenRemoteCloses()
    {
        var accepted = new TaskCompletionSource<NetworkConnection>();
        TcpListener listener = Server.StartServer(c => accepted.SetResult(c), 0);
        try
        {
            using var client = new NetworkConnection();
            client.Connect("localhost", ((System.Net.IPEndPoint)listener.LocalEndpoint).Port);
            accepted.Task.WaitAsync(TimeSpan.FromSeconds(3)).Result.Disconnect();

            Assert.Throws<InvalidOperationException>(() => client.ReadLine());
        }
        finally
        {
            listener.Stop();
        }
    }

    [Test]
    public void ReadLine_TimesOutWhenServerIsSilent()
    {
        TcpListener listener = Server.StartServer(_ => { }, 0);
        try
        {
            using var client = new NetworkConnection();
            client.Connect("localhost", ((System.Net.IPEndPoint)listener.LocalEndpoint).Port);
            client.ReceiveTimeout = 200;

            Assert.Throws<InvalidOperationException>(() => client.ReadLine());
        }
        finally
        {
            listener.Stop();
        }
    }

    [Test]
    public void Disconnect_CanBeCalledTwice()
    {
        var c = new NetworkConnection();
        c.Disconnect();
        Assert.DoesNotThrow(() => c.Disconnect());
    }

    [Test]
    public void Server_SurvivesThrowingHandler_AndKeepsAccepting()
    {
        int calls = 0;
        var second = new TaskCompletionSource();
        TcpListener listener = Server.StartServer(_ =>
        {
            if (Interlocked.Increment(ref calls) == 1) throw new InvalidOperationException("client left");
            second.SetResult();
        }, 0);
        try
        {
            int port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
            using var a = new NetworkConnection();
            a.Connect("localhost", port);
            using var b = new NetworkConnection();
            b.Connect("localhost", port);
            Assert.That(second.Task.Wait(TimeSpan.FromSeconds(3)), Is.True);
        }
        finally
        {
            listener.Stop();
        }
    }
}
