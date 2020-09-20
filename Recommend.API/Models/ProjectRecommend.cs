using System;
using System.Collections.Generic;

namespace Recommend.API.Models {
    /// <summary>
    /// 项目推荐
    /// </summary>
    public class ProjectRecommend {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int FromUserId { get; set; }
        public string FromUserName { get; set; }
        public string FromUserAvatar { get; set; }
        /// <summary>
        /// 项目推荐类型 1：平台 2：好友 3：2度好友
        /// </summary>
        /// <value></value>
        public EnumRecommendType RecommendType { get; set; }
        public int ProjectId { get; set; }
        /// <summary>
        /// 项目LOGO
        /// </summary>
        /// <value></value>
        public string ProjectAvatar { get; set; }
        /// <summary>
        /// 公司名称
        /// </summary>
        /// <value></value>
        public string Company { get; set; }
        /// <summary>
        /// 项目介绍
        /// </summary>
        /// <value></value>
        public string Introduction { get; set; }
        public string Tags { get; set; }
        /// <summary>
        /// 融资阶段
        /// </summary>
        /// <value></value>
        public string FinStage { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime RecommendTime { get; set; }
        //public List<ProjectRefrenceUser> ReferenceUser { get; set; }
    }
}