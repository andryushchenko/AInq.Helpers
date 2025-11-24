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

using System.Text.RegularExpressions;

namespace AInq.Helpers.Email;

/// <summary> Email <see cref="string" /> extension </summary>
#if NETSTANDARD
public static class EmailHelper
#else
public static partial class EmailHelper
#endif
{
#if NETSTANDARD
    private static readonly Lazy<Regex> PatternValue = new(() => new Regex(
        @"\s*(?<email>(?<user>\w[\w!#$%&'*/=?`{|}~^-]*(?:\.[\w!#$%&'*/=?`{|}~^-]+)*)(?<marker>\+[\w!#$%&'*/=?`{|}~^+\.-]*)?@(?<domain>(?:[a-z0-9-]+\.)+[a-z]{2,}))\s*",
        RegexOptions.IgnoreCase | RegexOptions.Compiled,
        Regex.InfiniteMatchTimeout));

    private static Regex Pattern()
        => PatternValue.Value;
#else
    [GeneratedRegex(
        @"\s*(?<email>(?<user>\w[\w!#$%&'*/=?`{|}~^-]*(?:\.[\w!#$%&'*/=?`{|}~^-]+)*)(?<marker>\+[\w!#$%&'*/=?`{|}~^+\.-]*)?@(?<domain>(?:[a-z0-9-]+\.)+[a-z]{2,}))\s*",
        RegexOptions.IgnoreCase | RegexOptions.NonBacktracking)]
    private static partial Regex Pattern();
#endif

    /// <param name="source"> Source string </param>
    extension(string? source)
    {
        /// <summary> Check is source string contains correct email </summary>
        [PublicAPI]
        public bool ContainsEmail()
            => !string.IsNullOrWhiteSpace(source) && Pattern().IsMatch(source);

        /// <summary> Check is source string is correct email </summary>
        [PublicAPI]
        public bool IsEmail()
        {
            if (string.IsNullOrWhiteSpace(source)) return false;
            var matches = Pattern().Matches(source);
            return matches.Count == 1 && string.Equals(matches[0].Value, source, StringComparison.InvariantCultureIgnoreCase);
        }
    }

    /// <param name="source"> Source string </param>
    extension(string source)
    {
        /// <summary> Get single email from source string </summary>
        /// <param name="trimMarker"> Remove additional marker (user+MARKER@domain) from email </param>
        /// <exception cref="ArgumentException"> Thrown if source is not correct email or contains more than one email </exception>
        [PublicAPI]
        public string GetEmail(bool trimMarker = false)
        {
            var matches = Pattern().Matches(source ?? throw new ArgumentNullException(nameof(source)));
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
        /// <param name="trimMarker"> Remove additional marker (user+MARKER@domain) from email </param>
        [PublicAPI]
        public IReadOnlyCollection<string> GetEmails(bool trimMarker = false)
            => string.IsNullOrWhiteSpace(source)
                ? Array.Empty<string>()
                : new HashSet<string>(Pattern()
                                      .Matches(source)
#if NETSTANDARD
                                      .Cast<Match>()
#endif
                                      .Select(match => trimMarker
                                          ? $"{match.Groups["user"].Value}@{match.Groups["domain"].Value}".ToLowerInvariant()
                                          : match.Groups["email"].Value.ToLowerInvariant()),
                    StringComparer.InvariantCultureIgnoreCase);

        /// <summary> Get user name (USER+marker@domain) from email </summary>
        /// <exception cref="ArgumentException"> Thrown if source is not correct email or contains more than one email </exception>
        [PublicAPI]
        public string GetEmailUser()
        {
            var matches = Pattern().Matches(source ?? throw new ArgumentNullException(nameof(source)));
            return matches.Count switch
            {
                0 => throw new ArgumentException("Not a correct email", nameof(source)),
                1 => matches[0].Groups["user"].Value.ToLowerInvariant(),
                _ => throw new ArgumentException("Source contains more than one email", nameof(source))
            };
        }

        /// <summary> Get domain name (user+marker@DOMAIN) from email </summary>
        /// <exception cref="ArgumentException"> Thrown if source is not correct email or contains more than one email </exception>
        [PublicAPI]
        public string GetEmailDomain()
        {
            var matches = Pattern().Matches(source ?? throw new ArgumentNullException(nameof(source)));
            return matches.Count switch
            {
                0 => throw new ArgumentException("Not a correct email", nameof(source)),
                1 => matches[0].Groups["domain"].Value.ToLowerInvariant(),
                _ => throw new ArgumentException("Source contains more than one email", nameof(source))
            };
        }
    }
}
