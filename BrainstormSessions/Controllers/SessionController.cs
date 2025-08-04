using System;
using System.Threading.Tasks;
using BrainstormSessions.Core.Interfaces;
using BrainstormSessions.Core.Model;
using BrainstormSessions.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BrainstormSessions.Controllers
{
    public class SessionController : Controller
    {
        private readonly IBrainstormSessionRepository _sessionRepository;
        private readonly ILogger<SessionController> _logger;

        public SessionController(IBrainstormSessionRepository sessionRepository,
                                 ILogger<SessionController> logger)
        {
            _sessionRepository = sessionRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? id)
        {
            _logger.LogInformation("[GET /Index] Request received, id={id}", id);

            if (!id.HasValue)
            {
                return RedirectToAction(actionName: nameof(Index),
                    controllerName: "Home");
            }

            BrainstormSession session;
            
            try
            {
                session = await _sessionRepository.GetByIdAsync(id.Value);
            } catch (Exception e)
            {
                _logger.LogError(e, "[GET /Index] Failed to retrieve brainstorm session by id={id}", id.Value);

                throw;
            }

            if (session == null)
            {
                _logger.LogWarning("[GET /Index] Session with id={sessionId} was not found", id.Value);
                return Content("Session not found.");
            }

            _logger.LogDebug("[GET /Index] Session: {@session}", session);

            var sessionViewModel = new StormSessionViewModel()
            {
                DateCreated = session.DateCreated,
                Name = session.Name,
                Id = session.Id
            };

            _logger.LogDebug("[GET /Index] Session VM: {@sessionVm}", sessionViewModel);

            return View(sessionViewModel);
        }
    }
}
