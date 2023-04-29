using Godot;

public partial class DrivingOverlay : CanvasLayer
{
    private TextureRect _mapTextureRect;

    public override void _Ready()
    {
        _mapTextureRect = GetNode<TextureRect>("%MapTextureRect");
    }

    public void Setup(ViewportTexture _cityMapTex)
    {
        _mapTextureRect.Texture = _cityMapTex;
    }
}
