// Native Vulkan presentation. No Java graphics calls, managed wrappers, or pixel readback.
#define VK_USE_PLATFORM_ANDROID_KHR 1
#include "xui_vulkan.h"
#include <vulkan/vulkan.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <pthread.h>

static _Thread_local char last_error[256];
const char *xui_vk_error(void) { return last_error; }
static int error(const char *operation,VkResult result) { snprintf(last_error,sizeof(last_error),"%s: VkResult %d",operation,result);return -1; }
#define CHECK(call) do { VkResult r=(call);if(r!=VK_SUCCESS){error(#call,r);goto fail;} } while(0)
#define MAX_IMAGES 8
struct xui_vk_device {
    VkInstance instance; VkPhysicalDevice physical; VkDevice device; VkQueue queue;
    uint32_t family; VkPhysicalDeviceMemoryProperties memory; pthread_t owner;
};
typedef struct { VkBuffer buffer;VkDeviceMemory memory;void *mapped;VkDeviceSize capacity; } upload;
struct xui_vk_target {
    xui_vk_device *d; ANativeWindow *window; VkSurfaceKHR surface; VkSwapchainKHR swapchain;
    VkFormat format;VkExtent2D size;uint32_t count,index;
    VkImage images[MAX_IMAGES],depth;VkDeviceMemory depthMemory;
    VkImageView views[MAX_IMAGES],depthView;VkFramebuffer frames[MAX_IMAGES];
    VkSemaphore acquired,finished[MAX_IMAGES];VkFence fence;
    VkRenderPass pass;VkCommandPool pool;VkCommandBuffer command;
    VkDescriptorSetLayout descriptorLayout;VkDescriptorPool descriptorPool;VkDescriptorSet descriptor;
    VkPipelineLayout layout;VkPipeline pipeline;VkShaderModule vertex,fragment;
    int depthTest,depthWrite,cull,stride,recording;
    upload vertices,uniforms;
};
static int owner(xui_vk_device *d) { if(d&&pthread_equal(d->owner,pthread_self()))return 1;error("Vulkan calls require the window UI thread",VK_ERROR_INITIALIZATION_FAILED);return 0; }
static uint32_t memory_type(xui_vk_device *d,uint32_t bits,VkMemoryPropertyFlags flags) {
    for(uint32_t i=0;i<d->memory.memoryTypeCount;i++)if((bits&(1u<<i))&&(d->memory.memoryTypes[i].propertyFlags&flags)==flags)return i;
    return UINT32_MAX;
}
xui_vk_device *xui_vk_create(void) {
    last_error[0]=0;xui_vk_device *d=calloc(1,sizeof(*d));if(!d)return NULL;d->owner=pthread_self();
    const char *extensions[]={VK_KHR_SURFACE_EXTENSION_NAME,VK_KHR_ANDROID_SURFACE_EXTENSION_NAME};
    VkApplicationInfo app={.sType=VK_STRUCTURE_TYPE_APPLICATION_INFO,.pApplicationName="Xui",.apiVersion=VK_API_VERSION_1_0};
    VkInstanceCreateInfo ci={.sType=VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO,.pApplicationInfo=&app,.enabledExtensionCount=2,.ppEnabledExtensionNames=extensions};
    CHECK(vkCreateInstance(&ci,NULL,&d->instance));
    uint32_t count=0;CHECK(vkEnumeratePhysicalDevices(d->instance,&count,NULL));
    if(!count){error("No Vulkan physical device",VK_ERROR_INITIALIZATION_FAILED);goto fail;}
    VkPhysicalDevice *devices=calloc(count,sizeof(*devices));if(!devices)goto fail;
    VkResult result=vkEnumeratePhysicalDevices(d->instance,&count,devices);
    if(result!=VK_SUCCESS){free(devices);error("vkEnumeratePhysicalDevices",result);goto fail;}
    for(uint32_t i=0;i<count&&!d->physical;i++) {
        uint32_t n=0;vkGetPhysicalDeviceQueueFamilyProperties(devices[i],&n,NULL);
        VkQueueFamilyProperties *q=calloc(n,sizeof(*q));if(!q){free(devices);goto fail;}
        vkGetPhysicalDeviceQueueFamilyProperties(devices[i],&n,q);
        for(uint32_t j=0;j<n;j++)if(q[j].queueFlags&VK_QUEUE_GRAPHICS_BIT){d->physical=devices[i];d->family=j;break;}
        free(q);
    }
    free(devices);if(!d->physical)goto fail;
    float priority=1;VkDeviceQueueCreateInfo qi={.sType=VK_STRUCTURE_TYPE_DEVICE_QUEUE_CREATE_INFO,.queueFamilyIndex=d->family,.queueCount=1,.pQueuePriorities=&priority};
    const char *deviceExtensions[]={VK_KHR_SWAPCHAIN_EXTENSION_NAME};
    VkDeviceCreateInfo di={.sType=VK_STRUCTURE_TYPE_DEVICE_CREATE_INFO,.queueCreateInfoCount=1,.pQueueCreateInfos=&qi,.enabledExtensionCount=1,.ppEnabledExtensionNames=deviceExtensions};
    CHECK(vkCreateDevice(d->physical,&di,NULL,&d->device));
    vkGetDeviceQueue(d->device,d->family,0,&d->queue);vkGetPhysicalDeviceMemoryProperties(d->physical,&d->memory);return d;
fail:xui_vk_destroy(d);return NULL;
}
void xui_vk_destroy(xui_vk_device *d) {if(!d||!owner(d))return;if(d->device){vkDeviceWaitIdle(d->device);vkDestroyDevice(d->device,NULL);}if(d->instance)vkDestroyInstance(d->instance,NULL);free(d);}
uint64_t xui_vk_shader(xui_vk_device *d,AAssetManager *assets,const char *path) {
    if(!owner(d))return 0;
    AAsset *asset=AAssetManager_open(assets,path,AASSET_MODE_BUFFER);
    if(!asset){snprintf(last_error,sizeof(last_error),"Missing build-time SPIR-V asset: %s",path);return 0;}
    off_t size=AAsset_getLength(asset);void *data=size>0?malloc((size_t)size):NULL;VkShaderModule shader=VK_NULL_HANDLE;
    if(!data||size%4||AAsset_read(asset,data,(size_t)size)!=size){error("Invalid SPIR-V asset",VK_ERROR_INITIALIZATION_FAILED);goto fail;}
    VkShaderModuleCreateInfo ci={.sType=VK_STRUCTURE_TYPE_SHADER_MODULE_CREATE_INFO,.codeSize=(size_t)size,.pCode=data};
    if(vkCreateShaderModule(d->device,&ci,NULL,&shader)!=VK_SUCCESS)error("vkCreateShaderModule",VK_ERROR_INITIALIZATION_FAILED);
fail:free(data);AAsset_close(asset);return (uint64_t)shader;
}
void xui_vk_shader_destroy(xui_vk_device *d,uint64_t shader){if(owner(d)&&shader)vkDestroyShaderModule(d->device,(VkShaderModule)shader,NULL);}
static void free_upload(xui_vk_device *d,upload *u) {
    if(u->mapped)vkUnmapMemory(d->device,u->memory);if(u->buffer)vkDestroyBuffer(d->device,u->buffer,NULL);if(u->memory)vkFreeMemory(d->device,u->memory,NULL);memset(u,0,sizeof(*u));
}
static int ensure_upload(xui_vk_device *d,upload *u,VkDeviceSize size,VkBufferUsageFlags usage) {
    if(u->capacity>=size)return 0;free_upload(d,u);
    VkBufferCreateInfo ci={.sType=VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO,.size=size,.usage=usage,.sharingMode=VK_SHARING_MODE_EXCLUSIVE};
    CHECK(vkCreateBuffer(d->device,&ci,NULL,&u->buffer));VkMemoryRequirements req;vkGetBufferMemoryRequirements(d->device,u->buffer,&req);
    uint32_t type=memory_type(d,req.memoryTypeBits,VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT|VK_MEMORY_PROPERTY_HOST_COHERENT_BIT);
    if(type==UINT32_MAX){error("No coherent upload memory",VK_ERROR_FEATURE_NOT_PRESENT);goto fail;}
    VkMemoryAllocateInfo ai={.sType=VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO,.allocationSize=req.size,.memoryTypeIndex=type};
    CHECK(vkAllocateMemory(d->device,&ai,NULL,&u->memory));CHECK(vkBindBufferMemory(d->device,u->buffer,u->memory,0));
    CHECK(vkMapMemory(d->device,u->memory,0,size,0,&u->mapped));u->capacity=size;return 0;
fail:free_upload(d,u);return -1;
}
static void free_swapchain(xui_vk_target *t) {
    VkDevice d=t->d->device;
    if(t->pipeline)vkDestroyPipeline(d,t->pipeline,NULL);t->pipeline=VK_NULL_HANDLE;
    for(uint32_t i=0;i<t->count;i++) {if(t->frames[i])vkDestroyFramebuffer(d,t->frames[i],NULL);if(t->views[i])vkDestroyImageView(d,t->views[i],NULL);if(t->finished[i])vkDestroySemaphore(d,t->finished[i],NULL);t->frames[i]=VK_NULL_HANDLE;t->views[i]=VK_NULL_HANDLE;t->finished[i]=VK_NULL_HANDLE;}
    t->count=0;
    if(t->depthView)vkDestroyImageView(d,t->depthView,NULL);t->depthView=VK_NULL_HANDLE;
    if(t->depth)vkDestroyImage(d,t->depth,NULL);t->depth=VK_NULL_HANDLE;
    if(t->depthMemory)vkFreeMemory(d,t->depthMemory,NULL);t->depthMemory=VK_NULL_HANDLE;
    if(t->pass)vkDestroyRenderPass(d,t->pass,NULL);t->pass=VK_NULL_HANDLE;
    if(t->swapchain)vkDestroySwapchainKHR(d,t->swapchain,NULL);t->swapchain=VK_NULL_HANDLE;
}
static int swapchain(xui_vk_target *t,int width,int height) {
    xui_vk_device *d=t->d;CHECK(vkDeviceWaitIdle(d->device));free_swapchain(t);
    VkSurfaceCapabilitiesKHR caps;CHECK(vkGetPhysicalDeviceSurfaceCapabilitiesKHR(d->physical,t->surface,&caps));
    t->size=caps.currentExtent;
    if(t->size.width==UINT32_MAX){t->size.width=(uint32_t)width;t->size.height=(uint32_t)height;}
    if(!t->size.width||!t->size.height)return 0;
    uint32_t n=0;CHECK(vkGetPhysicalDeviceSurfaceFormatsKHR(d->physical,t->surface,&n,NULL));
    if(!n||n>256){error("Invalid surface format count",VK_ERROR_FORMAT_NOT_SUPPORTED);goto fail;}
    VkSurfaceFormatKHR formats[256];CHECK(vkGetPhysicalDeviceSurfaceFormatsKHR(d->physical,t->surface,&n,formats));
    VkSurfaceFormatKHR format=formats[0];for(uint32_t i=0;i<n;i++)if(formats[i].format==VK_FORMAT_R8G8B8A8_UNORM||formats[i].format==VK_FORMAT_B8G8R8A8_UNORM){format=formats[i];break;}
    if(format.format==VK_FORMAT_UNDEFINED)format.format=VK_FORMAT_R8G8B8A8_UNORM;
    t->format=format.format;uint32_t count=caps.minImageCount+1;if(caps.maxImageCount&&count>caps.maxImageCount)count=caps.maxImageCount;
    VkCompositeAlphaFlagBitsKHR alpha=VK_COMPOSITE_ALPHA_OPAQUE_BIT_KHR;
    if(!(caps.supportedCompositeAlpha&alpha))alpha=(VkCompositeAlphaFlagBitsKHR)(caps.supportedCompositeAlpha&(~caps.supportedCompositeAlpha+1));
    VkSwapchainCreateInfoKHR ci={.sType=VK_STRUCTURE_TYPE_SWAPCHAIN_CREATE_INFO_KHR,.surface=t->surface,.minImageCount=count,.imageFormat=format.format,.imageColorSpace=format.colorSpace,.imageExtent=t->size,.imageArrayLayers=1,.imageUsage=VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT,.imageSharingMode=VK_SHARING_MODE_EXCLUSIVE,.preTransform=caps.currentTransform,.compositeAlpha=alpha,.presentMode=VK_PRESENT_MODE_FIFO_KHR,.clipped=VK_TRUE};
    CHECK(vkCreateSwapchainKHR(d->device,&ci,NULL,&t->swapchain));CHECK(vkGetSwapchainImagesKHR(d->device,t->swapchain,&count,NULL));
    if(count>MAX_IMAGES){error("Too many swapchain images",VK_ERROR_INITIALIZATION_FAILED);goto fail;}
    t->count=count;CHECK(vkGetSwapchainImagesKHR(d->device,t->swapchain,&count,t->images));
    VkFormat depthFormat=VK_FORMAT_D16_UNORM;VkFormatProperties props;vkGetPhysicalDeviceFormatProperties(d->physical,depthFormat,&props);
    if(!(props.optimalTilingFeatures&VK_FORMAT_FEATURE_DEPTH_STENCIL_ATTACHMENT_BIT)){error("No D16 depth attachment",VK_ERROR_FORMAT_NOT_SUPPORTED);goto fail;}
    VkAttachmentDescription attachments[2]={
        {.format=t->format,.samples=VK_SAMPLE_COUNT_1_BIT,.loadOp=VK_ATTACHMENT_LOAD_OP_CLEAR,.storeOp=VK_ATTACHMENT_STORE_OP_STORE,.stencilLoadOp=VK_ATTACHMENT_LOAD_OP_DONT_CARE,.stencilStoreOp=VK_ATTACHMENT_STORE_OP_DONT_CARE,.initialLayout=VK_IMAGE_LAYOUT_UNDEFINED,.finalLayout=VK_IMAGE_LAYOUT_PRESENT_SRC_KHR},
        {.format=depthFormat,.samples=VK_SAMPLE_COUNT_1_BIT,.loadOp=VK_ATTACHMENT_LOAD_OP_CLEAR,.storeOp=VK_ATTACHMENT_STORE_OP_DONT_CARE,.stencilLoadOp=VK_ATTACHMENT_LOAD_OP_DONT_CARE,.stencilStoreOp=VK_ATTACHMENT_STORE_OP_DONT_CARE,.initialLayout=VK_IMAGE_LAYOUT_UNDEFINED,.finalLayout=VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL}};
    VkAttachmentReference color={0,VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL},depth={1,VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL};
    VkSubpassDescription sub={.pipelineBindPoint=VK_PIPELINE_BIND_POINT_GRAPHICS,.colorAttachmentCount=1,.pColorAttachments=&color,.pDepthStencilAttachment=&depth};
    VkSubpassDependency dep={.srcSubpass=VK_SUBPASS_EXTERNAL,.dstSubpass=0,.srcStageMask=VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT|VK_PIPELINE_STAGE_EARLY_FRAGMENT_TESTS_BIT,.dstStageMask=VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT|VK_PIPELINE_STAGE_EARLY_FRAGMENT_TESTS_BIT,.dstAccessMask=VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT|VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_WRITE_BIT};
    VkRenderPassCreateInfo ri={.sType=VK_STRUCTURE_TYPE_RENDER_PASS_CREATE_INFO,.attachmentCount=2,.pAttachments=attachments,.subpassCount=1,.pSubpasses=&sub,.dependencyCount=1,.pDependencies=&dep};
    CHECK(vkCreateRenderPass(d->device,&ri,NULL,&t->pass));
    VkImageCreateInfo ii={.sType=VK_STRUCTURE_TYPE_IMAGE_CREATE_INFO,.imageType=VK_IMAGE_TYPE_2D,.format=depthFormat,.extent={t->size.width,t->size.height,1},.mipLevels=1,.arrayLayers=1,.samples=VK_SAMPLE_COUNT_1_BIT,.tiling=VK_IMAGE_TILING_OPTIMAL,.usage=VK_IMAGE_USAGE_DEPTH_STENCIL_ATTACHMENT_BIT,.sharingMode=VK_SHARING_MODE_EXCLUSIVE};
    CHECK(vkCreateImage(d->device,&ii,NULL,&t->depth));VkMemoryRequirements req;vkGetImageMemoryRequirements(d->device,t->depth,&req);
    uint32_t mt=memory_type(d,req.memoryTypeBits,VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT);if(mt==UINT32_MAX)goto fail;
    VkMemoryAllocateInfo ai={.sType=VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO,.allocationSize=req.size,.memoryTypeIndex=mt};
    CHECK(vkAllocateMemory(d->device,&ai,NULL,&t->depthMemory));CHECK(vkBindImageMemory(d->device,t->depth,t->depthMemory,0));
    VkImageViewCreateInfo vi={.sType=VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO,.image=t->depth,.viewType=VK_IMAGE_VIEW_TYPE_2D,.format=depthFormat,.subresourceRange={VK_IMAGE_ASPECT_DEPTH_BIT,0,1,0,1}};
    CHECK(vkCreateImageView(d->device,&vi,NULL,&t->depthView));
    VkSemaphoreCreateInfo si={.sType=VK_STRUCTURE_TYPE_SEMAPHORE_CREATE_INFO};
    for(uint32_t i=0;i<count;i++) {
        vi.image=t->images[i];vi.format=t->format;vi.subresourceRange.aspectMask=VK_IMAGE_ASPECT_COLOR_BIT;
        CHECK(vkCreateImageView(d->device,&vi,NULL,&t->views[i]));VkImageView views[]={t->views[i],t->depthView};
        VkFramebufferCreateInfo fi={.sType=VK_STRUCTURE_TYPE_FRAMEBUFFER_CREATE_INFO,.renderPass=t->pass,.attachmentCount=2,.pAttachments=views,.width=t->size.width,.height=t->size.height,.layers=1};
        CHECK(vkCreateFramebuffer(d->device,&fi,NULL,&t->frames[i]));CHECK(vkCreateSemaphore(d->device,&si,NULL,&t->finished[i]));
    }
    return 1;
fail:free_swapchain(t);return -1;
}
xui_vk_target *xui_vk_target_create(xui_vk_device *d,ANativeWindow *window) {
    if(!owner(d)||!window)return NULL;
    xui_vk_target *t=calloc(1,sizeof(*t));if(!t)return NULL;t->d=d;t->window=window;ANativeWindow_acquire(window);
    VkAndroidSurfaceCreateInfoKHR si={.sType=VK_STRUCTURE_TYPE_ANDROID_SURFACE_CREATE_INFO_KHR,.window=window};
    CHECK(vkCreateAndroidSurfaceKHR(d->instance,&si,NULL,&t->surface));VkBool32 supported=0;
    CHECK(vkGetPhysicalDeviceSurfaceSupportKHR(d->physical,d->family,t->surface,&supported));if(!supported){error("Queue cannot present",VK_ERROR_FEATURE_NOT_PRESENT);goto fail;}
    VkCommandPoolCreateInfo pi={.sType=VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO,.queueFamilyIndex=d->family,.flags=VK_COMMAND_POOL_CREATE_RESET_COMMAND_BUFFER_BIT};
    CHECK(vkCreateCommandPool(d->device,&pi,NULL,&t->pool));
    VkCommandBufferAllocateInfo ai={.sType=VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO,.commandPool=t->pool,.level=VK_COMMAND_BUFFER_LEVEL_PRIMARY,.commandBufferCount=1};
    CHECK(vkAllocateCommandBuffers(d->device,&ai,&t->command));
    VkFenceCreateInfo fi={.sType=VK_STRUCTURE_TYPE_FENCE_CREATE_INFO,.flags=VK_FENCE_CREATE_SIGNALED_BIT};CHECK(vkCreateFence(d->device,&fi,NULL,&t->fence));
    VkSemaphoreCreateInfo sem={.sType=VK_STRUCTURE_TYPE_SEMAPHORE_CREATE_INFO};CHECK(vkCreateSemaphore(d->device,&sem,NULL,&t->acquired));
    VkDescriptorSetLayoutBinding binding={.binding=0,.descriptorType=VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER,.descriptorCount=1,.stageFlags=VK_SHADER_STAGE_VERTEX_BIT|VK_SHADER_STAGE_FRAGMENT_BIT};
    VkDescriptorSetLayoutCreateInfo li={.sType=VK_STRUCTURE_TYPE_DESCRIPTOR_SET_LAYOUT_CREATE_INFO,.bindingCount=1,.pBindings=&binding};CHECK(vkCreateDescriptorSetLayout(d->device,&li,NULL,&t->descriptorLayout));
    VkPipelineLayoutCreateInfo pl={.sType=VK_STRUCTURE_TYPE_PIPELINE_LAYOUT_CREATE_INFO,.setLayoutCount=1,.pSetLayouts=&t->descriptorLayout};CHECK(vkCreatePipelineLayout(d->device,&pl,NULL,&t->layout));
    VkDescriptorPoolSize ps={VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER,1};VkDescriptorPoolCreateInfo dp={.sType=VK_STRUCTURE_TYPE_DESCRIPTOR_POOL_CREATE_INFO,.maxSets=1,.poolSizeCount=1,.pPoolSizes=&ps};CHECK(vkCreateDescriptorPool(d->device,&dp,NULL,&t->descriptorPool));
    VkDescriptorSetAllocateInfo da={.sType=VK_STRUCTURE_TYPE_DESCRIPTOR_SET_ALLOCATE_INFO,.descriptorPool=t->descriptorPool,.descriptorSetCount=1,.pSetLayouts=&t->descriptorLayout};CHECK(vkAllocateDescriptorSets(d->device,&da,&t->descriptor));return t;
fail:xui_vk_target_destroy(t);return NULL;
}
void xui_vk_target_destroy(xui_vk_target *t) {
    if(!t||!owner(t->d))return;VkDevice d=t->d->device;vkDeviceWaitIdle(d);free_swapchain(t);
    free_upload(t->d,&t->vertices);free_upload(t->d,&t->uniforms);
    if(t->descriptorPool)vkDestroyDescriptorPool(d,t->descriptorPool,NULL);
    if(t->layout)vkDestroyPipelineLayout(d,t->layout,NULL);if(t->descriptorLayout)vkDestroyDescriptorSetLayout(d,t->descriptorLayout,NULL);
    if(t->acquired)vkDestroySemaphore(d,t->acquired,NULL);if(t->fence)vkDestroyFence(d,t->fence,NULL);
    if(t->pool)vkDestroyCommandPool(d,t->pool,NULL);if(t->surface)vkDestroySurfaceKHR(t->d->instance,t->surface,NULL);
    ANativeWindow_release(t->window);free(t);
}
int xui_vk_begin(xui_vk_target *t,int width,int height,const float *color) {
    if(!owner(t->d)||t->recording)return -1;
    if(width<=0||height<=0)return 0;
    if(!t->swapchain||t->size.width!=(uint32_t)width||t->size.height!=(uint32_t)height){int r=swapchain(t,width,height);if(r<=0)return r;}
    CHECK(vkWaitForFences(t->d->device,1,&t->fence,VK_TRUE,UINT64_MAX));
    VkResult acquired=vkAcquireNextImageKHR(t->d->device,t->swapchain,UINT64_MAX,t->acquired,VK_NULL_HANDLE,&t->index);
    if(acquired==VK_ERROR_OUT_OF_DATE_KHR){int r=swapchain(t,width,height);return r<0?-1:0;}
    if(acquired!=VK_SUCCESS&&acquired!=VK_SUBOPTIMAL_KHR)return error("vkAcquireNextImageKHR",acquired);
    CHECK(vkResetCommandBuffer(t->command,0));VkCommandBufferBeginInfo bi={.sType=VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO,.flags=VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT};CHECK(vkBeginCommandBuffer(t->command,&bi));
    VkClearValue values[2];memset(values,0,sizeof(values));memcpy(values[0].color.float32,color,4*sizeof(float));values[1].depthStencil.depth=1;
    VkRenderPassBeginInfo ri={.sType=VK_STRUCTURE_TYPE_RENDER_PASS_BEGIN_INFO,.renderPass=t->pass,.framebuffer=t->frames[t->index],.renderArea={{0,0},t->size},.clearValueCount=2,.pClearValues=values};vkCmdBeginRenderPass(t->command,&ri,VK_SUBPASS_CONTENTS_INLINE);
    VkViewport vp={0,0,(float)t->size.width,(float)t->size.height,0,1};VkRect2D sc={{0,0},t->size};vkCmdSetViewport(t->command,0,1,&vp);vkCmdSetScissor(t->command,0,1,&sc);t->recording=1;return 1;
fail:return -1;
}
int xui_vk_pipeline(xui_vk_target *t,uint64_t vertex,uint64_t fragment,int depth,int write,int cull,int stride) {
    if(!owner(t->d)||!t->recording)return -1;
    if(!t->pipeline||t->vertex!=(VkShaderModule)vertex||t->fragment!=(VkShaderModule)fragment||t->depthTest!=depth||t->depthWrite!=write||t->cull!=cull||t->stride!=stride) {
        if(t->pipeline)vkDestroyPipeline(t->d->device,t->pipeline,NULL);t->pipeline=VK_NULL_HANDLE;
        VkPipelineShaderStageCreateInfo stages[2]={
            {.sType=VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO,.stage=VK_SHADER_STAGE_VERTEX_BIT,.module=(VkShaderModule)vertex,.pName="VSMain"},
            {.sType=VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO,.stage=VK_SHADER_STAGE_FRAGMENT_BIT,.module=(VkShaderModule)fragment,.pName="PSMain"}};
        VkVertexInputBindingDescription binding={0,(uint32_t)stride,VK_VERTEX_INPUT_RATE_VERTEX};
        // The current cross-platform GPU contract uses Position(float3), Color(float4).
        VkVertexInputAttributeDescription attrs[2]={{0,0,VK_FORMAT_R32G32B32_SFLOAT,0},{1,0,VK_FORMAT_R32G32B32A32_SFLOAT,12}};
        VkPipelineVertexInputStateCreateInfo vi={.sType=VK_STRUCTURE_TYPE_PIPELINE_VERTEX_INPUT_STATE_CREATE_INFO,.vertexBindingDescriptionCount=1,.pVertexBindingDescriptions=&binding,.vertexAttributeDescriptionCount=2,.pVertexAttributeDescriptions=attrs};
        VkPipelineInputAssemblyStateCreateInfo ia={.sType=VK_STRUCTURE_TYPE_PIPELINE_INPUT_ASSEMBLY_STATE_CREATE_INFO,.topology=VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST};
        VkPipelineViewportStateCreateInfo vp={.sType=VK_STRUCTURE_TYPE_PIPELINE_VIEWPORT_STATE_CREATE_INFO,.viewportCount=1,.scissorCount=1};
        VkPipelineRasterizationStateCreateInfo raster={.sType=VK_STRUCTURE_TYPE_PIPELINE_RASTERIZATION_STATE_CREATE_INFO,.polygonMode=VK_POLYGON_MODE_FILL,.cullMode=cull==1?VK_CULL_MODE_BACK_BIT:cull==2?VK_CULL_MODE_FRONT_BIT:VK_CULL_MODE_NONE,.frontFace=VK_FRONT_FACE_COUNTER_CLOCKWISE,.lineWidth=1};
        VkPipelineMultisampleStateCreateInfo ms={.sType=VK_STRUCTURE_TYPE_PIPELINE_MULTISAMPLE_STATE_CREATE_INFO,.rasterizationSamples=VK_SAMPLE_COUNT_1_BIT};
        VkPipelineDepthStencilStateCreateInfo ds={.sType=VK_STRUCTURE_TYPE_PIPELINE_DEPTH_STENCIL_STATE_CREATE_INFO,.depthTestEnable=depth,.depthWriteEnable=write,.depthCompareOp=VK_COMPARE_OP_LESS};
        VkPipelineColorBlendAttachmentState blend={.colorWriteMask=15};VkPipelineColorBlendStateCreateInfo cb={.sType=VK_STRUCTURE_TYPE_PIPELINE_COLOR_BLEND_STATE_CREATE_INFO,.attachmentCount=1,.pAttachments=&blend};
        VkDynamicState states[]={VK_DYNAMIC_STATE_VIEWPORT,VK_DYNAMIC_STATE_SCISSOR};VkPipelineDynamicStateCreateInfo dyn={.sType=VK_STRUCTURE_TYPE_PIPELINE_DYNAMIC_STATE_CREATE_INFO,.dynamicStateCount=2,.pDynamicStates=states};
        VkGraphicsPipelineCreateInfo pi={.sType=VK_STRUCTURE_TYPE_GRAPHICS_PIPELINE_CREATE_INFO,.stageCount=2,.pStages=stages,.pVertexInputState=&vi,.pInputAssemblyState=&ia,.pViewportState=&vp,.pRasterizationState=&raster,.pMultisampleState=&ms,.pDepthStencilState=&ds,.pColorBlendState=&cb,.pDynamicState=&dyn,.layout=t->layout,.renderPass=t->pass};
        CHECK(vkCreateGraphicsPipelines(t->d->device,VK_NULL_HANDLE,1,&pi,NULL,&t->pipeline));t->vertex=(VkShaderModule)vertex;t->fragment=(VkShaderModule)fragment;t->depthTest=depth;t->depthWrite=write;t->cull=cull;t->stride=stride;
    }
    vkCmdBindPipeline(t->command,VK_PIPELINE_BIND_POINT_GRAPHICS,t->pipeline);return 0;
fail:return -1;
}
int xui_vk_vertices(xui_vk_target *t,const void *data,int length) {
    if(!owner(t->d)||!t->recording||length<=0)return -1;
    if(ensure_upload(t->d,&t->vertices,(VkDeviceSize)length,VK_BUFFER_USAGE_VERTEX_BUFFER_BIT))return -1;
    memcpy(t->vertices.mapped,data,(size_t)length);VkDeviceSize offset=0;vkCmdBindVertexBuffers(t->command,0,1,&t->vertices.buffer,&offset);return 0;
}
int xui_vk_uniforms(xui_vk_target *t,const void *data,int length) {
    if(!owner(t->d)||!t->recording||length<=0||length>16384)return -1;
    if(ensure_upload(t->d,&t->uniforms,(VkDeviceSize)length,VK_BUFFER_USAGE_UNIFORM_BUFFER_BIT))return -1;
    memcpy(t->uniforms.mapped,data,(size_t)length);
    VkDescriptorBufferInfo bi={t->uniforms.buffer,0,(VkDeviceSize)length};VkWriteDescriptorSet wi={.sType=VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET,.dstSet=t->descriptor,.dstBinding=0,.descriptorCount=1,.descriptorType=VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER,.pBufferInfo=&bi};
    vkUpdateDescriptorSets(t->d->device,1,&wi,0,NULL);vkCmdBindDescriptorSets(t->command,VK_PIPELINE_BIND_POINT_GRAPHICS,t->layout,0,1,&t->descriptor,0,NULL);return 0;
}
int xui_vk_draw(xui_vk_target *t,int count) {if(!owner(t->d)||!t->recording||count<=0)return -1;vkCmdDraw(t->command,(uint32_t)count,1,0,0);return 0;}
int xui_vk_commit(xui_vk_target *t) {
    if(!owner(t->d)||!t->recording)return -1;t->recording=0;vkCmdEndRenderPass(t->command);CHECK(vkEndCommandBuffer(t->command));
    CHECK(vkResetFences(t->d->device,1,&t->fence));VkPipelineStageFlags wait=VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT;
    // Completion semaphores belong to acquired images, not CPU frames. Reacquiring
    // this image guarantees the previous presentation has consumed its semaphore.
    VkSubmitInfo si={.sType=VK_STRUCTURE_TYPE_SUBMIT_INFO,.waitSemaphoreCount=1,.pWaitSemaphores=&t->acquired,.pWaitDstStageMask=&wait,.commandBufferCount=1,.pCommandBuffers=&t->command,.signalSemaphoreCount=1,.pSignalSemaphores=&t->finished[t->index]};
    CHECK(vkQueueSubmit(t->d->queue,1,&si,t->fence));
    VkPresentInfoKHR pi={.sType=VK_STRUCTURE_TYPE_PRESENT_INFO_KHR,.waitSemaphoreCount=1,.pWaitSemaphores=&t->finished[t->index],.swapchainCount=1,.pSwapchains=&t->swapchain,.pImageIndices=&t->index};
    VkResult result=vkQueuePresentKHR(t->d->queue,&pi);
    if(result==VK_ERROR_OUT_OF_DATE_KHR||result==VK_SUBOPTIMAL_KHR){CHECK(vkDeviceWaitIdle(t->d->device));free_swapchain(t);return 0;}
    if(result!=VK_SUCCESS)return error("vkQueuePresentKHR",result);return 1;
fail:return -1;
}
