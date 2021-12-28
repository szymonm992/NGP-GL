using ModestTree;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace UInc.Core.Utilities
{
    public static class DiContainerExtensions
    {
        // Same as BindInterfaces except also binds to self
        public static ConcreteBinderNonGeneric BindInterfacesAnd<T>(this DiContainer di)
        {
            return BindInterfacesAnd(di, typeof(T));
        }

        public static ConcreteBinderNonGeneric BindInterfacesAnd(DiContainer di, Type type)
        {
            var list = new List<Type>();
            list.AddRange(type.Interfaces());
            list.Add(type);

            return di.Bind(list);
        }

        public class FactoryFromBinder2 : FactoryFromBinderBase
        {
            public FactoryFromBinder2(
               DiContainer container, BindInfo bindInfo, FactoryBindInfo factoryBindInfo, Type type)
               : base(container, type, bindInfo, factoryBindInfo)
            {
            }
        }
    }


}
