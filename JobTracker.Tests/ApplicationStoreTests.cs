using JobTracker.Models;
using JobTracker.Services;

namespace JobTracker.Tests;

public class ApplicationStoreTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");

    public void Dispose()
    {
        if (File.Exists(_path)) File.Delete(_path);
    }

    [Fact]
    public void Add_assigns_incrementing_ids()
    {
        var store = new ApplicationStore(_path);

        var first = store.Add("Company A", "Developer");
        var second = store.Add("Company B", "Developer");

        Assert.Equal(1, first.Id);
        Assert.Equal(2, second.Id);
    }

    [Fact]
    public void Add_without_date_uses_today()
    {
        var store = new ApplicationStore(_path);

        var added = store.Add("Company A", "Developer");

        Assert.Equal(DateOnly.FromDateTime(DateTime.Today), added.AppliedOn);
    }

    [Fact]
    public void Add_with_date_uses_the_given_date()
    {
        var store = new ApplicationStore(_path);
        var date = new DateOnly(2026, 10, 1);

        var added = store.Add("Company A", "Developer", date);

        Assert.Equal(date, added.AppliedOn);
    }

    [Fact]
    public void Data_is_persisted_between_instances()
    {
        new ApplicationStore(_path).Add("Company A", "Developer", new DateOnly(2026, 10, 1), "note");

        var reloaded = new ApplicationStore(_path);

        var loaded = Assert.Single(reloaded.GetAll());
        Assert.Equal("Company A", loaded.Company);
        Assert.Equal(new DateOnly(2026, 10, 1), loaded.AppliedOn);
        Assert.Equal("note", loaded.Notes);
    }

    [Fact]
    public void UpdateStatus_returns_false_for_unknown_id()
    {
        var store = new ApplicationStore(_path);

        Assert.False(store.UpdateStatus(999, ApplicationStatus.Offer));
    }

    [Fact]
    public void Delete_removes_the_application()
    {
        var store = new ApplicationStore(_path);
        var added = store.Add("Company A", "Developer");

        Assert.True(store.Delete(added.Id));
        Assert.Empty(store.GetAll());
    }

    [Fact]
    public void CountByStatus_groups_applications()
    {
        var store = new ApplicationStore(_path);
        store.Add("Company A", "Developer");
        var second = store.Add("Company B", "Developer");
        store.UpdateStatus(second.Id, ApplicationStatus.Interview);

        var counts = store.CountByStatus();

        Assert.Equal(1, counts[ApplicationStatus.Applied]);
        Assert.Equal(1, counts[ApplicationStatus.Interview]);
    }
}