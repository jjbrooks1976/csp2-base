using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;

public struct InputMessage : INetworkMessage
{
    public int startTick;
    public List<Input> inputs;

    public void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteInt(startTick);

        writer.WriteInt(inputs.Count);
        foreach (Input input in inputs)
        {
            input.Serialize(ref writer);
        }
    }

    public static InputMessage Deserialize(ref DataStreamReader reader)
    {
        InputMessage message = new()
        {
            startTick = reader.ReadInt(),
            inputs = new List<Input>()
        };

        int count = reader.ReadInt();
        for (int index = 0; index < count; index++)
        {
            message.inputs.Add(Input.Deserialize(ref reader));
        }

        return message;
    }

    public override string ToString()
    {
        return $"startTick={startTick}, " +
            $"inputs=[{string.Join("],[", inputs)}]";
    }
}