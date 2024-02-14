using System;
using System.IO;
using System.Threading.Tasks;
using LanguageExt.Common;
using LanguageExt.Traits;
using static LanguageExt.Prelude;
using static LanguageExt.Transducer;

namespace LanguageExt;

public static class Testing
{
    public static void Test1()
    {
        var m1 = ReaderT<string, Maybe, int>.Lift(Maybe.Just(123));
        var m2 = ReaderT<string, Maybe, int>.Lift(Maybe.Just(123));
        
        var mx = ReaderT<string, ReaderT<string, Maybe>, int>.Lift(m1);

                
        var m0 = from w in Pure(123)
                 from x in mx
             //  from r in use(() => File.Open("c:\\test.txt", FileMode.Open))
                 from y in mx
                 from z in asks((string env) => env.Length)
                 from e in ask<string>()
             //  from _ in release(r)
                 from n in ReaderT<string, Maybe, string>.Lift(Maybe.Just("Paul"))
                 select $"{e} {n}: {w + x + y + z}";

        var m3 = from w in Pure(123)
                 from x in m1
           //    from r in use(() => File.Open("c:\\test.txt", FileMode.Open))
                 from y in m2
                 from z in Maybe.Just(100)
                 from e in ask<string>()
           //    from _ in release(r)
                 select $"{e}: {w + x + y + z}";

        var r1 = m3.Run("Hello");
        
        var m4 = from x in m1
                 from y in m2
                 from z in Maybe<int>.Nothing
                 from e in ask<string>()
                 select $"{e}: {x + y + z}";

        var r2 = m4.Run("Hello");
    }
    
    public static void Test2()
    {
        var m1 = Reader<string, int>.Pure(123);
        var m2 = Reader<string, int>.Pure(123);
        
        var m3 = from x in m1
                 from y in m2
                 from e in ask<string>()
                 select $"{e}: {x + y}";
        
        var m4 = from x in m1
                 from y in m2
                 from e in ask<string>()
                 from z in Pure(234)
                 select $"{e}: {x + y}";
    }
    
    public static void Test3()
    {
        var m1 = ReaderT<string, Either<string>, int>.Lift(Right(123));
        var m2 = ReaderT<string, Either<string>, int>.Lift(Right(123));
        
        var m3 = from w in Pure(123)
                 from x in m1
                 from r in use(() => File.Open("c:\\test.txt", FileMode.Open))
                 from y in m2
                 from z in Right(100)
                 from e in ask<string>()
                 from _ in release(r)
                 select $"{e}: {w + x + y + z}";

        var r1 = m3.Run("Hello");
        
        var m4 = from x in m1
                 from y in m2
                 from z in Left<string, int>("fail")
                 from e in ask<string>()
                 select $"{e}: {x + y + z}";

        var r2 = m4.Run("Hello");        
    }
    
    
    public static async Task Test4()
    {
        var m1 = App.Pure(123);
        var m2 = App.Pure(123);
        var m3 = App.Fail<int>(Error.New("fail"));
        
        var m4 = from w in Pure(234)
                 from x in m1
                 from y in m2
                 from z in m3
                 from r in App.rootFolder
                 from t in liftAsync(async () => await File.ReadAllTextAsync($"{r}\\test.txt"))
                 select $"{t}: {w + x + y + z}";

        var r1 = await m4.RunAsync(new AppConfig("", ""));
    }
   
    public static void Test6()
    {
        var m1 = ReaderT<string, IdentityT<IO>>.lift(IdentityT<IO, int>.Lift(IO.Pure(123)));
        var m2 = ReaderT<string, IdentityT<IO>>.lift(IdentityT<IO, int>.Lift(IO.Pure(123)));
                
        var m0 = from w in Pure(123)
                 from p in ReaderT<string, IdentityT<IO>>.ask
                 from x in IO.Pure("Hello")
                 from i in ReaderT<string, IdentityT<IO>>.liftIO(IO.Pure("Hello"))
                 from j in IO.Pure("Hello").Fork()
                 from r in IO.envIO 
                 from y in m2
                 select $"{p} {y} {j}";

        var value = m0.Run("Hello").As().Value.As().Run(EnvIO.New());
    }
   
    public static void Test7()
    {
        var m1 = ResourceT<ReaderT<string, IO>>.lift(ReaderT<string, IO>.lift(IO.Pure(123)));
        var m2 = ResourceT<ReaderT<string, IO>>.lift(ReaderT<string, IO>.lift(IO.Pure(123)));
                
        var m0 = from w in Pure(123)
                 from q in m1
                 from f in ResourceT<ReaderT<string, IO>>.use(() => File.Open("c:\\test.txt", FileMode.Open))
                 from p in ReaderT<string, IO>.ask
                 from x in IO.Pure("Hello")
                 from i in ReaderT<string, IO>.liftIO(IO.Pure("Hello"))
                 from j in IO.Pure("Hello").Fork()
                 from r in IO.envIO 
                 from y in m2
                 select $"{p} {y} {j}";

        var value = m0.Run(ma => ma.As().Run("Hello").As());
    }
   
    public static void Test8()
    {
        var m1 = ResourceT.lift(ReaderT<string>.lift(OptionT.lift(IO.Pure(123))));
        var m2 = ResourceT.lift(ReaderT<string>.lift(OptionT.lift(IO.Pure(123))));

        var m0 = from w in Pure(123)
                 from q in m1
                 from f in use(() => File.Open("c:\\test.txt", FileMode.Open))
                 from p in ask<string>()
                 from i in liftIO(IO.Pure("Hello"))
                 from j in IO.Pure("Hello").Fork()
                 from r in IO.envIO 
                 from _ in release(f)
                 from y in m2
                 select $"{w} {f} {i}";

        var value = m0.Run(ma => ma.As().Run("Hello")
                                   .As().Match(Some: x => x, None: () => "hello")
                                   .As());

        ResourceT<ReaderT<Env, OptionT<IO>>, Env> ask<Env>() =>
            ResourceT.lift(ReaderT.ask<Env, OptionT<IO>>());
        
        ResourceT<ReaderT<string, OptionT<IO>>, A> use<A>(Func<A> f) where A : IDisposable =>
            ResourceT<ReaderT<string, OptionT<IO>>>.use(f);
        
        ResourceT<ReaderT<string, OptionT<IO>>, Unit> release<A>(A value) where A : IDisposable =>
            ResourceT<ReaderT<string, OptionT<IO>>>.release(value);

        ResourceT<ReaderT<string, OptionT<IO>>, A> liftIO<A>(IO<A> ma) =>
            ResourceT.lift(ReaderT.liftIO<string, OptionT<IO>, A>(ma));
        
    }

    
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// 
//  Maybe test
//

public class Maybe : Monad<Maybe>
{
    public static Maybe<A> Just<A>(A value) =>
        new Just<A>(value);

    public static K<Maybe, B> Bind<A, B>(K<Maybe, A> ma, Func<A, K<Maybe, B>> f) => 
        ma.As().Bind(f);

    public static K<Maybe, B> Map<A, B>(Func<A, B> f, K<Maybe, A> ma) => 
        ma.As().Map(f);
    
    public static K<Maybe, A> Pure<A>(A value) => 
        Just(value);

    public static K<Maybe, B> Apply<A, B>(K<Maybe, Func<A, B>> mf, K<Maybe, A> ma) =>
        mf.As().Bind(f => ma.As().Map(f));

    public static K<Maybe, B> Action<A, B>(K<Maybe, A> ma, K<Maybe, B> mb) =>
        ma.As().Bind(_ => mb);

    public static K<Maybe, A> LiftIO<A>(IO<A> ma) => 
        throw new NotImplementedException();
}

public abstract record Maybe<A> : K<Maybe, A>
{
    public static readonly Maybe<A> Nothing = 
        new Nothing<A>();
    
    public abstract Maybe<B> Map<B>(Func<A, B> f);

    public abstract Maybe<B> Bind<B>(Func<A, Maybe<B>> f);

    public virtual Maybe<B> Bind<B>(Func<A, K<Maybe, B>> f) =>
        Bind(x => f(x).As());

    public Maybe<C> SelectMany<B, C>(Func<A, Maybe<B>> bind, Func<A, B, C> project) =>
        Bind(x => bind(x).Map(y => project(x, y)));

    public Maybe<C> SelectMany<B, C>(Func<A, K<Maybe, B>> bind, Func<A, B, C> project) =>
        SelectMany(x => bind(x).As(), project);
}

public record Just<A>(A Value) : Maybe<A>
{
    public override Maybe<B> Map<B>(Func<A, B> f) => 
        new Just<B>(f(Value));

    public override Maybe<B> Bind<B>(Func<A, Maybe<B>> f) =>
        f(Value);
}

public record Nothing<A> : Maybe<A>
{
    public override Maybe<B> Map<B>(Func<A, B> f) => 
        Maybe<B>.Nothing;

    public override Maybe<B> Bind<B>(Func<A, Maybe<B>> f) =>
        Maybe<B>.Nothing;
}

public static class MaybeExt
{
    public static Maybe<A> As<A>(this K<Maybe, A> ma) =>
        (Maybe<A>)ma;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// 
//  App test
//

// Domain monad
public record App<A>(Func<AppConfig, K<Either<Error>, A>> runReader)
    : ReaderT<AppConfig, Either<Error>, A>(runReader);

// Application environment
public record AppConfig(string ConnectionString, string RootFolder);

public static class App
{
    public static App<A> Pure<A>(A value) =>
        (App<A>)App<A>.Pure(value);

    public static App<A> Fail<A>(Error error) =>
        (App<A>)App<A>.Lift(Left<Error, A>(error));

    public static App<string> connectionString =>
        (App<string>)App<string>.Asks(env => env.ConnectionString);

    public static App<string> rootFolder =>
        (App<string>)App<string>.Asks(env => env.RootFolder);    
}

