using System.Threading.Tasks;

namespace Project.API.Applications.Queries {
    public interface IProjectQueries {
        Task<dynamic> GetProjectsByUserId (int userId);
        Task<dynamic> GetProjectDetail (int projectId);
    }
}