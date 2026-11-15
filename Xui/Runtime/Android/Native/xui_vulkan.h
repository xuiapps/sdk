#pragma once
#include <stdint.h>
#include <android/native_window.h>
#include <android/asset_manager.h>
typedef struct xui_vk_device xui_vk_device;
typedef struct xui_vk_target xui_vk_target;
const char *xui_vk_error(void);
xui_vk_device *xui_vk_create(void);
void xui_vk_destroy(xui_vk_device *d);
uint64_t xui_vk_shader(xui_vk_device *d, AAssetManager *assets, const char *path);
void xui_vk_shader_destroy(xui_vk_device *d,uint64_t shader);
xui_vk_target *xui_vk_target_create(xui_vk_device *d,ANativeWindow *window);
void xui_vk_target_destroy(xui_vk_target *t);
int xui_vk_begin(xui_vk_target *t,int width,int height,const float *color);
int xui_vk_pipeline(xui_vk_target *t,uint64_t vertex,uint64_t fragment,int depth,int write,int cull,int stride);
int xui_vk_vertices(xui_vk_target *t,const void *data,int length);
int xui_vk_uniforms(xui_vk_target *t,const void *data,int length);
int xui_vk_draw(xui_vk_target *t,int count);
int xui_vk_commit(xui_vk_target *t);
