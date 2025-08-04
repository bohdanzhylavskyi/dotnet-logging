using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrainstormSessions.ClientModels;
using BrainstormSessions.Core.Interfaces;
using BrainstormSessions.Core.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BrainstormSessions.Api
{
    public class IdeasController : ControllerBase
    {
        private readonly IBrainstormSessionRepository _sessionRepository;
        private readonly ILogger<IdeasController> _logger;

        public IdeasController(IBrainstormSessionRepository sessionRepository,
                               ILogger<IdeasController> logger)
        {
            _sessionRepository = sessionRepository;
            _logger = logger;
        }

        #region snippet_ForSessionAndCreate
        [HttpGet("forsession/{sessionId}")]
        public async Task<IActionResult> ForSession(int sessionId)
        {
            var session = await _sessionRepository.GetByIdAsync(sessionId);
            if (session == null)
            {
                LogSessionNotFoundWarning(sessionId);

                return NotFound(sessionId);
            }

            var ideas = session.Ideas.Select(idea => new IdeaDTO()
            {
                Id = idea.Id,
                Name = idea.Name,
                Description = idea.Description,
                DateCreated = idea.DateCreated
            }).ToList();

            return Ok(ideas);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody]NewIdeaModel model)
        {
            if (!ModelState.IsValid)
            {
                LogInvalidIdeaCreationModelError(model);
                return BadRequest(ModelState);
            }

            var session = await _sessionRepository.GetByIdAsync(model.SessionId);
            if (session == null)
            {
                LogSessionNotFoundWarning(model.SessionId);
                return NotFound(model.SessionId);
            }

            var idea = new Idea()
            {
                DateCreated = DateTimeOffset.Now,
                Description = model.Description,
                Name = model.Name
            };

            session.AddIdea(idea);

            LogNewlyCreatedIdeaInstanceDebug(idea, session.Id);

            await _sessionRepository.UpdateAsync(session);

            return Ok(session);
        }
        #endregion

        #region snippet_ForSessionActionResult
        [HttpGet("forsessionactionresult/{sessionId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<List<IdeaDTO>>> ForSessionActionResult(int sessionId)
        {
            var session = await _sessionRepository.GetByIdAsync(sessionId);

            if (session == null)
            {
                LogSessionNotFoundWarning(sessionId);
                return NotFound(sessionId);
            }

            var result = session.Ideas.Select(idea => new IdeaDTO()
            {
                Id = idea.Id,
                Name = idea.Name,
                Description = idea.Description,
                DateCreated = idea.DateCreated
            }).ToList();

            return result;
        }
        #endregion

        #region snippet_CreateActionResult
        [HttpPost("createactionresult")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<BrainstormSession>> CreateActionResult([FromBody]NewIdeaModel model)
        {
            if (!ModelState.IsValid)
            {
                LogInvalidIdeaCreationModelError(model);
                return BadRequest(ModelState);
            }

            var session = await _sessionRepository.GetByIdAsync(model.SessionId);

            if (session == null)
            {
                LogSessionNotFoundWarning(model.SessionId);
                return NotFound(model.SessionId);
            }

            var idea = new Idea()
            {
                DateCreated = DateTimeOffset.Now,
                Description = model.Description,
                Name = model.Name
            };
            session.AddIdea(idea);

            LogNewlyCreatedIdeaInstanceDebug(idea, session.Id);

            await _sessionRepository.UpdateAsync(session);

            return CreatedAtAction(nameof(CreateActionResult), new { id = session.Id }, session);
        }
        #endregion

        private void LogSessionNotFoundWarning(int sessionId)
        {
            _logger.LogWarning("Session with id={sessionId} was not found", sessionId);
        }

        private void LogInvalidIdeaCreationModelError(NewIdeaModel model)
        {
            _logger.LogError("Invalid model for idea creation: {@model}", model);
        }

        private void LogNewlyCreatedIdeaInstanceDebug(Idea idea, int sessionId)
        {
            _logger.LogDebug("New idea for session with id={sessionId} was created: {@idea}", sessionId, idea);
        }
    }
}
