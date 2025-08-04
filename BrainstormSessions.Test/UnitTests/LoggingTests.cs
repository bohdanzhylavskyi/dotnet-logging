using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrainstormSessions.Api;
using BrainstormSessions.Controllers;
using BrainstormSessions.Core.Interfaces;
using BrainstormSessions.Core.Model;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using Xunit;
using Serilog.Sinks.TestCorrelator;
using FluentAssertions;
using Serilog.Extensions.Logging;

namespace BrainstormSessions.Test.UnitTests
{
    public class LoggingTests
    {

        [Fact]
        public async Task HomeController_Index_LogInfoMessages()
        {
            // Arrange
            var serilogLogger = new LoggerConfiguration().WriteTo.TestCorrelator().CreateLogger();
            var logger = new SerilogLoggerFactory(serilogLogger).CreateLogger<HomeController>();
            var mockRepo = new Mock<IBrainstormSessionRepository>();
            
            mockRepo.Setup(repo => repo.ListAsync())
                .ReturnsAsync(GetTestSessions());

            var controller = new HomeController(mockRepo.Object, logger);

            using (TestCorrelator.CreateContext())
            {
                // Act
                var result = await controller.Index();

                // Assert
                TestCorrelator.GetLogEventsFromCurrentContext()
                    .Should().ContainSingle()
                    .Which.Level
                    .Should().Be(Serilog.Events.LogEventLevel.Information);
            }
        }

        [Fact]
        public async Task HomeController_IndexPost_LogWarningMessage_WhenModelStateIsInvalid()
        {
            // Arrange
            var serilogLogger = new LoggerConfiguration().WriteTo.TestCorrelator().CreateLogger();
            var logger = new SerilogLoggerFactory(serilogLogger).CreateLogger<HomeController>();
            var mockRepo = new Mock<IBrainstormSessionRepository>();
            mockRepo.Setup(repo => repo.ListAsync())
                .ReturnsAsync(GetTestSessions());

            var controller = new HomeController(mockRepo.Object, logger);
            controller.ModelState.AddModelError("SessionName", "Required");
            var newSession = new HomeController.NewSessionModel();

            using (TestCorrelator.CreateContext())
            {
                // Act
                var result = await controller.Index(newSession);

                // Assert
                TestCorrelator.GetLogEventsFromCurrentContext()
                    .Should().ContainSingle()
                    .Which.Level
                    .Should().Be(Serilog.Events.LogEventLevel.Warning, "Expected Warn messages in the logs");
            }
        }

        [Fact]
        public async Task IdeasController_CreateActionResult_LogErrorMessage_WhenModelStateIsInvalid()
        {
            // Arrange & Act
            var serilogLogger = new LoggerConfiguration().WriteTo.TestCorrelator().CreateLogger();
            var logger = new SerilogLoggerFactory(serilogLogger).CreateLogger<IdeasController>();
            var mockRepo = new Mock<IBrainstormSessionRepository>();
            var controller = new IdeasController(mockRepo.Object, logger);
            controller.ModelState.AddModelError("error", "some error");

            using (TestCorrelator.CreateContext()) {
                // Act
                var result = await controller.CreateActionResult(model: null);

                // Assert
                TestCorrelator.GetLogEventsFromCurrentContext()
                    .Should().ContainSingle()
                    .Which.Level
                    .Should().Be(Serilog.Events.LogEventLevel.Error, "Expected Error messages in the logs");
            }
        }

        [Fact]
        public async Task SessionController_Index_LogDebugMessages()
        {
            // Arrange
            var serilogLogger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.TestCorrelator().CreateLogger();
            var logger = new SerilogLoggerFactory(serilogLogger).CreateLogger<SessionController>();
            int testSessionId = 1;
            var mockRepo = new Mock<IBrainstormSessionRepository>();
            mockRepo.Setup(repo => repo.GetByIdAsync(testSessionId))
                .ReturnsAsync(GetTestSessions().FirstOrDefault(
                    s => s.Id == testSessionId));
            var controller = new SessionController(mockRepo.Object, logger);


            using (TestCorrelator.CreateContext())
            {
                // Act
                var result = await controller.Index(testSessionId);
                var logs = TestCorrelator.GetLogEventsFromCurrentContext();

                // Assert
                Assert.True(
                    logs.Count(l => l.Level == Serilog.Events.LogEventLevel.Debug) == 2,
                    "Expected 2 Debug messages in the logs"
                );
            }
        }

        private List<BrainstormSession> GetTestSessions()
        {
            var sessions = new List<BrainstormSession>();
            sessions.Add(new BrainstormSession()
            {
                DateCreated = new DateTime(2016, 7, 2),
                Id = 1,
                Name = "Test One"
            });
            sessions.Add(new BrainstormSession()
            {
                DateCreated = new DateTime(2016, 7, 1),
                Id = 2,
                Name = "Test Two"
            });
            return sessions;
        }

    }
}
