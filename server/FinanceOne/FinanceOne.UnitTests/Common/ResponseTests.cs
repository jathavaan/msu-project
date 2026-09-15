namespace FinanceOne.UnitTests.Common;

// Response<T> is the contract every handler returns and every endpoint branches on, and the
// frontend's apiBaseQuery unwraps it by checking errorCode. IsSuccess being keyed off
// "ErrorCode is null" — not off Result being non-null — is the part worth pinning down.
public class ResponseTests
{
    [Fact]
    public void Success_Has_No_Error_Code()
    {
        var response = Response<Guid>.Success(Guid.Empty);

        Assert.True(response.IsSuccess);
        Assert.Null(response.ErrorCode);
        Assert.Null(response.ErrorMessage);
    }

    // Guid.Empty and an empty list are legitimate successful results; neither may be mistaken for
    // a failure just because it is the type's default.
    [Fact]
    public void Success_With_A_Default_Result_Is_Still_A_Success()
    {
        Assert.True(Response<Guid>.Success(Guid.Empty).IsSuccess);
        Assert.True(Response<List<string>>.Success([]).IsSuccess);
        Assert.True(Response<Unit>.Success(new Unit()).IsSuccess);
    }

    [Theory]
    [InlineData(StatusCodes.Status404NotFound)]
    [InlineData(StatusCodes.Status409Conflict)]
    public void Failure_Carries_Its_Status_Code_And_Message(int errorCode)
    {
        var response = Response<Guid>.Failure(errorCode, "Nope.");

        Assert.False(response.IsSuccess);
        Assert.Equal(errorCode, response.ErrorCode);
        Assert.Equal("Nope.", response.ErrorMessage);
        Assert.Equal(Guid.Empty, response.Result);
    }
}
