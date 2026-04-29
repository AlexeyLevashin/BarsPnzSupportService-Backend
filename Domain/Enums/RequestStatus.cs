namespace Domain.Enums;

public enum RequestStatus
{
    New = 0,
    InProgress = 1,
    ClientDataRequest = 2,
    PendingReview = 3,
    Closed = 4,
    Canceled = 5,
    Analysis = 6
}