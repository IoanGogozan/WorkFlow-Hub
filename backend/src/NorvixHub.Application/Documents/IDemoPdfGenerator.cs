namespace NorvixHub.Application.Documents;

public interface IDemoPdfGenerator
{
    byte[] Generate(string title, string body);
}
