// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using FsCheck.Fluent;

namespace MartinCostello.EurovisionHue;

public static class ParticipantsTests
{
    public static TheoryData<string, IReadOnlyList<Color>> ParticipantTestCases() =>
    [
        ("Albania", [new(228, 30, 32), new(1, 0, 0), new(227, 30, 32), new(13, 2, 2), new(225, 30, 32)]),
        ("Andorra", [new(16, 6, 159), new(213, 0, 50), new(254, 221, 0), new(198, 170, 118), new(218, 41, 28)]),
        ("Armenia", [new(217, 0, 18), new(0, 51, 160), new(242, 168, 0)]),
        ("Australia", [new(0, 39, 118), new(255, 255, 255), new(224, 0, 52), new(85, 111, 164), new(153, 169, 200)]),
        ("Austria", [new(237, 41, 57), new(255, 255, 255)]),
        ("Azerbaijan", [new(0, 185, 228), new(63, 156, 53), new(237, 41, 57), new(255, 255, 255), new(237, 44, 59)]),
        ("Belarus", [new(200, 49, 62), new(74, 166, 87), new(255, 255, 255), new(229, 156, 163), new(245, 219, 221)]),
        ("Belgium", [new(250, 224, 66), new(237, 41, 57)]),
        ("Bosnia and Herzegovina", [new(0, 35, 149), new(254, 203, 0), new(255, 255, 255), new(37, 59, 127), new(227, 186, 16)]),
        ("Bulgaria", [new(255, 255, 255), new(0, 150, 110), new(214, 38, 18)]),
        ("Croatia", [new(255, 0, 0), new(23, 23, 150), new(255, 255, 255), new(0, 147, 221), new(247, 219, 23)]),
        ("Cyprus", [new(255, 255, 255), new(213, 120, 0), new(78, 91, 49), new(79, 92, 50), new(254, 254, 254)]),
        ("Czechia", [new(255, 255, 255), new(215, 20, 26), new(17, 69, 126), new(37, 84, 137), new(176, 193, 212)]),
        ("Denmark", [new(198, 12, 48), new(255, 255, 255)]),
        ("Estonia", [new(0, 114, 206), new(255, 255, 255)]),
        ("Finland", [new(255, 255, 255), new(0, 53, 128)]),
        ("France", [new(0, 35, 149), new(255, 255, 255), new(237, 41, 57)]),
        ("Georgia", [new(255, 255, 255), new(255, 0, 0), new(255, 85, 85), new(255, 136, 136), new(255, 254, 254)]),
        ("Germany", [new(221, 0, 0), new(255, 206, 0)]),
        ("Greece", [new(13, 94, 175), new(255, 255, 255), new(202, 220, 237), new(148, 184, 220), new(229, 238, 247)]),
        ("Hungary", [new(205, 42, 62), new(255, 255, 255), new(67, 111, 77)]),
        ("Iceland", [new(2, 82, 156), new(220, 30, 53), new(255, 255, 255), new(247, 205, 210), new(115, 159, 200)]),
        ("Ireland", [new(22, 155, 98), new(255, 255, 255), new(255, 136, 62)]),
        ("Israel", [new(255, 255, 255), new(0, 56, 184), new(17, 69, 189), new(102, 136, 212), new(119, 149, 217)]),
        ("Italy", [new(0, 146, 70), new(255, 255, 255), new(206, 43, 55), new(109, 192, 149), new(213, 73, 83)]),
        ("Latvia", [new(158, 48, 57), new(255, 255, 255)]),
        ("Lithuania", [new(253, 185, 19), new(0, 106, 68), new(193, 39, 45)]),
        ("Luxembourg", [new(239, 51, 64), new(255, 255, 255), new(0, 163, 224)]),
        ("Malta", [new(207, 20, 43), new(255, 255, 255), new(204, 204, 204), new(150, 135, 125), new(230, 13, 46)]),
        ("Moldova", [new(204, 9, 47), new(0, 70, 174), new(255, 210, 0), new(176, 127, 85), new(209, 4, 41)]),
        ("Monaco", [new(206, 17, 38), new(255, 255, 255)]),
        ("Morocco", [new(193, 39, 45), new(0, 98, 51), new(191, 39, 45), new(90, 70, 48), new(116, 62, 47)]),
        ("Montenegro", [new(227, 0, 0), new(255, 192, 0), new(234, 48, 0), new(0, 113, 188), new(57, 181, 74)]),
        ("Netherlands", [new(174, 28, 40), new(255, 255, 255), new(33, 70, 139)]),
        ("North Macedonia", [new(216, 33, 38), new(248, 233, 46), new(231, 127, 41), new(233, 139, 43), new(237, 166, 44)]),
        ("Norway", [new(239, 43, 45), new(0, 40, 104), new(255, 255, 255)]),
        ("Poland", [new(255, 255, 255), new(220, 20, 60)]),
        ("Portugal", [new(255, 0, 0), new(0, 102, 0), new(255, 255, 0), new(255, 255, 255), new(0, 51, 153)]),
        ("Romania", [new(0, 43, 127), new(252, 209, 22), new(206, 17, 38)]),
        ("Russia", [new(255, 255, 255), new(0, 57, 166), new(213, 43, 30)]),
        ("San Marino", [new(255, 255, 255), new(94, 182, 228), new(101, 141, 92), new(160, 207, 235), new(241, 191, 49)]),
        ("Serbia", [new(255, 255, 255), new(198, 54, 60), new(12, 64, 118), new(237, 185, 46), new(92, 127, 163)]),
        ("Slovakia", [new(238, 28, 37), new(255, 255, 255), new(11, 78, 162), new(188, 56, 82), new(214, 33, 50)]),
        ("Slovenia", [new(0, 93, 164), new(237, 28, 36), new(255, 255, 255), new(255, 221, 0), new(2, 94, 165)]),
        ("Spain", [new(198, 11, 30), new(255, 196, 0), new(173, 21, 25), new(204, 204, 204), new(200, 177, 0)]),
        ("Sweden", [new(0, 106, 167), new(254, 204, 0)]),
        ("Switzerland", [new(213, 43, 30), new(255, 255, 255)]),
        ("Turkey", [new(227, 10, 23), new(255, 255, 255), new(227, 9, 21), new(245, 165, 169), new(227, 11, 24)]),
        ("Turkiye", [new(227, 10, 23), new(255, 255, 255), new(227, 9, 21), new(245, 165, 169), new(227, 11, 24)]),
        ("Ukraine", [new(0, 91, 187), new(255, 213, 0)]),
        ("United Kingdom", [new(207, 20, 43), new(255, 255, 255), new(0, 36, 125), new(210, 216, 232), new(84, 108, 168)]),
    ];

    [Theory]
#pragma warning disable xUnit1045 // Avoid using TheoryData type arguments that might not be serializable
    [MemberData(nameof(ParticipantTestCases))]
#pragma warning restore xUnit1045 // Avoid using TheoryData type arguments that might not be serializable
    public static void TryFind_Can_Load_A_Valid_Participant(string name, IReadOnlyList<Color> flagColors)
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

        foreach (var color in colors)
        {
            color.R.ShouldBeInRange(0, 255);
            color.G.ShouldBeInRange(0, 255);
            color.B.ShouldBeInRange(0, 255);
            color.ShouldNotBe(new(0, 0, 0));
        }

        flagColors.ShouldBeSubsetOf(colors);
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
        Gen.Elements(ParticipantTestCases().AsEnumerable()).Select((p) => p.Data.Item1).ToArbitrary(),
        Gen.Choose(1, 100).ToArbitrary(),
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
