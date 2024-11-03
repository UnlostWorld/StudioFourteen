namespace StudioFourteen.Effects;

using Serilog;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Effects;

public class ShaderEffectBase : ShaderEffect
{
	public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(WindowMaskEffect), 0);

	public ShaderEffectBase(string shaderUri)
	{
		this.PixelShader = new();
		this.PixelShader.UriSource = new(shaderUri);
		this.UpdateShaderValue(InputProperty);
	}

	public ShaderEffectBase(PixelShader shader)
	{
		this.PixelShader = shader;
		this.UpdateShaderValue(InputProperty);
	}

	public ServiceManager Services => ServiceManager.Instance;
	public ILogger Log => Logging.ForContext(this.GetType());

	public Brush Input
	{
		get => (Brush)this.GetValue(InputProperty);
		set => this.SetValue(InputProperty, value);
	}
}