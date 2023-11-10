#nullable enable
using LanguageExt.Effects.Traits;
using LanguageExt.Transducers;

namespace LanguageExt;

public static class IOExtensions
{
            
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //
    //  Monadic join
    //

    public static IO<RT, E, A> Flatten<RT, E, A>(this IO<RT, E, IO<RT, E, A>> mma)
        where RT : struct, HasCancel<RT> =>
        new(mma.Thunk.Map(ma => ma.Map(r => r.Thunk)).Flatten());
    
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //
    //  Zipping
    //

    public static IO<RT, E, (A First, B Second)> Zip<RT, E, A, B>(this (IO<RT, E, A> First, IO<RT, E, B> Second) tuple)
         where RT : struct, HasCancel<RT> =>
         new(new ZipSumTransducer2<RT, E, A, B>(tuple.First.Thunk, tuple.Second.Thunk));
}
