namespace ScreenshotStudio.Library.Sources;

using System;

public abstract class SourceBase : GroupEntryBase
{
	public SourceBase()
		: base(null)
	{
	}

	public bool IsScanning { get; private set; }

	public void ScanSource()
	{
		this.IsScanning = true;

		try
		{
			this.Clear();
			this.Scan();
		}
		catch(Exception ex)
		{
			this.Log.Error(ex, "Error scanning library source");
		}

		this.IsScanning = false;
		this.Services.Library.NotifyScanComplete();
	}

	protected abstract void Scan();
}
