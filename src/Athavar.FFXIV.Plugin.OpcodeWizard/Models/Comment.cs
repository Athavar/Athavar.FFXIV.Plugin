namespace Athavar.FFXIV.Plugin.OpcodeWizard.Models;

internal class Comment
{
    public string Text { get; set; }

    public static implicit operator string(Comment c) => c.Text;
}