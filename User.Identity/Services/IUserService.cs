using System.Threading.Tasks;

namespace User.Identity.Services {
    /// <summary>
    /// 检查手机号是否注册,否则创建
    /// </summary>
    public interface IUserService {
        Task<int> CheckOrCreate (string phone);
    }
}