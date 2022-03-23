# AInq.Helpers

[![GitHub release (latest by date)](https://img.shields.io/github/v/release/andryushchenko/AInq.Helpers)](https://github.com/andryushchenko/AInq.Helpers/releases) [![GitHub](https://img.shields.io/github/license/andryushchenko/AInq.Helpers)](LICENSE)

![AInq](https://raw.githubusercontent.com/andryushchenko/AInq.Helpers/main/AInq.png)

## What is it?

Collection of simple helpers for .Net

## Packages description
#### [![Nuget](https://img.shields.io/nuget/v/AInq.Helpers.Linq)](https://www.nuget.org/packages/AInq.Helpers.Linq/) AInq.Helpers.Linq

Helpers for `IEnumerable<T>` and `IAsyncEnumerable<T>`

- **Batch helper** - pack the given enumerable in batches with a specified maximum batch size

#### [![Nuget](https://img.shields.io/nuget/v/AInq.Helpers.Polly)](https://www.nuget.org/packages/AInq.Helpers.Polly/) AInq.Helpers.Polly

Helpers library to use with [Polly](https://github.com/App-vNext/Polly)

- Ready to use retry policies with logging
  - `TransientRetryAsyncPolicy` for transient errors
  - `TimeoutRetryAsyncPolicy` for HTTP 429
- Helpers to store data in `Polly.Context`
- Helpers to execute HTTP requests with `Polly.IAsyncPolicy<HttpResponseMessage>`

#### [![Nuget](https://img.shields.io/nuget/v/AInq.Helpers.Email)](https://www.nuget.org/packages/AInq.Helpers.Email/) AInq.Helpers.Email

Helpers to work with email address

- Get emails form string source
- Check if string is correct email 
- Get specific email address parts

#### [![Nuget](https://img.shields.io/nuget/v/AInq.Helpers.PhoneNumber)](https://www.nuget.org/packages/AInq.Helpers.PhoneNumber/) AInq.Helpers.PhoneNumber

Helpers to parse phone numbers from string

- Get all valid numbers or mobile only
- Parse one source with different regional formats at one call

This library uses [libphonenumber-csharp](https://github.com/twcclegg/libphonenumber-csharp) inside

## Documentation

As for now documentation is provided in this document and by XML documentation inside packages.

## Contribution

If you find a bug, have a question or something else - you are friendly welcome to open an issue.

## License

Copyright © 2021-2022 Anton Andryushchenko. AInq.Helpers is licensed under [Apache License 2.0](LICENSE)
