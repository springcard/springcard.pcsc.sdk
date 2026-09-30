using Avalonia;
using SpringCard.PCSC.Enums;
using System.Collections.Generic;

namespace PcscDiag3.Configurations;

public class ApplicationOptions
{
    public bool MainWindowMaximized { get; set; } = false;
    public MainWindowLocation? MainWindowLocation { get; set; } = null;

    public ReaderScope ContextScope { get; set; } = ReaderScope.System;
    public string ListGroup { get; set; } = ReaderType.ALL_READERS;
    public ReaderShare ShareMode { get; set; } = ReaderShare.Exclusive;
    public ReaderProtocol Protocol { get; set; } = ReaderProtocol.T0 | ReaderProtocol.T1;
    public CardDisposing CardDisposing { get; set; } = CardDisposing.Reset;

    public LinkedList<byte[]> TransmitCommandHistory { get; set; } = new LinkedList<byte[]>();
    public LinkedList<byte[]> ControlCommandHistory { get; set; } = new LinkedList<byte[]>();
}

public record MainWindowLocation(ScreenBounds ScreenBounds, int X, int Y);
public record ScreenBounds(int X, int Y, int Width, int Height);
