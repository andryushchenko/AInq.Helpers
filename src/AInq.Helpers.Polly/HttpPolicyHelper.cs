﻿// Copyright 2021 Anton Andryushchenko
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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Polly;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AInq.Helpers.Polly
{

/// <summary> Helper to make HTTP requests with given <see cref="IAsyncPolicy{TResult}" /> </summary>
public static class HttpPolicyHelper
{
    private const string ClientKey = "client";
    private const string MethodKey = "method";
    private const string ContentKey = "url";
    private const string UrlKey = "content";

    private static async Task<HttpResponseMessage> RequestAsync(this IAsyncPolicy<HttpResponseMessage> policy, HttpClient client, string? url,
        HttpMethod method, HttpContent? content, ILogger logger, CancellationToken cancellation = default)
    {
        var context = new Context().With(ClientKey, client)
                                   .With(MethodKey, method)
                                   .WithCancellation(cancellation)
                                   .WithLogger(logger);
        if (content != null)
            context.With(ContentKey, content);
        if (!string.IsNullOrWhiteSpace(url))
            context.With(UrlKey, url!);
        var result = await policy
                           .ExecuteAsync(async (ctx, cancel) =>
                               {
                                   using var request = new HttpRequestMessage(ctx.Get<HttpMethod>(MethodKey)!, ctx.Get<string>(UrlKey))
                                   {
                                       Content = ctx.Get<HttpContent>(ContentKey)
                                   };
                                   return await ctx.Get<HttpClient>(ClientKey)!.SendAsync(request, cancel);
                               },
                               context,
                               cancellation,
                               false)
                           .ConfigureAwait(false);
        logger.LogDebug("HTTP {Method} to {Url} - {Code}",
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
    /// <param name="cancellation"> Cancellation token </param>
    public static async Task<HttpResponseMessage> GetAsync(this IAsyncPolicy<HttpResponseMessage> policy, HttpClient client, string? url,
        ILogger logger, CancellationToken cancellation = default)
        => await (policy ?? throw new ArgumentNullException(nameof(policy)))
                 .RequestAsync(client ?? throw new ArgumentNullException(nameof(client)),
                     url,
                     HttpMethod.Get,
                     null,
                     logger ?? throw new ArgumentNullException(nameof(logger)),
                     cancellation)
                 .ConfigureAwait(false);

    /// <summary> HTTP Get using preconfigured <see cref="HttpClient" /> </summary>
    /// <param name="policy"> Request policy </param>
    /// <param name="client"> Preconfigured HttpClient </param>
    /// <param name="url"> Request URL </param>
    /// <param name="cancellation"> Cancellation token </param>
    public static async Task<HttpResponseMessage> GetAsync(this IAsyncPolicy<HttpResponseMessage> policy, HttpClient client, string? url,
        CancellationToken cancellation = default)
        => await (policy ?? throw new ArgumentNullException(nameof(policy)))
                 .RequestAsync(client ?? throw new ArgumentNullException(nameof(client)),
                     url,
                     HttpMethod.Get,
                     null,
                     NullLogger.Instance,
                     cancellation)
                 .ConfigureAwait(false);

    /// <summary> HTTP Get </summary>
    /// <param name="policy"> Request policy </param>
    /// <param name="url"> Request URL </param>
    /// <param name="logger"> Logger instance </param>
    /// <param name="cancellation"> Cancellation token </param>
    public static async Task<HttpResponseMessage> GetAsync(this IAsyncPolicy<HttpResponseMessage> policy, string? url, ILogger logger,
        CancellationToken cancellation = default)
    {
        using var client = new HttpClient();
        return await (policy ?? throw new ArgumentNullException(nameof(policy)))
                     .RequestAsync(client, url, HttpMethod.Get, null, logger ?? throw new ArgumentNullException(nameof(logger)), cancellation)
                     .ConfigureAwait(false);
    }

    /// <summary> HTTP Get </summary>
    /// <param name="policy"> Request policy </param>
    /// <param name="url"> Request URL </param>
    /// <param name="cancellation"> Cancellation token </param>
    public static async Task<HttpResponseMessage> GetAsync(this IAsyncPolicy<HttpResponseMessage> policy, string? url,
        CancellationToken cancellation = default)
    {
        using var client = new HttpClient();
        return await (policy ?? throw new ArgumentNullException(nameof(policy)))
                     .RequestAsync(client, url, HttpMethod.Get, null, NullLogger.Instance, cancellation)
                     .ConfigureAwait(false);
    }

#endregion

#region Delete

    /// <summary> HTTP Delete using preconfigured <see cref="HttpClient" /> </summary>
    /// <param name="policy"> Request policy </param>
    /// <param name="client"> Preconfigured HttpClient </param>
    /// <param name="url"> Request URL </param>
    /// <param name="logger"> Logger instance </param>
    /// <param name="cancellation"> Cancellation token </param>
    public static async Task<HttpResponseMessage> DeleteAsync(this IAsyncPolicy<HttpResponseMessage> policy, HttpClient client, string? url,
        ILogger logger, CancellationToken cancellation = default)
        => await (policy ?? throw new ArgumentNullException(nameof(policy)))
                 .RequestAsync(client ?? throw new ArgumentNullException(nameof(client)),
                     url,
                     HttpMethod.Delete,
                     null,
                     logger ?? throw new ArgumentNullException(nameof(logger)),
                     cancellation)
                 .ConfigureAwait(false);

    /// <summary> HTTP Delete using preconfigured <see cref="HttpClient" /> </summary>
    /// <param name="policy"> Request policy </param>
    /// <param name="client"> Preconfigured HttpClient </param>
    /// <param name="url"> Request URL </param>
    /// <param name="cancellation"> Cancellation token </param>
    public static async Task<HttpResponseMessage> DeleteAsync(this IAsyncPolicy<HttpResponseMessage> policy, HttpClient client, string? url,
        CancellationToken cancellation = default)
        => await (policy ?? throw new ArgumentNullException(nameof(policy)))
                 .RequestAsync(client ?? throw new ArgumentNullException(nameof(client)),
                     url,
                     HttpMethod.Delete,
                     null,
                     NullLogger.Instance,
                     cancellation)
                 .ConfigureAwait(false);

    /// <summary> HTTP Delete </summary>
    /// <param name="policy"> Request policy </param>
    /// <param name="url"> Request URL </param>
    /// <param name="logger"> Logger instance </param>
    /// <param name="cancellation"> Cancellation token </param>
    public static async Task<HttpResponseMessage> DeleteAsync(this IAsyncPolicy<HttpResponseMessage> policy, string? url, ILogger logger,
        CancellationToken cancellation = default)
    {
        using var client = new HttpClient();
        return await (policy ?? throw new ArgumentNullException(nameof(policy)))
                     .RequestAsync(client, url, HttpMethod.Delete, null, logger ?? throw new ArgumentNullException(nameof(logger)), cancellation)
                     .ConfigureAwait(false);
    }

    /// <summary> HTTP Delete </summary>
    /// <param name="policy"> Request policy </param>
    /// <param name="url"> Request URL </param>
    /// <param name="cancellation"> Cancellation token </param>
    public static async Task<HttpResponseMessage> DeleteAsync(this IAsyncPolicy<HttpResponseMessage> policy, string? url,
        CancellationToken cancellation = default)
    {
        using var client = new HttpClient();
        return await (policy ?? throw new ArgumentNullException(nameof(policy)))
                     .RequestAsync(client, url, HttpMethod.Delete, null, NullLogger.Instance, cancellation)
                     .ConfigureAwait(false);
    }

#endregion

#region Post

    /// <summary> HTTP Post using preconfigured <see cref="HttpClient" /> </summary>
    /// <param name="policy"> Request policy </param>
    /// <param name="client"> Preconfigured HttpClient </param>
    /// <param name="url"> Request URL </param>
    /// <param name="content"> Request content </param>
    /// <param name="logger"> Logger instance </param>
    /// <param name="cancellation"> Cancellation token </param>
    public static async Task<HttpResponseMessage> PostAsync(this IAsyncPolicy<HttpResponseMessage> policy, HttpClient client, string? url,
        HttpContent content, ILogger logger, CancellationToken cancellation = default)
        => await (policy ?? throw new ArgumentNullException(nameof(policy)))
                 .RequestAsync(client ?? throw new ArgumentNullException(nameof(client)),
                     url,
                     HttpMethod.Post,
                     content ?? throw new ArgumentNullException(nameof(content)),
                     logger ?? throw new ArgumentNullException(nameof(logger)),
                     cancellation)
                 .ConfigureAwait(false);

    /// <summary> HTTP Post using preconfigured <see cref="HttpClient" /> </summary>
    /// <param name="policy"> Request policy </param>
    /// <param name="client"> Preconfigured HttpClient </param>
    /// <param name="url"> Request URL </param>
    /// <param name="content"> Request content </param>
    /// <param name="cancellation"> Cancellation token </param>
    public static async Task<HttpResponseMessage> PostAsync(this IAsyncPolicy<HttpResponseMessage> policy, HttpClient client, string? url,
        HttpContent content, CancellationToken cancellation = default)
        => await (policy ?? throw new ArgumentNullException(nameof(policy)))
                 .RequestAsync(client ?? throw new ArgumentNullException(nameof(client)),
                     url,
                     HttpMethod.Post,
                     content ?? throw new ArgumentNullException(nameof(content)),
                     NullLogger.Instance,
                     cancellation)
                 .ConfigureAwait(false);

    /// <summary> HTTP Post </summary>
    /// <param name="policy"> Request policy </param>
    /// <param name="url"> Request URL </param>
    /// <param name="content"> Request content </param>
    /// <param name="logger"> Logger instance </param>
    /// <param name="cancellation"> Cancellation token </param>
    public static async Task<HttpResponseMessage> PostAsync(this IAsyncPolicy<HttpResponseMessage> policy, string? url, HttpContent content,
        ILogger logger, CancellationToken cancellation = default)
    {
        using var client = new HttpClient();
        return await (policy ?? throw new ArgumentNullException(nameof(policy)))
                     .RequestAsync(client,
                         url,
                         HttpMethod.Post,
                         content ?? throw new ArgumentNullException(nameof(content)),
                         logger ?? throw new ArgumentNullException(nameof(logger)),
                         cancellation)
                     .ConfigureAwait(false);
    }

    /// <summary> HTTP Post </summary>
    /// <param name="policy"> Request policy </param>
    /// <param name="url"> Request URL </param>
    /// <param name="content"> Request content </param>
    /// <param name="cancellation"> Cancellation token </param>
    public static async Task<HttpResponseMessage> PostAsync(this IAsyncPolicy<HttpResponseMessage> policy, string? url, HttpContent content,
        CancellationToken cancellation = default)
    {
        using var client = new HttpClient();
        return await (policy ?? throw new ArgumentNullException(nameof(policy)))
                     .RequestAsync(client,
                         url,
                         HttpMethod.Post,
                         content ?? throw new ArgumentNullException(nameof(content)),
                         NullLogger.Instance,
                         cancellation)
                     .ConfigureAwait(false);
    }

#endregion

#region Put

    /// <summary> HTTP Put using preconfigured <see cref="HttpClient" /> </summary>
    /// <param name="policy"> Request policy </param>
    /// <param name="client"> Preconfigured HttpClient </param>
    /// <param name="url"> Request URL </param>
    /// <param name="content"> Request content </param>
    /// <param name="logger"> Logger instance </param>
    /// <param name="cancellation"> Cancellation token </param>
    public static async Task<HttpResponseMessage> PutAsync(this IAsyncPolicy<HttpResponseMessage> policy, HttpClient client, string? url,
        HttpContent content, ILogger logger, CancellationToken cancellation = default)
        => await (policy ?? throw new ArgumentNullException(nameof(policy)))
                 .RequestAsync(client ?? throw new ArgumentNullException(nameof(client)),
                     url,
                     HttpMethod.Put,
                     content ?? throw new ArgumentNullException(nameof(content)),
                     logger ?? throw new ArgumentNullException(nameof(logger)),
                     cancellation)
                 .ConfigureAwait(false);

    /// <summary> HTTP Put using preconfigured <see cref="HttpClient" /> </summary>
    /// <param name="policy"> Request policy </param>
    /// <param name="client"> Preconfigured HttpClient </param>
    /// <param name="url"> Request URL </param>
    /// <param name="content"> Request content </param>
    /// <param name="cancellation"> Cancellation token </param>
    public static async Task<HttpResponseMessage> PutAsync(this IAsyncPolicy<HttpResponseMessage> policy, HttpClient client, string? url,
        HttpContent content, CancellationToken cancellation = default)
        => await (policy ?? throw new ArgumentNullException(nameof(policy)))
                 .RequestAsync(client ?? throw new ArgumentNullException(nameof(client)),
                     url,
                     HttpMethod.Put,
                     content ?? throw new ArgumentNullException(nameof(content)),
                     NullLogger.Instance,
                     cancellation)
                 .ConfigureAwait(false);

    /// <summary> HTTP Put </summary>
    /// <param name="policy"> Request policy </param>
    /// <param name="url"> Request URL </param>
    /// <param name="content"> Request content </param>
    /// <param name="logger"> Logger instance </param>
    /// <param name="cancellation"> Cancellation token </param>
    public static async Task<HttpResponseMessage> PutAsync(this IAsyncPolicy<HttpResponseMessage> policy, string? url, HttpContent content,
        ILogger logger, CancellationToken cancellation = default)
    {
        using var client = new HttpClient();
        return await (policy ?? throw new ArgumentNullException(nameof(policy)))
                     .RequestAsync(client,
                         url,
                         HttpMethod.Put,
                         content ?? throw new ArgumentNullException(nameof(content)),
                         logger ?? throw new ArgumentNullException(nameof(logger)),
                         cancellation)
                     .ConfigureAwait(false);
    }

    /// <summary> HTTP Put </summary>
    /// <param name="policy"> Request policy </param>
    /// <param name="url"> Request URL </param>
    /// <param name="content"> Request content </param>
    /// <param name="cancellation"> Cancellation token </param>
    public static async Task<HttpResponseMessage> PutAsync(this IAsyncPolicy<HttpResponseMessage> policy, string? url, HttpContent content,
        CancellationToken cancellation = default)
    {
        using var client = new HttpClient();
        return await (policy ?? throw new ArgumentNullException(nameof(policy)))
                     .RequestAsync(client,
                         url,
                         HttpMethod.Put,
                         content ?? throw new ArgumentNullException(nameof(content)),
                         NullLogger.Instance,
                         cancellation)
                     .ConfigureAwait(false);
    }

#endregion
}

}
