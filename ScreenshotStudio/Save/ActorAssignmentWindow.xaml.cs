namespace ScreenshotStudio.Save;
using ScreenshotStudio.Files;
using ScreenshotStudio.Library;
using ScreenshotStudio.Services;
using ScreenshotStudio.Tags;
using ScreenshotStudio.Utilities;
using ScreenshotStudio.Windows;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WpfUtils;
using WpfUtils.Extensions;

using Panel = ScreenshotStudio.Windows.Panel;

public partial class ActorAssignmentWindow : Panel
{
	[AutoNotify] public FileTypeInfoBase? SceneType => this.Services.Files.GetTypeInfo(this.Scene);
	[AutoNotify] public SceneFile? Scene { get; set; }
	[AutoNotify] public FastObservableCollection<Assignment> Assignments { get; init; } = new();

	public bool Result { get; set; }

	public static async Task<Dictionary<string, ICharacterAppearance?>?> GetAssignments(SceneFile scene)
	{
		ActorAssignmentWindow? panel = await ServiceManager.Instance.Panels.Open<ActorAssignmentWindow>();

		if (panel == null)
			throw new Exception("No Actor Assignment Window");

		await panel.Dispatcher.MainThread();
		panel.SetScene(scene);

		await Threads.NonUiThread();
		await PanelService.WhileShown(panel);

		if (panel.Result == false)
			return null;

		Dictionary<string, ICharacterAppearance?> result = new();
		foreach (Assignment assignment in panel.Assignments)
		{
			result.Add(assignment.Role, assignment.Appearance);
		}

		return result;
	}

	protected void SetScene(SceneFile scene)
	{
		this.Scene = scene;

		List<string> roles = new();
		foreach (SceneFile.Actor actor in scene.Actors)
		{
			if (actor.Role == null)
				continue;

			roles.Add(actor.Role);
		}

		this.Assignments.Clear();
		foreach (string role in roles)
		{
			this.Assignments.Add(new(role));
		}
	}

	private void OnConfirmClicked(object sender, RoutedEventArgs e)
	{
		this.Result = true;
		this.Close();
	}

	private void OnCancelClicked(object sender, RoutedEventArgs e)
	{
		this.Result = false;
		this.Close();
	}

	private void OnChooseAppearanceClicked(object sender, RoutedEventArgs e)
	{
		TagCollection defaultTags = new();
		defaultTags.Add("Named");

		Assignment? assignment = (sender as Button)?.DataContext as Assignment;
		if (assignment == null)
			return;

		LibraryModal.Show<ICharacterAppearance>(
			sender,
			"Create Character",
			defaultTags,
			null,
			(appearance, isFinal) =>
			{
				if (!isFinal)
					return;

				assignment.Appearance = appearance;
			});
	}
}

public class Assignment(string role)
	: ViewModel
{
	[AutoNotify] public string Role { get; init; } = role;
	[AutoNotify] public ICharacterAppearance? Appearance { get; set; }
}