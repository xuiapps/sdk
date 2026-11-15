#ifndef XUI_ANDROID_H
#define XUI_ANDROID_H
#include <stdint.h>
#include <jni.h>
// All calls except host bootstrap must be on the Activity UI thread. Borrowed
// input is valid only during the callback. No Mono or .NET Android ABI dependency.
typedef int (*xui_event)(void *user, int kind, int64_t time, const void *bytes, int length);
typedef struct xui_host xui_host;
xui_host *xui_create(JavaVM *vm, jobject activity, void *user, xui_event callback);
void *xui_buffer(xui_host *host, int capacity);
int xui_call(xui_host *host, int operation, int length);
const char *xui_error(xui_host *host);
void xui_destroy(xui_host *host);
#endif
