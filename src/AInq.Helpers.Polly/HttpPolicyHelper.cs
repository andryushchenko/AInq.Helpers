// Copyright 2021 Anton Andryushchenko
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace AInq.Helpers.Polly;

/// <summary> Helper to make HTTP requests with given <see cref="IAsyncPolicy{TResult}" /> </summary>
public static class HttpPolicyHelper
{
    private const string ClientKey = "AInq.Helpers.Polly.HttpPolicyHelper.Keys.Client";
    private const string MethodKey = "AInq.Helpers.Polly.HttpPolicyHelper.Keys.Method";
    private const string ContentKey = "AInq.Helpers.Polly.HttpPolicyHelper.Keys.Url";
    private const string UrlKey = "AInq.Helpers.Polly.HttpPolicyHelper.Keys.Content";

    private static async Task<HttpResponseMessage> RequestAsync(this IAsyncPolicy<HttpResponseMessage> policy, HttpClient client, string? url,
        HttpMethod method, HttpContent? content, ILogger logger, bool continueOnCapturedContext, LogLevel logLevel, CancellationToken cancellation)
    {
        var context = new Context().With(ClientKey, client).With(MethodKey, method).WithCancellation(cancellation).WithLogger(logger);
        if (content != null)
            context.With(ContentKey, content);
        if (!string.IsNullOrWhiteSpace(url))
#if NETSTANDARD
            context.With(UrlKey, url!);
#else
            context.With(UrlKey, url);
#endif
        var result = await policy
                           .ExecuteAsync(async (ctx, cancel) =>
                               {
                                   var request = new HttpRequestMessage(ctx.Get<HttpMethod>(MethodKey)!, ctx.Get<string>(UrlKey));
                                   if (ctx.Get<HttpContent>(ContentKey) is { } requestContent) request.Content = requestContent;
                                   return await ctx.Get<HttpClient>(ClientKey)!.SendAsync(request, cancel).ConfigureAwait(continueOnCapturedContext);
                               },
                               context,
                               cancellation,
                               continueOnCapturedContext)
                           .ConfigureAwait(continueOnCapturedContext);
        logger.Log(logLevel,
            "HTTP {Method} to {Url} - {Code}",
            result.RequestMessage?.Method,
            result.RequestMessage?.RequestUri,
            result.StatusCode);
        return result;
    }

#region Get

    /// <summary> HTTP Get using preconfigured <see cref="HttpClient" /> </summary>
    /// <param name="policy"> Request policy </param>
    /// <param name="client"> Preconfigured HttpClient </param>
    /// <param name="url"> Request URL </param>
    /// <param name="logger"> Logger instance </param>
    /// <param name="continueOnCapturedContext"> Continue on captured context </param>
    /// <param name="requestLogLevel"> Log Level for request result </param>
    /// <param name="cancellation"> Cancellation token </param>
    [PublicAPI]
    public static async Task<HttpResponseMessage> GetAsync(this IAsyncPolicy<HttpResponseMessage> policy, HttpClient client, string? url,
        ILogger logger, bool continueOnCapturedContext = false, LogLevel requestLogLevel = LogLevel.Debug, CancellationToken cancellation = default)
        => await (policy ?? throw new ArgumentNullException(nameof(policy)))
                 .RequestAsync(client ?? throw new ArgumentNullException(nameof(client)),
                     url,
                     HttpMethod.Get,
                     null,
                     logger ?? throw new ArgumentNullException(nameof(logger)),
                     continueOnCapturedContext,
                     requestLogLevel,
                     cancellation)
                 .ConfigureAwait(continueOnCapturedContext);

    /// <summary> HTTP Get using preconfigured <see cref="HttpClient" /> </summary>
    /// <param name="policy"> Request policy </param>
    /// <param name="client"> Preconfigured HttpClient </param>
    /// <param name="url"> Request URL </param>
    /// <param name="continueOnCapturedContext"> Continue on captured context </param>
    /// <param name="cancellation"> Cancellation token </param>
    [PublicAPI]
    public static async Task<HttpResponseMessage> GetAsync(this IAsyncPolicy<HttpResponseMessage> policy, HttpClient client, string? url,
        bool continueOnCapturedContext = false, CancellationToken cancellation = default)
        => await (policy ?? throw new ArgumentNullException(nameof(policy)))
                 .RequestAsync(client ?? throw new ArgumentNullException(nameof(client)),
                     url,
                     HttpMethod.Get,
                     null,
                     NullLogger.Instance,
                     continueOnCapturedContext,
                     LogLevel.None,
                     cancellation)
                 .ConfigureAwait(continueOnCapturedContext);

    /// <summary> HTTP Get </summary>
    /// <param name="policy"> Request policy </param>
    /// <param name="url"> Request URL </param>
    /// <param name="logger"> Logger instance </param>
    /// <param name="continueOnCapturedContext"> Continue on captured context </param>
    /// <param name="requestLogLevel"> Log Level for request result </param>
    /// <param name="cancellation"> Cancellation token </param>
    [PublicAPI]
    public static async Task<HttpResponseMessage> GetAsync(this IAsyncPolicy<HttpResponseMessage> policy, string url, ILogger logger,
        bool continueOnCapturedContext = false, LogLevel requestLogLevel = LogLevel.Debug, CancellationToken cancellation = default)
    {
        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(nameof(url));
        using var client = new HttpClient();
        return await (policy ?? throw new ArgumentNullException(nameof(policy)))
                     .RequestAsync(client,
                         url,
                         HttpMethod.Get,
                         null,
                         logger ?? throw new ArgumentNullException(nameof(logger)),
                         continueOnCapturedContext,
                         requestLogLevel,
                         cancellation)
                     .ConfigureAwait(continueOnCapturedContext);
    }

    /// <summary> HTTP Get </summary>
    /// <param name="policy"> Request policy </param>
    /// <param name="url"> Request URL </param>
    /// <param name="continueOnCapturedContext"> Continue on captured context </param>
    /// <param name="cancellation"> Cancellation token </param>
    [PublicAPI]
    public static async Task<HttpResponseMessage> GetAsync(this IAsyncPolicy<HttpResponseMessage> policy, string url,
        bool continueOnCapturedContext = false, CancellationToken cancellation = default)
    {
        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(nameof(url));
        using var client = new HttpClient();
        return await (policy ?? throw new ArgumentNullException(nameof(policy)))
                     .RequestAsync(client, url, HttpMethod.Get, null, NullLogger.Instance, continueOnCapturedContext, LogLevel.None, cancellation)
                     .ConfigureAwait(continueOnCapturedContext);
    }

#endregion

#region Delete

    /// <summary> HTTP Delete using preconfigured <see cref="HttpClient" /> </summary>
    /// <param name="policy"> Request policy </param>
    /// <param name="client"> Preconfigured HttpClient </param>
    /// <param name="url"> Request URL </param>
    /// <param name="logger"> Logger instance </param>
    /// <param name="continueOnCapturedContext"> Continue on captured context </param>
    /// <param name="requestLogLevel"> Log Level for request result </param>
    /// <param name="cancellation"> Cancellation token </param>
    [PublicAPI]
    public static async Task<HttpResponseMessage> DeleteAsync(this IAsyncPolicy<HttpResponseMessage> policy, HttpClient client, string? url,
        ILogger logger, bool continueOnCapturedContext = false, LogLevel requestLogLevel = LogLevel.Debug, CancellationToken cancellation = default)
        => await (policy ?? throw new ArgumentNullException(nameof(policy)))
                 .RequestAsync(client ?? throw new ArgumentNullException(nameof(client)),
                     url,
                     HttpMethod.Delete,
                     null,
                     logger ?? throw new ArgumentNullException(nameof(logger)),
                     continueOnCapturedContext,
                     requestLogLevel,
                     cancellation)
                 .ConfigureAwait(continueOnCapturedContext);

    /// <summary> HTTP Delete using preconfigured <see cref="HttpClient" /> </summary>
    /// <param name="policy"> Request policy </param>
    /// <param name="client"> Preconfigured HttpClient </param>
    /// <param name="url"> Request URL </param>
    /// <param name="continueOnCapturedContext"> Continue on captured context </param>
    /// <param name="cancellation"> Cancellation token </param>
    [PublicAPI]
    public static async Task<HttpResponseMessage> DeleteAsync(this IAsyncPolicy<HttpResponseMessage> policy, HttpClient client, string? url,
        bool continueOnCapturedContext = false, CancellationToken cancellation = default)
        => await (policy ?? throw new ArgumentNullException(nameof(policy)))
                 .RequestAsync(client ?? throw new ArgumentNullException(nameof(client)),
                     url,
                     HttpMethod.Delete,
                     null,
                     NullLogger.Instance,
                     continueOnCapturedContext,
                     LogLevel.None,
                     cancellation)
                 .ConfigureAwait(continueOnCapturedContext);

    /// <summary> HTTP Delete </summary>
    /// <param name="policy"> Request policy </param>
    /// <param name="url"> Request URL </param>
    /// <param name="logger"> Logger instance </param>
    /// <param name="continueOnCapturedContext"> Continue on captured context </param>
    /// <param name="requestLogLevel"> Log Level for request result </param>
    /// <param name="cancellation"> Cancellation token </param>
    [PublicAPI]
    public static async Task<HttpResponseMessage> DeleteAsync(this IAsyncPolicy<HttpResponseMessage> policy, string url, ILogger logger,
        bool continueOnCapturedContext = false, LogLevel requestLogLevel = LogLevel.Debug, CancellationToken cancellation = default)
    {
        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(nameof(url));
        using var client = new HttpClient();
        return await (policy ?? throw new ArgumentNullException(nameof(policy)))
                     .RequestAsync(client,
                         url,
                         HttpMethod.Delete,
                         null,
                         logger ?? throw new ArgumentNullException(nameof(logger)),
                         continueOnCapturedContext,
                         requestLogLevel,
                         cancellation)
                     .ConfigureAwait(continueOnCapturedContext);
    }

    /// <summary> HTTP Delete </summary>
    /// <param name="policy"> Request policy </param>
    /// <param name="url"> Request URL </param>
    /// <param name="continueOnCapturedContext"> Continue on captured context </param>
    /// <param name="cancellation"> Cancellation token </param>
    [PublicAPI]
    public static async Task<HttpResponseMessage> DeleteAsync(this IAsyncPolicy<HttpResponseMessage> policy, string url,
        bool continueOnCapturedContext = false, CancellationToken cancellation = default)
    {
        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(nameof(url));
        using var client = new HttpClient();
        return await (policy ?? throw new ArgumentNullException(nameof(policy)))
                     .RequestAsync(client, url, HttpMethod.Delete, null, NullLogger.Instance, continueOnCapturedContext, LogLevel.None, cancellation)
                     .ConfigureAwait(continueOnCapturedContext);
    }

#endregion

#region Post

    /// <summary> HTTP Post using preconfigured <see cref="HttpClient" /> </summary>
    /// <param name="policy"> Request policy </param>
    /// <param name="client"> Preconfigured HttpClient </param>
    /// <param name="url"> Request URL </param>
    /// <param name="content"> Request content </param>
    /// <param name="logger"> Logger instance </param>
    /// <param name="continueOnCapturedContext"> Continue on captured context </param>
    /// <param name="requestLogLevel"> Log Level for request result </param>
    /// <param name="cancellation"> Cancellation token </param>
    [PublicAPI]
    public static async Task<HttpResponseMessage> PostAsync(this IAsyncPolicy<HttpResponseMessage> policy, HttpClient client, string? url,
        HttpContent content, ILogger logger, bool continueOnCapturedContext = false, LogLevel requestLogLevel = LogLevel.Debug,
        CancellationToken cancellation = default)
        => await (policy ?? throw new ArgumentNullException(nameof(policy)))
                 .RequestAsync(client ?? throw new ArgumentNullException(nameof(client)),
                     url,
                     HttpMethod.Post,
                     content ?? throw new ArgumentNullException(nameof(content)),
                     logger ?? throw new ArgumentNullException(nameof(logger)),
                     continueOnCapturedContext,
                     requestLogLevel,
                     cancellation)
                 .ConfigureAwait(continueOnCapturedContext);

    /// <summary> HTTP Post using preconfigured <see cref="HttpClient" /> </summary>
    /// <param name="policy"> Request policy </param>
    /// <param name="client"> Preconfigured HttpClient </param>
    /// <param name="url"> Request URL </param>
    /// <param name="content"> Request content </param>
    /// <param name="continueOnCapturedContext"> Continue on captured context </param>
    /// <param name="cancellation"> Cancellation token </param>
    [PublicAPI]
    public static async Task<HttpResponseMessage> PostAsync(this IAsyncPolicy<HttpResponseMessage> policy, HttpClient client, string? url,
        HttpContent content, bool continueOnCapturedContext = false, CancellationToken cancellation = default)
        => await (policy ?? throw new ArgumentNullException(nameof(policy)))
                 .RequestAsync(client ?? throw new ArgumentNullException(nameof(client)),
                     url,
                     HttpMethod.Post,
                     content ?? throw new ArgumentNullException(nameof(content)),
                     NullLogger.Instance,
                     continueOnCapturedContext,
                     LogLevel.None,
                     cancellation)
                 .ConfigureAwait(continueOnCapturedContext);

    /// <summary> HTTP Post </summary>
    /// <param name="policy"> Request policy </param>
    /// <param name="url"> Request URL </param>
    /// <param name="content"> Request content </param>
    /// <param name="logger"> Logger instance </param>
    /// <param name="continueOnCapturedContext"> Continue on captured context </param>
    /// <param name="requestLogLevel"> Log Level for request result </param>
    /// <param name="cancellation"> Cancellation token </param>
    [PublicAPI]
    public static async Task<HttpResponseMessage> PostAsync(this IAsyncPolicy<HttpResponseMessage> policy, string url, HttpContent content,
        ILogger logger, bool continueOnCapturedContext = false, LogLevel requestLogLevel = LogLevel.Debug, CancellationToken cancellation = default)
    {
        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(nameof(url));
        using var client = new HttpClient();
        return await (policy ?? throw new ArgumentNullException(nameof(policy)))
                     .RequestAsync(client,
                         url,
                         HttpMethod.Post,
                         content ?? throw new ArgumentNullException(nameof(content)),
                         logger ?? throw new ArgumentNullException(nameof(logger)),
                         continueOnCapturedContext,
                         requestLogLevel,
                         cancellation)
                     .ConfigureAwait(continueOnCapturedContext);
    }

    /// <summary> HTTP Post </summary>
    /// <param name="policy"> Request policy </param>
    /// <param name="url"> Request URL </param>
    /// <param name="content"> Request content </param>
    /// <param name="continueOnCapturedContext"> Continue on captured context </param>
    /// <param name="cancellation"> Cancellation token </param>
    [PublicAPI]
    public static async Task<HttpResponseMessage> PostAsync(this IAsyncPolicy<HttpResponseMessage> policy, string url, HttpContent content,
        bool continueOnCapturedContext = false, CancellationToken cancellation = default)
    {
        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(nameof(url));
        using var client = new HttpClient();
        return await (policy ?? throw new ArgumentNullException(nameof(policy)))
                     .RequestAsync(client,
                         url,
                         HttpMethod.Post,
                         content ?? throw new ArgumentNullException(nameof(content)),
                         NullLogger.Instance,
                         continueOnCapturedContext,
                         LogLevel.None,
                         cancellation)
                     .ConfigureAwait(continueOnCapturedContext);
    }

#endregion

#region Put

    /// <summary> HTTP Put using preconfigured <see cref="HttpClient" /> </summary>
    /// <param name="policy"> Request policy </param>
    /// <param name="client"> Preconfigured HttpClient </param>
    /// <param name="url"> Request URL </param>
    /// <param name="content"> Request content </param>
    /// <param name="logger"> Logger instance </param>
    /// <param name="continueOnCapturedContext"> Continue on captured context </param>
    /// <param name="requestLogLevel"> Log Level for request result </param>
    /// <param name="cancellation"> Cancellation token </param>
    [PublicAPI]
    public static async Task<HttpResponseMessage> PutAsync(this IAsyncPolicy<HttpResponseMessage> policy, HttpClient client, string? url,
        HttpContent content, ILogger logger, bool continueOnCapturedContext = false, LogLevel requestLogLevel = LogLevel.Debug,
        CancellationToken cancellation = default)
        => await (policy ?? throw new ArgumentNullException(nameof(policy)))
                 .RequestAsync(client ?? throw new ArgumentNullException(nameof(client)),
                     url,
                     HttpMethod.Put,
                     content ?? throw new ArgumentNullException(nameof(content)),
                     logger ?? throw new ArgumentNullException(nameof(logger)),
                     continueOnCapturedContext,
                     requestLogLevel,
                     cancellation)
                 .ConfigureAwait(continueOnCapturedContext);

    /// <summary> HTTP Put using preconfigured <see cref="HttpClient" /> </summary>
    /// <param name="policy"> Request policy </param>
    /// <param name="client"> Preconfigured HttpClient </param>
    /// <param name="url"> Request URL </param>
    /// <param name="content"> Request content </param>
    /// <param name="continueOnCapturedContext"> Continue on captured context </param>
    /// <param name="cancellation"> Cancellation token </param>
    [PublicAPI]
    public static async Task<HttpResponseMessage> PutAsync(this IAsyncPolicy<HttpResponseMessage> policy, HttpClient client, string? url,
        HttpContent content, bool continueOnCapturedContext = false, CancellationToken cancellation = default)
        => await (policy ?? throw new ArgumentNullException(nameof(policy)))
                 .RequestAsync(client ?? throw new ArgumentNullException(nameof(client)),
                     url,
                     HttpMethod.Put,
                     content ?? throw new ArgumentNullException(nameof(content)),
                     NullLogger.Instance,
                     continueOnCapturedContext,
                     LogLevel.None,
                     cancellation)
                 .ConfigureAwait(continueOnCapturedContext);

    /// <summary> HTTP Put </summary>
    /// <param name="policy"> Request policy </param>
    /// <param name="url"> Request URL </param>
    /// <param name="content"> Request content </param>
    /// <param name="logger"> Logger instance </param>
    /// <param name="continueOnCapturedContext"> Continue on captured context </param>
    /// <param name="requestLogLevel"> Log Level for request result </param>
    /// <param name="cancellation"> Cancellation token </param>
    [PublicAPI]
    public static async Task<HttpResponseMessage> PutAsync(this IAsyncPolicy<HttpResponseMessage> policy, string url, HttpContent content,
        ILogger logger, bool continueOnCapturedContext = false, LogLevel requestLogLevel = LogLevel.Debug, CancellationToken cancellation = default)
    {
        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(nameof(url));
        using var client = new HttpClient();
        return await (policy ?? throw new ArgumentNullException(nameof(policy)))
                     .RequestAsync(client,
                         url,
                         HttpMethod.Put,
                         content ?? throw new ArgumentNullException(nameof(content)),
                         logger ?? throw new ArgumentNullException(nameof(logger)),
                         continueOnCapturedContext,
                         requestLogLevel,
                         cancellation)
                     .ConfigureAwait(continueOnCapturedContext);
    }

    /// <summary> HTTP Put </summary>
    /// <param name="policy"> Request policy </param>
    /// <param name="url"> Request URL </param>
    /// <param name="content"> Request content </param>
    /// <param name="continueOnCapturedContext"> Continue on captured context </param>
    /// <param name="cancellation"> Cancellation token </param>
    [PublicAPI]
    public static async Task<HttpResponseMessage> PutAsync(this IAsyncPolicy<HttpResponseMessage> policy, string url, HttpContent content,
        bool continueOnCapturedContext = false, CancellationToken cancellation = default)
    {
        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(nameof(url));
        using var client = new HttpClient();
        return await (policy ?? throw new ArgumentNullException(nameof(policy)))
                     .RequestAsync(client,
                         url,
                         HttpMethod.Put,
                         content ?? throw new ArgumentNullException(nameof(content)),
                         NullLogger.Instance,
                         continueOnCapturedContext,
                         LogLevel.None,
                         cancellation)
                     .ConfigureAwait(continueOnCapturedContext);
    }

#endregion
}
