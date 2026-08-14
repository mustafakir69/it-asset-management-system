using TakipProgrami.Api.Entities;

namespace TakipProgrami.Api.Helpers;

public static class MaintenanceRules
{
    public static DateOnly GetNextPlannedDate(DateOnly plannedDate, int frequencyDays) =>
        plannedDate.AddDays(frequencyDays);

    public static MaintenanceNotificationType? GetNotificationType(
        MaintenanceTask task,
        DateOnly today)
    {
        if (task.Status != MaintenanceTaskStatus.Planned) return null;
        if (task.PlannedDate < today) return MaintenanceNotificationType.Overdue;
        return task.PlannedDate.DayNumber - today.DayNumber <= 7
            ? MaintenanceNotificationType.Upcoming
            : null;
    }
}
