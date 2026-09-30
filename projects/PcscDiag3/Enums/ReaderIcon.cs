using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;

namespace PcscDiag3.Enums;

public static class ReaderIcon
{
    public readonly static Bitmap? Contactless = new(AssetLoader.Open(new Uri("avares://PcscDiag3/Assets/Reader/contactless.png")));
    public readonly static Bitmap? Contact = new(AssetLoader.Open(new Uri("avares://PcscDiag3/Assets/Reader/card.png")));
    public readonly static Bitmap? SimSam = new(AssetLoader.Open(new Uri("avares://PcscDiag3/Assets/Reader/simsam.png")));
}
