using System.Text;
using Azure.Storage.Blobs.Models;
using FinanceOne.Api.Common.BlobStorage;
using FinanceOne.Api.Features.Transactions.ImportTransactions;
using FinanceOne.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace FinanceOne.IntegrationTests.Features.Transactions.ImportTransactions;

public class ImportTransactionsTests(MySqlFixture mySqlFixture, AzuriteFixture azuriteFixture)
    : IntegrationTest(mySqlFixture)
{
    private const string Header = "Dato;Beskrivelse;Type;Undertype;Beløp inn;Beløp ut;Status";

    private ImportTransactionsHandler Handler =>
        new(new ImportTransactionsRepository(Context), azuriteFixture.BlobStorageService);

    private static MemoryStream BuildCsv(params string[] dataRows) =>
        new(Encoding.UTF8.GetBytes(string.Join('\n', [Header, .. dataRows])));

    [Fact]
    public async Task Persists_Booked_Transactions_And_Stages_The_File_In_Blob_Storage()
    {
        var csv = BuildCsv(
            "01.06.2026;REMA 1000 Majorstuen;Varekjøp;;;150,00;Bokført",
            "15.06.2026;Salary;Lønn;;35000,00;;Bokført",
            "10.06.2026;Til sparekonto;Overføring;Til egen konto;;5000,00;Bokført",
            "20.06.2026;Pending purchase;Varekjøp;;;99,00;Reservert");

        var response = await Handler.Handle(
            new ImportTransactionsCommand(csv, "june.csv", 500), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(2, response.Result!.ImportedCount);
        Assert.Equal(1, response.Result.TransferExcludedCount);
        Assert.Equal(1, response.Result.PendingSkippedCount);
        Assert.Equal(2, response.Result.UncategorizedCount);

        await using var context = NewContext();
        var saved = await context.Transactions.OrderBy(t => t.Date).ToListAsync();
        Assert.Equal(2, saved.Count);
        Assert.Equal(-150.00m, saved[0].Amount);
        Assert.Equal(35_000.00m, saved[1].Amount);
    }

    [Fact]
    public async Task Reimporting_The_Same_File_Skips_Every_Row_As_A_Duplicate()
    {
        var firstImport = BuildCsv("01.06.2026;REMA 1000 Majorstuen;Varekjøp;;;150,00;Bokført");
        await Handler.Handle(new ImportTransactionsCommand(firstImport, "june.csv", 500), CancellationToken.None);

        var secondImport = BuildCsv("01.06.2026;REMA 1000 Majorstuen;Varekjøp;;;150,00;Bokført");
        var response = await Handler.Handle(
            new ImportTransactionsCommand(secondImport, "june.csv", 500), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(0, response.Result!.ImportedCount);
        Assert.Equal(1, response.Result.DuplicateSkippedCount);

        await using var context = NewContext();
        Assert.Equal(1, await context.Transactions.CountAsync());
    }

    [Fact]
    public async Task Applies_A_Matching_Categorization_Rule()
    {
        var food = await GivenCategory("Food", CategoryType.Expense);
        await GivenCategorizationRule("REMA", food.Id);
        var csv = BuildCsv("01.06.2026;REMA 1000 Majorstuen;Varekjøp;;;150,00;Bokført");

        var response = await Handler.Handle(new ImportTransactionsCommand(csv, "june.csv", 500), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(0, response.Result!.UncategorizedCount);

        await using var context = NewContext();
        var saved = await context.Transactions.SingleAsync();
        Assert.Equal(food.Id, saved.CategoryId);
    }

    [Fact]
    public async Task Stages_The_Uploaded_Bytes_In_The_StagedCsv_Container()
    {
        // A unique filename per test, since Azurite's container is shared across the whole
        // assembly — this makes the blob this test just staged unambiguous to find.
        var fileName = $"{Guid.NewGuid()}.csv";
        var csv = BuildCsv("01.06.2026;REMA 1000 Majorstuen;Varekjøp;;;150,00;Bokført");

        await Handler.Handle(new ImportTransactionsCommand(csv, fileName, 500), CancellationToken.None);

        var containerClient = azuriteFixture.BlobServiceClient.GetBlobContainerClient(BlobContainers.StagedCsv);
        BlobItem? staged = null;
        await foreach (var blob in containerClient.GetBlobsAsync(cancellationToken: CancellationToken.None))
        {
            if (blob.Name.EndsWith(fileName, StringComparison.Ordinal))
            {
                staged = blob;
                break;
            }
        }

        Assert.NotNull(staged);
        Assert.Equal("text/csv", staged.Properties.ContentType);
    }
}
