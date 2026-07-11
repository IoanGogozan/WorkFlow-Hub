using System.Text;
using FluentAssertions;
using NorvixHub.Infrastructure.Documents;
using Xunit;

namespace NorvixHub.UnitTests.Documents;

public sealed class SimpleDemoPdfGeneratorTests
{
    [Fact]
    public void Generate_returns_valid_pdf_header_with_fictional_data_label()
    {
        var generator = new SimpleDemoPdfGenerator();

        var pdf = generator.Generate("Demo report", "Fictional inspection result");

        pdf.Should().StartWith("%PDF"u8.ToArray());
        Encoding.ASCII.GetString(pdf).Should().Contain("fictional demo data");
    }

    [Fact]
    public void Generate_is_deterministic_and_uses_only_its_input()
    {
        var generator = new SimpleDemoPdfGenerator();

        var first = generator.Generate("Demo report", "Fictional inspection result");
        var second = generator.Generate("Demo report", "Fictional inspection result");
        var changed = generator.Generate("Different report", "Fictional inspection result");

        first.Should().Equal(second);
        first.Should().NotEqual(changed);
    }
}
