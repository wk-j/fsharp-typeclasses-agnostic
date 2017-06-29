﻿module InlineAbstractions.Types.ReaderT

open InlineAbstractions.Prelude
open InlineAbstractions.TypeClasses
open InlineAbstractions.TypeClasses.Monad
open InlineAbstractions.TypeClasses.MonadPlus
open InlineAbstractions.Types.MonadTrans
open InlineAbstractions.Types.MonadAsync
open InlineAbstractions.Types.Reader
open InlineAbstractions.Types.State
open InlineAbstractions.Types.Writer
open InlineAbstractions.Types.Cont

type ReaderT<'R,'Ma> = ReaderT of ('R -> 'Ma)

module ReaderT =
    let  runReaderT   (ReaderT x) = x
    let  mapReaderT f (ReaderT m) = ReaderT(f << m)
    let withReaderT f (ReaderT m) = ReaderT(m << f)

open ReaderT

type ReaderT<'R,'Ma> with
    static member inline instance (Functor.Fmap  , ReaderT m    , _) = fun f -> ReaderT <| fun r -> do'(){
        let! a = m r
        return (f a)}
type ReaderT<'R,'Ma> with
    static member inline instance (Monad.Return, _:ReaderT<'r,'ma>            ) :'a  -> ReaderT<'r,'ma> = fun a -> ReaderT <| fun _ -> return' a
    static member inline instance (Monad.Bind  ,   ReaderT m, _:ReaderT<'r,'m>) :('b -> ReaderT<'r,'m>) -> ReaderT<'r,'m> = 
        fun k -> ReaderT <| fun r -> do'(){
            let! a = m r
            return! runReaderT (k a) r}

    static member inline instance (MonadPlus.Mzero, _:ReaderT<_,_>        ) = fun ()          -> ReaderT <| fun _ -> mzero()
    static member inline instance (MonadPlus.Mplus,   ReaderT m   ,      _) = fun (ReaderT n) -> ReaderT <| fun r -> mplus (m r) (n r)

    static member inline instance (MonadTrans.Lift, _:ReaderT<'r,'ma>     ) = fun m -> (ReaderT <| fun _ -> m) : ReaderT<'r,'ma>

    static member inline instance (MonadReader.Ask, _:ReaderT<'r,'a>      ) = fun () -> ReaderT return' :ReaderT<'r,'a>
    static member inline instance (MonadReader.Local, ReaderT m, _:ReaderT<_,_>) = fun f  -> ReaderT(fun r -> m (f r))

    static member inline instance (MonadAsync.LiftAsync,  _:ReaderT<_,_>        ) = fun (x: Async<_>) -> lift (liftAsync x)

    static member instance (MonadCont.CallCC , _:ReaderT<'r,Cont<'c,'a>> ) : (('a -> ReaderT<'t,Cont<'c,'u>>) -> ReaderT<'r,Cont<'c,'a>>) -> ReaderT<'r,Cont<'c,'a>> =
        fun f -> ReaderT(fun r -> callCC <| fun c -> runReaderT (f (fun a -> ReaderT <| fun _ -> c a)) r)

    static member instance (MonadState.Get    , _:ReaderT<'s,State<'a,'a>>  ) = fun () -> lift (get()) :ReaderT<'s,State<'a,'a>>
    static member instance (MonadState.Put    , _:ReaderT<'s,State<'a,unit>>) = lift << put : 'a -> ReaderT<'s,State<'a,unit>>

    static member instance (MonadWriter.Tell  , _:ReaderT<'t,'a->Writer<'a,unit>>          ) :        ReaderT<'t,'a->Writer<'a,unit>> = lift tell
    static member instance (MonadWriter.Listen,   ReaderT m, _:ReaderT<'t,Writer<'a,'b*'a>>) :unit -> ReaderT<'t,Writer<'a,'b*'a>> = fun () -> ReaderT <| fun w -> listen (m w)  
    static member instance (MonadWriter.Pass  ,   ReaderT m, _:ReaderT<'t,Writer<'a,'b>>   ) :unit -> ReaderT<'t,Writer<'a,'b>>    = fun () -> ReaderT <| fun w -> pass   (m w)