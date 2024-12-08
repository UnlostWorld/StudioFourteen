#include <reshade.hpp>
#include "Log.h"

using namespace reshade;
using namespace reshade::api;

#define RESHADE_API extern "C" __declspec(dllexport)
#define STUDIO_API extern "C" __declspec(dllexport)

RESHADE_API const char* NAME = "Studio Fourteen Reshade sync";
RESHADE_API const char* DESCRIPTION = "Enabled Studio Fourteen to communicate with Reshade";

static const Log* Logger;

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

	Logger->Information("Add-On Started");

	return true;
}

STUDIO_API void Shutdown()
{
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