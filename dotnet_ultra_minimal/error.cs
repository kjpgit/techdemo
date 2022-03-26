// Application level exception, that maps to HTTP

public class MyWebException : Exception
{
    public MyWebException(int code, string message) : base(message)
    {
        m_code = code;
    }

    public int m_code;
}
