 namespace JobTracker.Models;

 public class JobApplication
 {
     public int Id { get; set; }
     public string Company { get; set; } = "";
     public string Position { get; set; } = "";
     public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;
     public DateOnly AppliedOn { get; set; }
     public string? Notes { get; set; }
 }