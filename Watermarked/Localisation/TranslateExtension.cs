using System;
using System.Resources;
using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace Watermarked.Localisation;

public class TranslateExtension : MarkupExtension
{
    private static readonly ResourceManager ResManager = 
        new("Watermarked.Resources.Strings", typeof(TranslateExtension).Assembly);

    public string Key { get; set; }

    public TranslateExtension(string key)
    {
        Key = key;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        try
        {
            return ResManager.GetString(Key) ?? $"#{Key}#";
        }
        catch
        {
            return $"!{Key}!";
        }
    }
}