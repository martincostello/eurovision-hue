// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using FsCheck;
using FsCheck.Fluent;

namespace MartinCostello.EurovisionHue;

public static class ParticipantsTests
{
    public static IEnumerable<string> ParticipantNames() =>
    [
        "Albania",
        "Andorra",
        "Armenia",
        "Australia",
        "Austria",
        "Azerbaijan",
        "Belarus",
        "Belgium",
        "Bosnia and Herzegovina",
        "Bulgaria",
        "Croatia",
        "Cyprus",
        "Czechia",
        "Denmark",
        "Estonia",
        "Finland",
        "France",
        "Georgia",
        "Germany",
        "Greece",
        "Hungary",
        "Iceland",
        "Ireland",
        "Israel",
        "Italy",
        "Latvia",
        "Lithuania",
        "Luxembourg",
        "Malta",
        "Moldova",
        "Monaco",
        "Morocco",
        "Montenegro",
        "Netherlands",
        "North Macedonia",
        "Norway",
        "Poland",
        "Portugal",
        "Romania",
        "Russia",
        "San Marino",
        "Serbia",
        "Slovakia",
        "Slovenia",
        "Spain",
        "Sweden",
        "Switzerland",
        "Turkey",
        "Turkiye",
        "Ukraine",
        "United Kingdom",
    ];

    public static TheoryData<string> ParticipantTestCases()
    {
        var data = new TheoryData<string>();

        foreach (var participant in ParticipantNames())
        {
            data.Add(participant);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(ParticipantTestCases))]
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

    [FsCheck.Xunit.Property(MaxTest = 500)]
    public static void Participant_Colors_Meets_Specification() => Prop.ForAll(
        Gen.Elements(ParticipantNames()).ToArbitrary(),
        Gen.Choose(0, 100).ToArbitrary(),
        (name, count) =>
        {
            // Act
            var actual = Participants.TryFind(name, out var participant);

            // Assert
            actual.ShouldBeTrue();
            participant.ShouldNotBeNull();

            // Act
            var colors = participant.Colors(count);

            // Assert
            colors.ShouldNotBeNull();
            colors.ShouldNotBeEmpty();
        });
}
