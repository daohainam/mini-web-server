namespace MiniWebServer.Mvc
{
    [Flags]
    public enum ParameterSources
    {
        None = 0,
        Query = 1,
        Form = 2,
        Header = 4,
        Body = 8,

        Any = Query | Form | Header, // except Body, Body parameter can only be declarded once and explicitly 
    }
}
