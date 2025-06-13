using Bunit;
using Voter.Blazor.WebAssembly.Pages;
using Voter.Blazor.WebAssembly.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Microsoft.JSInterop;
using Voter.Shared.Models;

namespace Voter.Blazor.ComponentTests
{
    public class ActivePollsComponentTests : IDisposable
    {
        private readonly TestContext _context = new();
        private readonly Mock<IPollService> _pollServiceMock = new();
        private readonly Mock<IJSRuntime> _jsRuntimeMock = new();

        public ActivePollsComponentTests()
        {
            // Setup authentication
            TestAuthorizationContext authContext = new()
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                        new Claim(ClaimTypes.Name, "testuser@example.com"),
                        new Claim(ClaimTypes.NameIdentifier, "test-user-id")
                    ], "mock"))
            };

            TestAuthenticationStateProvider authenticationStateProvider = new(authContext);

            // Register services
            _ = _context.Services.AddSingleton<AuthenticationStateProvider>(authenticationStateProvider);
            _ = _context.Services.AddAuthorizationCore();
            _ = _context.Services.AddSingleton<IPollService>(_pollServiceMock.Object);
            _ = _context.Services.AddSingleton<IJSRuntime>(_jsRuntimeMock.Object);

        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public void ActivePolls_ShouldShowLoadingState_Initially()
        {
            // Arrange - delay the mock response to ensure we see the loading state
            _ = _pollServiceMock
                .Setup(x => x.GetActivePollsAsync())
                .Returns(new Func<Task<List<PollDTO>>>(() => Task.Delay(1000).ContinueWith(_ => new List<PollDTO>())));

            // Act
            IRenderedComponent<ActivePolls> cut = _context.RenderComponent<ActivePolls>();

            // Assert
            AngleSharp.Dom.IElement spinner = cut.Find(".spinner-border");
            Assert.NotNull(spinner);
        }

        [Fact]
        public void ActivePolls_ShouldShowNoPolls_WhenEmpty()
        {
            // Arrange
            Moq.Language.Flow.IReturnsResult<IPollService> returnsResult = _pollServiceMock
                .Setup(x => x.GetActivePollsAsync())
                .ReturnsAsync([]);

            // Act
            IRenderedComponent<ActivePolls> cut = _context.RenderComponent<ActivePolls>();

            // Assert
            AngleSharp.Dom.IElement alert = cut.Find(".alert");
            Assert.Contains("No active polls found", alert.TextContent);
        }

        [Fact]
        public void ActivePolls_ShouldShowPolls_WhenDataAvailable()
        {
            // Arrange
            List<PollDTO> polls =
                [
                    new() {
                        Id = 1,
                        Question = "Test Poll 1",
                        StartDate = DateTime.Today.AddDays(-1),
                        EndDate = DateTime.Today.AddDays(5),
                        Options =
                        [
                            new() { Id = 1, Text = "Option 1" },
                            new() { Id = 2, Text = "Option 2" }
                        ]
                    },
                    new() {
                        Id = 2,
                        Question = "Test Poll 2",
                        StartDate = DateTime.Today,
                        EndDate = DateTime.Today.AddDays(7),
                        Options =
                        [
                            new PollOptionDTO { Id = 3, Text = "Option A" },
                            new PollOptionDTO { Id = 4, Text = "Option B" },
                            new PollOptionDTO { Id = 5, Text = "Option C" }
                        ]
                    }
                ];

            _ = _pollServiceMock
                .Setup(x => x.GetActivePollsAsync())
                .ReturnsAsync(polls);

            // Act
            IRenderedComponent<ActivePolls> cut = _context.RenderComponent<ActivePolls>();

            // Assert
            IRefreshableElementCollection<AngleSharp.Dom.IElement> tableRows = cut.FindAll("tbody tr");
            Assert.Equal(2, tableRows.Count);

            // Check first row content
            AngleSharp.Dom.IElement firstRow = tableRows[0];
            AngleSharp.Dom.IHtmlCollection<AngleSharp.Dom.IElement> cols = firstRow.QuerySelectorAll("td");
            Assert.Equal("Test Poll 1", cols[0].TextContent);
            Assert.Equal("2", cols[3].TextContent); // Number of options

            // Check for View button
            AngleSharp.Dom.IElement? viewButton = firstRow.QuerySelector(".btn-primary");
            Assert.NotNull(viewButton);
            Assert.Contains("View", viewButton!.TextContent);
            Assert.Equal("/polls/1", viewButton.GetAttribute("href"));
        }

        [Fact]
        public void ActivePolls_ShouldShowError_WhenLoadFails()
        {
            // Arrange
            _ = _pollServiceMock
                .Setup(x => x.GetActivePollsAsync())
                .ThrowsAsync(new Exception("Test error"));

            // Act
            IRenderedComponent<ActivePolls> cut = _context.RenderComponent<ActivePolls>();

            // Assert
            AngleSharp.Dom.IElement error = cut.Find(".alert-danger");
            Assert.Contains("Error loading polls", error.TextContent);
        }

        [Fact]
        public void ActivePolls_ViewButton_ShouldHaveCorrectLink()
        {
            // Arrange
            List<PollDTO> polls =
            [
                    new() {
                        Id = 123,
                        Question = "Test Poll",
                        StartDate = DateTime.Today,
                        EndDate = DateTime.Today.AddDays(5),
                        Options =
                        [
                            new PollOptionDTO { Id = 1, Text = "Option 1" },
                            new PollOptionDTO { Id = 2, Text = "Option 2" }
                        ]
                    }
                ];

            _ = _pollServiceMock
                .Setup(x => x.GetActivePollsAsync())
                .ReturnsAsync(polls);

            // Act
            IRenderedComponent<ActivePolls> cut = _context.RenderComponent<ActivePolls>();

            // Assert
            AngleSharp.Dom.IElement viewButton = cut.Find(".btn-primary");
            Assert.Equal("/polls/123", viewButton.GetAttribute("href"));
        }
    }

    // Test authentication state provider for testing authorized components
    internal class TestAuthenticationStateProvider(TestAuthorizationContext context) : AuthenticationStateProvider
    {
        private readonly AuthenticationState _authState = new(context.User);

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(_authState);
        }
    }

    internal class TestAuthorizationContext
    {
        public ClaimsPrincipal User { get; set; } = new(new ClaimsIdentity());
    }
}
