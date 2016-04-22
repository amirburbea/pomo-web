using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.Windsor;

namespace PoMo.Common.Windsor
{
    public interface IFactory<TComponent>
        where TComponent : class
    {
        void Release(TComponent component);

        TComponent Resolve();
    }

    public static class FactoryMethods
    {
        public static void RegisterFactory(IWindsorContainer container)
        {
            container.AddFacility<TypedFactoryFacility>().Register(Component.For(typeof(IFactory<>)).AsFactory());
        }
    }
}