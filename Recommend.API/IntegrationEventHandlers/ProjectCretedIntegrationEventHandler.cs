using System;
using System.Threading.Tasks;
using DotNetCore.CAP;
using Recommend.API.Data;
using Recommend.API.IntegrationEvents;
using Recommend.API.Models;
using Recommend.API.Services;

namespace Recommend.API.IntegrationEventHandlers {
    /// <summary>
    /// 项目创建推送接收
    /// </summary>
    public class ProjectCretedIntegrationEventHandler : ICapSubscribe {
        private readonly RecommendDbContext dbContext;
        private readonly IUserService userService;
        private readonly IContactService contactService;

        public ProjectCretedIntegrationEventHandler (RecommendDbContext dbContext, IUserService userService, IContactService contactService) {
            this.dbContext = dbContext;
            this.userService = userService;
            this.contactService = contactService;
        }

        /// <summary>
        /// 获取项目推送消息
        /// </summary>
        /// <param name="events">项目信息</param>
        /// <returns></returns>
        [CapSubscribe ("finbook.projectapi.projectcreated")]
        // [CapSubscribe ("finbook.projectapi.projectJoined")]
        // [CapSubscribe ("finbook.projectapi.projectviewed")]
        public async Task CreateRecommendFromProject (ProjectCreatedIntegrationEvent @events) {
            //获取用户服务，获取创建项目的用户信息
            var fromUser = await userService.GetBaseUserInfoAsync (@events.UserId);
            //获取联系人服务的用户通讯录信息
            var contacs = await contactService.GetContactsByUserId (@events.UserId);
            //遍历通讯录发布创建项目通知
            foreach (var contact in contacs) {
                //创建项目的推送消息
                var recommend = new ProjectRecommend {
                    FromUserId = @events.UserId,
                    Company = @events.Company,
                    Tags = @events.Tags,
                    ProjectId = @events.ProjectId,
                    ProjectAvatar = @events.ProjectAvatar,
                    FinStage = @events.FinStage,
                    RecommendTime = DateTime.Now,
                    CreateTime = @events.CreatedTime,
                    Introduction = @events.Introduction,
                    RecommendType = EnumRecommendType.Friend,
                    FromUserAvatar = fromUser.Avatar,
                    FromUserName = fromUser.Name,
                    UserId = contact.UserId
                };
                dbContext.Recommends.Add (recommend);
            }
            await dbContext.SaveChangesAsync ();
        }
    }
}