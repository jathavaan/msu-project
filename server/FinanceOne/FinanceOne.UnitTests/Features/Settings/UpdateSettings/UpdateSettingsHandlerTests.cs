using FinanceOne.Api.Features.Settings.UpdateSettings;

namespace FinanceOne.UnitTests.Features.Settings.UpdateSettings;

public class UpdateSettingsHandlerTests
{
    private readonly IUpdateSettingsRepository _repository = Substitute.For<IUpdateSettingsRepository>();

    private UpdateSettingsHandler Handler => new(_repository);

    [Fact]
    public async Task Creates_A_Row_When_None_Exists_Yet()
    {
        _repository.GetSettings(Arg.Any<CancellationToken>()).Returns((AppSettings?)null);

        var response = await Handler.Handle(new UpdateSettingsCommand(25), CancellationToken.None);

        Assert.True(response.IsSuccess);
        await _repository.Received(1).Add(
            Arg.Is<AppSettings>(s => s.PeriodStartDay == 25), Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().Update(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Updates_The_Existing_Row_Instead_Of_Adding_A_Second_One()
    {
        var settings = new AppSettings { Id = Guid.NewGuid(), PeriodStartDay = 1 };
        _repository.GetSettings(Arg.Any<CancellationToken>()).Returns(settings);

        var response = await Handler.Handle(new UpdateSettingsCommand(25), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(25, settings.PeriodStartDay);
        await _repository.Received(1).Update(Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().Add(Arg.Any<AppSettings>(), Arg.Any<CancellationToken>());
    }
}
