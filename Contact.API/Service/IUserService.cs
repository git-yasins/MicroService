using System.Threading.Tasks;
using Contact.API.Dtos;

namespace Contact.API.Service
{
    public interface IUserService
    {
        /// <summary>
        /// 获取用户基本信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
         Task<UserIdentity> GetBaseUserInfoAsync(int userId);
    }
}