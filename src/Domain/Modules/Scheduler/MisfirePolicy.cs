namespace Domain.Scheduler;

public enum MisfirePolicy
{
    FireNow = 1,
    Skip = 2,
    CatchUp = 3
}
