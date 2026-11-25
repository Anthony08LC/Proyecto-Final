namespace QuoteFlow.Application.Settings.Common;

/// <summary>Acumula los cambios «valor anterior → valor nuevo» para el detalle de auditoría.</summary>
internal sealed class ChangeLog
{
    private readonly List<string> _changes = [];

    public bool HasChanges => _changes.Count > 0;

    public void Track(string label, string? before, string? after)
    {
        if (!string.Equals(before, after, StringComparison.Ordinal))
        {
            _changes.Add($"{label}: {Show(before)} → {Show(after)}");
        }
    }

    public override string ToString() => string.Join("; ", _changes);

    private static string Show(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "(vacío)" : $"«{value}»";
}
