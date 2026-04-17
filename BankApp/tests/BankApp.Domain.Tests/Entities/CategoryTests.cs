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
    private const int DefaultCategoryId = 0;

    [Fact]
    public void Constructor_WhenCreated_SetsExpectedDefaults()
    {
        // Act
        var category = new Category();

        // Assert
        category.Id.Should().Be(DefaultCategoryId);
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
        const int expectedCategoryId = 7;
        const string expectedCategoryName = "Groceries";
        const string expectedCategoryIcon = "shopping-cart";
        const bool expectedIsSystemCategory = false;

        var category = new Category
        {
            Id = expectedCategoryId,
            Name = expectedCategoryName,
            Icon = expectedCategoryIcon,
            IsSystem = expectedIsSystemCategory,
        };

        // Assert
        category.Id.Should().Be(expectedCategoryId);
        category.Name.Should().Be(expectedCategoryName);
        category.Icon.Should().Be(expectedCategoryIcon);
        category.IsSystem.Should().Be(expectedIsSystemCategory);
    }
}
