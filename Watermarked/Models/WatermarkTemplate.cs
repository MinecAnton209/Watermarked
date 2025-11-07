using Avalonia.Layout;
using Avalonia.Media;
using System;

namespace Watermarked.Models;

public class WatermarkTemplate
{
    public string Name { get; set; } = "New Template";
    
    public string WatermarkText { get; set; } = "Your Watermark";
    public double FontSize { get; set; } = 48;
    public Color TextColor { get; set; } = Colors.White;
    public bool IsTextWrappingEnabled { get; set; } = false;
    public double Opacity { get; set; } = 0.6;
    public string FontFamilyName { get; set; } = "Arial";

    public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Center;
    public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Center;
}