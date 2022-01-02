using System;
using System.Collections.Generic;
using CommonServiceLocator;
using Unity;

namespace Infrastructure.BootStrap
{
    public class UnityServiceLocator:IServiceLocator
    {
        private readonly IUnityContainer _unity;

        public UnityServiceLocator(IUnityContainer unity)
        {
            _unity = unity;
        }

        #region Implementation of IServiceProvider

        public object GetService(Type serviceType)
        {
            return _unity.Resolve(serviceType);
        }

        #endregion

        #region Implementation of IServiceLocator

        public object GetInstance(Type serviceType)
        {
            return _unity.Resolve(serviceType);
        }

        public object GetInstance(Type serviceType, string key)
        {
            return _unity.Resolve(serviceType,key);
        }

        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return _unity.ResolveAll(serviceType);
        }

        public TService GetInstance<TService>()
        {
            return _unity.Resolve<TService>();
        }

        public TService GetInstance<TService>(string key)
        {
            return _unity.Resolve<TService>(key);
        }

        public IEnumerable<TService> GetAllInstances<TService>()
        {
            return _unity.ResolveAll<TService>();
        }

        #endregion
    }
}