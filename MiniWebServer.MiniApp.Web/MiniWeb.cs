namespace MiniWebServer.MiniApp.Web
{
    public class MiniWeb : IMiniApp
    {
        private readonly List<ICallableService> callableServices;

        public MiniWeb()
        {
            callableServices = new List<ICallableService>();
        }

        public MiniWeb AddCallableService(ICallableService callableService)
        {
            callableServices.Add(callableService);

            return this;
        }

        public async Task Get(IMiniAppRequest request, IMiniAppResponse response, CancellationToken cancellationToken)
        {
            foreach (var cf in callableServices)
            {
                var callable = cf.Find(request);

                if (callable != null)
                {
                    await callable.Get(request, response, cancellationToken);
                }
            }
        }

        public async Task Post(IMiniAppRequest request, IMiniAppResponse response, CancellationToken cancellationToken)
        {
            foreach (var cf in callableServices)
            {
                var callable = cf.Find(request);

                if (callable != null)
                {
                    await callable.Post(request, response, cancellationToken);
                }
            }
        }
    }
}