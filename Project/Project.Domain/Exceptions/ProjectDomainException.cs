using System;

namespace Project.Domain.Exceptions {
    /// <summary>
    /// 领域异常
    /// </summary>
    public class ProjectDomainException : Exception {
        public ProjectDomainException () {

        }

        public ProjectDomainException (string message) : base (message) {

        }

        public ProjectDomainException (string message, Exception inner) : base (message, inner) {

        }
    }
}