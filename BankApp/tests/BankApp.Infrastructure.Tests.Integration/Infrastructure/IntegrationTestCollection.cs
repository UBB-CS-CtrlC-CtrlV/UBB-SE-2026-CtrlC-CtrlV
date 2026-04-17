// Copyright (c) BankApp. All rights reserved.
// Licensed under the MIT license.

using Xunit;

namespace BankApp.Infrastructure.Tests.Integration.Infrastructure;

[CollectionDefinition("Integration")]
public sealed class IntegrationTestCollection : ICollectionFixture<DatabaseFixture>
{
}
