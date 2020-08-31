namespace User.Identity.Services {
    /// <summary>
    /// 根据手机号验证手机验证码
    /// </summary>
    public interface IAuthCodeService {
        bool Validate (string phone, string authCode);
    }
}