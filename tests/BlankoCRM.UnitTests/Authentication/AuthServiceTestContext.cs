using BlankoCRM.Application.Abstract;
using BlankoCRM.Application.Services;
using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace BlankoCRM.UnitTests.Authentication;

internal sealed class AuthServiceTestContext
{
    public const string JwtSigningKey = "test-jwt-signing-key-with-at-least-32-characters";
    public const long VkUserId = 24680;
    public Guid OrganizationId { get; } = Guid.NewGuid();
    public Mock<IUnitOfWork> UnitOfWork { get; } = new();
    public Mock<IUnitOfWorkTransaction> Transaction { get; } = new();
    public Mock<IVkMiniAppAuthenticator> VkAuthenticator { get; } = new();
    public Mock<UserManager<AppUser>> UserManager { get; }
    public Mock<SignInManager<AppUser>> SignInManager { get; }
    public List<Organization> AddedOrganizations { get; } = [];
    public List<MasterReminderSetting> AddedReminderSettings { get; } = [];
    public AuthService Service { get; }

    public AuthServiceTestContext()
    {
        var identityOptions = Options.Create(new IdentityOptions());
        var userStore = new Mock<IUserStore<AppUser>>();
        UserManager = new Mock<UserManager<AppUser>>(
            userStore.Object,
            identityOptions,
            new PasswordHasher<AppUser>(),
            Array.Empty<IUserValidator<AppUser>>(),
            Array.Empty<IPasswordValidator<AppUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            new Mock<IServiceProvider>().Object,
            NullLogger<UserManager<AppUser>>.Instance);
        SignInManager = new Mock<SignInManager<AppUser>>(
            UserManager.Object,
            new HttpContextAccessor(),
            new Mock<IUserClaimsPrincipalFactory<AppUser>>().Object,
            identityOptions,
            NullLogger<SignInManager<AppUser>>.Instance,
            new Mock<IAuthenticationSchemeProvider>().Object);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = JwtSigningKey,
                ["Jwt:Issuer"] = "BlankoCRM.Tests",
                ["Jwt:Audience"] = "BlankoCRM.UnitTests"
            })
            .Build();

        UnitOfWork
            .Setup(unitOfWork => unitOfWork.HasActiveTransaction)
            .Returns(false);
        UnitOfWork
            .Setup(unitOfWork => unitOfWork.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Transaction.Object);
        UnitOfWork
            .Setup(unitOfWork => unitOfWork.Add(It.IsAny<Organization>()))
            .Callback<Organization>(organization => AddedOrganizations.Add(organization));
        UnitOfWork
            .Setup(unitOfWork => unitOfWork.Add(It.IsAny<MasterReminderSetting>()))
            .Callback<MasterReminderSetting>(setting => AddedReminderSettings.Add(setting));
        Transaction
            .Setup(transaction => transaction.DisposeAsync())
            .Returns(ValueTask.CompletedTask);

        Service = new AuthService(
            UnitOfWork.Object,
            UserManager.Object,
            SignInManager.Object,
            configuration,
            VkAuthenticator.Object,
            NullLogger<AuthService>.Instance);
    }
}
