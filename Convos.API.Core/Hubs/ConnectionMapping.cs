namespace Convos.API.Core.Hubs;

public class ConnectionMapping<T> where T : notnull
{
    private readonly Dictionary<T, HashSet<string>> _connections = new();
    private readonly Dictionary<string, T> _connectionIdToUser = new();
    private readonly List<T> _connectedUsers = new();

    public int Count
    {
        get
        {
            lock (_connections)
            {
                return _connections.Count;
            }
        }
    }

    public void Add(T key, string connectionId)
    {
        lock (_connections)
        {
            if (!_connections.TryGetValue(key, out var connections))
            {
                connections = new HashSet<string>();
                _connections.Add(key, connections);
            }

            if (connections.Count == 0)
            {
                _connectionIdToUser.Add(connectionId, key);
                _connectedUsers.Add(key);
            }

            lock (connections)
            {
                connections.Add(connectionId);
            }
        }
    }

    public IEnumerable<string> GetConnections(T key)
    {
        lock (_connections)
        {
            if (_connections.TryGetValue(key, out var connections))
            {
                return connections;
            }
        }

        return Enumerable.Empty<string>();
    }

    public T GetConnectionKey(string connectionId)
    {
        lock (_connectionIdToUser)
        {
            return _connectionIdToUser[connectionId];
        }
    }

    public void Remove(T key, string connectionId)
    {
        lock (_connections)
        {
            if (!_connections.TryGetValue(key, out var connections))
            {
                return;
            }

            lock (connections)
            {
                connections.Remove(connectionId);
                if (connections.Count != 0) return;

                _connections.Remove(key);
                _connectionIdToUser.Remove(connectionId);
                _connectedUsers.Remove(key);
            }
        }
    }

    public List<T> GetConnectedUsers() => _connectedUsers.ToList();
}