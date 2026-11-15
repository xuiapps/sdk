#include "xui_android.h"
#include <stdlib.h>
#include <string.h>
#include <stdio.h>
#include <pthread.h>
#include <android/native_window_jni.h>
#include <android/asset_manager_jni.h>

struct xui_host {
    JavaVM *vm;
    pthread_t owner;
    jobject view, buffer, assets;
    jmethodID invoke, dispose;
    void *bytes, *user;
    int capacity;
    xui_event callback;
    char error[1024];
};
static JNIEnv *env(xui_host *host) {
    JNIEnv *e=NULL;
    // A host never silently attaches a worker thread to mutate the view hierarchy.
    if((*host->vm)->GetEnv(host->vm,(void**)&e,JNI_VERSION_1_6)!=JNI_OK)return NULL;
    return e;
}
static int failure(JNIEnv *e,xui_host *host) {
    if(!(*e)->ExceptionCheck(e))return 0;
    jthrowable exception=(*e)->ExceptionOccurred(e);(*e)->ExceptionClear(e);
    jclass type=(*e)->GetObjectClass(e,exception);
    jmethodID method=(*e)->GetMethodID(e,type,"toString","()Ljava/lang/String;");
    jstring message=method?(*e)->CallObjectMethod(e,exception,method):NULL;
    if((*e)->ExceptionCheck(e)){(*e)->ExceptionClear(e);message=NULL;}
    const char *text=message?(*e)->GetStringUTFChars(e,message,NULL):NULL;
    snprintf(host->error,sizeof(host->error),"%s",text?text:"Android Java exception");
    if(text)(*e)->ReleaseStringUTFChars(e,message,text);
    if(message)(*e)->DeleteLocalRef(e,message);
    (*e)->DeleteLocalRef(e,type);(*e)->DeleteLocalRef(e,exception);return -1;
}
static void JNICALL event(JNIEnv *e,jclass type,jlong handle,jint kind,jlong time,jobject buffer,jint length) {
    (void)type;xui_host *host=(xui_host*)(intptr_t)handle;
    if(!host||!host->callback)return;
    void *bytes=(*e)->GetDirectBufferAddress(e,buffer);
    jlong capacity=(*e)->GetDirectBufferCapacity(e,buffer);
    if(!bytes||length<0||length>capacity||host->callback(host->user,kind,time,bytes,length)!=0) {
        jclass error=(*e)->FindClass(e,"java/lang/IllegalStateException");
        (*e)->ThrowNew(e,error,"Xui callback failed; see the managed exception in logcat");(*e)->DeleteLocalRef(e,error);
    }
}
static void JNICALL surface(JNIEnv *e,jclass type,jlong handle,jint id,jobject surface,jint width,jint height) {
    (void)type;xui_host *host=(xui_host*)(intptr_t)handle;if(!host||!host->callback)return;
    ANativeWindow *window=surface?ANativeWindow_fromSurface(e,surface):NULL;
    struct { int32_t id,width,height,reserved;int64_t window; } packet={id,width,height,0,(int64_t)(intptr_t)window};
    int result=host->callback(host->user,3,0,&packet,sizeof(packet));
    if(window)ANativeWindow_release(window);
    if(result){jclass error=(*e)->FindClass(e,"java/lang/IllegalStateException");(*e)->ThrowNew(e,error,"Xui GPU surface callback failed; see managed log");(*e)->DeleteLocalRef(e,error);}
}
AAssetManager *xui_assets(xui_host *host) { return AAssetManager_fromJava(env(host),host->assets); }
xui_host *xui_create(JavaVM *vm,jobject activity,void *user,xui_event callback) {
    xui_host *host=calloc(1,sizeof(*host));if(!host)return NULL;
    host->vm=vm;host->owner=pthread_self();host->user=user;host->callback=callback;JNIEnv *e=env(host);
    if(!e){free(host);return NULL;}
    if((*e)->PushLocalFrame(e,16)<0){free(host);return NULL;}
    // Resolve through the application's class loader: FindClass from a managed
    // P/Invoke stack does not necessarily use the application's loader.
    jclass activityClass=(*e)->GetObjectClass(e,activity);
    jmethodID getLoader=(*e)->GetMethodID(e,activityClass,"getClassLoader","()Ljava/lang/ClassLoader;");
    jobject loader=(*e)->CallObjectMethod(e,activity,getLoader);
    jclass loaderClass=(*e)->GetObjectClass(e,loader);
    jmethodID load=(*e)->GetMethodID(e,loaderClass,"loadClass","(Ljava/lang/String;)Ljava/lang/Class;");
    jstring name=(*e)->NewStringUTF(e,"xui.runtime.Host");
    jclass type=(jclass)(*e)->CallObjectMethod(e,loader,load,name);
    if(failure(e,host)||!type)goto fail;
    JNINativeMethod natives[]={ {"event","(JIJLjava/nio/ByteBuffer;I)V",(void*)event}, {"surface","(JILandroid/view/Surface;II)V",(void*)surface} };
    if((*e)->RegisterNatives(e,type,natives,2)!=0)goto fail;
    jmethodID ctor=(*e)->GetMethodID(e,type,"<init>","(Landroid/app/Activity;J)V");
    host->invoke=(*e)->GetMethodID(e,type,"invoke","(ILjava/nio/ByteBuffer;I)V");
    host->dispose=(*e)->GetMethodID(e,type,"dispose","()V");
    if(failure(e,host)||!ctor||!host->invoke||!host->dispose)goto fail;
    jmethodID getAssets=(*e)->GetMethodID(e,activityClass,"getAssets","()Landroid/content/res/AssetManager;");
    jobject assets=(*e)->CallObjectMethod(e,activity,getAssets);
    if(failure(e,host)||!assets)goto fail;
    host->assets=(*e)->NewGlobalRef(e,assets);
    jobject view=(*e)->NewObject(e,type,ctor,activity,(jlong)(intptr_t)host);
    if(failure(e,host)||!view)goto fail;
    host->view=(*e)->NewGlobalRef(e,view);
    if(!host->view)goto fail;
    (*e)->PopLocalFrame(e,NULL);return host;
fail:
    failure(e,host);fprintf(stderr,"Xui Android bootstrap: %s\n",host->error);
    if(host->assets)(*e)->DeleteGlobalRef(e,host->assets);
    (*e)->PopLocalFrame(e,NULL);free(host);return NULL;
}
void *xui_buffer(xui_host *host,int capacity) {
    if(!pthread_equal(host->owner,pthread_self()))return NULL;
    JNIEnv *e=env(host);if(!e||capacity<0)return NULL;
    if(host->bytes&&capacity<=host->capacity)return host->bytes;
    if(capacity<4096)capacity=4096;
    if(host->capacity<=1073741823 && capacity<host->capacity*2)capacity=host->capacity*2;
    void *bytes=malloc((size_t)capacity);if(!bytes)return NULL;
    jobject local=(*e)->NewDirectByteBuffer(e,bytes,capacity);
    if(failure(e,host)||!local){free(bytes);return NULL;}
    jobject buffer=(*e)->NewGlobalRef(e,local);(*e)->DeleteLocalRef(e,local);
    if(!buffer){free(bytes);return NULL;}
    // Java consumes this channel synchronously; node command data is copied into
    // Java-owned direct storage before invoke returns. No stale buffer survives.
    if(host->buffer)(*e)->DeleteGlobalRef(e,host->buffer);
    free(host->bytes);host->bytes=bytes;host->capacity=capacity;host->buffer=buffer;return bytes;
}
int xui_call(xui_host *host,int operation,int length) {
    if(!pthread_equal(host->owner,pthread_self()))return -1;
    JNIEnv *e=env(host);if(!e||!host->buffer||length<0||length>host->capacity)return -1;
    host->error[0]=0;
    (*e)->CallVoidMethod(e,host->view,host->invoke,operation,host->buffer,length);
    return failure(e,host);
}
const char *xui_error(xui_host *host) {return pthread_equal(host->owner,pthread_self())?host->error:"Android bridge calls require the Activity UI thread";}
void xui_destroy(xui_host *host) {
    if(!host||!pthread_equal(host->owner,pthread_self()))return;JNIEnv *e=env(host);if(!e)return;
    host->callback=NULL;
    (*e)->CallVoidMethod(e,host->view,host->dispose);failure(e,host);
    (*e)->DeleteGlobalRef(e,host->view);
    (*e)->DeleteGlobalRef(e,host->assets);
    if(host->buffer)(*e)->DeleteGlobalRef(e,host->buffer);
    free(host->bytes);free(host);
}
