using System.Collections.Generic;
using Unity.Networking.Transport;

public struct InputMessage : INetworkMessage
{
    public int startTick;
    public List<Input> inputs;

    public void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteInt(startTick);

        foreach (Input input in inputs)
        {
            input.Serialize(ref writer);
        }
    }

    public void Deserialize(ref DataStreamReader reader)
    {
    }

    public override string ToString()
    {
        return $"startTick={startTick} " +
            $"inputs=[{string.Join("],[", inputs)}]";
    }
}