using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Exceptions;
using Moq;

namespace BlankoCRM.UnitTests.PublicAppointments;

public sealed class PublicAppointmentServiceQueryTests
{
    [Fact]
    public async Task GetMyAppointmentsAsync_returns_the_vk_clients_appointments()
    {
        var context = new PublicAppointmentTestContext();
        var client = PublicAppointmentTestData.CreateClient();
        List<Appointment> appointments = [new Appointment
        {
            Id = Guid.NewGuid(),
            Status = AppointmentStatus.Pending
        }];
        context.ClientRepository
            .Setup(repository => repository.FindByVkUserIdAsync(
                context.OrganizationId, PublicAppointmentTestContext.VkUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);
        context.AppointmentRepository
            .Setup(repository => repository.GetClientAppointmentsAsync(
                context.OrganizationId, client.Id, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointments);

        var result = await context.Service.GetMyAppointmentsAsync(context.VkParams);

        Assert.Equal(appointments, result);
    }

    [Fact]
    public async Task GetMyAppointmentsAsync_returns_an_empty_list_when_the_vk_client_does_not_exist()
    {
        var context = new PublicAppointmentTestContext();
        context.ClientRepository
            .Setup(repository => repository.FindByVkUserIdAsync(
                context.OrganizationId, PublicAppointmentTestContext.VkUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        var result = await context.Service.GetMyAppointmentsAsync(context.VkParams);

        Assert.Empty(result);
        context.AppointmentRepository.Verify(repository => repository.GetClientAppointmentsAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetMyAppointmentsAsync_propagates_an_invalid_vk_signature()
    {
        var context = new PublicAppointmentTestContext();
        context.Authenticator
            .Setup(authenticator => authenticator.Authenticate(It.IsAny<IReadOnlyDictionary<string, string>>()))
            .Throws(new UnauthorizedException("Invalid VK signature"));

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => context.Service.GetMyAppointmentsAsync(context.VkParams));

        context.ClientRepository.Verify(repository => repository.FindByVkUserIdAsync(
            It.IsAny<Guid>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
