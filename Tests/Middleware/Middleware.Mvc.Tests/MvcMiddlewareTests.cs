using Microsoft.Extensions.DependencyInjection;
using MiniWebServer.Abstractions.Http;
using MvcMiddlewareTests;

namespace MiniWebServer.Mvc.Tests;

[TestClass()]
public class MvcMiddlewareTests
{
    [TestMethod()]
    [DataRow("q1", "q1Value", true, ParameterSources.Query)]
    [DataRow("q1", "q1Value", false, ParameterSources.Form)]
    [DataRow("q1", "q1Value", false, ParameterSources.Header)]
    public async Task CreateValueWithQuery(string parameterName, string parameterValue, bool isCreated, ParameterSources source)
    {
        var serviceCollection = new ServiceCollection();
        var service = serviceCollection.BuildServiceProvider();

        var r = await MvcMiddleware.TryCreateValueAsync(
            parameterName,
            typeof(string),
            false,
            null,
            false, source, service,
            () => new ParametersContainer()
            {
                QueryParameters = new HttpParameters(new HttpParameter(parameterName, parameterValue))
            },
            () => new RequestHeadersContainer()
            {
                Headers = []
            },
            () => new RequestBodyReader(string.Empty),
            () => new FormContainer()
            );

        if (isCreated)
        {
            Assert.IsTrue(r.IsCreated);
            Assert.IsTrue(r.Value as string == parameterValue);
        }
        else
        {
            Assert.IsFalse(r.IsCreated);
        }
    }

    [TestMethod()]
    [DataRow("q1", "q1Value", false, ParameterSources.Query)]
    [DataRow("q1", "q1Value", false, ParameterSources.Form)]
    [DataRow("q1", "q1Value", true, ParameterSources.Header)]
    public async Task CreateValueWithHeader(string parameterName, string parameterValue, bool isCreated, ParameterSources source)
    {
        var serviceCollection = new ServiceCollection();
        var service = serviceCollection.BuildServiceProvider();

        var r = await MvcMiddleware.TryCreateValueAsync(parameterName,
            typeof(string),
            false,
            null,
            false,
            source, service,
            () => new ParametersContainer()
            {
                QueryParameters = []
            },
            () => new RequestHeadersContainer()
            {
                Headers = new HttpRequestHeaders(parameterName, parameterValue)
            },
            () => new RequestBodyReader(string.Empty),
            () => new FormContainer()
            );

        if (isCreated)
        {
            Assert.IsTrue(r.IsCreated);
            Assert.IsTrue(r.Value as string == parameterValue);
        }
        else
        {
            Assert.IsFalse(r.IsCreated);
        }
    }

    [TestMethod()]
    [DataRow("q1", "q1Value", false, ParameterSources.Query)]
    [DataRow("q1", "q1Value", true, ParameterSources.Form)]
    [DataRow("q1", "q1Value", false, ParameterSources.Header)]
    public async Task CreateValueWithForm(string parameterName, string parameterValue, bool isCreated, ParameterSources source)
    {
        var serviceCollection = new ServiceCollection();
        var service = serviceCollection.BuildServiceProvider();

        var r = await MvcMiddleware.TryCreateValueAsync(
            parameterName,
            typeof(string),
            false,
            null,
            true, source, service,
            () => new ParametersContainer()
            {
                QueryParameters = []
            },
            () => new RequestHeadersContainer()
            {
                Headers = []
            },
            () => new RequestBodyReader(string.Empty),
            () => new FormContainer(new RequestForm(
                    parameterName, parameterValue
                ))
            );

        if (isCreated)
        {
            Assert.IsTrue(r.IsCreated);
            Assert.IsTrue(r.Value as string == parameterValue);
        }
        else
        {
            Assert.IsFalse(r.IsCreated);
        }
    }
}
