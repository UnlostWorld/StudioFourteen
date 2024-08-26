namespace ScreenshotStudio.Save;

using Dalamud.Plugin.Services;
using ScreenshotStudio.Utilities;
using ScreenshotStudio.Windows;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WpfUtils;
using WpfUtils.Extensions;

public partial class ActorAssignmentWindow : PanelWindow
{
	public FastObservableCollection<Assignment> Assignments { get; init; } = new();
	public bool Result { get; set; }

	public static async Task<Dictionary<string, int>?> GetAssignments(List<string> roles)
	{
		ActorAssignmentWindow? panel = await Panel.ShowAsync<ActorAssignmentWindow>();

		if (panel == null)
			throw new Exception("No Actor Assignment Window");

		await panel.Dispatcher.MainThread();
		panel.SetRoles(roles);

		await Threads.NonUiThread();
		await Panel.WhileShown(panel);

		if (panel.Result == false)
			return null;

		Dictionary<string, int> result = new();
		foreach (Assignment assignment in panel.Assignments)
		{
			result.Add(assignment.Role, assignment.ObjectTableIndex);
		}

		return result;
	}

	protected void SetRoles(List<string> roles)
	{
		this.Assignments.Clear();
		foreach (string role in roles)
		{
			this.Assignments.Add(new(role));
		}
	}

	protected override void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);
	}

	private void OnConfirmClicked(object sender, System.Windows.RoutedEventArgs e)
	{
		this.Result = true;
		this.Close();
	}

	private void OnCancelClicked(object sender, System.Windows.RoutedEventArgs e)
	{
		this.Result = false;
		this.Close();
	}
}

public class Assignment(string role)
{
	public string Role { get; init; } = role;
	public int ObjectTableIndex { get; set; } = -1;
}