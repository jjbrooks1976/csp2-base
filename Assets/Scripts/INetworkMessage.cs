using Unity.Networking.Transport;

public interface INetworkMessage
{
    public void Serialize(ref DataStreamWriter writer);

    public void Deserialize(ref DataStreamReader reader);
}