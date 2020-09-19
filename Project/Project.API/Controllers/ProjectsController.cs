using System;
using Microsoft.AspNetCore.Mvc;
using ProjectModel = Project.Domain.AggregatesModel.Project;
using System.Threading.Tasks;
using MediatR;
using Project.API.Applications.Commands;
using Project.API.Applications.Queries;
using Project.API.Applications.Service;
using Project.Domain.AggregatesModel;

namespace Project.API.Controllers {
    [Route ("api/[controller]")]
    public class ProjectsController : BaseController {
        private readonly IMediator mediator;
        private readonly IRecommendService recommend;
        private readonly IProjectQueries projectQueries;

        public ProjectsController (IMediator mediator, IRecommendService recommendService, IProjectQueries projectQueries) {
            this.mediator = mediator;
            this.recommend = recommendService;
            this.projectQueries = projectQueries;
        }

        [HttpGet]
        public async Task<IActionResult> GetProject () {
            return Ok (await projectQueries.GetProjectsByUserId (UserIdentity.UserId));
        }

        [HttpGet]
        [Route ("my/{projectId}")]
        public async Task<IActionResult> GetMyProjectDetail (int projectId) {
            var project = await projectQueries.GetProjectDetail (projectId);
            if (project.UserId == UserIdentity.UserId)
                return Ok (project);
            else
                return BadRequest ("无权查看该项目");
        }

        [HttpGet]
        [Route ("recommends/{projectId}")]
        public async Task<IActionResult> GetRecommendProjectDetail (int projectId) {
            if (await recommend.IsProjectInRecommendAsync (projectId, UserIdentity.UserId)) {
                return Ok (await projectQueries.GetProjectDetail (projectId));
            } else
                return BadRequest ("无权查看该项目");
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject ([FromBody] ProjectModel project) {
            if (project == null) {
                throw new ArgumentNullException (nameof (project));
            }

            project.UserId = UserIdentity.UserId;
            var command = new CreateProjectCommands () { Project = project };
            var projectResult = await mediator.Send (command);
            return Ok (projectResult);
        }

        [HttpPut]
        [Route ("view/{projectId}")]
        public async Task<IActionResult> ViewProject (int projectId) {
            if (!await recommend.IsProjectInRecommendAsync (projectId, UserIdentity.UserId)) {
                return BadRequest ("没有查看该项目的权限");
            }
            var command = new ViewProjectCommand () {
                UserId = UserIdentity.UserId,
                UserName = UserIdentity.Name,
                Avatar = UserIdentity.Avatar,
                ProjectId = projectId
            };
            await mediator.Send (command);
            return Ok ();
        }

        [HttpPut]
        [Route ("join/{projectId}")]
        public async Task<IActionResult> JoinProject (int projectId, [FromBody] ProjectContributor projectContributor) {
            if (!await recommend.IsProjectInRecommendAsync (projectId, UserIdentity.UserId)) {
                return BadRequest ("没有查看该项目的权限");
            }
            var command = new JoinProjectCommand () { ProjectContributor = projectContributor };
            await mediator.Send (command);
            return Ok ();
        }
    }
}