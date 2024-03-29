﻿using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using QRWells.MapReduce.Rpc.Attributes;
using QRWells.MapReduce.Rpc.Codecs;
using QRWells.MapReduce.Rpc.Data;
using static QRWells.MapReduce.Rpc.Extensions.ObjectExtensions;

namespace QRWells.MapReduce.Rpc.Service;

public class RpcService
{
    private readonly ConcurrentDictionary<string, Type> _rpcTypes = new();
    private ILogger<RpcService> _logger;
    private IServiceProvider _serviceProvider;
    public ICodec Codec { get; set; } = new JsonCodec();
    public Assembly Assembly { get; set; } = Assembly.GetEntryAssembly()!;

    internal void Init(IServiceCollection configuredServices)
    {
        IServiceCollection services = new ServiceCollection();

        {
            var array = new ServiceDescriptor[configuredServices.Count];
            configuredServices.CopyTo(array, 0);
            services.Add(array);
        }

        foreach (var type in Assembly.GetTypes())
        {
            if (!type.IsClass || type.IsAbstract)
                continue;
            var attribute = type.GetCustomAttribute<ServiceAttribute>();
            if (attribute == null)
                continue;
            var name = attribute.Name ?? attribute.ServiceType.Name;
            _rpcTypes.TryAdd(name, attribute.ServiceType);
            var serviceDescriptor = new ServiceDescriptor(attribute.ServiceType, type, attribute.Lifetime);
            services.Add(serviceDescriptor);
        }

        _serviceProvider = services.BuildServiceProvider();
        _logger = _serviceProvider.GetService<ILoggerFactory>()?.CreateLogger<RpcService>() ??
                  throw new InvalidOperationException("LoggerFactory not found.");
    }

    public async Task<InvokeResult> Invoke(Stream content)
    {
        var request = await Codec.DecodeAsync<RpcRequest>(content);
        var response = new RpcResponse();
        if (request == null)
        {
            _logger.LogError("Invalid request.");
            response.Error = "Invalid request.";
            return InvokeResult.FromError(await Codec.EncodeAsync(response));
        }

        _logger.LogInformation("{}.{} is called.", request.Service, request.Method);

        var rpcType = _rpcTypes[request.Service];
        var rpc = _serviceProvider.GetService(rpcType);
        var method = rpcType.GetMethod(request.Method);
        if (!TryArrangeParameter(method, request.Parameters, out var parameters))
            return InvokeResult.FromError(await Codec.EncodeAsync(response));

        var invoke = method?.Invoke(rpc, parameters);

        response.Result = invoke;
        return InvokeResult.FromResult(await Codec.EncodeAsync(response));
    }

    private static bool TryArrangeParameter(MethodBase? methodInfo, IDictionary<string, object?>? parameters,
        out dynamic?[] result)
    {
        if (methodInfo == null)
        {
            result = Array.Empty<object>();
            return false;
        }

        if (methodInfo.GetParameters().Length == 0 || parameters == null || parameters.Count == 0)
        {
            result = Array.Empty<object>();
            return true;
        }

        var methodParams = methodInfo.GetParameters();
        result = new dynamic?[methodParams.Length];

        for (var i = 0; i < methodParams.Length; i++)
        {
            var methodParam = methodParams[i];
            if (parameters.TryGetValue(methodParam.Name, out var value))
            {
                result[i] = RegulateObject(methodParam.ParameterType, value);
                continue;
            }

            if (!methodParam.HasDefaultValue) return false;
            result[i] = methodParam.DefaultValue;
        }

        return true;
    }

    public class InvokeResult
    {
        public string? Error { get; set; }
        public string? Result { get; set; }

        public static InvokeResult FromException(Exception e)
        {
            return new InvokeResult
            {
                Error = e.Message
            };
        }

        public static InvokeResult FromResult(object? result)
        {
            return new InvokeResult
            {
                Result = result?.ToString()
            };
        }

        public static InvokeResult FromError(string error)
        {
            return new InvokeResult
            {
                Error = error
            };
        }
    }
}