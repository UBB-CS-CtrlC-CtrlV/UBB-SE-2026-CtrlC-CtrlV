// <copyright file="CategoryTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Domain.Entities;

namespace BankApp.Domain.Tests.Entities;

/// <summary>
/// Unit tests for <see cref="Category"/>.
/// </summary>
public class CategoryTests
{
    /// <summary>
    /// Verifies the default values assigned to a new category.
    /// </summary>
    [Fact]
    public void Constructor_WhenCreated_SetsExpectedDefaults()
    {
        // Act
        var category = new Category();

        // Assert
        category.Id.Should().Be(0);
        category.Name.Should().BeEmpty();
        category.Icon.Should().BeNull();
        category.IsSystem.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that all category properties can be assigned and read back.
    /// </summary>
    [Fact]
    public void Properties_WhenAssigned_ReturnAssignedValues()
    {
        // Arrange
        var category = new Category
        {
            Id = 7,
            Name = "Groceries",
            Icon = "shopping-cart",
            IsSystem = false,
        };

        // Assert
        category.Id.Should().Be(7);
        category.Name.Should().Be("Groceries");
        category.Icon.Should().Be("shopping-cart");
        category.IsSystem.Should().BeFalse();
    }
}
