public interface Logger
{
    public Logger Debug(string log, params object[] args);
    public Logger Info(string log, params object[] args);
    public Logger Error(string log, params object[] args);
}
