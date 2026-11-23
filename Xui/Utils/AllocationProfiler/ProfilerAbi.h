// Portions derived from dotnet/runtime v10.0.0 src/coreclr/inc/corprof.idl.
// Licensed to the .NET Foundation under the MIT license. See LICENSE.txt.
// This is the 64-bit COM callback ABI, without Windows SDK/PAL header dependencies.
#pragma once
#include <cstdint>
#include <cstring>
#ifdef _WIN32
#define ABI __stdcall
#define EXPORT extern "C" __declspec(dllexport)
#else
#define ABI
#define EXPORT extern "C" __attribute__((visibility("default")))
#endif
using HRESULT = std::int32_t;
using ULONG = std::uint32_t;
using DWORD = std::uint32_t;
using UINT = std::uint32_t;
using BOOL = std::int32_t;
using WCHAR = char16_t;
using UINT_PTR = std::uintptr_t;
using AppDomainID = UINT_PTR;
using AssemblyID = UINT_PTR;
using ModuleID = UINT_PTR;
using ClassID = UINT_PTR;
using FunctionID = UINT_PTR;
using ThreadID = UINT_PTR;
using ObjectID = UINT_PTR;
using GCHandleID = UINT_PTR;
using COR_PRF_JIT_CACHE = std::int32_t;
using COR_PRF_TRANSITION_REASON = std::int32_t;
using COR_PRF_SUSPEND_REASON = std::int32_t;
using COR_PRF_GC_REASON = std::int32_t;
using COR_PRF_GC_ROOT_KIND = std::int32_t;
using COR_PRF_GC_ROOT_FLAGS = std::int32_t;
struct GUID { std::uint32_t a; std::uint16_t b,c; std::uint8_t d[8]; };
using REFGUID = const GUID&;
inline bool operator==(REFGUID a, REFGUID b) { return std::memcmp(&a,&b,sizeof(GUID)) == 0; }
constexpr HRESULT S_OK = 0;
constexpr HRESULT E_NOINTERFACE = static_cast<HRESULT>(0x80004002);
constexpr HRESULT E_FAIL = static_cast<HRESULT>(0x80004005);
struct IUnknown {
    virtual HRESULT ABI QueryInterface(REFGUID id, void** result) = 0;
    virtual ULONG ABI AddRef() = 0;
    virtual ULONG ABI Release() = 0;
};
struct ICorProfilerCallback : IUnknown {
    virtual HRESULT ABI Initialize(IUnknown *pICorProfilerInfoUnk) { return S_OK; }
    virtual HRESULT ABI Shutdown() { return S_OK; }
    virtual HRESULT ABI AppDomainCreationStarted(AppDomainID appDomainId) { return S_OK; }
    virtual HRESULT ABI AppDomainCreationFinished(AppDomainID appDomainId, HRESULT hrStatus) { return S_OK; }
    virtual HRESULT ABI AppDomainShutdownStarted(AppDomainID appDomainId) { return S_OK; }
    virtual HRESULT ABI AppDomainShutdownFinished(AppDomainID appDomainId, HRESULT hrStatus) { return S_OK; }
    virtual HRESULT ABI AssemblyLoadStarted(AssemblyID assemblyId) { return S_OK; }
    virtual HRESULT ABI AssemblyLoadFinished(AssemblyID assemblyId, HRESULT hrStatus) { return S_OK; }
    virtual HRESULT ABI AssemblyUnloadStarted(AssemblyID assemblyId) { return S_OK; }
    virtual HRESULT ABI AssemblyUnloadFinished(AssemblyID assemblyId, HRESULT hrStatus) { return S_OK; }
    virtual HRESULT ABI ModuleLoadStarted(ModuleID moduleId) { return S_OK; }
    virtual HRESULT ABI ModuleLoadFinished(ModuleID moduleId, HRESULT hrStatus) { return S_OK; }
    virtual HRESULT ABI ModuleUnloadStarted(ModuleID moduleId) { return S_OK; }
    virtual HRESULT ABI ModuleUnloadFinished(ModuleID moduleId, HRESULT hrStatus) { return S_OK; }
    virtual HRESULT ABI ModuleAttachedToAssembly(ModuleID moduleId, AssemblyID AssemblyId) { return S_OK; }
    virtual HRESULT ABI ClassLoadStarted(ClassID classId) { return S_OK; }
    virtual HRESULT ABI ClassLoadFinished(ClassID classId, HRESULT hrStatus) { return S_OK; }
    virtual HRESULT ABI ClassUnloadStarted(ClassID classId) { return S_OK; }
    virtual HRESULT ABI ClassUnloadFinished(ClassID classId, HRESULT hrStatus) { return S_OK; }
    virtual HRESULT ABI FunctionUnloadStarted(FunctionID functionId) { return S_OK; }
    virtual HRESULT ABI JITCompilationStarted(FunctionID functionId, BOOL fIsSafeToBlock) { return S_OK; }
    virtual HRESULT ABI JITCompilationFinished(FunctionID functionId, HRESULT hrStatus, BOOL fIsSafeToBlock) { return S_OK; }
    virtual HRESULT ABI JITCachedFunctionSearchStarted(FunctionID functionId, BOOL *pbUseCachedFunction) { return S_OK; }
    virtual HRESULT ABI JITCachedFunctionSearchFinished(FunctionID functionId, COR_PRF_JIT_CACHE result) { return S_OK; }
    virtual HRESULT ABI JITFunctionPitched(FunctionID functionId) { return S_OK; }
    virtual HRESULT ABI JITInlining(FunctionID callerId, FunctionID calleeId, BOOL *pfShouldInline) { return S_OK; }
    virtual HRESULT ABI ThreadCreated(ThreadID threadId) { return S_OK; }
    virtual HRESULT ABI ThreadDestroyed(ThreadID threadId) { return S_OK; }
    virtual HRESULT ABI ThreadAssignedToOSThread(ThreadID managedThreadId, DWORD osThreadId) { return S_OK; }
    virtual HRESULT ABI RemotingClientInvocationStarted() { return S_OK; }
    virtual HRESULT ABI RemotingClientSendingMessage(GUID *pCookie, BOOL fIsAsync) { return S_OK; }
    virtual HRESULT ABI RemotingClientReceivingReply(GUID *pCookie, BOOL fIsAsync) { return S_OK; }
    virtual HRESULT ABI RemotingClientInvocationFinished() { return S_OK; }
    virtual HRESULT ABI RemotingServerReceivingMessage(GUID *pCookie, BOOL fIsAsync) { return S_OK; }
    virtual HRESULT ABI RemotingServerInvocationStarted() { return S_OK; }
    virtual HRESULT ABI RemotingServerInvocationReturned() { return S_OK; }
    virtual HRESULT ABI RemotingServerSendingReply(GUID *pCookie, BOOL fIsAsync) { return S_OK; }
    virtual HRESULT ABI UnmanagedToManagedTransition(FunctionID functionId, COR_PRF_TRANSITION_REASON reason) { return S_OK; }
    virtual HRESULT ABI ManagedToUnmanagedTransition(FunctionID functionId, COR_PRF_TRANSITION_REASON reason) { return S_OK; }
    virtual HRESULT ABI RuntimeSuspendStarted(COR_PRF_SUSPEND_REASON suspendReason) { return S_OK; }
    virtual HRESULT ABI RuntimeSuspendFinished() { return S_OK; }
    virtual HRESULT ABI RuntimeSuspendAborted() { return S_OK; }
    virtual HRESULT ABI RuntimeResumeStarted() { return S_OK; }
    virtual HRESULT ABI RuntimeResumeFinished() { return S_OK; }
    virtual HRESULT ABI RuntimeThreadSuspended(ThreadID threadId) { return S_OK; }
    virtual HRESULT ABI RuntimeThreadResumed(ThreadID threadId) { return S_OK; }
    virtual HRESULT ABI MovedReferences(ULONG cMovedObjectIDRanges, ObjectID oldObjectIDRangeStart[] , ObjectID newObjectIDRangeStart[] , ULONG cObjectIDRangeLength[]) { return S_OK; }
    virtual HRESULT ABI ObjectAllocated(ObjectID objectId, ClassID classId) { return S_OK; }
    virtual HRESULT ABI ObjectsAllocatedByClass(ULONG cClassCount, ClassID classIds[] , ULONG cObjects[]) { return S_OK; }
    virtual HRESULT ABI ObjectReferences(ObjectID objectId, ClassID classId, ULONG cObjectRefs, ObjectID objectRefIds[]) { return S_OK; }
    virtual HRESULT ABI RootReferences(ULONG cRootRefs, ObjectID rootRefIds[]) { return S_OK; }
    virtual HRESULT ABI ExceptionThrown(ObjectID thrownObjectId) { return S_OK; }
    virtual HRESULT ABI ExceptionSearchFunctionEnter(FunctionID functionId) { return S_OK; }
    virtual HRESULT ABI ExceptionSearchFunctionLeave() { return S_OK; }
    virtual HRESULT ABI ExceptionSearchFilterEnter(FunctionID functionId) { return S_OK; }
    virtual HRESULT ABI ExceptionSearchFilterLeave() { return S_OK; }
    virtual HRESULT ABI ExceptionSearchCatcherFound(FunctionID functionId) { return S_OK; }
    virtual HRESULT ABI ExceptionOSHandlerEnter(UINT_PTR __unused) { return S_OK; }
    virtual HRESULT ABI ExceptionOSHandlerLeave(UINT_PTR __unused) { return S_OK; }
    virtual HRESULT ABI ExceptionUnwindFunctionEnter(FunctionID functionId) { return S_OK; }
    virtual HRESULT ABI ExceptionUnwindFunctionLeave() { return S_OK; }
    virtual HRESULT ABI ExceptionUnwindFinallyEnter(FunctionID functionId) { return S_OK; }
    virtual HRESULT ABI ExceptionUnwindFinallyLeave() { return S_OK; }
    virtual HRESULT ABI ExceptionCatcherEnter(FunctionID functionId, ObjectID objectId) { return S_OK; }
    virtual HRESULT ABI ExceptionCatcherLeave() { return S_OK; }
    virtual HRESULT ABI COMClassicVTableCreated(ClassID wrappedClassId, REFGUID implementedIID, void *pVTable, ULONG cSlots) { return S_OK; }
    virtual HRESULT ABI COMClassicVTableDestroyed(ClassID wrappedClassId, REFGUID implementedIID, void *pVTable) { return S_OK; }
    virtual HRESULT ABI ExceptionCLRCatcherFound() { return S_OK; }
    virtual HRESULT ABI ExceptionCLRCatcherExecute() { return S_OK; }
};
struct ICorProfilerCallback2 : ICorProfilerCallback {
    virtual HRESULT ABI ThreadNameChanged(ThreadID threadId, ULONG cchName, WCHAR name[]) { return S_OK; }
    virtual HRESULT ABI GarbageCollectionStarted(int cGenerations, BOOL generationCollected[], COR_PRF_GC_REASON reason) { return S_OK; }
    virtual HRESULT ABI SurvivingReferences(ULONG cSurvivingObjectIDRanges, ObjectID objectIDRangeStart[] , ULONG cObjectIDRangeLength[]) { return S_OK; }
    virtual HRESULT ABI GarbageCollectionFinished() { return S_OK; }
    virtual HRESULT ABI FinalizeableObjectQueued(DWORD finalizerFlags, ObjectID objectID) { return S_OK; }
    virtual HRESULT ABI RootReferences2(ULONG cRootRefs, ObjectID rootRefIds[], COR_PRF_GC_ROOT_KIND rootKinds[], COR_PRF_GC_ROOT_FLAGS rootFlags[], UINT_PTR rootIds[]) { return S_OK; }
    virtual HRESULT ABI HandleCreated(GCHandleID handleId, ObjectID initialObjectId) { return S_OK; }
    virtual HRESULT ABI HandleDestroyed(GCHandleID handleId) { return S_OK; }
};
