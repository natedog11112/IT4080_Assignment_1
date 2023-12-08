using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting.FullSerializer;
using Unity.Collections;

public struct NetworkPlayerInfo : INetworkSerializable, System.IEquatable<NetworkPlayerInfo>
{
    public ulong clientId;
    public bool ready;
    public Color color;
    public FixedString128Bytes name;

    public NetworkPlayerInfo(ulong id) {
        clientId = id;
        ready = false;
        color = Color.magenta;
        name = "player " + id;
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref ready);
        serializer.SerializeValue(ref color);
        serializer.SerializeValue(ref name);
    }
    public bool Equals(NetworkPlayerInfo other) {
        return false;
    }
}
