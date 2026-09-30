namespace PcscDiag3.Controls.ReaderInformations;

public class ReaderInformationModel(string title, string value)
{
    public string Title { get; } = title;
    public string Value { get; } = value;
}
