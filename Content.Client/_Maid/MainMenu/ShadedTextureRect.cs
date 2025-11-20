using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;

namespace Content.Client._Maid.MainMenu;

public sealed class ShadedTextureRect : TextureRect
{
    [Dependency] private readonly IClyde _clyde = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    private IRenderTexture? _renderTarget;
    private ShaderInstance? _shader;

    public string? ShaderName
    {
        get => _shaderName;
        set
        {
            if (_shaderName == value) return;

            _shaderName = value;
            UpdateShader();
        }
    }
    private string? _shaderName;

    public ShadedTextureRect()
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _renderTarget?.Dispose();
        _shader?.Dispose();
    }

    protected override void Resized()
    {
        base.Resized();

        _renderTarget?.Dispose();
        _renderTarget = _clyde.CreateRenderTarget(
            PixelSize,
            RenderTargetColorFormat.Rgba8Srgb
        );
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        if (_renderTarget is null)
            return;

        handle.RenderInRenderTarget(_renderTarget, () =>
        {
            base.Draw(handle);
        },
        Color.Transparent);

        if (_shader is not null)
        {
            handle.UseShader(_shader);
            handle.DrawTextureRect(_renderTarget.Texture, PixelSizeBox);
            handle.UseShader(null);
        }
        else
        {
            handle.DrawTextureRect(_renderTarget.Texture, PixelSizeBox);
        }
    }

    private void UpdateShader()
    {
        _shader?.Dispose();
        _shader = null;

        if (string.IsNullOrEmpty(ShaderName))
            return;

        try
        {
            var shaderPrototype = _proto.Index<ShaderPrototype>(ShaderName);
            _shader = shaderPrototype.Instance().Duplicate();
        }
        catch (Exception ex)
        {
            // ignored
        }
    }
}
