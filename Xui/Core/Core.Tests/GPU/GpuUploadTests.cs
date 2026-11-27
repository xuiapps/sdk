using System.Runtime.InteropServices;
using Xui.GPU.Hardware;
using Xui.GPU.Shaders.Types;
using Xunit;

namespace Xui.GPU.Tests;

public class GpuUploadTests
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    private struct Vertex { public short Tag; public float X; }

    [Fact]
    public void DefaultSpanOverloadsBorrowExactNativeLayoutAndAcceptEmptyUploads()
    {
        var recorder = new Recorder(); IGpuCommandList commands = recorder;
        Vertex[] data = [new() {Tag=7,X=1.25f}, new() {Tag=-2,X=-3.5f}];
        commands.SetVertexBuffer<Vertex>(data);
        Assert.Equal(6, recorder.Stride); Assert.Equal(2, recorder.Count);
        Assert.Equal(MemoryMarshal.AsBytes(data.AsSpan()).ToArray(), recorder.Bytes);
        commands.SetConstantBuffer(in data[1]);
        Assert.Equal(MemoryMarshal.AsBytes(data.AsSpan(1)).ToArray(), recorder.Bytes);
        commands.SetVertexBuffer<Vertex>(ReadOnlySpan<Vertex>.Empty);
        Assert.Empty(recorder.Bytes); Assert.Equal(0, recorder.Count);
    }

    private sealed unsafe class Recorder : IGpuCommandList
    {
        public byte[] Bytes = []; public int Stride, Count;
        public void SetVertexBuffer(void* data, GpuVertexBufferDesc desc)
        { Stride=desc.Stride;Count=desc.VertexCount;Bytes=new ReadOnlySpan<byte>(data,Stride*Count).ToArray(); }
        public void SetConstantBuffer(void* data,int size) => Bytes=new ReadOnlySpan<byte>(data,size).ToArray();
        public void BeginRenderPass(IGpuRenderTarget target,Color4 color)=>throw new NotSupportedException();
        public void SetPipeline(GpuPipelineDesc pipeline)=>throw new NotSupportedException();
        public void Draw(int count)=>throw new NotSupportedException();
        public void EndRenderPass()=>throw new NotSupportedException();
        public void Commit()=>throw new NotSupportedException();
        public void Dispose() { }
    }
}
