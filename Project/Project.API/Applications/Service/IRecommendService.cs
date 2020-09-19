using System.Threading.Tasks;

namespace Project.API.Applications.Service {
    public interface IRecommendService {
        Task<bool> IsProjectInRecommendAsync (int projectId, int userId);
    }
}