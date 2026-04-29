using System.Net;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    [SerializeField] private string _ip = "127.0.0.1";
    [SerializeField] private ushort _port = 7979;

    enum Role
    {
        ServerClient = 0,
        Server = 1,
        Client = 2
    }

    private static Role _role = Role.ServerClient;

    void Start()
    {
        if (Application.isEditor)
        {
            _role = Role.ServerClient;
        }
        else if (Application.platform == RuntimePlatform.WindowsServer || Application.platform == RuntimePlatform.LinuxServer)
        {
            _role = Role.Server;
        }
        else
        {
            _role = Role.Client;
        }
        Connect();
    }

    void Connect()
    {
        World serverWorld = null;
        World clientWorld = null;

        if (_role == Role.ServerClient || _role == Role.Server)
        {
            serverWorld = ClientServerBootstrap.CreateServerWorld("ServerWorld");
        }

        if (_role == Role.ServerClient || _role == Role.Client)
        {
            clientWorld = ClientServerBootstrap.CreateClientWorld("ClientWorld");
        }

        // https://docs.unity3d.com/Packages/com.unity.entities@1.0/api/Unity.Entities.WorldFlags.html
        foreach (var world in World.All)
        {
            if (world.Flags == WorldFlags.Game)
            {
                world.Dispose();
                break;
            }
        }

        if (serverWorld != null)
        {
            World.DefaultGameObjectInjectionWorld = serverWorld;
        }
        else if(clientWorld != null)
        {
            World.DefaultGameObjectInjectionWorld = clientWorld;
        }

        if (serverWorld != null)
        {
            using var query = serverWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            query.GetSingletonRW<NetworkStreamDriver>().ValueRW.Listen(ClientServerBootstrap.DefaultListenAddress.WithPort(_port));
        }

        if (clientWorld != null)
        {
            IPAddress serverAddress = IPAddress.Parse(_ip);
            NativeArray<byte> nativeArrayAddress = new NativeArray<byte>(serverAddress.GetAddressBytes().Length, Allocator.Temp);
            nativeArrayAddress.CopyFrom(serverAddress.GetAddressBytes());
            
            NetworkEndpoint endpoint = NetworkEndpoint.AnyIpv4;
            endpoint.SetRawAddressBytes(nativeArrayAddress);
            endpoint.Port = _port;

            using var query = clientWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            query.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(entityManager: clientWorld.EntityManager, endpoint: endpoint);
        }
    }
}
