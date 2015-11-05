using System;
using System.Diagnostics;
using System.Linq;
using Ninject;
using Ninject.Extensions.Interception;
using Ninject.Extensions.Interception.Infrastructure.Language;
using Ninject.Infrastructure.Language;

namespace Interception.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Init...");
            var kernel = ConfigureKernel();
            var foo = kernel.Get<Foo>();

            Console.WriteLine("Do without timer interception begining call...");
            foo.Do();
            Console.WriteLine("Do without timer interception executed.");


            Console.WriteLine("Method DoWithTimer with timer interception begining call...");
            foo.DoWithTimer();
            Console.WriteLine("Method DoWithTimer with timer interception executed.");

            Console.ReadKey();
        }
        static IKernel ConfigureKernel() => new StandardKernel(new DefaultInterceptModule());
    }

    public class DefaultInterceptModule : InterceptionModule
    {
        public override void Load()
        {
            Kernel.Bind<Foo>().ToSelf();
            //Here i told to Intercept to intercept just types with any method that contains TimerInterceptAttribute and is virtual, and use the TimerInterceptor
            Kernel.Intercept(ctx => ctx.Request.Service.GetMethods().Any(mtd => mtd.IsDefined(typeof(TimerInterceptAttribute), true) && mtd.IsVirtual)).With(new TimerInterceptor());
        }
    }


    public class Foo
    {
        //Note: if you don't use interface's binding only virtual methods can be intercepted
        [TimerIntercept]
        public virtual void DoWithTimer()
        {
            Console.WriteLine("Doing..");
        }

        public void Do()
        {
            Console.WriteLine("Doing..");
        }

    }

    public class TimerInterceptAttribute : Attribute { }

    public class TimerInterceptor : SimpleInterceptor
    {
        readonly Stopwatch _stopwatch = new Stopwatch();
        private bool _hasTimerAttr;


        protected override void BeforeInvoke(IInvocation invocation)
        {
            //Note: you can use an Attribute that inherit from InterceptAttribute to intercept an method, but here i'm preventing that any method without the attribute excute the interception method.
            _hasTimerAttr = invocation.Request.Method.HasAttribute<TimerInterceptAttribute>();
            if (!_hasTimerAttr) return;
            Console.WriteLine($"The method {invocation.Request.Method.Name} start's running.");
            _stopwatch.Start();
        }

        protected override void AfterInvoke(IInvocation invocation)
        {
            if (!_hasTimerAttr) return;

            _stopwatch.Start();
            Console.WriteLine($"The method {invocation.Request.Method.Name} execution took {_stopwatch.ElapsedTicks} ticks.");
            _stopwatch.Reset();
        }
    }
}
