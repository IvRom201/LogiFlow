using FluentAssertions;
using LogiFlow.Domain.Common;
using LogiFlow.Domain.Entities;
using LogiFlow.Domain.Enums;

namespace LogiFlow.Domain.Tests;

public class CargoTests
{
    [Fact]
    public void Create_ShouldCreateCargoWithPendingStatus()
    {
        var id = Guid.NewGuid();

        var cargo = Cargo.Create(id, "  Electronics  ", 1200m);

        cargo.Id.Should().Be(id);
        cargo.Description.Should().Be("Electronics");
        cargo.WeightKg.Should().Be(1200m);
        cargo.Status.Should().Be(CargoStatus.Pending);
        cargo.CanBeAssigned.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldThrow_WhenDescriptionIsEmpty()
    {
        Action act = () => Cargo.Create(Guid.NewGuid(), "   ", 100m);

        act.Should().Throw<DomainRuleException>()
            .WithMessage("*description*");
    }

    [Fact]
    public void Create_ShouldThrow_WhenWeightIsNotPositive()
    {
        Action act = () => Cargo.Create(Guid.NewGuid(), "Cargo", 0m);

        act.Should().Throw<DomainRuleException>()
            .WithMessage("*weight*");
    }

    [Fact]
    public void AssignToTrip_ShouldChangeStatusToAssigned()
    {
        var cargo = Cargo.Create(Guid.NewGuid(), "Cargo", 100m);

        cargo.AssignToTrip();

        cargo.Status.Should().Be(CargoStatus.Assigned);
        cargo.CanBeAssigned.Should().BeFalse();
    }

    [Fact]
    public void AssignToTrip_ShouldThrow_WhenCargoIsAlreadyAssigned()
    {
        var cargo = Cargo.Create(Guid.NewGuid(), "Cargo", 100m);
        cargo.AssignToTrip();

        Action act = () => cargo.AssignToTrip();

        act.Should().Throw<DomainRuleException>()
            .WithMessage("*cannot be assigned*");
    }

    [Fact]
    public void MarkInTransit_ShouldChangeStatus_WhenCargoIsAssigned()
    {
        var cargo = Cargo.Create(Guid.NewGuid(), "Cargo", 100m);
        cargo.AssignToTrip();

        cargo.MarkInTransit();

        cargo.Status.Should().Be(CargoStatus.InTransit);
    }

    [Fact]
    public void MarkInTransit_ShouldThrow_WhenCargoIsNotAssigned()
    {
        var cargo = Cargo.Create(Guid.NewGuid(), "Cargo", 100m);

        Action act = () => cargo.MarkInTransit();

        act.Should().Throw<DomainRuleException>()
            .WithMessage("*only after assignment*");
    }

    [Fact]
    public void MarkDelivered_ShouldChangeStatus_WhenCargoIsAssigned()
    {
        var cargo = Cargo.Create(Guid.NewGuid(), "Cargo", 100m);
        cargo.AssignToTrip();

        cargo.MarkDelivered();

        cargo.Status.Should().Be(CargoStatus.Delivered);
    }
}

public class DriverTests
{
    [Fact]
    public void Create_ShouldCreateDriverWithAvailableStatus()
    {
        var id = Guid.NewGuid();

        var driver = Driver.Create(id, "  John Smith  ", "  abc123  ");

        driver.Id.Should().Be(id);
        driver.FullName.Should().Be("John Smith");
        driver.LicenseNumber.Should().Be("ABC123");
        driver.Status.Should().Be(DriverStatus.Available);
        driver.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldThrow_WhenFullNameIsEmpty()
    {
        Action act = () => Driver.Create(Guid.NewGuid(), "   ", "LIC123");

        act.Should().Throw<DomainRuleException>()
            .WithMessage("*full name*");
    }

    [Fact]
    public void AssignToTrip_ShouldChangeStatusToOnTrip()
    {
        var driver = Driver.Create(Guid.NewGuid(), "John Smith", "LIC123");

        driver.AssignToTrip();

        driver.Status.Should().Be(DriverStatus.OnTrip);
        driver.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void AssignToTrip_ShouldThrow_WhenDriverIsNotAvailable()
    {
        var driver = Driver.Create(Guid.NewGuid(), "John Smith", "LIC123");
        driver.AssignToTrip();

        Action act = () => driver.AssignToTrip();

        act.Should().Throw<DomainRuleException>()
            .WithMessage("*not available*");
    }

    [Fact]
    public void Release_ShouldChangeStatusToAvailable_WhenDriverIsOnTrip()
    {
        var driver = Driver.Create(Guid.NewGuid(), "John Smith", "LIC123");
        driver.AssignToTrip();

        driver.Release();

        driver.Status.Should().Be(DriverStatus.Available);
        driver.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void Release_ShouldThrow_WhenDriverIsNotOnTrip()
    {
        var driver = Driver.Create(Guid.NewGuid(), "John Smith", "LIC123");

        Action act = () => driver.Release();

        act.Should().Throw<DomainRuleException>()
            .WithMessage("*assigned driver*");
    }

    [Fact]
    public void MarkOffDuty_ShouldThrow_WhenDriverIsOnTrip()
    {
        var driver = Driver.Create(Guid.NewGuid(), "John Smith", "LIC123");
        driver.AssignToTrip();

        Action act = () => driver.MarkOffDuty();

        act.Should().Throw<DomainRuleException>()
            .WithMessage("*on trip*");
    }
}

public class VehicleTests
{
    [Fact]
    public void Create_ShouldCreateVehicleWithIdleStatus()
    {
        var id = Guid.NewGuid();

        var vehicle = Vehicle.Create(id, "  wa12345  ", "  Volvo  ", "  FH16  ", 20_000m);

        vehicle.Id.Should().Be(id);
        vehicle.PlateNumber.Should().Be("WA12345");
        vehicle.Make.Should().Be("Volvo");
        vehicle.Model.Should().Be("FH16");
        vehicle.MaxWeightKg.Should().Be(20_000m);
        vehicle.Status.Should().Be(VehicleStatus.Idle);
    }

    [Fact]
    public void IsAvailableFor_ShouldReturnTrue_WhenVehicleIsIdleAndCapacityIsEnough()
    {
        var vehicle = Vehicle.Create(Guid.NewGuid(), "WA12345", "Volvo", "FH16", 20_000m);

        var result = vehicle.IsAvailableFor(15_000m);

        result.Should().BeTrue();
    }

    [Fact]
    public void AssignToTrip_ShouldChangeStatusToBusy()
    {
        var vehicle = Vehicle.Create(Guid.NewGuid(), "WA12345", "Volvo", "FH16", 20_000m);

        vehicle.AssignToTrip(15_000m);

        vehicle.Status.Should().Be(VehicleStatus.Busy);
    }

    [Fact]
    public void AssignToTrip_ShouldThrow_WhenCargoIsTooHeavy()
    {
        var vehicle = Vehicle.Create(Guid.NewGuid(), "WA12345", "Volvo", "FH16", 10_000m);

        Action act = () => vehicle.AssignToTrip(15_000m);

        act.Should().Throw<DomainRuleException>()
            .WithMessage("*capacity*");
    }

    [Fact]
    public void Release_ShouldChangeStatusToIdle_WhenVehicleIsBusy()
    {
        var vehicle = Vehicle.Create(Guid.NewGuid(), "WA12345", "Volvo", "FH16", 20_000m);
        vehicle.AssignToTrip(15_000m);

        vehicle.Release();

        vehicle.Status.Should().Be(VehicleStatus.Idle);
    }

    [Fact]
    public void SendToMaintenance_ShouldChangeStatusToMaintenance_WhenVehicleIsIdle()
    {
        var vehicle = Vehicle.Create(Guid.NewGuid(), "WA12345", "Volvo", "FH16", 20_000m);

        vehicle.SendToMaintenance();

        vehicle.Status.Should().Be(VehicleStatus.Maintenance);
    }

    [Fact]
    public void SendToMaintenance_ShouldThrow_WhenVehicleIsBusy()
    {
        var vehicle = Vehicle.Create(Guid.NewGuid(), "WA12345", "Volvo", "FH16", 20_000m);
        vehicle.AssignToTrip(10_000m);

        Action act = () => vehicle.SendToMaintenance();

        act.Should().Throw<DomainRuleException>()
            .WithMessage("*busy vehicle*");
    }

    [Fact]
    public void ReturnFromMaintenance_ShouldChangeStatusToIdle()
    {
        var vehicle = Vehicle.Create(Guid.NewGuid(), "WA12345", "Volvo", "FH16", 20_000m);
        vehicle.SendToMaintenance();

        vehicle.ReturnFromMaintenance();

        vehicle.Status.Should().Be(VehicleStatus.Idle);
    }
}

public class TripTests
{
    [Fact]
    public void Create_ShouldCreateActiveTrip()
    {
        var cargoId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        var start = DateTimeOffset.UtcNow.AddHours(1);
        var end = start.AddHours(5);

        var trip = Trip.Create(
            cargoId,
            vehicleId,
            driverId,
            "  Warsaw  ",
            "  Berlin  ",
            start,
            end);

        trip.Id.Should().NotBeEmpty();
        trip.CargoId.Should().Be(cargoId);
        trip.VehicleId.Should().Be(vehicleId);
        trip.DriverId.Should().Be(driverId);
        trip.Origin.Should().Be("Warsaw");
        trip.Destination.Should().Be("Berlin");
        trip.ScheduledStart.Should().Be(start);
        trip.ScheduledEnd.Should().Be(end);
        trip.Status.Should().Be(TripStatus.Active);
        trip.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldThrow_WhenScheduledEndIsBeforeStart()
    {
        var start = DateTimeOffset.UtcNow.AddHours(5);
        var end = start.AddHours(-1);

        Action act = () => Trip.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Warsaw",
            "Berlin",
            start,
            end);

        act.Should().Throw<DomainRuleException>()
            .WithMessage("*after scheduled start*");
    }

    [Fact]
    public void Complete_ShouldChangeStatusToCompleted()
    {
        var trip = CreateTrip();

        trip.Complete();

        trip.Status.Should().Be(TripStatus.Completed);
        trip.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Complete_ShouldThrow_WhenTripIsCancelled()
    {
        var trip = CreateTrip();
        trip.Cancel();

        Action act = () => trip.Complete();

        act.Should().Throw<DomainRuleException>()
            .WithMessage("*cannot be completed*");
    }

    [Fact]
    public void Cancel_ShouldChangeStatusToCancelled()
    {
        var trip = CreateTrip();

        trip.Cancel();

        trip.Status.Should().Be(TripStatus.Cancelled);
    }

    [Fact]
    public void Cancel_ShouldThrow_WhenTripIsCompleted()
    {
        var trip = CreateTrip();
        trip.Complete();

        Action act = () => trip.Cancel();

        act.Should().Throw<DomainRuleException>()
            .WithMessage("*Completed trip cannot be cancelled*");
    }

    private static Trip CreateTrip()
    {
        var start = DateTimeOffset.UtcNow.AddHours(1);
        var end = start.AddHours(5);

        return Trip.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Warsaw",
            "Berlin",
            start,
            end);
    }
}