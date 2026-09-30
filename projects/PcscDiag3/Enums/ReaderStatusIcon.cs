using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;

namespace PcscDiag3.Enums;

public static class ReaderStatusIcon
{
    public readonly static Bitmap? CardAbsent = new(AssetLoader.Open(new Uri("avares://PcscDiag3/Assets/Reader/Status/card-absent.png")));
    public readonly static Bitmap? CardExclusive = new(AssetLoader.Open(new Uri("avares://PcscDiag3/Assets/Reader/Status/card-exclusive.png")));
    public readonly static Bitmap? CardInuse = new(AssetLoader.Open(new Uri("avares://PcscDiag3/Assets/Reader/Status/card-inuse.png")));
    public readonly static Bitmap? CardMute = new(AssetLoader.Open(new Uri("avares://PcscDiag3/Assets/Reader/Status/card-mute.png")));
    public readonly static Bitmap? CardPresent = new(AssetLoader.Open(new Uri("avares://PcscDiag3/Assets/Reader/Status/card-present.png")));
    public readonly static Bitmap? Error = new(AssetLoader.Open(new Uri("avares://PcscDiag3/Assets/Reader/Status/error.png")));
    public readonly static Bitmap? ReaderUnavailable = new(AssetLoader.Open(new Uri("avares://PcscDiag3/Assets/Reader/Status/reader-unavailable.png")));
    public readonly static Bitmap? Unknown = new(AssetLoader.Open(new Uri("avares://PcscDiag3/Assets/Reader/Status/unknown.png")));
}
