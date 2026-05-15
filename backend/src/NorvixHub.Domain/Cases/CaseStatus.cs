namespace NorvixHub.Domain.Cases;

public enum CaseStatus
{
    Draft,
    Open,
    WaitingForCustomer,
    WaitingForInternalReview,
    ReadyForDelivery,
    Delivered,
    Closed
}

