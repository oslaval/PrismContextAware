using System;
using System.Diagnostics;
using System.Reflection;

namespace PrismContextAware.Services
{
    #region WeakAction Inner Class
    /// <summary>
    /// This class creates a weak delegate of form Action(Of Object)
    /// </summary>
    public class WeakAction
    {
        #region Data
        private readonly WeakReference _target;
        private readonly Type _ownerType;
        private readonly string _methodName;
        #endregion

        #region Public Properties/Methods
        public WeakAction(object target, MethodBase mi, Type actionType)
        {
            if (target == null)
            {
                Debug.Assert(mi.IsStatic);
                _ownerType = mi.DeclaringType;
            }
            else
            {
                _target = new WeakReference(target);
            }

            _methodName = mi.Name;
            ActionType = actionType;
        }

        public Type ActionType { get; }

        /// <summary>
        /// Is the object still in memory? Returns true if it is, false otherwise.
        /// </summary>
        public bool IsAlive { get { return _target.IsAlive; } }

        public bool HasBeenCollected { get { return _ownerType == null && (_target == null || !_target.IsAlive); } }

        public Delegate GetMethod()
        {
            if (_ownerType != null)
            {
                return Delegate.CreateDelegate(ActionType, _ownerType, _methodName);
            }

            if (_target != null && _target.IsAlive)
            {
                object target = _target.Target;
                if (target != null)
                {
                    return Delegate.CreateDelegate(ActionType, target, _methodName);
                }
            }

            return null;
        }
        #endregion
    }
    #endregion
}
