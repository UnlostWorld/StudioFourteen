#pragma once

#include <string>

class Log
{
public:

	// Must match Serilog.Events.LogEventLevel
	enum Level
	{
		LevelVerbose,
		LevelDebug,
		LevelInformation,
		LevelWarning,
		LevelError,
		LevelFatal,
	};

	typedef void (*LogDelegate)(Level level, const char*);

	Log(LogDelegate logger);

	void Verbose(std::string message) const;
	void Debug(std::string message) const;
	void Information(std::string message) const;
	void Warning(std::string message) const;
	void Error(std::string message) const;
	void Fatal(std::string message) const;

private:
	LogDelegate m_logToStudio;
};