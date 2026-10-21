// Project D | Codename: Zantetsuken — ServiceLocator.cs (register/resolve services, no singletons)
using System; using System.Collections.Generic;
namespace ProjectD.P15.Services
{
    public sealed class ServiceLocator
    {
        readonly Dictionary<Type,object> _services=new();
        public void Register<T>(T service) where T:class { if(service!=null) _services[typeof(T)]=service; }
        public T Resolve<T>() where T:class { return _services.TryGetValue(typeof(T),out var s)?(T)s:null; }
        public bool Has<T>() where T:class => _services.ContainsKey(typeof(T));
        public T Require<T>() where T:class { var s=Resolve<T>(); if(s==null) throw new InvalidOperationException($"Service {typeof(T).Name} not registered."); return s; }
        public void Clear()=>_services.Clear();
        public int Count=>_services.Count;
    }
}
