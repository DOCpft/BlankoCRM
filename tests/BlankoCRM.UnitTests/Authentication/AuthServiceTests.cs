using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Exceptions;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using Moq;

namespace BlankoCRM.UnitTests.Authentication;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_creates_organization_user_and_default_reminders()
    {
        var context = new AuthServiceTestContext();
        AppUser? createdUser = null;
        context.UserManager
            .Setup(manager => manager.CreateAsync(It.IsAny<AppUser>(), "secret1"))
            .Callback<AppUser, string>((user, _) => createdUser = user)
            .ReturnsAsync(IdentityResult.Success);

        await context.Service.RegisterAsync("  Master  ", "  master@example.com  ", "secret1", "  Studio  ");

        var organization = Assert.Single(context.AddedOrganizations);
        Assert.Equal("Studio", organization.Name);
        Assert.False(string.IsNullOrWhiteSpace(organization.Slug));
        Assert.NotNull(createdUser);
        Assert.Equal("master@example.com", createdUser.Email);
        Assert.Equal("master@example.com", createdUser.UserName);
        Assert.Equal("Master", createdUser.Name);
        Assert.Equal(organization.Id, createdUser.OrganizationId);
        Assert.Equal(5, context.AddedReminderSettings.Count);
        Assert.Equal(new[] { 1, 3, 6, 12, 24 }, context.AddedReminderSettings.Select(setting => setting.OffsetHours));
        Assert.Equal(new[] { true, true, true, false, false }, context.AddedReminderSettings.Select(setting => setting.Enabled));
        context.Transaction.Verify(transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        context.Transaction.Verify(transaction => transaction.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_rolls_back_when_identity_rejects_duplicate_email()
    {
        var context = new AuthServiceTestContext();
        context.UserManager
            .Setup(manager => manager.CreateAsync(It.IsAny<AppUser>(), "secret1"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Email is already taken" }));

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            context.Service.RegisterAsync("Master", "master@example.com", "secret1", null));

        Assert.Contains("Email is already taken", exception.Message);
        context.Transaction.Verify(transaction => transaction.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        context.Transaction.Verify(transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        Assert.Empty(context.AddedReminderSettings);
    }

    [Fact]
    public async Task LoginAsync_returns_jwt_for_valid_credentials()
    {
        var context = new AuthServiceTestContext();
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Name = "Master",
            Email = "master@example.com",
            OrganizationId = Guid.NewGuid()
        };
        context.UserManager
            .Setup(manager => manager.FindByEmailAsync("master@example.com"))
            .ReturnsAsync(user);
        context.SignInManager
            .Setup(manager => manager.CheckPasswordSignInAsync(user, "secret1", true))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
        context.UserManager
            .Setup(manager => manager.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Admin" });

        var result = await context.Service.LoginAsync(" master@example.com ", "secret1");

        Assert.Equal("Master", result.Name);
        Assert.Equal("master@example.com", result.Email);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);
        Assert.Equal(user.Id.ToString(), jwt.Subject);
        Assert.Contains(jwt.Claims, claim => claim.Type == "organizationId" && claim.Value == user.OrganizationId.ToString());
        Assert.Contains(jwt.Claims, claim => claim.Type == System.Security.Claims.ClaimTypes.Role && claim.Value == "Admin");
        Assert.Equal(DateTime.UtcNow.Date.AddDays(7), jwt.ValidTo.Date);
    }

    [Fact]
    public async Task LoginAsync_rejects_unknown_email()
    {
        var context = new AuthServiceTestContext();
        context.UserManager
            .Setup(manager => manager.FindByEmailAsync("missing@example.com"))
            .ReturnsAsync((AppUser?)null);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            context.Service.LoginAsync("missing@example.com", "secret1"));

        context.SignInManager.Verify(
            manager => manager.CheckPasswordSignInAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<bool>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_rejects_invalid_password()
    {
        var context = new AuthServiceTestContext();
        var user = new AppUser { Id = Guid.NewGuid(), Email = "master@example.com" };
        context.UserManager
            .Setup(manager => manager.FindByEmailAsync("master@example.com"))
            .ReturnsAsync(user);
        context.SignInManager
            .Setup(manager => manager.CheckPasswordSignInAsync(user, "wrong", true))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            context.Service.LoginAsync("master@example.com", "wrong"));
    }

    [Fact]
    public async Task AuthVkMiniAppAsync_returns_jwt_for_existing_user()
    {
        var context = new AuthServiceTestContext();
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Name = "VK Master",
            OrganizationId = Guid.NewGuid(),
            VkUserId = AuthServiceTestContext.VkUserId
        };
        context.VkAuthenticator
            .Setup(authenticator => authenticator.Authenticate(It.IsAny<IReadOnlyDictionary<string, string>>()))
            .Returns(AuthServiceTestContext.VkUserId);
        context.UserManager
            .Setup(manager => manager.Users)
            .Returns(new AsyncQueryable<AppUser>([user]));
        context.UserManager
            .Setup(manager => manager.GetRolesAsync(user))
            .ReturnsAsync(new List<string>());
        var vkParams = new Dictionary<string, string> { ["vk_user_id"] = AuthServiceTestContext.VkUserId.ToString() };

        var result = await context.Service.AuthVkMiniAppAsync(vkParams);

        Assert.Equal("VK Master", result.Name);
        Assert.Equal(string.Empty, result.Email);
        Assert.Equal(user.Id.ToString(), new JwtSecurityTokenHandler().ReadJwtToken(result.Token).Subject);
        Assert.Empty(context.AddedOrganizations);
        Assert.Empty(context.AddedReminderSettings);
    }

    [Fact]
    public async Task AuthVkMiniAppAsync_registers_new_user_and_seeds_reminders()
    {
        var context = new AuthServiceTestContext();
        AppUser? createdUser = null;
        context.VkAuthenticator
            .Setup(authenticator => authenticator.Authenticate(It.IsAny<IReadOnlyDictionary<string, string>>()))
            .Returns(AuthServiceTestContext.VkUserId);
        context.UserManager
            .Setup(manager => manager.Users)
            .Returns(new AsyncQueryable<AppUser>(Array.Empty<AppUser>()));
        context.UserManager
            .Setup(manager => manager.CreateAsync(It.IsAny<AppUser>()))
            .Callback<AppUser>(user => createdUser = user)
            .ReturnsAsync(IdentityResult.Success);
        var vkParams = new Dictionary<string, string> { ["vk_user_id"] = AuthServiceTestContext.VkUserId.ToString() };

        var result = await context.Service.AuthVkMiniAppAsync(vkParams);

        var organization = Assert.Single(context.AddedOrganizations);
        Assert.NotNull(createdUser);
        Assert.Equal($"vk_{AuthServiceTestContext.VkUserId}", createdUser.UserName);
        Assert.Null(createdUser.Email);
        Assert.Equal(AuthServiceTestContext.VkUserId, createdUser.VkUserId);
        Assert.Equal(organization.Id, createdUser.OrganizationId);
        Assert.Equal($"VK {AuthServiceTestContext.VkUserId}", result.Name);
        Assert.Equal(5, context.AddedReminderSettings.Count);
        Assert.Equal(createdUser.Id.ToString(), new JwtSecurityTokenHandler().ReadJwtToken(result.Token).Subject);
        context.Transaction.Verify(transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AuthVkMiniAppAsync_rejects_invalid_signature_before_lookup()
    {
        var context = new AuthServiceTestContext();
        context.VkAuthenticator
            .Setup(authenticator => authenticator.Authenticate(It.IsAny<IReadOnlyDictionary<string, string>>()))
            .Throws(new UnauthorizedException("Недействительная подпись VK"));

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            context.Service.AuthVkMiniAppAsync(new Dictionary<string, string>()));

        context.UserManager.VerifyGet(manager => manager.Users, Times.Never);
    }
}
