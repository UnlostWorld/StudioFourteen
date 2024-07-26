// Brio
// https://github.com/Etheirys/Brio/tree/main/Brio/UI/Controls/Stateless/ImBrio.Gizmo.cs

namespace ScreenshotStudio.Posing;

using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using ImGuiNET;
using System;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

public class RotationGizmo : View
{
	private readonly Canvas canvas;
	private readonly Ellipse sphere;
	private readonly RotationGizmoAxis xAxis;
	private readonly RotationGizmoAxis yAxis;
	private readonly RotationGizmoAxis zAxis;

	public RotationGizmo()
	{
		this.canvas = new();
		this.Content = this.canvas;

		int radius = 70;

		this.sphere = new();
		this.sphere.Width = radius * 2;
		this.sphere.Height = radius * 2;
		this.sphere.Fill = new SolidColorBrush(Color.FromArgb(0x50, 0, 0, 0));
		this.canvas.Children.Add(this.sphere);

		this.xAxis = new(RotationGizmoAxis.Axis.X, radius, this.canvas);
		this.xAxis.ForegroundBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x33, 0x33, 0xFF));
		this.xAxis.BackgroundBrush = new SolidColorBrush(Color.FromArgb(0x10, 0x33, 0x33, 0xFF));

		this.yAxis = new(RotationGizmoAxis.Axis.Y, radius, this.canvas);
		this.yAxis.ForegroundBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x33, 0xFF, 0x33));
		this.yAxis.BackgroundBrush = new SolidColorBrush(Color.FromArgb(0x10, 0x33, 0xFF, 0x33));

		this.zAxis = new(RotationGizmoAxis.Axis.Z, radius, this.canvas);
		this.zAxis.ForegroundBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x33, 0x33));
		this.zAxis.BackgroundBrush = new SolidColorBrush(Color.FromArgb(0x10, 0xFF, 0x33, 0x33));
	}

	protected unsafe override void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);

		Camera* pCamera = CameraManager.Instance()->GetActiveCamera();
		Matrix4x4 viewMatrix = pCamera->GetViewMatrix();

		// extract just rotation from camera view
		Matrix4x4.Decompose(viewMatrix, out var _, out var rotation, out var _);
		viewMatrix = Matrix4x4.CreateFromQuaternion(rotation);

		// invert camera x
		Matrix4x4 mat = Matrix4x4.CreateScale(-1, 1, 1);
		viewMatrix = viewMatrix * mat;

		Matrix4x4 transformMatrix = Matrix4x4.Identity; ////Matrix4x4.CreateFromQuaternion(rotation);
		transformMatrix.Translation = new Vector3(0, 0, 0);

		this.Dispatcher.Invoke(() =>
		{
			Vector2 center = default;
			center.X = (float)(this.ActualWidth / 2);
			center.Y = (float)(this.ActualHeight / 2);

			Canvas.SetLeft(this.sphere, center.X - (this.sphere.Width / 2));
			Canvas.SetTop(this.sphere, center.Y - (this.sphere.Height / 2));

			this.xAxis.Transform(transformMatrix, viewMatrix, center);
			this.yAxis.Transform(transformMatrix, viewMatrix, center);
			this.zAxis.Transform(transformMatrix, viewMatrix, center);
		});
	}

	public class RotationGizmoAxis
	{
		private readonly Vector3[] points3d = new Vector3[144];
		private readonly Line[] lines = new Line[144];
		private int strokeThickness = 1;

		public RotationGizmoAxis(Axis axis, float radius, Canvas canvas)
		{
			for (int i = 0; i < this.points3d.Length; i++)
			{
				float p = i / (float)(this.points3d.Length - 1);
				float r = p * (MathF.PI * 2);

				if (axis == Axis.Z)
				{
					this.points3d[i] = new Vector3(radius * MathF.Cos(r), radius * MathF.Sin(r), 0);
				}
				else if (axis == Axis.X)
				{
					this.points3d[i] = new Vector3(0, radius * MathF.Cos(r), radius * MathF.Sin(r));
				}
				else if (axis == Axis.Y)
				{
					this.points3d[i] = new Vector3(radius * MathF.Cos(r), 0, radius * MathF.Sin(r));
				}
			}

			for (int i = 0; i < this.lines.Length; i++)
			{
				this.lines[i] = new();
				this.lines[i].StrokeThickness = this.strokeThickness;
				this.lines[i].Stroke = this.ForegroundBrush;
				canvas.Children.Add(this.lines[i]);
			}
		}

		public enum Axis
		{
			X,
			Y,
			Z,
		}

		public int StrokeThickness
		{
			get => this.strokeThickness;
			set
			{
				this.strokeThickness = value;
				foreach(Line line in this.lines)
				{
					line.StrokeThickness = value;
				}
			}
		}

		public Brush ForegroundBrush { get; set; } = new SolidColorBrush(Colors.White);
		public Brush BackgroundBrush { get; set; } = new SolidColorBrush(Colors.Black);

		public void Transform(Matrix4x4 transformMatrix, Matrix4x4 viewMatrix, Vector2 center)
		{
			float zClip = 0f;
			for (int i = 1; i < this.points3d.Length; i++)
			{
				Vector3 fromPoint = Vector3.Transform(this.points3d[i - 1], transformMatrix);
				fromPoint = Vector3.Transform(fromPoint, viewMatrix);

				Vector3 toPoint = Vector3.Transform(this.points3d[i], transformMatrix);
				toPoint = Vector3.Transform(toPoint, viewMatrix);

				bool isVisible = toPoint.Z < zClip;
				Vector2 fromPos = center + new Vector2(fromPoint.X, fromPoint.Y);
				Vector2 toPos = center + new Vector2(toPoint.X, toPoint.Y);

				Line line = this.lines[i];
				line.X1 = fromPos.X;
				line.Y1 = fromPos.Y;
				line.X2 = toPos.X;
				line.Y2 = toPos.Y;

				line.StrokeThickness = this.StrokeThickness;
				line.Stroke = isVisible ? this.ForegroundBrush : this.BackgroundBrush;
			}
		}
	}
}