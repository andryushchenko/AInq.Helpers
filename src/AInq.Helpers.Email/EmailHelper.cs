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

using System.Text.RegularExpressions;

namespace AInq.Helpers.Email;

/// <summary> Email <see cref="string" /> extension </summary>
public static class EmailHelper
{
    private static readonly Lazy<Regex> Pattern = new(() => new Regex(
        @"\s*(?<email>(?<user>\w[\w!#$%&'*/=?`{|}~^-]*(?:\.[\w!#$%&'*/=?`{|}~^-]+)*)(?<marker>\+[\w!#$%&'*/=?`{|}~^+\.-]*)?@(?<domain>(?:[a-z0-9-]+\.)+[a-z]{2,}))\s*",
        RegexOptions.IgnoreCase | RegexOptions.Compiled,
        Regex.InfiniteMatchTimeout));

    /// <summary> Check is source string contains correct email </summary>
    /// <param name="source"> Source </param>
    [PublicAPI]
    public static bool ContainsEmail(this string? source)
        => !string.IsNullOrWhiteSpace(source) && Pattern.Value.IsMatch(source);

    /// <summary> Check is source string is correct email </summary>
    /// <param name="source"> Source </param>
    [PublicAPI]
    public static bool IsEmail(this string? source)
    {
        if (string.IsNullOrWhiteSpace(source)) return false;
        var matches = Pattern.Value.Matches(source);
        return matches.Count == 1 && string.Equals(matches[0].Value, source, StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary> Get single email from source string </summary>
    /// <param name="source"> Source </param>
    /// <param name="trimMarker"> Remove additional marker (user+MARKER@domain) from email </param>
    /// <exception cref="ArgumentException"> Thrown if source is not correct email or contains more then one email </exception>
    [PublicAPI]
    public static string GetEmail(this string source, bool trimMarker = false)
    {
        var matches = Pattern.Value.Matches(source ?? throw new ArgumentNullException(nameof(source)));
        return matches.Count switch
        {
            0 => throw new ArgumentException("Not a correct email", nameof(source)),
            1 => trimMarker
                ? $"{matches[0].Groups["user"].Value}@{matches[0].Groups["domain"].Value}".ToLowerInvariant()
                : matches[0].Groups["email"].Value.ToLowerInvariant(),
            _ => throw new ArgumentException("Source contains more than one email", nameof(source))
        };
    }

    /// <summary> Get emails from source string </summary>
    /// <param name="source"> Source </param>
    /// <param name="trimMarker"> Remove additional marker (user+MARKER@domain) from email </param>
    [PublicAPI]
    public static IReadOnlyCollection<string> GetEmails(this string source, bool trimMarker = false)
        => string.IsNullOrWhiteSpace(source)
            ? Array.Empty<string>()
            : new HashSet<string>(Pattern.Value.Matches(source)
#if NETSTANDARD2_0
                                         .Cast<Match>()
#endif
                                         .Select(match => trimMarker
                                             ? $"{match.Groups["user"].Value}@{match.Groups["domain"].Value}".ToLowerInvariant()
                                             : match.Groups["email"].Value.ToLowerInvariant()),
                StringComparer.InvariantCultureIgnoreCase);

    /// <summary> Get user name (USER+marker@domain) from email </summary>
    /// <param name="email"> Email </param>
    /// <exception cref="ArgumentException"> Thrown if source is not correct email or contains more then one email </exception>
    [PublicAPI]
    public static string GetEmailUser(this string email)
    {
        var matches = Pattern.Value.Matches(email ?? throw new ArgumentNullException(nameof(email)));
        return matches.Count switch
        {
            0 => throw new ArgumentException("Not a correct email", nameof(email)),
            1 => matches[0].Groups["user"].Value.ToLowerInvariant(),
            _ => throw new ArgumentException("Source contains more than one email", nameof(email))
        };
    }

    /// <summary> Get domain name (user+marker@DOMAIN) from email </summary>
    /// <param name="email"> Email </param>
    /// <exception cref="ArgumentException"> Thrown if source is not correct email or contains more then one email </exception>
    [PublicAPI]
    public static string GetEmailDomain(this string email)
    {
        var matches = Pattern.Value.Matches(email ?? throw new ArgumentNullException(nameof(email)));
        return matches.Count switch
        {
            0 => throw new ArgumentException("Not a correct email", nameof(email)),
            1 => matches[0].Groups["domain"].Value.ToLowerInvariant(),
            _ => throw new ArgumentException("Source contains more than one email", nameof(email))
        };
    }
}
