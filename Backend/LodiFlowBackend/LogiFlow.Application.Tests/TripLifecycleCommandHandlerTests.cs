using FluentAssertions;
using LogiFlow.Application.Abstractions;
using LogiFlow.Application.Abstractions.Repositories;
using LogiFlow.Application.Common.Exceptions;
using LogiFlow.Application.Trips.Commands;
using LogiFlow.Domain.Entities;
using LogiFlow.Domain.Enums;
using Moq;

namespace LogiFlow.Application.Tests;

public class CompleteTripCommandHandlerTests
{
    private readonly Mock<ITripRepository> _tripRepository = new();
    private readonly Mock<ICargoRepository> _cargoRepository = new();
    private readonly Mock<IVehicleRepository> _vehicleRepository = new();
    private readonly Mock<IDriverRepository> _driverRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IAppTransaction> _transaction = new();

    private readonly CompleteTripCommandHandler _handler;

    public CompleteTripCommandHandlerTests()
    {
        _unitOfWork
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_transaction.Object);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _transaction
            .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _transaction
            .Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new CompleteTripCommandHandler(
            _tripRepository.Object,
            _cargoRepository.Object,
            _vehicleRepository.Object,
            _driverRepository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_ShouldCompleteTripAndReleaseResources_WhenTripIsActive()
    {
        var cargo = Cargo.Create(Guid.NewGuid(), "Electronics", 1200m);
        var vehicle = Vehicle.Create(Guid.NewGuid(), "WA12345", "Volvo", "FH16", 20_000m);
        var driver = Driver.Create(Guid.NewGuid(), "John Smith", "LIC123");

        cargo.AssignToTrip();
        vehicle.AssignToTrip(cargo.WeightKg);
        driver.AssignToTrip();

        var trip = CreateTrip(cargo.Id, vehicle.Id, driver.Id);

        SetupRepositories(trip, cargo, vehicle, driver);

        var command = new CompleteTripCommand(trip.Id);

        var response = await _handler.Handle(command, CancellationToken.None);

        response.Id.Should().Be(trip.Id);
        response.Status.Should().Be(nameof(TripStatus.Completed));

        trip.Status.Should().Be(TripStatus.Completed);
        trip.CompletedAt.Should().NotBeNull();

        cargo.Status.Should().Be(CargoStatus.Delivered);
        vehicle.Status.Should().Be(VehicleStatus.Idle);
        driver.Status.Should().Be(DriverStatus.Available);

        _unitOfWork.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _transaction.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _transaction.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenTripDoesNotExist()
    {
        var tripId = Guid.NewGuid();

        _tripRepository
            .Setup(x => x.GetByIdForUpdateAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Trip?)null);

        var command = new CompleteTripCommand(tripId);

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Trip*was not found*");

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _transaction.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _transaction.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowConflictException_WhenTripIsCancelled()
    {
        var cargo = Cargo.Create(Guid.NewGuid(), "Electronics", 1200m);
        var vehicle = Vehicle.Create(Guid.NewGuid(), "WA12345", "Volvo", "FH16", 20_000m);
        var driver = Driver.Create(Guid.NewGuid(), "John Smith", "LIC123");

        cargo.AssignToTrip();
        vehicle.AssignToTrip(cargo.WeightKg);
        driver.AssignToTrip();

        var trip = CreateTrip(cargo.Id, vehicle.Id, driver.Id);
        trip.Cancel();

        SetupRepositories(trip, cargo, vehicle, driver);

        var command = new CompleteTripCommand(trip.Id);

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*cannot be completed*");

        cargo.Status.Should().Be(CargoStatus.Assigned);
        vehicle.Status.Should().Be(VehicleStatus.Busy);
        driver.Status.Should().Be(DriverStatus.OnTrip);

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _transaction.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _transaction.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private void SetupRepositories(Trip trip, Cargo cargo, Vehicle vehicle, Driver driver)
    {
        _tripRepository
            .Setup(x => x.GetByIdForUpdateAsync(trip.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        _cargoRepository
            .Setup(x => x.GetByIdForUpdateAsync(cargo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cargo);

        _vehicleRepository
            .Setup(x => x.GetByIdForUpdateAsync(vehicle.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        _driverRepository
            .Setup(x => x.GetByIdForUpdateAsync(driver.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(driver);
    }

    private static Trip CreateTrip(Guid cargoId, Guid vehicleId, Guid driverId)
    {
        var start = DateTimeOffset.UtcNow.AddHours(1);

        return Trip.Create(
            cargoId,
            vehicleId,
            driverId,
            "Warsaw",
            "Berlin",
            start,
            start.AddHours(5));
    }
}

public class CancelTripCommandHandlerTests
{
    private readonly Mock<ITripRepository> _tripRepository = new();
    private readonly Mock<ICargoRepository> _cargoRepository = new();
    private readonly Mock<IVehicleRepository> _vehicleRepository = new();
    private readonly Mock<IDriverRepository> _driverRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IAppTransaction> _transaction = new();

    private readonly CancelTripCommandHandler _handler;

    public CancelTripCommandHandlerTests()
    {
        _unitOfWork
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_transaction.Object);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _transaction
            .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _transaction
            .Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new CancelTripCommandHandler(
            _tripRepository.Object,
            _cargoRepository.Object,
            _vehicleRepository.Object,
            _driverRepository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_ShouldCancelTripAndReleaseResources_WhenTripIsActive()
    {
        var cargo = Cargo.Create(Guid.NewGuid(), "Electronics", 1200m);
        var vehicle = Vehicle.Create(Guid.NewGuid(), "WA12345", "Volvo", "FH16", 20_000m);
        var driver = Driver.Create(Guid.NewGuid(), "John Smith", "LIC123");

        cargo.AssignToTrip();
        vehicle.AssignToTrip(cargo.WeightKg);
        driver.AssignToTrip();

        var trip = CreateTrip(cargo.Id, vehicle.Id, driver.Id);

        SetupRepositories(trip, cargo, vehicle, driver);

        var command = new CancelTripCommand(trip.Id);

        var response = await _handler.Handle(command, CancellationToken.None);

        response.Id.Should().Be(trip.Id);
        response.Status.Should().Be(nameof(TripStatus.Cancelled));

        trip.Status.Should().Be(TripStatus.Cancelled);
        cargo.Status.Should().Be(CargoStatus.Cancelled);
        vehicle.Status.Should().Be(VehicleStatus.Idle);
        driver.Status.Should().Be(DriverStatus.Available);

        _unitOfWork.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _transaction.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _transaction.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenTripDoesNotExist()
    {
        var tripId = Guid.NewGuid();

        _tripRepository
            .Setup(x => x.GetByIdForUpdateAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Trip?)null);

        var command = new CancelTripCommand(tripId);

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Trip*was not found*");

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _transaction.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _transaction.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowConflictException_WhenTripIsCompleted()
    {
        var cargo = Cargo.Create(Guid.NewGuid(), "Electronics", 1200m);
        var vehicle = Vehicle.Create(Guid.NewGuid(), "WA12345", "Volvo", "FH16", 20_000m);
        var driver = Driver.Create(Guid.NewGuid(), "John Smith", "LIC123");

        cargo.AssignToTrip();
        vehicle.AssignToTrip(cargo.WeightKg);
        driver.AssignToTrip();

        var trip = CreateTrip(cargo.Id, vehicle.Id, driver.Id);
        trip.Complete();

        SetupRepositories(trip, cargo, vehicle, driver);

        var command = new CancelTripCommand(trip.Id);

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*Completed trip cannot be cancelled*");

        cargo.Status.Should().Be(CargoStatus.Assigned);
        vehicle.Status.Should().Be(VehicleStatus.Busy);
        driver.Status.Should().Be(DriverStatus.OnTrip);

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _transaction.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _transaction.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowConflictException_WhenTripIsAlreadyCancelled()
    {
        var cargo = Cargo.Create(Guid.NewGuid(), "Electronics", 1200m);
        var vehicle = Vehicle.Create(Guid.NewGuid(), "WA12345", "Volvo", "FH16", 20_000m);
        var driver = Driver.Create(Guid.NewGuid(), "John Smith", "LIC123");

        cargo.AssignToTrip();
        vehicle.AssignToTrip(cargo.WeightKg);
        driver.AssignToTrip();

        var trip = CreateTrip(cargo.Id, vehicle.Id, driver.Id);
        trip.Cancel();

        SetupRepositories(trip, cargo, vehicle, driver);

        var command = new CancelTripCommand(trip.Id);

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*Cancelled trip cannot be cancelled again*");

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _transaction.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _transaction.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private void SetupRepositories(Trip trip, Cargo cargo, Vehicle vehicle, Driver driver)
    {
        _tripRepository
            .Setup(x => x.GetByIdForUpdateAsync(trip.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        _cargoRepository
            .Setup(x => x.GetByIdForUpdateAsync(cargo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cargo);

        _vehicleRepository
            .Setup(x => x.GetByIdForUpdateAsync(vehicle.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        _driverRepository
            .Setup(x => x.GetByIdForUpdateAsync(driver.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(driver);
    }

    private static Trip CreateTrip(Guid cargoId, Guid vehicleId, Guid driverId)
    {
        var start = DateTimeOffset.UtcNow.AddHours(1);

        return Trip.Create(
            cargoId,
            vehicleId,
            driverId,
            "Warsaw",
            "Berlin",
            start,
            start.AddHours(5));
    }
}