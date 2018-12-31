using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;

public class PingPongBuffer : System.IDisposable {

    protected ComputeBuffer buffer0, buffer1;

    public ComputeBuffer Read { get { return buffer0; } }
    public ComputeBuffer Write { get { return buffer1; } }

    public PingPongBuffer(int count, int stride, ComputeBufferType type = ComputeBufferType.Default)
    {
        buffer0 = new ComputeBuffer(count, stride, type);
        buffer1 = new ComputeBuffer(count, stride, type);
    }

    public void SetData(Array data)
    {
        buffer0.SetData(data);
        buffer1.SetData(data);
    }

    public void SetCounterValue(uint counter)
    {
        buffer0.SetCounterValue(counter);
        buffer1.SetCounterValue(counter);
    }

    public void Swap()
    {
        var tmp = buffer0;
        buffer0 = buffer1;
        buffer1 = tmp;
    }

    public void Dispose()
    {
        buffer0.Dispose();
        buffer1.Dispose();
    }

    public static implicit operator ComputeBuffer(PingPongBuffer buf)
    {
        return buf.Read;
    }

}
