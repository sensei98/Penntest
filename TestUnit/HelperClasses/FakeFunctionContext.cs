using System.Security.Claims;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TestHelper;
public class FakeFunctionContext : FunctionContext
{
    private string id;
    private string role;
    public override string InvocationId => throw new NotImplementedException();

    public override string FunctionId => throw new NotImplementedException();

    public override TraceContext TraceContext => throw new NotImplementedException();

    public override BindingContext BindingContext => throw new NotImplementedException();

    public override RetryContext RetryContext => throw new NotImplementedException();

    public override IServiceProvider InstanceServices { get; set; }

    public override FunctionDefinition FunctionDefinition => throw new NotImplementedException();

    public override IDictionary<object, object> Items
    {
        get
        {
            var dict = new Dictionary<object, object>() { };
            dict.Add("User", new FakeClaimsPrincipal(id, role));
            return dict;
        }

        set => throw new NotImplementedException();
    }
    public FakeFunctionContext(string id, string role, IHost host)
    {
        this.id = id;
        this.role = role;
        this.InstanceServices = host.Services;
    }

    public override IInvocationFeatures Features => throw new NotImplementedException();

    public ClaimsPrincipal GetUser()
    {
        return new FakeClaimsPrincipal(id, role);
    }
}