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

using System.Globalization;

namespace AInq.Helpers.PhoneNumber;

/// <summary> Phone number parsing <see cref="string" /> extension </summary>
public static class PhoneNumberHelper
{
    private const string DefaultExtensionSeparator = " # ";
    private static readonly Lazy<PhoneNumberUtil> Util = new(PhoneNumberUtil.GetInstance);
    private static readonly PhoneNumberMatchEqualityComparer Comparer = new();

    /// <summary> Regions (ISO 3166) used for number parsing in order of priority </summary>
    /// <remarks> Default value is country code from <see cref="CultureInfo.CurrentCulture" /> </remarks>
    [PublicAPI]
    public static IReadOnlyCollection<string> PhoneRegions { get; set; } =
        new[] {CultureInfo.CurrentCulture.Name.Split('-').Last().ToUpperInvariant()};

    /// <summary> Phone number extension separator used for result formatting </summary>
    [PublicAPI]
    public static string ExtensionSeparator { get; set; } = DefaultExtensionSeparator;

    /// <inheritdoc cref="GetPhones(string,bool,string[])" />
    [PublicAPI]
    public static ISet<string> GetPhones(this string source, IEnumerable<string> phoneRegions, bool trimExtension = false)
        => GetPhones(source, trimExtension, (phoneRegions ?? throw new ArgumentNullException(nameof(phoneRegions))).ToArray());

    /// <summary> Get phone numbers from string </summary>
    /// <param name="source"> Source string </param>
    /// <param name="trimExtension"> Flag to trim phone number extensions </param>
    /// <param name="phoneRegions"> Regions for number parsing in order of priority (default is <see cref="PhoneRegions" />) </param>
    [PublicAPI]
    public static ISet<string> GetPhones(this string source, bool trimExtension = false, params string[] phoneRegions)
        => string.IsNullOrWhiteSpace(source)
            ? new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
            : new HashSet<string>((phoneRegions.Length == 0 ? PhoneRegions : phoneRegions)
                                  .Aggregate(Enumerable.Empty<PhoneNumberMatch>(),
                                      (matches, region)
                                          => matches.Union(Util.Value.FindNumbers(source, region)
                                                               .Where(match => Util.Value.IsValidNumber(match.Number)),
                                              Comparer))
                                  .Select(match => match.Number.HasExtension && !trimExtension
                                      ? string.Join(string.Empty,
                                          Util.Value.Format(match.Number, PhoneNumberFormat.E164),
                                          ExtensionSeparator,
                                          match.Number.Extension)
                                      : Util.Value.Format(match.Number, PhoneNumberFormat.E164)),
                StringComparer.InvariantCultureIgnoreCase);

    /// <inheritdoc cref="GetMobilePhones(string,bool,string[])" />
    [PublicAPI]
    public static ISet<string> GetMobilePhones(this string source, IEnumerable<string> phoneRegions, bool strict = true)
        => GetMobilePhones(source, strict, (phoneRegions ?? throw new ArgumentNullException(nameof(phoneRegions))).ToArray());

    /// <summary> Get mobile phone numbers from string </summary>
    /// <param name="source"> Source string </param>
    /// <param name="strict"> Flag to strictly check number type (ignore <see cref="PhoneNumberType.FIXED_LINE_OR_MOBILE" />) </param>
    /// <param name="phoneRegions"> Regions for number parsing in order of priority (default is <see cref="PhoneRegions" />) </param>
    [PublicAPI]
    public static ISet<string> GetMobilePhones(this string source, bool strict = true, params string[] phoneRegions)
        => string.IsNullOrWhiteSpace(source)
            ? new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
            : new HashSet<string>((phoneRegions.Length == 0 ? PhoneRegions : phoneRegions)
                                  .Aggregate(Enumerable.Empty<PhoneNumberMatch>(),
                                      (matches, region)
                                          => matches.Union(Util.Value.FindNumbers(source, region)
                                                               .Where(match => strict
                                                                   ? IsNumberOfType(match.Number, PhoneNumberType.MOBILE)
                                                                   : IsNumberOfType(match.Number,
                                                                       PhoneNumberType.MOBILE,
                                                                       PhoneNumberType.FIXED_LINE_OR_MOBILE)),
                                              Comparer))
                                  .Select(match => Util.Value.Format(match.Number, PhoneNumberFormat.E164)),
                StringComparer.InvariantCultureIgnoreCase);

    private static bool IsNumberOfType(PhoneNumbers.PhoneNumber phone, params PhoneNumberType[] types)
    {
        if (!Util.Value.IsValidNumber(phone)) return false;
        var numberType = Util.Value.GetNumberType(phone);
        return types.Any(type => numberType == type);
    }
}
