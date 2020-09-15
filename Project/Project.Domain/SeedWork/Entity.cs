using System;
using System.Collections.Generic;
using MediatR;

namespace Project.Domain.SeedWork {
    /// <summary>
    /// 领域事件发布
    /// </summary>
    public abstract class Entity {
        int? _requestdHashCode;
        int _Id;

        public virtual int Id {
            get { return _Id; }
            protected set {
                _Id = value;
            }
        }
        /// <summary>
        /// 事件集合
        /// </summary>
        private List<INotification> _domainEvents;

        public IReadOnlyCollection<INotification> DomainEvents => _domainEvents;

        /// <summary>
        /// 添加一个事件
        /// </summary>
        /// <param name="eventItem"></param>
        public void AddDomainEvent (INotification eventItem) {
            _domainEvents = _domainEvents??new List<INotification> ();
            _domainEvents.Add (eventItem); 
        }
        /// <summary>
        /// 移除一个事件
        /// </summary>
        /// <param name="eventItem"></param>
        public void RemoveDomainEvent (INotification eventItem) {
            _domainEvents?.Remove (eventItem);
        }
        /// <summary>
        /// 清除所有事件
        /// </summary>
        public void ClearDomainEvents () {
            _domainEvents?.Clear ();
        }

        public bool IsTransient () {
            return this.Id == default (Int32);
        }
        /// <summary>
        /// 校验对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals (object obj) {
            if (obj == null || !(obj is Entity))
                return false;

            if (Object.ReferenceEquals (this, obj))
                return true;

            if (this.GetType () != obj.GetType ())
                return false;

            Entity item = (Entity) obj;

            if (item.IsTransient () || this.IsTransient ())
                return false;
            else
                return item.Id == this.Id;
        }

        public override int GetHashCode () {
            if (!IsTransient ()) {
                if (!_requestdHashCode.HasValue)
                    _requestdHashCode = this.Id.GetHashCode () ^ 31; //XOR 异或运算
                return _requestdHashCode.Value;
            } else
                return base.GetHashCode ();
        }

        public static bool operator == (Entity left, Entity right) {
            if ((right as object) == null) return (left as object) == null;
            else return right.Equals (left);
        }

        public static bool operator != (Entity left, Entity right) {
            return !(left == right);
        }
    }
}