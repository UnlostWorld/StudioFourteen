#include <reshade.hpp>
#include <string>
#include <vector>
#include "Log.h"

using namespace reshade;
using namespace reshade::api;

#define RESHADE_API extern "C" __declspec(dllexport)
#define STUDIO_API extern "C" __declspec(dllexport)

RESHADE_API const char* NAME = "Studio Fourteen Reshade sync";
RESHADE_API const char* DESCRIPTION = "Enables Studio Fourteen to communicate with Reshade";

static const Log* Logger = nullptr;
static uint64_t s_depthPointer = 0;
static int s_renderedFrames = 0;

static void on_begin_render_effects(effect_runtime* runtime, command_list* cmd_list, resource_view, resource_view)
{
	// Find the depth texture pointer:
	effect_texture_variable var = runtime->find_texture_variable(nullptr, "DepthBufferTex");
	resource_view srv = resource_view();
	resource_view srv_srgb = resource_view();
	runtime->get_texture_binding(var, &srv, &srv_srgb);
	resource resource = runtime->get_device()->get_resource_from_view(srv);
	s_depthPointer = resource.handle;
}

static void on_end_render_effects(effect_runtime* runtime, command_list* cmd_list, resource_view, resource_view)
{
	s_renderedFrames++;
}

static void on_init(effect_runtime* pRuntime)
{
}

static void on_destroy(effect_runtime* pRuntime)
{
	s_renderedFrames = 0;
}

// https://stackoverflow.com/a/557774/9934501
HMODULE GetCurrentModule()
{
	HMODULE hModule = NULL;
	GetModuleHandleEx(
		GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS,
		(LPCTSTR)GetCurrentModule,
		&hModule);

	return hModule;
}

STUDIO_API bool Initialize(Log::LogDelegate onLog)
{
	Logger = new Log(onLog);

	if (!reshade::register_addon(GetCurrentModule()))
		return false;

	reshade::register_event<reshade::addon_event::init_effect_runtime>(on_init);
	reshade::register_event<reshade::addon_event::destroy_effect_runtime>(on_destroy);
	reshade::register_event<reshade::addon_event::reshade_begin_effects>(on_begin_render_effects);
	reshade::register_event<reshade::addon_event::reshade_finish_effects>(on_end_render_effects);

	Logger->Information("Add-On Started");

	return true;
}

STUDIO_API void Shutdown()
{
	reshade::unregister_event<reshade::addon_event::init_effect_runtime>(on_init);
	reshade::unregister_event<reshade::addon_event::destroy_effect_runtime>(on_destroy);
	reshade::unregister_event<reshade::addon_event::reshade_begin_effects>(on_begin_render_effects);
	reshade::unregister_event<reshade::addon_event::reshade_finish_effects>(on_end_render_effects);
	reshade::unregister_addon(GetCurrentModule());
}

STUDIO_API bool RegisterEvent(addon_event ev, void* callback)
{
	// void ReShadeRegisterEventForAddon(HMODULE module, reshade::addon_event ev, void *callback);
	static const auto func = reinterpret_cast<void(*)(HMODULE, addon_event, void*)>(GetProcAddress(internal::get_reshade_module_handle(), "ReShadeRegisterEventForAddon"));
	if (func == nullptr)
		return false;

	func(GetCurrentModule(), ev, callback);
	return true;
}

STUDIO_API bool UnregisterEvent(addon_event ev, void* callback)
{
	// void ReShadeUnregisterEventForAddon(HMODULE module, reshade::addon_event ev, void *callback);
	static const auto func = reinterpret_cast<void(*)(HMODULE, addon_event, void*)>(GetProcAddress(internal::get_reshade_module_handle(), "ReShadeUnregisterEventForAddon"));
	if (func == nullptr)
		return false;

	func(GetCurrentModule(), ev, callback);
	return true;
}

STUDIO_API uint64_t GetDepthTexture()
{
	return s_depthPointer;
}

STUDIO_API void ResetRenderedFrames()
{
	s_renderedFrames = 0;
}

STUDIO_API int GetRenderedFrames()
{
	return s_renderedFrames;
}