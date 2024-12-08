#include "Log.h"

Log::Log(LogDelegate logger)
{
	this->m_logToStudio = logger;
}

void Log::Verbose(const char* message) const
{
	this->m_logToStudio(LevelVerbose, message);
}

void Log::Debug(const char* message) const
{
	this->m_logToStudio(LevelDebug, message);
}

void Log::Information(const char* message) const
{
	this->m_logToStudio(LevelInformation, message);
}

void Log::Warning(const char* message) const
{
	this->m_logToStudio(LevelWarning, message);
}

void Log::Error(const char* message) const
{
	this->m_logToStudio(LevelError, message);
}

void Log::Fatal(const char* message) const
{
	this->m_logToStudio(LevelFatal, message);
}