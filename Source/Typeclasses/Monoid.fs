﻿namespace InlineAbstractions.TypeClasses

open InlineAbstractions.Prelude

module Monoid =
    type Mempty = Mempty with   
        static member        instance (Mempty, _:List<'a>  ) = fun () -> []   :  List<'a>
        static member        instance (Mempty, _:option<'a>) = fun () -> None :option<'a>
        static member        instance (Mempty, _:array<'a> ) = fun () -> [||] : array<'a>
        static member        instance (Mempty, _:string    ) = fun () -> ""
        static member        instance (Mempty, _:unit      ) = fun () -> ()

    let inline internal mempty() = Inline.instance Mempty ()

    type Mempty with static member inline instance (Mempty, _ : 'a*'b         ) = fun () ->
                        (mempty(),mempty()                           ): 'a*'b
    type Mempty with static member inline instance (Mempty, _ : 'a*'b*'c      ) = fun () ->
                        (mempty(),mempty(),mempty()                  ): 'a*'b*'c
    type Mempty with static member inline instance (Mempty, _ : 'a*'b*'c*'d   ) = fun () ->
                        (mempty(),mempty(),mempty(),mempty()         ): 'a*'b*'c*'d
    type Mempty with static member inline instance (Mempty, _ : 'a*'b*'c*'d*'e) = fun () ->
                        (mempty(),mempty(),mempty(),mempty(),mempty()): 'a*'b*'c*'d*'e


    type Mappend = Mappend with       
        static member        instance (Mappend, x:List<_>  , _) = fun y -> x @ y       
        static member        instance (Mappend, x:array<_> , _) = fun y -> Array.append x y
        static member        instance (Mappend, x:string   , _) = fun y -> x + y
        static member        instance (Mappend, ()         , _) = fun () -> ()

    let inline internal mappend (x:'a) (y:'a) :'a = Inline.instance (Mappend, x) y

    type Mappend with
        static member inline instance (Mappend, x:option<_> , _) = fun y ->
            match (x,y) with
            | (Some a , Some b) -> Some (mappend a b)
            | (Some a , None  ) -> Some a
            | (None   , Some b) -> Some b
            | _                 -> None


    type Mappend with static member inline instance (Mappend, (x1,x2         ), _) = fun (y1,y2         ) ->
                        (mappend x1 y1,mappend x2 y2                                          ) :'a*'b
    type Mappend with static member inline instance (Mappend, (x1,x2,x3      ), _) = fun (y1,y2,y3      ) ->
                        (mappend x1 y1,mappend x2 y2,mappend x3 y3                            ) :'a*'b*'c
    type Mappend with static member inline instance (Mappend, (x1,x2,x3,x4   ), _) = fun (y1,y2,y3,y4   ) ->
                        (mappend x1 y1,mappend x2 y2,mappend x3 y3,mappend x4 y4              ) :'a*'b*'c*'d
    type Mappend with static member inline instance (Mappend, (x1,x2,x3,x4,x5), _) = fun (y1,y2,y3,y4,y5) ->
                        (mappend x1 y1,mappend x2 y2,mappend x3 y3,mappend x4 y4,mappend x5 y5) :'a*'b*'c*'d*'e


    let inline internal mconcat x =
        let foldR f s lst = List.foldBack f lst s
        foldR mappend (mempty()) x

namespace InlineAbstractions.Types
open InlineAbstractions.Prelude
open InlineAbstractions.TypeClasses
open InlineAbstractions.TypeClasses.Monoid


type Dual<'a> = Dual of 'a with
    static member inline instance (Monoid.Mempty , _:Dual<'m>   ) = fun () -> Dual (mempty()) :Dual<'m>
    static member inline instance (Monoid.Mappend,   Dual x  , _) = fun (Dual y) -> Dual (y </mappend/> x)
module Dual = let inline  internal getDual (Dual x) = x

type Endo<'a> = Endo of ('a -> 'a) with
    static member        instance (Monoid.Mempty , _:Endo<'m>   ) = fun () -> Endo id  :Endo<'m>
    static member        instance (Monoid.Mappend,   Endo f  , _) = fun (Endo g) -> Endo (f << g)
module Endo = let inline  internal appEndo (Endo f) = f


type All = All of bool with
    static member instance (Monoid.Mempty, _:All     ) = fun () -> All true
    static member instance (Monoid.Mappend,  All x, _) = fun (All y) -> All (x && y)

type Any = Any of bool with
    static member instance (Monoid.Mempty, _:Any     ) = fun () -> Any false
    static member instance (Monoid.Mappend,  Any x, _) = fun (Any y) -> Any (x || y)