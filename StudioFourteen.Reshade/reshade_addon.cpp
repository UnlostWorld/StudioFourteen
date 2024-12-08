#include <reshade.hpp>

using namespace reshade::api;

#define RESHADE_API extern "C" __declspec(dllexport)
#define STUDIO_API extern "C" __declspec(dllexport)

RESHADE_API const char* NAME = "Studio Fourteen Reshade sync";
RESHADE_API const char* DESCRIPTION = "Enabled Studio Fourteen to communicate with Reshade";

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

typedef void (*LogDelegate)(const char*);

STUDIO_API bool Initialize(LogDelegate onLog)
{
	if (!reshade::register_addon(GetCurrentModule()))
		return false;

	onLog("Hello World!");

	return true;
}

STUDIO_API void Shutdown()
{
	reshade::unregister_addon(GetCurrentModule());
}