using Dapper;
using MySql.Data.MySqlClient;

namespace Project.API.Applications.Queries
{
    public class ProjectQueries : IProjectQueries {
        private readonly string connStrings;

        public ProjectQueries (string connStrings) {
            this.connStrings = connStrings;
        }
        public async System.Threading.Tasks.Task<dynamic> GetProjectDetail (int projectId) {
            using (var conn = new MySqlConnection (connStrings)) {
                conn.Open ();
                var sql = @"SELECT 
                            Projects.Company,
                            Projects.City,
                            Projects.Areaname,
                            Projects.FinStage,
                            Projects.FinMoney,
                            Projects.Valuation,
                            Projects.FinPercentage,
                            Projects.Introduction,
                            Projects.UserId,
                            Projects.Income,
                            Projects.Revenue,
                            Projects.Avatar,
                            Projects.BrokerageOptions,
                            ProjectVisibleRules.Tags,
                            ProjectVisibleRules.Visible
                            FROM Projects
                            INNER JOIN ProjectVisibleRules
                            ON Projects.Id = ProjectVisibleRules.ProjectId
                            WHERE Projects.Id = projectId";
                return await conn.QueryAsync<dynamic> (sql, new { projectId });
            }
        }

        public async System.Threading.Tasks.Task<dynamic> GetProjectsByUserId (int userId) {
            using (var conn = new MySqlConnection (connStrings)) {
                conn.Open ();

                var sql = @"SELECT Projects.Id,Projects.Avatar,Projects.Company,Projects.FinStage,
                            Projects.Introduction,Projects.Tags,Projects.ShowSecurityInfo,Projects.CreateTime FROM Projects
                            where Projects.UserId = @userId";
                return await conn.QueryAsync<dynamic> (sql, new { userId });
            }
        }
    }
}