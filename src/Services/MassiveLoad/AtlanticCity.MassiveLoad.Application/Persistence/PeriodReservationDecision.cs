namespace AtlanticCity.MassiveLoad.Application.Persistence;

public enum PeriodReservationDecision
{
    Reserved = 1,
    AlreadyReserved = 2,
    Blocked = 3,
    Rejected = 4,
    LoadNotFound = 5,
    InvalidStatus = 6
}