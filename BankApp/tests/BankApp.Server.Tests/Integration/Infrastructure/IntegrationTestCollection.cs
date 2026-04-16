// Copyright (c) BankApp. All rights reserved.
// Licensed under the MIT license.

using Xunit;

namespace BankApp.Server.Tests.Integration.Infrastructure;

[CollectionDefinition("Integration")]
public sealed class IntegrationTestCollection : ICollectionFixture<DatabaseFixture>
{
}
