using Microsoft.EntityFrameworkCore;
using MoneyManagement.Application.Common;
using MoneyManagement.Application.Common.Interfaces;
using MoneyManagement.Application.Features.Access.Requests;
using MoneyManagement.Application.Features.Access.Services;
using MoneyManagement.Application.Features.Auth.Services;
using MoneyManagement.Domain.Entities;
using MoneyManagement.Domain.Enums;
using MoneyManagement.Infrastructure.Persistence;
using MoneyManagement.Infrastructure.Persistence.Repository;

namespace MoneyManagement.Application.Tests.Features.Access;

public class AccessServiceTests
{
    private class FakeCurrentUser(Guid id) : ICurrentUserService
    {
        public bool IsAuthenticated => true;
        public Guid? UserId => id;
        public IReadOnlyList<string> Roles => [];
    }

    private class FakeGoogle(string email) : IGoogleOAuthClient
    {
        public GooglePkceChallenge CreatePkceChallenge() => new("s", "v", "c");
        public string BuildAuthorizationUrl(string state, string codeChallenge) => "";
        public Task<GoogleTokenResult> ExchangeCodeAsync(string code, string codeVerifier, CancellationToken ct = default) => Task.FromResult(new GoogleTokenResult("id", "at"));
        public Task<GoogleIdentity> ValidateIdTokenAsync(string idToken, CancellationToken ct = default) => Task.FromResult(new GoogleIdentity("sub", email, "New Person"));
    }

    private class FakeTokens : ITokenService
    {
        public AccessToken GenerateAccessToken(Guid userId, string email, string name) => new("jwt", 900);
        public RawRefreshToken GenerateRefreshToken() { var raw = Guid.NewGuid().ToString("N"); return new(raw, raw, DateTime.UtcNow.AddDays(1)); }
        public string HashRefreshToken(string rawToken) => rawToken;
    }

    private class Env
    {
        public AppDbContext Context { get; } = new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        public User Admin { get; } = new() { Name = "Admin", Email = "admin@example.com", Role = UserRole.Admin };
        public AccessService Access { get; }

        public Env()
        {
            Context.Users.Add(Admin);
            Context.SaveChanges();
            Access = new AccessService(new UserRepository(Context), new AdminAuditLogRepository(Context), new AppSettingRepository(Context), new FakeCurrentUser(Admin.Id), new UnitOfWork(Context));
        }

        public AuthService AuthFor(string email) => new(
            new UserRepository(Context), new RefreshTokenRepository(Context), new FakeGoogle(email), new FakeTokens(), new UnitOfWork(Context), new AppSettingRepository(Context));

        public User AddUser(UserStatus status, string email = "person@example.com")
        {
            var user = new User { Name = "Person", Email = email, Status = status };
            Context.Users.Add(user);
            Context.SaveChanges();
            return user;
        }
    }

    [Fact]
    public async Task NewGoogleSignIn_CreatesPendingUser()
    {
        var env = new Env();

        var result = await env.AuthFor("new@example.com").HandleGoogleCallbackAsync("code", "verifier");

        Assert.True(result.IsSuccess);
        Assert.Equal(UserStatus.Pending, env.Context.Users.Single(u => u.Email == "new@example.com").Status);
    }

    [Fact]
    public async Task SignupsOff_TurnsAwayUnknownEmail_WithoutCreatingARow()
    {
        var env = new Env();
        await env.Access.UpdateSettingsAsync(new UpdateAccessSettingsRequest(false));

        var result = await env.AuthFor("new@example.com").HandleGoogleCallbackAsync("code", "verifier");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorStatus.Forbidden, result.ErrorStatus);
        Assert.DoesNotContain(env.Context.Users, u => u.Email == "new@example.com");
        Assert.Contains(env.Context.AdminAuditLogs, l => l.Action == AdminAction.ToggleSignups);
    }

    [Fact]
    public async Task SignupsOff_StillLetsExistingUsersSignIn()
    {
        var env = new Env();
        env.AddUser(UserStatus.Suspended, "known@example.com");
        await env.Access.UpdateSettingsAsync(new UpdateAccessSettingsRequest(false));

        var result = await env.AuthFor("known@example.com").HandleGoogleCallbackAsync("code", "verifier");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Reject_DeletesTheUserAndTheirTokens_AndTheAuditEntrySurvives()
    {
        var env = new Env();
        await env.AuthFor("new@example.com").HandleGoogleCallbackAsync("code", "verifier"); // Pending user + refresh token
        var pending = env.Context.Users.Single(u => u.Email == "new@example.com");

        var result = await env.Access.RejectAsync(pending.Id, new RejectMemberRequest("not now"));

        Assert.True(result.IsSuccess);
        Assert.Empty(env.Context.Users.Where(u => u.Id == pending.Id));
        Assert.Empty(env.Context.RefreshTokens.Where(t => t.UserId == pending.Id));
        var entry = env.Context.AdminAuditLogs.Single();
        Assert.Equal(AdminAction.Reject, entry.Action);
        Assert.Equal("new@example.com", entry.TargetEmail);
        Assert.Equal("not now", entry.Reason);
        Assert.Equal(pending.Id, entry.TargetUserId);
    }

    [Fact]
    public async Task Suspend_OnlyFlipsStatus_AndKeepsTheirData()
    {
        var env = new Env();
        var user = env.AddUser(UserStatus.Approved);
        env.Context.Accounts.Add(new Account { UserId = user.Id, Name = "Bank", Type = AccountType.Bank });
        env.Context.SaveChanges();

        var result = await env.Access.SuspendAsync(user.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(UserStatus.Suspended, env.Context.Users.Single(u => u.Id == user.Id).Status);
        Assert.Single(env.Context.Accounts.Where(a => a.UserId == user.Id));

        await env.Access.ReactivateAsync(user.Id);
        Assert.Equal(UserStatus.Approved, env.Context.Users.Single(u => u.Id == user.Id).Status);
    }

    [Fact]
    public async Task Delete_RemovesTheUserAndAllTheirData_AndLogsIt()
    {
        var env = new Env();
        var user = env.AddUser(UserStatus.Suspended);
        var bank = new Account { UserId = user.Id, Name = "Bank", Type = AccountType.Bank };
        var food = new Category { UserId = user.Id, Name = "Food", Type = CategoryType.Expense };
        env.Context.Accounts.Add(bank);
        env.Context.Categories.Add(food);
        env.Context.Transactions.Add(new Transaction { UserId = user.Id, AccountId = bank.Id, CategoryId = food.Id, Type = TransactionType.Expense, Amount = 100 });
        env.Context.SaveChanges();

        var result = await env.Access.DeleteAsync(user.Id);

        Assert.True(result.IsSuccess);
        Assert.Empty(env.Context.Users.Where(u => u.Id == user.Id));
        Assert.Empty(env.Context.Accounts.Where(a => a.UserId == user.Id));
        Assert.Empty(env.Context.Categories.Where(c => c.UserId == user.Id));
        Assert.Empty(env.Context.Transactions.Where(t => t.UserId == user.Id));
        Assert.Equal(AdminAction.DeleteUser, env.Context.AdminAuditLogs.Single().Action);
    }

    [Fact]
    public async Task WrongStatusOrAdminTarget_IsRefused_AndChangesNothing()
    {
        var env = new Env();
        var approved = env.AddUser(UserStatus.Approved);

        Assert.False((await env.Access.RejectAsync(approved.Id, new RejectMemberRequest(null))).IsSuccess); // reject is Pending-only
        Assert.False((await env.Access.SuspendAsync(env.Admin.Id)).IsSuccess);
        Assert.False((await env.Access.DeleteAsync(env.Admin.Id)).IsSuccess);
        Assert.Equal(2, env.Context.Users.Count());
        Assert.Empty(env.Context.AdminAuditLogs);
    }
}
