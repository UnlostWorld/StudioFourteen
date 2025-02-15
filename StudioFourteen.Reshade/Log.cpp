#include "Log.h"

Log::Log(LogDelegate logger)
{
	this->m_logToStudio = logger;
}

void Log::Verbose(std::string message) const
{
	this->m_logToStudio(LevelVerbose, message.c_str());
}

void Log::Debug(std::string message) const
{
	this->m_logToStudio(LevelDebug, message.c_str());
}

void Log::Information(std::string message) const
{
	this->m_logToStudio(LevelInformation, message.c_str());
}

void Log::Warning(std::string message) const
{
	this->m_logToStudio(LevelWarning, message.c_str());
}

void Log::Error(std::string message) const
{
	this->m_logToStudio(LevelError, message.c_str());
}

void Log::Fatal(std::string message) const
{
	this->m_logToStudio(LevelFatal, message.c_str());
}