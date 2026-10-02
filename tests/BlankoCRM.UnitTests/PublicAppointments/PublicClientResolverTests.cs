using BlankoCRM.Application.Services;
using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Exceptions;
using BlankoCRM.Core.Interfaces;
using Moq;

namespace BlankoCRM.UnitTests.PublicAppointments;

public sealed class PublicClientResolverTests
{
    [Fact]
    public async Task ResolveAsync_reuses_a_vk_client_and_adds_a_missing_phone()
    {
        var repository = new Mock<IClientRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var organizationId = Guid.NewGuid();
        var client = new Client { Id = Guid.NewGuid(), Name = "Existing" };
        repository.Setup(item => item.FindByVkUserIdAsync(organizationId, 1001, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);
        repository.Setup(item => item.FindByPhoneAsync(organizationId, "79005551234", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);
        var resolver = new PublicClientResolver(repository.Object, unitOfWork.Object);

        var result = await resolver.ResolveAsync(organizationId, 1001, "Ignored new name", "8 (900) 555-12-34");

        Assert.Same(client, result);
        Assert.Equal("79005551234", client.Phone);
        unitOfWork.Verify(item => item.Add(It.IsAny<ClientContact>()), Times.Never);
        repository.Verify(item => item.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ResolveAsync_adds_a_vk_contact_to_a_client_found_by_phone()
    {
        var repository = new Mock<IClientRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var organizationId = Guid.NewGuid();
        var client = new Client { Id = Guid.NewGuid(), Name = "Existing", Contacts = [] };
        repository.Setup(item => item.FindByVkUserIdAsync(organizationId, 1002, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);
        repository.Setup(item => item.FindByPhoneAsync(organizationId, "79005551234", It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);
        var resolver = new PublicClientResolver(repository.Object, unitOfWork.Object);

        var result = await resolver.ResolveAsync(organizationId, 1002, "Name", "9005551234");

        Assert.Same(client, result);
        var contact = Assert.Single(client.Contacts);
        Assert.Equal(ContactType.Vk, contact.Type);
        Assert.Equal("1002", contact.Value);
        Assert.Equal(client.Id, contact.ClientId);
        unitOfWork.Verify(item => item.Add(It.Is<ClientContact>(value => value.ClientId == client.Id)), Times.Once);
    }

    [Fact]
    public async Task ResolveAsync_creates_a_client_with_a_vk_contact_when_no_match_exists()
    {
        var repository = new Mock<IClientRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var organizationId = Guid.NewGuid();
        repository.Setup(item => item.FindByVkUserIdAsync(organizationId, 1003, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);
        repository.Setup(item => item.FindByPhoneAsync(organizationId, "79005551234", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);
        Client? createdClient = null;
        repository.Setup(item => item.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
            .Callback<Client, CancellationToken>((client, _) => createdClient = client)
            .ReturnsAsync((Client client, CancellationToken _) => client);
        var resolver = new PublicClientResolver(repository.Object, unitOfWork.Object);

        var result = await resolver.ResolveAsync(organizationId, 1003, "  New client  ", "9005551234");

        Assert.Same(createdClient, result);
        Assert.Equal(organizationId, result.OrganizationId);
        Assert.Equal("New client", result.Name);
        Assert.Equal("79005551234", result.Phone);
        var contact = Assert.Single(result.Contacts);
        Assert.Equal(ContactType.Vk, contact.Type);
        Assert.Equal("1003", contact.Value);
        Assert.Equal(result.Id, contact.ClientId);
    }

    [Fact]
    public async Task ResolveAsync_rejects_vk_and_phone_that_belong_to_different_clients()
    {
        var repository = new Mock<IClientRepository>();
        var organizationId = Guid.NewGuid();
        repository.Setup(item => item.FindByVkUserIdAsync(organizationId, 1004, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Client { Id = Guid.NewGuid() });
        repository.Setup(item => item.FindByPhoneAsync(organizationId, "79005551234", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Client { Id = Guid.NewGuid() });
        var resolver = new PublicClientResolver(repository.Object, new Mock<IUnitOfWork>().Object);

        await Assert.ThrowsAsync<ConflictException>(() =>
            resolver.ResolveAsync(organizationId, 1004, "Name", "9005551234"));
    }

    [Fact]
    public async Task ResolveAsync_rejects_a_vk_id_when_the_phone_client_has_another_vk_contact()
    {
        var repository = new Mock<IClientRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var organizationId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var client = new Client
        {
            Id = clientId,
            Name = "Existing",
            Contacts =
            [
                new ClientContact
                {
                    Id = Guid.NewGuid(),
                    ClientId = clientId,
                    OrganizationId = organizationId,
                    Type = ContactType.Vk,
                    Value = "1005"
                }
            ]
        };
        repository.Setup(item => item.FindByVkUserIdAsync(organizationId, 1006, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);
        repository.Setup(item => item.FindByPhoneAsync(organizationId, "79005551234", It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);
        var resolver = new PublicClientResolver(repository.Object, unitOfWork.Object);

        await Assert.ThrowsAsync<ConflictException>(() =>
            resolver.ResolveAsync(organizationId, 1006, "Name", "9005551234"));

        unitOfWork.Verify(item => item.Add(It.IsAny<ClientContact>()), Times.Never);
        repository.Verify(item => item.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
