using System.Net.Http.Headers;
using System.Reflection;
using QRWells.MapReduce.Rpc.Attributes;
using QRWells.MapReduce.Rpc.Data;

namespace QRWells.MapReduce.Rpc.Client;

public class CallingProxy : DispatchProxy
{
    public string ServiceName { get; set; }
    public Type InterfaceType { get; set; }
    public RpcClient Client { get; set; }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        var request = new RpcRequest
        {
            Service = ServiceName,
            Method = targetMethod?.Name
        };

        if (args != null)
            for (var i = 0; i < args.Length; i++)
                request.Parameters.Add(targetMethod.GetParameters()[i].Name, args[i]);

        var body = Client.Codec.EncodeAsync(request).Result;
        var req = new HttpRequestMessage();

        req.RequestUri = new Uri("/");
        req.Version = new Version(1, 1);
        req.Method = HttpMethod.Post;
        req.Content = new StringContent(body);
        req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        try
        {
            var response = Client.Send(req);
            var result = response.Content.ReadAsStream();
            var rpcResponse = Client.Codec.DecodeAsync<RpcResponse>(result).Result;
            if (rpcResponse == null) return null;
            if (rpcResponse.Error != null) throw new Exception(rpcResponse.Error);
            return rpcResponse.Result;
        }
        catch (HttpRequestException e)
        {
            // todo: handle timeout
            Console.WriteLine(e);
        }

        return null;
    }
}