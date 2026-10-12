using JobTracker.Models;
using JobTracker.Services;

var store = new ApplicationStore("applications.json");

while(true){
    Console.WriteLine("Job Application Tracker");
    Console.WriteLine("1) List  2) Add  3) Change status  4) Delete  5) Stats  0) Exit");
    Console.Write(">");

    switch(Console.ReadLine()){
        case "1": PrintAll(store.GetAll); break;
        case "2": AddApplication(store); break;
        case "3": ChangeStatus(store); break;
        case "4": DeleteApplication(store); break;
        case "5": PrintStats(store); break;
        case "0": return;
        default: Console.WriteLine("Invalid option. Please try again."); break;
    }
}

static void PrintAll(IReadOnlyList<JobApplication> applications){
    if(applications.Count == 0){
        Console.WriteLine("No applications found.");
        return;
    }

    foreach(var app in applications){
        Console.WriteLine($"{app.Id}. {app.Company} - {app.Position} (Status: {app.Status}, Applied On: {app.AppliedOn})");
        if(!string.IsNullOrEmpty(app.Notes)){
            Console.WriteLine($"   Notes: {app.Notes}");
        }
    }
}

static void AddApplication(ApplicationStore store){
    Console.Write("Company name: ");
    var company = (Console.ReadLine() ?? "").Trim();
    Console.Write("Position: ");
    var position = (Console.ReadLine() ?? "").Trim();

    if (string.IsNullOrEmpty(company) || string.IsNullOrEmpty(position))
    {
        Console.WriteLine("Company and position are required.");
        return;
    }

    Console.Write("Applied on (yyyy-MM-dd, Enter for today): ");
    var dateInput = Console.ReadLine()?.Trim();
    DateOnly? appliedOn = null;
    if (!string.IsNullOrEmpty(dateInput))
    {
        if (!DateOnly.TryParseExact(dateInput, "yyyy-MM-dd", out var parsed))
        {
            Console.WriteLine("Invalid date. Use the format yyyy-MM-dd, e.g. 2026-10-15.");
            return;
        }
        appliedOn = parsed;
    }

    Console.Write("Notes (optional): ");
    var notes = Console.ReadLine();

    var added = store.Add(company, position, appliedOn, notes);
    Console.WriteLine($"Added #{added.Id} (applied on {added.AppliedOn:yyyy-MM-dd}).");
}

static void ChangeStatus(ApplicationStore store){
    Console.Write("Enter application ID to change status: ");
    if(!int.TryParse(Console.ReadLine(), out int id)){
        Console.WriteLine("Invalid ID.");
        return;
    }

    var application = store.GetAll.FirstOrDefault(a => a.Id == id);
    if(application == null){
        Console.WriteLine($"No application found with ID {id}.");
        return;
    }

    Console.Write($"New status ({string.Join("/", Enum.GetNames<ApplicationStatus>())}): ");
    if (!Enum.TryParse<ApplicationStatus>(Console.ReadLine(), ignoreCase: true, out var status))
    {
        Console.WriteLine("Unknown status.");
        return;
    }

    Console.WriteLine(store.UpdateStatus(id, status) ? "Updated." : "No application with that id.");
}

static void DeleteApplication(ApplicationStore store)
{
    Console.Write("Application id: ");
    if (!int.TryParse(Console.ReadLine(), out var id))
    {
        Console.WriteLine("That is not a number.");
        return;
    }

    Console.WriteLine(store.Delete(id) ? "Deleted." : "No application with that id.");
}

static void PrintStats(ApplicationStore store)
{
    var counts = store.CountByStatus();
    foreach (var status in Enum.GetValues<ApplicationStatus>())
    {
        Console.WriteLine($"{status}: {counts.GetValueOrDefault(status)}");
    }
}

// var application = new JobApplication
// {
//     Id = 1,
//     Company = "Example Corp",
//     Position = "Junior Software Engineer",
//     AppliedOn = DateOnly.FromDateTime(DateTime.Today),
//     Notes = "Submitted resume and cover letter."
// };

// Console.WriteLine($"{application.Id}. {application.Company} - {application.Position} (Status: {application.Status}, Applied On: {application.AppliedOn})");