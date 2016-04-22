using System;
using System.Windows;
using Castle.Windsor;
using Castle.Windsor.Installer;
using PoMo.Common.Windsor;

namespace PoMo.Client
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            using (IWindsorContainer container = new WindsorContainer())
            {
                FactoryMethods.RegisterFactory(container);
                container.Install(FromAssembly.This());
                container.Resolve<Application>().Run();
            }
        }
    }
}