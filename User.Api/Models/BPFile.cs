using System;
namespace User.API.Models
{
    /// <summary>
    /// 文件
    /// </summary>
    public class BPFile
    {
        /// <summary>
        /// BP ID
        /// </summary>
        /// <value></value>
        public int Id { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        /// <value></value>
        public int UserId { get; set; }
        /// <summary>
        /// 文件名称
        /// </summary>
        /// <value></value>
        public string FileName { get; set; }
        /// <summary>
        /// 上传的源文件地址
        /// </summary>
        /// <value></value>
        public string OriginFilePath { get; set; }
        /// <summary>
        /// 格式化后的文件地址
        /// </summary>
        /// <value></value>
        public string formatFilePath { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        /// <value></value>
        public DateTime CreateTime { get; set; }
    }
}