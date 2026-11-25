#include "ProfilerAbi.h"
#include <atomic>
#include <array>
#include <mutex>
#include <string>
#include <vector>
#include <algorithm>

static_assert(sizeof(void*) == 8, "The test profiler supports 64-bit runtimes.");
static constexpr GUID UnknownId{0,0,0,{0xc0,0,0,0,0,0,0,0x46}};
static constexpr GUID FactoryId{1,0,0,{0xc0,0,0,0,0,0,0,0x46}};
static constexpr GUID CallbackId{0x176fbed1,0xa55c,0x4796,{0x98,0xca,0xa9,0xda,0x0e,0xf8,0x83,0xe7}};
static constexpr GUID Callback2Id{0x8a8cc829,0xccf2,0x49fe,{0xbb,0xae,0x0f,0x02,0x22,0x28,0x07,0x1a}};
static constexpr GUID Info2Id{0xcc0935cd,0xa518,0x487d,{0xb0,0xbb,0xa9,0x32,0x14,0xe6,0x54,0x78}};
static constexpr GUID ProfilerId{0x59a7e9f2,0x76c5,0x4d77,{0x91,0x46,0x8d,0x19,0xef,0xba,0x35,0x72}};

// ICorProfilerInfo / ICorProfilerInfo2 vtable slots from .NET 10 corprof.idl.
// Exact signatures are supplied at each call site; pointers and metadata tokens
// have different sizes on the 64-bit ABI.
template<class... Args> static HRESULT InfoCall(void* info, unsigned slot, Args... args)
{
    using Call = HRESULT (ABI *)(void*, Args...);
    return reinterpret_cast<Call>((*static_cast<void***>(info))[slot])(info, args...);
}
struct Mapping { std::uint32_t il, start, end; };
struct Frame { std::u16string module; std::uint32_t token = 0; std::int32_t il = -1; };
struct Allocation
{
    unsigned phase = 0;
    std::uint32_t bytes = 0, typeToken = 0, elementType = 0, rank = 0;
    std::u16string typeModule;
    HRESULT stackStatus = S_OK;
    bool truncated = false;
    std::vector<Frame> frames;
};
static void* info = nullptr;
static std::atomic<bool> ready{false};
static std::mutex gate;
static std::vector<Allocation> allocations;
static unsigned dropped = 0;
static constexpr unsigned MaxAllocations = 4096, MaxFrames = 64;
static thread_local unsigned phase = 0;
static thread_local bool collecting = false;
static bool armed = false;
static std::string report;

static std::u16string ModulePath(ModuleID module)
{
    if (!module) return {};
    std::array<char16_t,4096> path{};
    const std::uint8_t* base = nullptr;
    std::uint32_t count = 0;
    AssemblyID assembly = 0;
    HRESULT hr = InfoCall(info,20,module,&base,static_cast<std::uint32_t>(path.size()),&count,path.data(),&assembly);
    if (hr < 0 || count == 0 || count > path.size()) return {};
    return std::u16string(path.data(),count-1);
}
static HRESULT ABI StackFrame(FunctionID function, UINT_PTR ip, UINT_PTR frameInfo,
    std::uint32_t, std::uint8_t*, void* data)
{
    auto& allocation = *static_cast<Allocation*>(data);
    try
    {
        if (!function) return S_OK; // Native transitions carry no managed method token.
        if (allocation.frames.size() == MaxFrames) { allocation.truncated = true; return 1; }
        Frame frame;
        ClassID klass = 0;
        ModuleID module = 0;
        std::uint32_t typeArguments = 0;
        HRESULT hr = InfoCall(info,38,function,frameInfo,&klass,&module,&frame.token,
            std::uint32_t{0},&typeArguments,static_cast<ClassID*>(nullptr));
        if (hr >= 0)
        {
            frame.module = ModulePath(module);
            const std::uint8_t* start = nullptr;
            std::uint32_t size = 0, count = 0;
            if (InfoCall(info,5,function,&start,&size) >= 0 && ip >= reinterpret_cast<UINT_PTR>(start) &&
                InfoCall(info,35,function,std::uint32_t{0},&count,static_cast<Mapping*>(nullptr)) >= 0 && count <= 65536)
            {
                std::vector<Mapping> map(count);
                if (count && InfoCall(info,35,function,count,&count,map.data()) >= 0)
                {
                    // Stack IPs are return addresses: map the instruction before the return
                    // address to the call's sequence point, not the following statement.
                    const auto offset = ip - reinterpret_cast<UINT_PTR>(start);
                    const auto instruction = offset ? offset-1 : 0;
                    for (auto& entry : map)
                        if (instruction >= entry.start && instruction < entry.end && static_cast<std::int32_t>(entry.il) >= 0)
                        { frame.il = static_cast<std::int32_t>(entry.il); break; }
                }
            }
        }
        allocation.frames.push_back(std::move(frame));
        return S_OK;
    }
    catch (...) { allocation.stackStatus = E_FAIL; return E_FAIL; }
}
class Profiler final : public ICorProfilerCallback2
{
    std::atomic<ULONG> references{1};
public:
    HRESULT ABI QueryInterface(REFGUID id, void** result) override
    {
        if (!result) return E_FAIL;
        *result = nullptr;
        if (!(id == UnknownId || id == CallbackId || id == Callback2Id)) return E_NOINTERFACE;
        *result = static_cast<ICorProfilerCallback2*>(this); AddRef(); return S_OK;
    }
    ULONG ABI AddRef() override { return ++references; }
    ULONG ABI Release() override { auto left = --references; if (!left) delete this; return left; }
    HRESULT ABI Initialize(IUnknown* unknown) override
    {
        if (unknown->QueryInterface(Info2Id,&info) < 0) return E_NOINTERFACE;
        // ObjectAllocated is enabled at startup, before allocation helpers are JITted.
        // Disabling inlining preserves callers for actionable test diagnostics.
        constexpr DWORD mask = 0x00000100 | 0x00800000 | 0x10000000 | 0x00200000;
        HRESULT hr = InfoCall(info,16,mask);
        ready = hr >= 0;
        return hr;
    }
    HRESULT ABI Shutdown() override
    {
        ready = false;
        if (info) { static_cast<IUnknown*>(info)->Release(); info = nullptr; }
        return S_OK;
    }
    HRESULT ABI ObjectAllocated(ObjectID object, ClassID klass) override
    {
        if (!phase || collecting || !ready) return S_OK;
        collecting = true;
        try
        {
            std::lock_guard<std::mutex> lock(gate);
            if (armed)
            {
                if (allocations.size() == MaxAllocations) ++dropped;
                else
                {
                    Allocation allocation;
                    allocation.phase = phase;
                    if (InfoCall(info,10,object,&allocation.bytes) < 0) allocation.stackStatus = E_FAIL;
                    ModuleID module = 0; ClassID parent = 0; std::uint32_t arguments = 0;
                    // Arrays expose their element class rather than a TypeDef for the array itself.
                    ClassID elementClass = 0;
                    if (InfoCall(info,11,klass,&allocation.elementType,&elementClass,&allocation.rank) == S_OK && elementClass)
                        klass = elementClass;
                    if (InfoCall(info,41,klass,&module,&allocation.typeToken,&parent,std::uint32_t{0},&arguments,static_cast<ClassID*>(nullptr)) >= 0)
                        allocation.typeModule = ModulePath(module);
                    using StackCallback = HRESULT (ABI *)(FunctionID,UINT_PTR,UINT_PTR,std::uint32_t,std::uint8_t*,void*);
                    HRESULT hr = InfoCall(info,36,ThreadID{0},static_cast<StackCallback>(StackFrame),std::uint32_t{0},
                        static_cast<void*>(&allocation),static_cast<std::uint8_t*>(nullptr),std::uint32_t{0});
                    if (hr < 0) allocation.stackStatus = hr;
                    allocations.push_back(std::move(allocation));
                }
            }
        }
        catch (...) { ++dropped; } // No exception may cross a CLR profiler callback.
        collecting = false;
        return S_OK;
    }
};
struct Factory final : IUnknown
{
    std::atomic<ULONG> references{1};
    HRESULT ABI QueryInterface(REFGUID id, void** result) override
    {
        if (!result) return E_FAIL;
        *result = nullptr;
        if (!(id == UnknownId || id == FactoryId)) return E_NOINTERFACE;
        *result = this; AddRef(); return S_OK;
    }
    ULONG ABI AddRef() override { return ++references; }
    ULONG ABI Release() override { auto left = --references; if (!left) delete this; return left; }
    virtual HRESULT ABI CreateInstance(IUnknown* outer, REFGUID id, void** result)
    {
        if (outer) return static_cast<HRESULT>(0x80040110);
        try
        {
            auto profiler = new Profiler;
            auto hr = profiler->QueryInterface(id,result);
            profiler->Release(); return hr;
        }
        catch (...) { if (result) *result = nullptr; return E_FAIL; }
    }
    virtual HRESULT ABI LockServer(BOOL) { return S_OK; }
};
EXPORT HRESULT ABI DllGetClassObject(REFGUID clsid, REFGUID iid, void** result)
{
    if (!(clsid == ProfilerId)) return static_cast<HRESULT>(0x80040111);
    try
    {
        auto factory = new Factory;
        auto hr = factory->QueryInterface(iid,result);
        factory->Release(); return hr;
    }
    catch (...) { if (result) *result = nullptr; return E_FAIL; }
}
EXPORT HRESULT ABI DllCanUnloadNow() { return 1; }
EXPORT int XuiProfiler_Ready() { return ready ? 1 : 0; }
EXPORT void XuiProfiler_Begin(unsigned value) { phase = value; }
EXPORT void XuiProfiler_End() { phase = 0; }
EXPORT void XuiProfiler_Reset()
{
    std::lock_guard<std::mutex> lock(gate);
    allocations.clear(); dropped = 0; armed = true;
}
static void Quote(std::string& text, const std::u16string& value)
{
    static constexpr char digits[] = "0123456789abcdef";
    text += '"';
    for (char16_t c : value)
    {
        // ASCII JSON plus UTF-16 escapes avoids locale and filesystem encoding assumptions.
        if (c >= 32 && c < 127 && c != '"' && c != '\\') text += static_cast<char>(c);
        else { text += "\\u"; for (int shift=12;shift>=0;shift-=4) text += digits[(c>>shift)&15]; }
    }
    text += '"';
}
static const char* BuildReport()
{
    std::lock_guard<std::mutex> lock(gate);
    report = "{\"dropped\":"+std::to_string(dropped)+",\"allocations\":[";
    bool first = true;
    for (const auto& a : allocations)
    {
        if (!first) report += ','; first = false;
        report += "{\"phase\":"+std::to_string(a.phase)+",\"bytes\":"+std::to_string(a.bytes)+",\"typeToken\":"+std::to_string(a.typeToken)+",\"elementType\":"+std::to_string(a.elementType)+",\"rank\":"+std::to_string(a.rank)+",\"typeModule\":";
        Quote(report,a.typeModule);
        report += ",\"stackStatus\":"+std::to_string(a.stackStatus)+",\"truncated\":"+(a.truncated?"true":"false")+",\"frames\":[";
        bool firstFrame = true;
        for (const auto& f : a.frames)
        {
            if (!firstFrame) report += ','; firstFrame = false;
            report += "{\"module\":"; Quote(report,f.module);
            report += ",\"token\":"+std::to_string(f.token)+",\"ilOffset\":"+std::to_string(f.il)+'}';
        }
        report += "]}";
    }
    report += "]}";
    return report.c_str(); // Valid until the next Read. Caller copies before issuing another command.
}

EXPORT const char* XuiProfiler_Read()
{
    try { return BuildReport(); }
    catch (...) { return "{\"dropped\":1,\"allocations\":[]}"; }
}
