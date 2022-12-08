using UnityEngine;
using Unity.Networking.Transport;

public struct Input : INetworkMessage
{
    public bool up;
    public bool down;
    public bool right;
    public bool left;
    public bool jump;

    public void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteInt(up ? 1 : 0);
        writer.WriteInt(down ? 1 : 0);
        writer.WriteInt(right ? 1 : 0);
        writer.WriteInt(left ? 1 : 0);
        writer.WriteInt(jump ? 1 : 0);
    }

    public static Input Deserialize(ref DataStreamReader reader)
    {
        return new()
        {
            up = reader.ReadInt() == 1,
            down = reader.ReadInt() == 1,
            right = reader.ReadInt() == 1,
            left = reader.ReadInt() == 1,
            jump = reader.ReadInt() == 1
        };
    }

    public override string ToString()
    {
        return $"up={up}, " +
            $"down={down}, " +
            $"right={right}, " +
            $"left={left}, " +
            $"jump={jump}";
    }
}