namespace MiniWebServer.MiniApp.Authorization
{
    public interface IClaimValidator
    {
        bool Validate(IMiniAppContext context);
    }
}
