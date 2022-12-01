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

    public void Deserialize(ref DataStreamReader reader)
    {
    }

    public override string ToString()
    {
        return $"up={up} " +
            $"down={down} " +
            $"right={right} " +
            $"left={right} " +
            $"jump={jump}";
    }
}