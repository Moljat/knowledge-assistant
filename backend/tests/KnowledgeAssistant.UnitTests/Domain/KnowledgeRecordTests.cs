using KnowledgeAssistant.Domain.Entities;

namespace KnowledgeAssistant.UnitTests.Domain;

public sealed class KnowledgeRecordTests
{
    [Fact]
    public void Create_NormalizesValuesAndSetsDates()
    {
        var now = new DateTimeOffset(2026, 6, 9, 12, 0, 0, TimeSpan.Zero);

        var record = KnowledgeRecord.Create(
            "  Política de ventas  ",
            "  Contenido empresarial  ",
            "  Intranet  ",
            now);

        Assert.NotEqual(Guid.Empty, record.Id);
        Assert.Equal("Política de ventas", record.Title);
        Assert.Equal("Contenido empresarial", record.Content);
        Assert.Equal("Intranet", record.Source);
        Assert.Equal(now, record.CreatedAtUtc);
        Assert.Equal(now, record.UpdatedAtUtc);
    }

    [Fact]
    public void Create_WithoutTitle_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            KnowledgeRecord.Create(" ", "Contenido", null));
    }
}
