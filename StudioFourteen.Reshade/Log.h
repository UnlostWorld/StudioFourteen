#pragma once

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

	void Verbose(const char* message) const;
	void Debug(const char* message) const;
	void Information(const char* message) const;
	void Warning(const char* message) const;
	void Error(const char* message) const;
	void Fatal(const char* message) const;

private:
	LogDelegate m_logToStudio;
};