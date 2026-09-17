using System.Security.Cryptography;
using System.Text;
using FinanceOne.Api.Common.BlobStorage;
using FinanceOne.Api.Features.Transactions.ImportTransactions;

namespace FinanceOne.UnitTests.Features.Transactions.ImportTransactions;

public class ImportTransactionsHandlerTests
{
    private const string Header = "Dato;Beskrivelse;Type;Undertype;Beløp inn;Beløp ut;Status";

    private readonly IImportTransactionsRepository _repository = Substitute.For<IImportTransactionsRepository>();
    private readonly IBlobStorageService _blobStorage = Substitute.For<IBlobStorageService>();

    private ImportTransactionsHandler Handler => new(_repository, _blobStorage);

    public ImportTransactionsHandlerTests()
    {
        _repository.GetCategorizationRules(Arg.Any<CancellationToken>()).Returns([]);
        _repository.GetExistingHashes(Arg.Any<List<string>>(), Arg.Any<CancellationToken>()).Returns([]);
    }

    private static MemoryStream BuildCsv(params string[] dataRows) =>
        new(Encoding.UTF8.GetBytes(string.Join('\n', [Header, .. dataRows])));

    private static string ExpectedHash(DateOnly date, decimal amount, string description) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{date:O}|{amount}|{description}")));

    [Fact]
    public async Task Returns_400_When_The_File_Is_Not_Named_Csv()
    {
        var response = await Handler.Handle(
            new ImportTransactionsCommand(BuildCsv(), "transactions.txt", 100), CancellationToken.None);

        Assert.Equal(StatusCodes.Status400BadRequest, response.ErrorCode);
        await _blobStorage.DidNotReceive().UploadAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10 * 1024 * 1024 + 1)]
    public async Task Returns_400_When_The_File_Is_Empty_Or_Too_Large(long length)
    {
        var response = await Handler.Handle(
            new ImportTransactionsCommand(BuildCsv(), "transactions.csv", length), CancellationToken.None);

        Assert.Equal(StatusCodes.Status400BadRequest, response.ErrorCode);
        await _blobStorage.DidNotReceive().UploadAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Returns_400_When_The_File_Cannot_Be_Parsed()
    {
        // Nonzero Length (passes the size check) but no actual header row in the content — the
        // mismatch is realistic for a truncated/corrupted upload.
        var response = await Handler.Handle(
            new ImportTransactionsCommand(new MemoryStream(), "transactions.csv", 10), CancellationToken.None);

        Assert.Equal(StatusCodes.Status400BadRequest, response.ErrorCode);
        await _repository.DidNotReceive().AddRange(Arg.Any<List<Transaction>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Stages_The_Raw_File_In_Blob_Storage_Before_Parsing()
    {
        var csv = BuildCsv("01.06.2026;REMA 1000;Varekjøp;;;150,00;Bokført");

        await Handler.Handle(new ImportTransactionsCommand(csv, "june.csv", 100), CancellationToken.None);

        await _blobStorage.Received(1).UploadAsync(
            BlobContainers.StagedCsv, Arg.Is<string>(n => n.EndsWith("june.csv")), Arg.Any<Stream>(), "text/csv", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Imports_A_Booked_Debit_Row_And_Applies_The_Longest_Matching_Rule()
    {
        var foodCategoryId = Guid.NewGuid();
        var groceryCategoryId = Guid.NewGuid();
        _repository.GetCategorizationRules(Arg.Any<CancellationToken>()).Returns(
        [
            new CategorizationRule { Id = Guid.NewGuid(), Keyword = "REMA", CategoryId = foodCategoryId },
            new CategorizationRule { Id = Guid.NewGuid(), Keyword = "REMA 1000", CategoryId = groceryCategoryId }
        ]);
        var csv = BuildCsv("01.06.2026;REMA 1000 Majorstuen;Varekjøp;;;150,00;Bokført");

        var response = await Handler.Handle(new ImportTransactionsCommand(csv, "june.csv", 100), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(1, response.Result!.ImportedCount);
        Assert.Equal(0, response.Result.UncategorizedCount);
        await _repository.Received(1).AddRange(
            Arg.Is<List<Transaction>>(t =>
                t.Count == 1
                && t[0].Date == new DateOnly(2026, 6, 1)
                && t[0].Description == "REMA 1000 Majorstuen"
                && t[0].Amount == -150.00m
                && t[0].CategoryId == groceryCategoryId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Imports_A_Booked_Credit_Row()
    {
        var csv = BuildCsv("15.06.2026;Salary;Lønn;;35000,00;;Bokført");

        var response = await Handler.Handle(new ImportTransactionsCommand(csv, "june.csv", 100), CancellationToken.None);

        Assert.True(response.IsSuccess);
        await _repository.Received(1).AddRange(
            Arg.Is<List<Transaction>>(t => t.Count == 1 && t[0].Amount == 35_000.00m),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Skips_Reservert_Rows_As_Pending()
    {
        var csv = BuildCsv("01.06.2026;REMA 1000;Varekjøp;;;150,00;Reservert");

        var response = await Handler.Handle(new ImportTransactionsCommand(csv, "june.csv", 100), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(0, response.Result!.ImportedCount);
        Assert.Equal(1, response.Result.PendingSkippedCount);
        await _repository.Received(1).AddRange(Arg.Is<List<Transaction>>(t => t.Count == 0), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Excludes_Transfers_Between_The_Users_Own_Accounts()
    {
        var csv = BuildCsv("01.06.2026;Til sparekonto;Overføring;Til egen konto;;5000,00;Bokført");

        var response = await Handler.Handle(new ImportTransactionsCommand(csv, "june.csv", 100), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(0, response.Result!.ImportedCount);
        Assert.Equal(1, response.Result.TransferExcludedCount);
    }

    [Fact]
    public async Task Skips_Rows_Already_Imported_By_Hash()
    {
        var date = new DateOnly(2026, 6, 1);
        var existingHash = ExpectedHash(date, -150.00m, "REMA 1000");
        _repository.GetExistingHashes(Arg.Any<List<string>>(), Arg.Any<CancellationToken>()).Returns([existingHash]);
        var csv = BuildCsv("01.06.2026;REMA 1000;Varekjøp;;;150,00;Bokført");

        var response = await Handler.Handle(new ImportTransactionsCommand(csv, "june.csv", 100), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(0, response.Result!.ImportedCount);
        Assert.Equal(1, response.Result.DuplicateSkippedCount);
    }

    [Fact]
    public async Task Skips_The_Second_Occurrence_Of_The_Same_Row_Within_One_File()
    {
        var csv = BuildCsv(
            "01.06.2026;REMA 1000;Varekjøp;;;150,00;Bokført",
            "01.06.2026;REMA 1000;Varekjøp;;;150,00;Bokført");

        var response = await Handler.Handle(new ImportTransactionsCommand(csv, "june.csv", 100), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(1, response.Result!.ImportedCount);
        Assert.Equal(1, response.Result.DuplicateSkippedCount);
    }

    [Fact]
    public async Task Leaves_Rows_Matching_No_Rule_Uncategorized()
    {
        var csv = BuildCsv("01.06.2026;Unknown Merchant AS;Varekjøp;;;99,00;Bokført");

        var response = await Handler.Handle(new ImportTransactionsCommand(csv, "june.csv", 100), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(1, response.Result!.ImportedCount);
        Assert.Equal(1, response.Result.UncategorizedCount);
        await _repository.Received(1).AddRange(
            Arg.Is<List<Transaction>>(t => t.Count == 1 && t[0].CategoryId == null),
            Arg.Any<CancellationToken>());
    }

    // Real bank exports use "." as a thousands separator (e.g. "1.500,00"), which .NET's nb-NO
    // culture itself doesn't accept (its NumberGroupSeparator is a non-breaking space) — see
    // issue #96. This pins the parser's own normalization down instead of relying on that culture.
    [Fact]
    public async Task Imports_A_Debit_Row_With_A_Thousands_Separator_In_The_Amount()
    {
        var csv = BuildCsv("01.06.2026;Rent;Husleie;;;1.500,00;Bokført");

        var response = await Handler.Handle(new ImportTransactionsCommand(csv, "june.csv", 100), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(1, response.Result!.ImportedCount);
        Assert.Equal(0, response.Result.InvalidRowCount);
        await _repository.Received(1).AddRange(
            Arg.Is<List<Transaction>>(t => t.Count == 1 && t[0].Amount == -1_500.00m),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Imports_A_Credit_Row_With_A_Thousands_Separator_In_The_Amount()
    {
        var csv = BuildCsv("15.06.2026;Salary;Lønn;;12.345,67;;Bokført");

        var response = await Handler.Handle(new ImportTransactionsCommand(csv, "june.csv", 100), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(0, response.Result!.InvalidRowCount);
        await _repository.Received(1).AddRange(
            Arg.Is<List<Transaction>>(t => t.Count == 1 && t[0].Amount == 12_345.67m),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Counts_A_Row_With_Both_Amount_Columns_Populated_As_Invalid()
    {
        var csv = BuildCsv("01.06.2026;Ambiguous;Varekjøp;;100,00;50,00;Bokført");

        var response = await Handler.Handle(new ImportTransactionsCommand(csv, "june.csv", 100), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(0, response.Result!.ImportedCount);
        Assert.Equal(1, response.Result.InvalidRowCount);
    }
}
