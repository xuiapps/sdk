using Xui.GPU.Hardware;
using Xui.GPU.Shaders.Types;
namespace Xui.Runtime.Android.GPU;

/// <summary>One draw per pass for the current Position/Color hardware contract.</summary>
internal sealed unsafe class VulkanCommandList(VulkanDevice device) : IGpuCommandList
{
    private VulkanTarget? target;
    private GpuPipelineDesc? pipeline;
    private bool acquired, ended, drawn, verticesSet, uniformsSet, disposed;
    private int vertexCount;
    public void BeginRenderPass(IGpuRenderTarget renderTarget,Color4 color)
    {
        device.Verify();ObjectDisposedException.ThrowIf(disposed,this);
        if(target!=null)throw new InvalidOperationException("Previous pass must be committed.");
        if(renderTarget is not VulkanTarget t||t.Device!=device||!t.Borrowed||t.Handle==0)throw new ArgumentException("Expected a borrowed target from this Vulkan device.");
        target=t;pipeline=null;ended=drawn=verticesSet=uniformsSet=false;
        acquired=VulkanNative.Check(VulkanNative.Begin(t.Handle,t.Width,t.Height,(float*)&color))!=0;
    }
    private VulkanTarget Active()
    {
        device.Verify();ObjectDisposedException.ThrowIf(disposed,this);
        if(target is not {Borrowed:true}||ended)throw new InvalidOperationException("No active Vulkan pass.");return target;
    }
    public void SetPipeline(GpuPipelineDesc value)
    {
        if (value.PremultipliedAlpha || value.VertexComponents is not null)
            throw new NotSupportedException("Custom vertex layouts and premultiplied blending require the GPU resource capability.");
        Active();if(drawn)throw new NotSupportedException("This backend currently supports one draw per pass.");
        if(value.VertexShader is not VulkanVertexShader {Handle: not 0} v||v.Device!=device || value.FragmentShader is not VulkanFragmentShader {Handle: not 0} f||f.Device!=device)
            throw new ArgumentException("Pipeline shaders must belong to this Vulkan device.");
        pipeline=value;
    }
    public void SetVertexBuffer(void* data,GpuVertexBufferDesc desc)
    {
        var t=Active();if(drawn||verticesSet)throw new NotSupportedException("One vertex upload per pass is supported.");
        if(pipeline is null)throw new InvalidOperationException("Set pipeline before vertices.");
        if(desc.Stride!=28||desc.VertexCount<=0||data==null)throw new NotSupportedException("Expected Position(float3), Color(float4), stride 28.");
        vertexCount=desc.VertexCount;verticesSet=true;
        if(!acquired)return;
        VulkanNative.Check(VulkanNative.Pipeline(t.Handle,((VulkanShader)pipeline.VertexShader!).Handle,((VulkanShader)pipeline.FragmentShader!).Handle,
            pipeline.DepthTestEnabled?1:0,pipeline.DepthWriteEnabled?1:0,(int)pipeline.CullMode,desc.Stride));
        VulkanNative.Check(VulkanNative.Vertices(t.Handle,data,checked(desc.Stride*desc.VertexCount)));
    }
    public void SetConstantBuffer(void* data,int sizeInBytes)
    {
        var t=Active();if(drawn||uniformsSet)throw new NotSupportedException("One uniform upload per pass is supported.");
        if(data==null||sizeInBytes<=0||sizeInBytes>16384)throw new ArgumentOutOfRangeException(nameof(sizeInBytes));
        uniformsSet=true;if(acquired)VulkanNative.Check(VulkanNative.Uniforms(t.Handle,data,sizeInBytes));
    }
    public void Draw(int count)
    {
        var t=Active();if(drawn)throw new NotSupportedException("One draw per pass is supported.");
        if(!verticesSet||!uniformsSet||count<=0||count>vertexCount)throw new InvalidOperationException("Bind vertices and uniforms before drawing.");
        drawn=true;if(acquired)VulkanNative.Check(VulkanNative.Draw(t.Handle,count));
    }
    public void EndRenderPass(){Active();ended=true;}
    public void Commit()
    {
        device.Verify();if(target is not {Borrowed:true} t||!ended)throw new InvalidOperationException("End the render pass before Commit.");
        t.Presented=acquired&&VulkanNative.Check(VulkanNative.Commit(t.Handle))!=0;target=null;pipeline=null;
    }
    public void Dispose(){target=null;pipeline=null;disposed=true;}
}
