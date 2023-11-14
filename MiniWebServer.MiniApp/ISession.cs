namespace MiniWebServer.MiniApp
{
    public interface ISession
    {
        string Id { get; }
        // session is not always avaiable, we need to initialize it when server starts, in some scenarios such as IoT, API servers, we don't need session
        bool IsAvaiable { get; }
        // session is type-independent, so we support only byte array data type
        bool Clear();
        byte[]? Get(string key);
        string? GetString(string key);
        byte[]? Set(string key, byte[] value);
        string? SetString(string key, string value);
        bool Remove(string key);
        Task<bool> LoadAsync(); // we don't load/save automatically since it takes time, and we don't always use session
        Task<bool> SaveAsync();
    }
}
