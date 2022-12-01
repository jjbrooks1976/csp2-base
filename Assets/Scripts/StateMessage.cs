using UnityEngine;
using Unity.Networking.Transport;

public struct StateMessage : INetworkMessage
{
    public int tick;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 velocity;
    public Vector3 angularVelocity;

    public void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteInt(tick);
        writer.WriteFloat(position.x);
        writer.WriteFloat(position.y);
        writer.WriteFloat(position.z);
        writer.WriteFloat(rotation.x);
        writer.WriteFloat(rotation.y);
        writer.WriteFloat(rotation.z);
        writer.WriteFloat(rotation.w);
        writer.WriteFloat(velocity.x);
        writer.WriteFloat(velocity.y);
        writer.WriteFloat(velocity.z);
        writer.WriteFloat(angularVelocity.x);
        writer.WriteFloat(angularVelocity.y);
        writer.WriteFloat(angularVelocity.z);
    }

    public void Deserialize(ref DataStreamReader reader)
    {
    }

    public override string ToString()
    {
        return $"tick={tick} " +
            $"position={position} " +
            $"rotation={rotation}" +
            $"velocity={velocity} " +
            $"angularVelocity={angularVelocity}";
    }
}