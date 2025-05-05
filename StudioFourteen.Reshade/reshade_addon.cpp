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

typedef void (*EmptyDelegate)();

static const Log* Logger = nullptr;
static effect_runtime* s_pRuntime = nullptr;
static EmptyDelegate m_onBeginRenderEffects;
static EmptyDelegate m_onFinishRenderEffects;

static int s_renderedFrames = 0;
STUDIO_API void ResetRenderedFrames() { s_renderedFrames = 0; }
STUDIO_API int GetRenderedFrames() { return s_renderedFrames; }

static bool s_effectsState = false;
STUDIO_API bool GetEffectsState() { return s_effectsState; }
STUDIO_API void SetEffectsState(bool state)
{
	if (s_pRuntime == nullptr)
		return;

	s_pRuntime->set_effects_state(state);
	s_effectsState = state;
}

static bool s_isOverlayOpen = false;
STUDIO_API bool GetIsOverlayOpen() { return s_isOverlayOpen; }

static void on_begin_render_effects(effect_runtime* runtime, command_list* cmd_list, resource_view, resource_view)
{
	m_onBeginRenderEffects();
}

static void on_end_render_effects(effect_runtime* pRuntime, command_list*, resource_view, resource_view)
{
	if (s_pRuntime != nullptr && s_pRuntime != pRuntime)
		Logger->Error("Effect runtime changed! (Do you have MaxEffectRuntimes set to 1?)");

	s_pRuntime = pRuntime;
	s_effectsState = true;
	s_renderedFrames++;

	m_onFinishRenderEffects();
}

static void on_init(effect_runtime* pRuntime)
{
}

static void on_destroy(effect_runtime* pRuntime)
{
	s_renderedFrames = 0;
}

static bool on_set_effects_state(effect_runtime* pRuntime, bool newState)
{
	s_effectsState = newState;
	return false;
}

static bool on_open_overlay(effect_runtime* pRuntime, bool newState, input_source source)
{
	s_isOverlayOpen = newState;
	return false;
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
	reshade::register_event<reshade::addon_event::reshade_set_effects_state>(on_set_effects_state);
	reshade::register_event<reshade::addon_event::reshade_open_overlay>(on_open_overlay);

	Logger->Information("Add-On Started");

	return true;
}

STUDIO_API void Shutdown()
{
	reshade::unregister_event<reshade::addon_event::init_effect_runtime>(on_init);
	reshade::unregister_event<reshade::addon_event::destroy_effect_runtime>(on_destroy);
	reshade::unregister_event<reshade::addon_event::reshade_begin_effects>(on_begin_render_effects);
	reshade::unregister_event<reshade::addon_event::reshade_finish_effects>(on_end_render_effects);
	reshade::unregister_event<reshade::addon_event::reshade_set_effects_state>(on_set_effects_state);
	reshade::unregister_event<reshade::addon_event::reshade_open_overlay>(on_open_overlay);
	reshade::unregister_addon(GetCurrentModule());
}

STUDIO_API void SetBeginRenderingEffectsCallback(EmptyDelegate callback)
{
	m_onBeginRenderEffects = callback;
}

STUDIO_API void SetFinishRenderingEffectsCallback(EmptyDelegate callback)
{
	m_onFinishRenderEffects = callback;
}