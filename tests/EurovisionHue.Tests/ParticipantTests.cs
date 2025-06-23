// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.EurovisionHue;

#pragma warning disable CA1508

public static class ParticipantTests
{
    [Fact]
    public static void Should_Be_Equal_By_Id()
    {
        // Arrange
        var target = new Participant("DEU", "Germany", "🇩🇪");

        var nullTarget = (Participant?)null;
        var other = new Participant("DEU", "Deutschland", "🇩🇪");

        // Act and Assert
        target.Equals(nullTarget).ShouldBeFalse();
        target.Equals(target).ShouldBeTrue();
        target.Equals(other).ShouldBeTrue();
    }

    [Fact]
    public static void GetHashCode_Returns_Same_For_Equal_Participants()
    {
        // Arrange
        var target = new Participant("DEU", "Germany", "🇩🇪");
        var other = new Participant("DEU", "Deutschland", "🇩🇪");

        // Act
        var first = target.GetHashCode();
        var second = other.GetHashCode();

        // Assert
        first.ShouldNotBe(0);
        first.ShouldBe(second);
    }

    [Fact]
    public static void ToString_Returns_Name()
    {
        // Arrange
        var target = new Participant("DEU", "Germany", "🇩🇪");

        // Act
        var actual = target.ToString();

        // Assert
        actual.ShouldBe("Germany");
    }

    [Fact]
    public static void Names_Should_Contain_Name()
    {
        // Arrange
        var target = new Participant("DEU", "Germany", "🇩🇪", ["Deutschland"]);

        // Act
        var names = target.Names;

        // Assert
        names.ShouldNotBeNull();
        names.ShouldBe(["Germany", "Deutschland"]);
    }
}
