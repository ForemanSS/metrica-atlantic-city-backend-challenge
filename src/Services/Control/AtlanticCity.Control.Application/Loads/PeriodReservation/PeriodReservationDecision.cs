namespace AtlanticCity.Control.Application.Loads.PeriodReservation;

public enum PeriodReservationDecision
{
    Reserved = 1,
    AlreadyReserved = 2,
    Blocked = 3,
    Rejected = 4,
    LoadNotFound = 5,
    InvalidStatus = 6
}