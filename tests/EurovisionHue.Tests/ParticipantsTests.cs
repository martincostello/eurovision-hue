// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.EurovisionHue;

public static class ParticipantsTests
{
    [Theory]
    [InlineData("Albania")]
    [InlineData("Andorra")]
    [InlineData("Armenia")]
    [InlineData("Australia")]
    [InlineData("Austria")]
    [InlineData("Azerbaijan")]
    [InlineData("Belarus")]
    [InlineData("Belgium")]
    [InlineData("Bosnia and Herzegovina")]
    [InlineData("Bulgaria")]
    [InlineData("Croatia")]
    [InlineData("Cyprus")]
    [InlineData("Czechia")]
    [InlineData("Denmark")]
    [InlineData("Estonia")]
    [InlineData("Finland")]
    [InlineData("France")]
    [InlineData("Georgia")]
    [InlineData("Germany")]
    [InlineData("Greece")]
    [InlineData("Hungary")]
    [InlineData("Iceland")]
    [InlineData("Ireland")]
    [InlineData("Israel")]
    [InlineData("Italy")]
    [InlineData("Latvia")]
    [InlineData("Lithuania")]
    [InlineData("Luxembourg")]
    [InlineData("Malta")]
    [InlineData("Moldova")]
    [InlineData("Monaco")]
    [InlineData("Morocco")]
    [InlineData("Montenegro")]
    [InlineData("Netherlands")]
    [InlineData("North Macedonia")]
    [InlineData("Norway")]
    [InlineData("Poland")]
    [InlineData("Portugal")]
    [InlineData("Romania")]
    [InlineData("Russia")]
    [InlineData("San Marino")]
    [InlineData("Serbia")]
    [InlineData("Slovakia")]
    [InlineData("Slovenia")]
    [InlineData("Spain")]
    [InlineData("Sweden")]
    [InlineData("Switzerland")]
    [InlineData("Turkey")]
    [InlineData("Turkiye")]
    [InlineData("Ukraine")]
    [InlineData("United Kingdom")]
    public static void TryFind_Can_Load_A_Valid_Participant(string name)
    {
        // Arrange
        string article = $"This is a paragraph about the {name} act at the Eurovision Song Contest.";

        // Act
        var actual = Participants.TryFind(article, out var participant);

        // Assert
        actual.ShouldBeTrue();

        participant.ShouldNotBeNull();
        participant.Id.ShouldNotBeNullOrWhiteSpace();
        participant.Emoji.ShouldNotBeNullOrWhiteSpace();
        participant.Name.ShouldNotBeNullOrWhiteSpace();
        participant.Names.ShouldNotBeNull();
        participant.Names.ShouldContain(name);

        // Act
        var colors = participant.Colors(int.MaxValue);

        // Assert
        colors.ShouldNotBeNull();
        colors.ShouldNotBeEmpty();
    }

    [Fact]
    public static void TryFind_Cannot_Load_A_Participant_If_None_Found()
    {
        // Arrange
        string article = "This is a paragraph about the Eurovision Song Contest.";

        // Act
        var actual = Participants.TryFind(article, out var participant);

        // Assert
        actual.ShouldBeFalse();
        participant.ShouldBeNull();
    }
}
