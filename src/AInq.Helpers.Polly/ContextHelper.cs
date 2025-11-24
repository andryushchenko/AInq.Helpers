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

/// <summary> Helpers to store data in <see cref="Context" /> </summary>
public static class ContextHelper
{
    private const string CancellationKey = "AInq.Helpers.Polly.ContextHelper.Keys.Cancellation";
    private const string LoggerKey = "AInq.Helpers.Polly.ContextHelper.Keys.Logger";

    /// <param name="context"> Context </param>
    extension(Context context)
    {
        /// <summary> Get data from context by key </summary>
        /// <param name="key"> Data key </param>
        /// <typeparam name="T"> Data type </typeparam>
        [PublicAPI]
        public T? Get<T>(string key)
            => (context ?? throw new ArgumentNullException(nameof(context)))
               .TryGetValue(string.IsNullOrWhiteSpace(key) ? throw new ArgumentNullException(nameof(key)) : key, out var value)
               && value is T result
                ? result
                : default;

        /// <summary> Add data to context with key </summary>
        /// <param name="key"> Data key </param>
        /// <param name="value"> Data value </param>
        [PublicAPI]
        public Context With(string key, object value)
        {
            (context ?? throw new ArgumentNullException(nameof(context)))[string.IsNullOrWhiteSpace(key)
                ? throw new ArgumentNullException(nameof(key))
                : key] = value;
            return context;
        }

        /// <summary> Add <see cref="CancellationToken" /> to context </summary>
        /// <param name="cancellation"> Cancellation token </param>
        [PublicAPI]
        public Context WithCancellation(CancellationToken cancellation)
            => (context ?? throw new ArgumentNullException(nameof(context))).With(CancellationKey, cancellation);

        /// <summary> Get <see cref="CancellationToken" /> from context </summary>
        [PublicAPI]
        public CancellationToken GetCancellationToken()
            => (context ?? throw new ArgumentNullException(nameof(context))).Get<CancellationToken>(CancellationKey);

        /// <summary> Add logger to context </summary>
        /// <param name="logger"> Logger instance </param>
        [PublicAPI]
        public Context WithLogger(ILogger logger)
            => (context ?? throw new ArgumentNullException(nameof(context))).With(LoggerKey,
                logger ?? throw new ArgumentNullException(nameof(logger)));

        /// <summary> Get logger from context </summary>
        [PublicAPI]
        public ILogger GetLogger()
            => (context ?? throw new ArgumentNullException(nameof(context))).Get<ILogger>(LoggerKey) ?? NullLogger.Instance;
    }
}
