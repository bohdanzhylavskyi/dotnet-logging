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
    [ApiController]
    [Route("api/[controller]")]
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

            _logger.LogInformation("[GET /forsession/{{sessionId}}] Request received, sessionId={sessionId}", sessionId);

            var session = await _sessionRepository.GetByIdAsync(sessionId);
            if (session == null)
            {
                _logger.LogWarning("[GET /forsession/{{sessionId}}] Session with id={sessionId} was not found", session);

                return NotFound(sessionId);
            }

            var ideas = session.Ideas.Select(idea => new IdeaDTO()
            {
                Id = idea.Id,
                Name = idea.Name,
                Description = idea.Description,
                DateCreated = idea.DateCreated
            }).ToList();

            _logger.LogDebug("[GET /forsession/{{sessionId}}] Ideas: {@ideas}", ideas);

            return Ok(ideas);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody]NewIdeaModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("[POST /create] Invalid model for idea creation: {@model}", model);

                return BadRequest(ModelState);
            }

            BrainstormSession session;
            
            try
            {
                session = await _sessionRepository.GetByIdAsync(model.SessionId);
            } catch (Exception e)
            {
                _logger.LogError("[POST /create] Failed to get session by id={sessionId}", model.SessionId);

                throw;
            }
            
            if (session == null)
            {
                _logger.LogWarning("[POST /create] Session with id={sessionId} was not found", model.SessionId);
                return NotFound(model.SessionId);
            }

            var idea = new Idea()
            {
                DateCreated = DateTimeOffset.Now,
                Description = model.Description,
                Name = model.Name
            };

            session.AddIdea(idea);

            _logger.LogInformation("[POST /create] New idea for session with id={sessionId} was created, ideaId={ideaId}", session.Id, idea.Id);
            _logger.LogDebug("[POST /create] New idea for session with id={sessionId} was created: {@idea}", session.Id, idea);

            try
            {
                await _sessionRepository.UpdateAsync(session);
            } catch (Exception e)
            {
                _logger.LogError("[POST /create] Failed to add new idea={@idea} for session (sessionId={sessionId}", idea, session.Id);

                throw;
            }

            return Ok(session);
        }
        #endregion 
    }
}
