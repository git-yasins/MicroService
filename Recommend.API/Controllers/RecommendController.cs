using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recommend.API.Data;

namespace Recommend.API.Controllers {
    [Route ("api/recommends")]
    public class RecommendController : BaseController {
        private readonly RecommendDbContext _dbContext;

        public RecommendController (RecommendDbContext dbContext) {
            this._dbContext = dbContext;
        }
        
        /// <summary>
        /// 获取当前用户所有的项目推送
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route ("")]
        public async Task<IActionResult> Get () {
            return Ok (await _dbContext.Recommends.AsNoTracking ()
                .Where (x => x.UserId == UserIdentity.UserId).ToListAsync ());
        }
    }
}