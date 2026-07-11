// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Security.Claims;
using LoganBussell.ExeDev;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ExeDev.Tests.Authentication;

public sealed class ExeDevClaimsPrincipalExtensionsTests
{
    [Fact]
    public async Task TryGetExeDevUserUsesExeDevIdentity()
    {
        ClaimsIdentity otherIdentity = new(
            [
                new Claim(ClaimTypes.NameIdentifier, "other-user"),
                new Claim(ClaimTypes.Email, "other@example.com"),
            ],
            "Other"
        );
        ClaimsIdentity exeDevIdentity = await AuthenticateAsync("exe-user", "owner@example.com");
        ClaimsPrincipal principal = new([otherIdentity, exeDevIdentity]);

        bool found = principal.TryGetExeDevUser(out ExeDevUser? user);

        Assert.True(found);
        Assert.Equal(new ExeDevUser("exe-user", "owner@example.com"), user);
    }

    [Fact]
    public void TryGetExeDevUserRejectsOtherAuthenticatedIdentity()
    {
        ClaimsPrincipal principal = new(
            new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, "other-user"),
                    new Claim(ClaimTypes.Email, "owner@example.com"),
                ],
                "Other"
            )
        );

        bool found = principal.TryGetExeDevUser(out ExeDevUser? user);

        Assert.False(found);
        Assert.Null(user);
    }

    private static async Task<ClaimsIdentity> AuthenticateAsync(string userId, string email)
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddExeDevAuthentication();

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();
        DefaultHttpContext context = new() { RequestServices = serviceProvider };
        context.Request.Headers[ExeDevAuthenticationDefaults.UserIdHeader] = userId;
        context.Request.Headers[ExeDevAuthenticationDefaults.EmailHeader] = email;

        AuthenticateResult result = await context.AuthenticateAsync();

        Assert.True(result.Succeeded);
        return Assert.IsType<ClaimsIdentity>(result.Principal.Identity);
    }
}
