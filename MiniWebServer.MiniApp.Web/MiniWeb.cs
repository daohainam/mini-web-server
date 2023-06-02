using System.Threading;

namespace MiniWebServer.MiniApp.Web
{
    public class MiniWeb : BaseMiniApp
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

        public override ICallable? Find(IMiniAppRequest request)
        {
            var mapedCalls = base.Find(request);

            if (mapedCalls == null)
            {
                foreach (var cf in callableServices)
                {
                    var callable = cf.Find(request);

                    if (callable != null)
                    {
                        return callable;
                    }
                }
            }

            return mapedCalls;
        }
    }
}