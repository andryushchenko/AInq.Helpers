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

using System.Net;

namespace AInq.Helpers.Polly;

/// <summary> Retry policies for HTTP requests </summary>
public static class HttpRetryPolicies
{
    /// <summary> Retry forever on transient errors with logging if request context contains logger </summary>
    [PublicAPI]
    public static IAsyncPolicy<HttpResponseMessage> TransientRetryAsyncPolicy()
        => Policy.Handle<HttpRequestException>()
                 .Or<TaskCanceledException>(ex => ex.InnerException is TimeoutException)
                 .OrResult<HttpResponseMessage>(response => response.StatusCode == HttpStatusCode.RequestTimeout || (int) response.StatusCode >= 500)
                 .RetryForeverAsync(OnTransientRetry);

    /// <summary> Retry on transient errors with logging if request context contains logger </summary>
    /// <param name="maxRetry"> Max retry count </param>
    [PublicAPI]
    public static IAsyncPolicy<HttpResponseMessage> TransientRetryAsyncPolicy(int maxRetry)
        => Policy.Handle<HttpRequestException>()
                 .Or<TaskCanceledException>(ex => ex.InnerException is TimeoutException)
                 .OrResult<HttpResponseMessage>(response => response.StatusCode == HttpStatusCode.RequestTimeout || (int) response.StatusCode >= 500)
                 .RetryAsync(maxRetry >= 1 ? maxRetry : throw new ArgumentOutOfRangeException(nameof(maxRetry)), OnTransientRetry);

    /// <summary> Retry forever with given timeout on HTTP 429 with logging if request context contains logger </summary>
    /// <param name="timeout"> Retry timeout </param>
    [PublicAPI]
    public static IAsyncPolicy<HttpResponseMessage> TimeoutRetryAsyncPolicy(TimeSpan timeout)
    {
        if (timeout < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));
        return CreateHttp429PolicyBuilder().WaitAndRetryForeverAsync((_, _) => timeout, OnTimeoutRetry);
    }

    /// <summary> Retry with given timeout on HTTP 429 with logging if request context contains logger </summary>
    /// <param name="timeout"> Retry timeout </param>
    /// <param name="maxRetry"> Max retry count </param>
    [PublicAPI]
    public static IAsyncPolicy<HttpResponseMessage> TimeoutRetryAsyncPolicy(TimeSpan timeout, int maxRetry)
    {
        if (timeout < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));
        if (maxRetry < 1) throw new ArgumentOutOfRangeException(nameof(maxRetry));
        return CreateHttp429PolicyBuilder().WaitAndRetryAsync(maxRetry, (_, _) => timeout, OnTimeoutRetry);
    }

    /// <summary> Retry forever with provided timeout on HTTP 429 with logging if request context contains logger </summary>
    /// <param name="timeoutProvider"> Timeout value provider </param>
    [PublicAPI]
    public static IAsyncPolicy<HttpResponseMessage> TimeoutRetryAsyncPolicy(Func<int, TimeSpan> timeoutProvider)
    {
        _ = timeoutProvider ?? throw new ArgumentNullException(nameof(timeoutProvider));
        return CreateHttp429PolicyBuilder().WaitAndRetryForeverAsync((attempt, _) => timeoutProvider.Invoke(attempt), OnTimeoutRetryAsync);
    }

    /// <summary> Retry with provided timeout on HTTP 429 with logging if request context contains logger </summary>
    /// <param name="timeoutProvider"> Timeout value provider </param>
    /// <param name="maxRetry"> Max retry count </param>
    [PublicAPI]
    public static IAsyncPolicy<HttpResponseMessage> TimeoutRetryAsyncPolicy(Func<int, TimeSpan> timeoutProvider, int maxRetry)
    {
        _ = timeoutProvider ?? throw new ArgumentNullException(nameof(timeoutProvider));
        if (maxRetry < 1) throw new ArgumentOutOfRangeException(nameof(maxRetry));
        return CreateHttp429PolicyBuilder().WaitAndRetryAsync(maxRetry, (attempt, _) => timeoutProvider.Invoke(attempt), OnTimeoutRetryAsync);
    }

    /// <summary> Retry forever with provided timeout on HTTP 429 with logging if request context contains logger </summary>
    /// <param name="timeoutProvider"> Timeout value provider </param>
    [PublicAPI]
    public static IAsyncPolicy<HttpResponseMessage> TimeoutRetryAsyncPolicy(Func<HttpResponseMessage, int, TimeSpan> timeoutProvider)
    {
        _ = timeoutProvider ?? throw new ArgumentNullException(nameof(timeoutProvider));
        return CreateHttp429PolicyBuilder()
            .WaitAndRetryForeverAsync((attempt, result, _) => timeoutProvider.Invoke(result.Result, attempt), OnTimeoutRetryAsync);
    }

    /// <summary> Retry with provided timeout on HTTP 429 with logging if request context contains logger </summary>
    /// <param name="timeoutProvider"> Timeout value provider </param>
    /// <param name="maxRetry"> Max retry count </param>
    [PublicAPI]
    public static IAsyncPolicy<HttpResponseMessage> TimeoutRetryAsyncPolicy(Func<HttpResponseMessage, int, TimeSpan> timeoutProvider, int maxRetry)
    {
        _ = timeoutProvider ?? throw new ArgumentNullException(nameof(timeoutProvider));
        if (maxRetry < 1) throw new ArgumentOutOfRangeException(nameof(maxRetry));
        return CreateHttp429PolicyBuilder()
            .WaitAndRetryAsync(maxRetry, (attempt, result, _) => timeoutProvider.Invoke(result.Result, attempt), OnTimeoutRetryAsync);
    }

    private static Task OnTimeoutRetryAsync(DelegateResult<HttpResponseMessage> result, int attempt, TimeSpan wait, Context context)
    {
        OnTimeoutRetry(result, attempt, wait, context);
        return Task.CompletedTask;
    }

    private static Task OnTimeoutRetryAsync(DelegateResult<HttpResponseMessage> result, TimeSpan wait, int attempt, Context context)
    {
        OnTimeoutRetry(result, attempt, wait, context);
        return Task.CompletedTask;
    }

    private static void OnTimeoutRetry(DelegateResult<HttpResponseMessage> result, TimeSpan wait, int attempt, Context context)
        => OnTimeoutRetry(result, attempt, wait, context);

    private static void OnTimeoutRetry(DelegateResult<HttpResponseMessage> result, int attempt, TimeSpan wait, Context context)
    {
        var logger = context.GetLogger();
        if (!logger.IsEnabled(LogLevel.Warning)) return;
        logger.LogWarning("HTTP {Method} to {Url} failed with {Code} - retry {Attempt} after {Timeout}",
            result.Result.RequestMessage?.Method,
            result.Result.RequestMessage?.RequestUri,
            result.Result.StatusCode,
            attempt,
            wait);
    }

    private static void OnTransientRetry(DelegateResult<HttpResponseMessage> result, int attempt, Context context)
    {
        var logger = context.GetLogger();
        if (!logger.IsEnabled(LogLevel.Warning)) return;
        if (result.Exception == null)
            logger.LogWarning("HTTP {Method} to {Url} failed with {Code} - retry {Attempt}",
                result.Result.RequestMessage?.Method,
                result.Result.RequestMessage?.RequestUri,
                result.Result.StatusCode,
                attempt);
        else
            logger.LogWarning(result.Exception, "HTTP Request failed with exception - retry {Attempt}", attempt);
    }

    private static PolicyBuilder<HttpResponseMessage> CreateHttp429PolicyBuilder()
#if NETSTANDARD
        => Policy.HandleResult<HttpResponseMessage>(response => response.StatusCode == (HttpStatusCode) 429);
#else
        => Policy.HandleResult<HttpResponseMessage>(response => response.StatusCode == HttpStatusCode.TooManyRequests);
#endif
}
