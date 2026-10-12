using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using JobTracker.Models;

namespace JobTracker.Services;

public class ApplicationStore{
    private readonly JsonSerializerOptions JsonOptions = new(){
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _filePath;
    private readonly List<JobApplication> _applications;

    public ApplicationStore(string filePath){
        _filePath = filePath;
        _applications = LoadApplications();
    }

    public IReadOnlyList<JobApplication> GetAll => _applications;

    public JobApplication Add(string company, string position, string? notes = null){
        var application = new JobApplication{
            Id = _applications.Count > 0 ? _applications.Max(a => a.Id) + 1 : 1,
            Company = company,
            Position = position,
            AppliedOn = DateOnly.FromDateTime(DateTime.Today),
            Notes = notes
        };

        _applications.Add(application);
        SaveOptions();
        return application;
    }

    private List<JobApplication> LoadApplications(){
        if(!File.Exists(_filePath)){
            return new List<JobApplication>();
        }

        var json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<List<JobApplication>>(json, JsonOptions) ?? new List<JobApplication>();
    }

    private void SaveOptions(){
        var json = JsonSerializer.Serialize(_applications, JsonOptions);
        File.WriteAllText(_filePath, json);
    }

    public bool UpdateStatus(int id, ApplicationStatus newStatus){
        var application = _applications.FirstOrDefault(a => a.Id == id); // LINQ
        if(application == null) return false;
        
        application.Status = newStatus;
        SaveOptions();
        return true;
    }

    public bool Delete(int id){
        var removedCount = _applications.RemoveAll(a => a.Id == id);
        if(removedCount == 0 ) return false;

        SaveOptions();
        return true;
    }

    public IEnumerable<JobApplication> FilterByStatus(ApplicationStatus status) 
        => _applications.Where(a => a.Status == status);

    public Dictionary<ApplicationStatus, int> CountByStatus() 
        => _applications.GroupBy(a => a.Status)
                        .ToDictionary(group => group.Key, group => group.Count());
    

}
