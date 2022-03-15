// Copyright 2021-2022 Anton Andryushchenko
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

namespace AInq.Helpers.Linq;

/// <summary> Enumerable batch extension </summary>
public static class BatchHelper
{
    /// <summary> Pack the given enumerable in batches with a specified maximum batch size </summary>
    /// <param name="source"> Source enumerable </param>
    /// <param name="batchSize"> Maximum batch size </param>
    /// <typeparam name="T"> Element type </typeparam>
    /// <exception cref="ArgumentOutOfRangeException"> Thrown for incorrect <paramref name="batchSize" /> </exception>
    [PublicAPI]
    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, long batchSize)
    {
        _ = source ?? throw new ArgumentNullException(nameof(source));
        if (batchSize < 1) throw new ArgumentOutOfRangeException(nameof(batchSize), batchSize, "Should be >= 1");
        using var enumerator = source.GetEnumerator();
        while (enumerator.MoveNext())
            yield return enumerator.Take(batchSize);
    }

    /// <summary> Pack the given async enumerable in batches with a specified maximum batch size </summary>
    /// <param name="source"> Source enumerable </param>
    /// <param name="batchSize"> Maximum batch size </param>
    /// <typeparam name="T"> Element type </typeparam>
    /// <exception cref="ArgumentOutOfRangeException"> Thrown for incorrect <paramref name="batchSize" /> </exception>
    [PublicAPI]
    public static async IAsyncEnumerable<IAsyncEnumerable<T>> Batch<T>(this IAsyncEnumerable<T> source, long batchSize)
    {
        _ = source ?? throw new ArgumentNullException(nameof(source));
        if (batchSize < 1) throw new ArgumentOutOfRangeException(nameof(batchSize), batchSize, "Should be >= 1");
        var enumerator = source.GetAsyncEnumerator();
        await using var disposable = enumerator.ConfigureAwait(false);
        while (await enumerator.MoveNextAsync().ConfigureAwait(false))
            yield return enumerator.TakeAsync(batchSize);
    }

    private static IEnumerable<T> Take<T>(this IEnumerator<T> enumerator, long count)
    {
        yield return enumerator.Current;
        count--;
        while (count > 0 && enumerator.MoveNext())
        {
            count--;
            yield return enumerator.Current;
        }
    }

    private static async IAsyncEnumerable<T> TakeAsync<T>(this IAsyncEnumerator<T> enumerator, long count)
    {
        yield return enumerator.Current;
        count--;
        while (count > 0 && await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            count--;
            yield return enumerator.Current;
        }
    }
}
