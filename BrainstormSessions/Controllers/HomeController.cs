using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BrainstormSessions.Core.Interfaces;
using BrainstormSessions.Core.Model;
using BrainstormSessions.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BrainstormSessions.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBrainstormSessionRepository _sessionRepository;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IBrainstormSessionRepository sessionRepository,
            ILogger<HomeController> logger)
        {
            _sessionRepository = sessionRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("[GET /Index] Request received");

            List<BrainstormSession> sessionList;

            try
            {
                sessionList = await _sessionRepository.ListAsync();
            } catch (Exception e)
            {
                _logger.LogError(e, "[GET /Index] Failed to retrieve list of brainstorm sessions");

                throw;
            }

            var model = sessionList.Select(session => new StormSessionViewModel()
            {
                Id = session.Id,
                DateCreated = session.DateCreated,
                Name = session.Name,
                IdeaCount = session.Ideas.Count
            });

            _logger.LogDebug("[GET /Index] ViewModel: {@model}", model);

            return View(model);
        }

        public class NewSessionModel
        {
            [Required]
            public string SessionName { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> Index(NewSessionModel model)
        {
            _logger.LogInformation("[POST /Index] Request received");


            if (!ModelState.IsValid)
            {
                _logger.LogWarning("[POST /Index] Invalid new session model was submitted: {@model}", model);

                return BadRequest(ModelState);
            }

            var session = new BrainstormSession()
            {
                DateCreated = DateTimeOffset.Now,
                Name = model.SessionName
            };

            try
            {
                await _sessionRepository.AddAsync(session);
            } catch (Exception e)
            {
                _logger.LogError(e, "[POST /Index] Failed to create new brainstorm session");

                throw;
            }

            _logger.LogInformation("[POST /Index] New brainstorm session was created: sessionId={@sessionId}", session.Id);
            _logger.LogDebug("[POST /Index] New brainstorm session was created: {@session}", session);

            return RedirectToAction(actionName: nameof(Index));
        }
    }
}
