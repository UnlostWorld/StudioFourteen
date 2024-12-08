#include <reshade.hpp>
#include "Log.h"

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

static bool OnReshadeOverlayChanging(effect_runtime* pRuntime, bool open, input_source source)
{
	if (open)
	{
		Logger->Information("Open overlay");
	}
	else
	{
		Logger->Information("Close overlay");
	}

	// We can stop the overlay from opening by returning true.
	// We may want to do this if studio will have its own reshade UI.
	return false;
}

STUDIO_API bool Initialize(Log::LogDelegate onLog)
{
	Logger = new Log(onLog);

	if (!reshade::register_addon(GetCurrentModule()))
		return false;

	Logger->Information("Hello World!");

	reshade::register_event<reshade::addon_event::reshade_open_overlay>(OnReshadeOverlayChanging);

	return true;
}

STUDIO_API void Shutdown()
{
	reshade::unregister_addon(GetCurrentModule());
}