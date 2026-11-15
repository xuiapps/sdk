#include "xui_android.h"
#include <string.h>
#include "xui_vulkan.h"
#include <android/asset_manager_jni.h>
static xui_vk_device *gpu;
static xui_vk_target *target;
static uint64_t vertex,fragment;
static int draws,width,height;
extern AAssetManager *xui_assets(xui_host *host);
static int callback(void *user,int kind,int64_t time,const void *data,int length) {
    (void)user;(void)time;if(kind!=3)return 0;
    if(length!=24)return -1;
    const int32_t *packet=data;ANativeWindow *window=(ANativeWindow*)(intptr_t)((const int64_t*)data)[2];
    if(target)xui_vk_target_destroy(target);target=NULL;
    if(!window)return 0;
    target=xui_vk_target_create(gpu,window);if(!target)return -1;
    width=packet[1];height=packet[2];return 0;
}
static int render(void) {
    if(!target)return -1;
    float clear[]={.5f,.5f,.5f,1};
    // Front-facing triangle tests vertex locations, uniform matrix layout, Y and culling.
    float vertices[]={-.7f,-.7f,.5f,1,0,0,1, .7f,-.7f,.5f,1,0,0,1, 0,.7f,.5f,1,0,0,1};
    float matrix[]={1,0,0,.2f, 0,1,0,.1f, 0,0,1,0, 0,0,0,1};
    if(xui_vk_begin(target,width,height,clear)!=1 ||
       xui_vk_pipeline(target,vertex,fragment,1,1,1,28)!=0 ||
       xui_vk_vertices(target,vertices,sizeof(vertices))!=0 ||
       xui_vk_uniforms(target,matrix,sizeof(matrix))!=0 ||
       xui_vk_draw(target,3)!=0 || xui_vk_commit(target)!=1)return -1;
    draws++;return 0;
}
JNIEXPORT void JNICALL Java_xui_runtime_CompositionSmoke_gpuRender(JNIEnv *e,jclass type) {
    (void)type;if(render()!=0){jclass error=(*e)->FindClass(e,"java/lang/IllegalStateException");(*e)->ThrowNew(e,error,xui_vk_error());(*e)->DeleteLocalRef(e,error);}
}
JNIEXPORT void JNICALL Java_xui_runtime_CompositionSmoke_gpuInit(JNIEnv *e,jclass type,jlong host) {
    (void)type;gpu=xui_vk_create();
    if(gpu){AAssetManager *assets=xui_assets((xui_host*)(intptr_t)host);vertex=xui_vk_shader(gpu,assets,"vertex.spv");fragment=xui_vk_shader(gpu,assets,"fragment.spv");}
    if(!gpu||!vertex||!fragment){jclass error=(*e)->FindClass(e,"java/lang/IllegalStateException");(*e)->ThrowNew(e,error,xui_vk_error());(*e)->DeleteLocalRef(e,error);}
}
JNIEXPORT jint JNICALL Java_xui_runtime_CompositionSmoke_gpuDraws(JNIEnv *e,jclass type) {(void)e;(void)type;return draws;}
JNIEXPORT jlong JNICALL Java_xui_runtime_CompositionSmoke_create(JNIEnv *e,jclass type,jobject activity) {
    (void)type;JavaVM *vm=NULL;(*e)->GetJavaVM(e,&vm);
    return (jlong)(intptr_t)xui_create(vm,activity,NULL,callback);
}
JNIEXPORT void JNICALL Java_xui_runtime_CompositionSmoke_call(JNIEnv *e,jclass type,jlong value,jint op,jobject input,jint length) {
    (void)type;xui_host *host=(xui_host*)(intptr_t)value;
    void *target=xui_buffer(host,length),*source=(*e)->GetDirectBufferAddress(e,input);
    if(!target||!source||length<0||length>(*e)->GetDirectBufferCapacity(e,input))return;
    memcpy(target,source,(size_t)length);
    if(xui_call(host,op,length)!=0) {
        jclass error=(*e)->FindClass(e,"java/lang/IllegalStateException");
        (*e)->ThrowNew(e,error,xui_error(host));(*e)->DeleteLocalRef(e,error);
    }
    if(op==5 && (*e)->GetDirectBufferCapacity(e,input)>=48)memcpy(source,target,48);
    // Poison the submission channel. Java must own its retained command bytes.
    memset(target,0xa5,(size_t)length);
}
JNIEXPORT void JNICALL Java_xui_runtime_CompositionSmoke_close(JNIEnv *e,jclass type,jlong value) {
    (void)e;(void)type;
    if(target)xui_vk_target_destroy(target);target=NULL;
    if(gpu){xui_vk_shader_destroy(gpu,vertex);xui_vk_shader_destroy(gpu,fragment);xui_vk_destroy(gpu);gpu=NULL;}
    xui_destroy((xui_host*)(intptr_t)value);
}
