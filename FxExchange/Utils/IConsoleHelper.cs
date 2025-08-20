namespace FxExchange.Utils;

public interface IConsoleHelper
{
    void Write(string value);
    void WriteLine(string value);
    string? ReadLine();
}