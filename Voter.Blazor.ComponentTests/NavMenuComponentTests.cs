using Bunit;
using Bunit.TestDoubles;
using Voter.Blazor.WebAssembly.Layout;
using Voter.Blazor.WebAssembly.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Voter.Blazor.ComponentTests
{
    public class NavMenuComponentTests : IDisposable
    {
        private readonly TestContext _context = new();
        private readonly Mock<IAuthService> _authServiceMock = new();

        public NavMenuComponentTests()
        {
            // Setup mocks and register services
            TestAuthorizationContext authContext = new()
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.Name, "testuser@example.com"),
                    new Claim(ClaimTypes.NameIdentifier, "test-user-id")
                ], "mock"))
            };

            TestAuthenticationStateProvider authenticationStateProvider = new(authContext);
            _ = _context.Services.AddSingleton<AuthenticationStateProvider>(authenticationStateProvider);
            _ = _context.Services.AddSingleton<IAuthService>(_authServiceMock.Object);
            // Set up authorization to always succeed
            _ = _context.Services.AddAuthorizationCore();
            _ = _context.AddTestAuthorization().SetAuthorized("test-user-id");
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public void NavMenu_WhenAuthenticated_ShouldShowFullMenu()
        {
            // Arrange & Act
            IRenderedComponent<CascadingAuthenticationState> cut = _context.RenderComponent<CascadingAuthenticationState>(parameters =>
                parameters.AddChildContent<NavMenu>());

            // Assert
            AngleSharp.Dom.IElement brand = cut.Find(".navbar-brand");
            Assert.Equal("VoteHub", brand.TextContent);

            AngleSharp.Dom.IElement? activePolls = cut.FindAll(".nav-link").FirstOrDefault(e => e.TextContent.Contains("Active Polls"));
            Assert.NotNull(activePolls);

            AngleSharp.Dom.IElement? closedPolls = cut.FindAll(".nav-link").FirstOrDefault(e => e.TextContent.Contains("Closed Polls"));
            Assert.NotNull(closedPolls);

            AngleSharp.Dom.IElement? myPolls = cut.FindAll(".nav-link").FirstOrDefault(e => e.TextContent.Contains("My Polls"));
            Assert.NotNull(myPolls);

            AngleSharp.Dom.IElement? createPoll = cut.FindAll(".nav-link").FirstOrDefault(e => e.TextContent.Contains("Create Poll"));
            Assert.NotNull(createPoll);
        }

        [Fact]
        public void NavMenu_NavLinks_ShouldHaveCorrectHrefs()
        {
            // Arrange & Act
            IRenderedComponent<CascadingAuthenticationState> cut = _context.RenderComponent<CascadingAuthenticationState>(parameters =>
                parameters.AddChildContent<NavMenu>());

            // Assert
            AngleSharp.Dom.IElement? home = cut.FindAll(".nav-link").FirstOrDefault(e => e.TextContent.Contains("Home"));
            Assert.Equal("home", home?.GetAttribute("href"));

            AngleSharp.Dom.IElement? activePolls = cut.FindAll(".nav-link").FirstOrDefault(e => e.TextContent.Contains("Active Polls"));
            Assert.Equal("polls/active", activePolls?.GetAttribute("href"));

            AngleSharp.Dom.IElement? closedPolls = cut.FindAll(".nav-link").FirstOrDefault(e => e.TextContent.Contains("Closed Polls"));
            Assert.Equal("polls/closed", closedPolls?.GetAttribute("href"));

            AngleSharp.Dom.IElement? myPolls = cut.FindAll(".nav-link").FirstOrDefault(e => e.TextContent.Contains("My Polls"));
            Assert.Equal("polls/my-polls", myPolls?.GetAttribute("href"));

            AngleSharp.Dom.IElement? createPoll = cut.FindAll(".nav-link").FirstOrDefault(e => e.TextContent.Contains("Create Poll"));
            Assert.Equal("polls/create", createPoll?.GetAttribute("href"));
        }

        [Fact]
        public void ToggleNavMenu_ShouldToggleCollapseClass()
        {
            // Arrange
            IRenderedComponent<CascadingAuthenticationState> cut = _context.RenderComponent<CascadingAuthenticationState>(parameters =>
                parameters.AddChildContent<NavMenu>());

            // Get initial state - should be collapsed
            AngleSharp.Dom.IElement? navMenu = cut.Find(".nav-scrollable");
            Assert.Contains("collapse", navMenu.GetAttribute("class"));

            // Act - click the toggle button
            AngleSharp.Dom.IElement? toggleButton = cut.Find(".navbar-toggler");
            toggleButton.Click();

            // Assert - should no longer be collapsed
            navMenu = cut.Find(".nav-scrollable");
            Assert.DoesNotContain("collapse", navMenu.GetAttribute("class"));

            // Act again - click to collapse
            toggleButton.Click();

            // Assert - should be collapsed again
            navMenu = cut.Find(".nav-scrollable");
            Assert.Contains("collapse", navMenu.GetAttribute("class"));
        }
    }
}
