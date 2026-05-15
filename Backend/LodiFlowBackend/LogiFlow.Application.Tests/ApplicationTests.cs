using FluentAssertions;
using LogiFlow.Application.Abstractions;
using LogiFlow.Application.Abstractions.Repositories;
using LogiFlow.Application.Common.Exceptions;
using LogiFlow.Application.DTOs;
using LogiFlow.Application.Trips.Commands;
using LogiFlow.Domain.Entities;
using LogiFlow.Domain.Enums;
using Moq;

namespace LogiFlow.Application.Tests;

public class CreateTripCommandHandlerTests
{
    private readonly Mock<ICargoRepository> _cargoRepository = new();
    private readonly Mock<IVehicleRepository> _vehicleRepository = new();
    private readonly Mock<IDriverRepository> _driverRepository = new();
    private readonly Mock<ITripRepository> _tripRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IAppTransaction> _transaction = new();

    private readonly CreateTripCommandHandler _handler;

    public CreateTripCommandHandlerTests()
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

        _tripRepository
            .Setup(x => x.AddAsync(It.IsAny<Trip>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new CreateTripCommandHandler(
            _cargoRepository.Object,
            _vehicleRepository.Object,
            _driverRepository.Object,
            _tripRepository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateTrip_WhenResourcesAreAvailable()
    {
        var cargo = Cargo.Create(Guid.NewGuid(), "Electronics", 1200m);
        var vehicle = Vehicle.Create(Guid.NewGuid(), "WA12345", "Volvo", "FH16", 20_000m);
        var driver = Driver.Create(Guid.NewGuid(), "John Smith", "LIC123");

        Trip? createdTrip = null;

        _cargoRepository
            .Setup(x => x.GetByIdForUpdateAsync(cargo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cargo);

        _vehicleRepository
            .Setup(x => x.GetByIdForUpdateAsync(vehicle.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        _driverRepository
            .Setup(x => x.GetByIdForUpdateAsync(driver.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(driver);

        _tripRepository
            .Setup(x => x.AddAsync(It.IsAny<Trip>(), It.IsAny<CancellationToken>()))
            .Callback<Trip, CancellationToken>((trip, _) => createdTrip = trip)
            .Returns(Task.CompletedTask);

        var command = new CreateTripCommand(new CreateTripRequest
        {
            CargoId = cargo.Id,
            VehicleId = vehicle.Id,
            DriverId = driver.Id,
            Origin = "Warsaw",
            Destination = "Berlin",
            ScheduledStart = DateTimeOffset.UtcNow.AddHours(1),
            ScheduledEnd = DateTimeOffset.UtcNow.AddHours(5)
        });

        var response = await _handler.Handle(command, CancellationToken.None);

        response.Id.Should().NotBeEmpty();
        response.CargoId.Should().Be(cargo.Id);
        response.VehicleId.Should().Be(vehicle.Id);
        response.DriverId.Should().Be(driver.Id);
        response.Origin.Should().Be("Warsaw");
        response.Destination.Should().Be("Berlin");
        response.Status.Should().Be(nameof(TripStatus.Active));

        createdTrip.Should().NotBeNull();
        createdTrip!.CargoId.Should().Be(cargo.Id);
        createdTrip.VehicleId.Should().Be(vehicle.Id);
        createdTrip.DriverId.Should().Be(driver.Id);

        cargo.Status.Should().Be(CargoStatus.Assigned);
        vehicle.Status.Should().Be(VehicleStatus.Busy);
        driver.Status.Should().Be(DriverStatus.OnTrip);

        _unitOfWork.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _tripRepository.Verify(x => x.AddAsync(It.IsAny<Trip>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _transaction.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _transaction.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenCargoDoesNotExist()
    {
        var request = CreateValidRequest();

        _cargoRepository
            .Setup(x => x.GetByIdForUpdateAsync(request.CargoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cargo?)null);

        var command = new CreateTripCommand(request);

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Cargo*");

        _transaction.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _transaction.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowConflictException_WhenCargoIsAlreadyAssigned()
    {
        var cargo = Cargo.Create(Guid.NewGuid(), "Electronics", 1200m);
        cargo.AssignToTrip();

        var vehicle = Vehicle.Create(Guid.NewGuid(), "WA12345", "Volvo", "FH16", 20_000m);
        var driver = Driver.Create(Guid.NewGuid(), "John Smith", "LIC123");

        var request = CreateValidRequest(cargo.Id, vehicle.Id, driver.Id);

        SetupRepositories(cargo, vehicle, driver);

        var command = new CreateTripCommand(request);

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*Cargo*cannot be assigned*");

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _transaction.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _transaction.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowConflictException_WhenVehicleIsBusy()
    {
        var cargo = Cargo.Create(Guid.NewGuid(), "Electronics", 1200m);
        var vehicle = Vehicle.Create(Guid.NewGuid(), "WA12345", "Volvo", "FH16", 20_000m);
        vehicle.AssignToTrip(1000m);

        var driver = Driver.Create(Guid.NewGuid(), "John Smith", "LIC123");

        var request = CreateValidRequest(cargo.Id, vehicle.Id, driver.Id);

        SetupRepositories(cargo, vehicle, driver);

        var command = new CreateTripCommand(request);

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*Vehicle*not available*");

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _transaction.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _transaction.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowConflictException_WhenVehicleCapacityIsTooLow()
    {
        var cargo = Cargo.Create(Guid.NewGuid(), "Heavy cargo", 30_000m);
        var vehicle = Vehicle.Create(Guid.NewGuid(), "WA12345", "Volvo", "FH16", 20_000m);
        var driver = Driver.Create(Guid.NewGuid(), "John Smith", "LIC123");

        var request = CreateValidRequest(cargo.Id, vehicle.Id, driver.Id);

        SetupRepositories(cargo, vehicle, driver);

        var command = new CreateTripCommand(request);

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*capacity*");

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _transaction.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _transaction.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowConflictException_WhenDriverIsNotAvailable()
    {
        var cargo = Cargo.Create(Guid.NewGuid(), "Electronics", 1200m);
        var vehicle = Vehicle.Create(Guid.NewGuid(), "WA12345", "Volvo", "FH16", 20_000m);
        var driver = Driver.Create(Guid.NewGuid(), "John Smith", "LIC123");
        driver.AssignToTrip();

        var request = CreateValidRequest(cargo.Id, vehicle.Id, driver.Id);

        SetupRepositories(cargo, vehicle, driver);

        var command = new CreateTripCommand(request);

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*Driver*not available*");

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _transaction.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _transaction.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenScheduledEndIsBeforeScheduledStart()
    {
        var start = DateTimeOffset.UtcNow.AddHours(5);

        var request = new CreateTripRequest
        {
            CargoId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            DriverId = Guid.NewGuid(),
            Origin = "Warsaw",
            Destination = "Berlin",
            ScheduledStart = start,
            ScheduledEnd = start.AddHours(-1)
        };

        var command = new CreateTripCommand(request);

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*ScheduledEnd must be after ScheduledStart*");

        _unitOfWork.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _transaction.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        _transaction.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private void SetupRepositories(Cargo cargo, Vehicle vehicle, Driver driver)
    {
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

    private static CreateTripRequest CreateValidRequest(
        Guid? cargoId = null,
        Guid? vehicleId = null,
        Guid? driverId = null)
    {
        var start = DateTimeOffset.UtcNow.AddHours(1);

        return new CreateTripRequest
        {
            CargoId = cargoId ?? Guid.NewGuid(),
            VehicleId = vehicleId ?? Guid.NewGuid(),
            DriverId = driverId ?? Guid.NewGuid(),
            Origin = "Warsaw",
            Destination = "Berlin",
            ScheduledStart = start,
            ScheduledEnd = start.AddHours(5)
        };
    }
}