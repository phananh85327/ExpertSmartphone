:- use_module(library(readutil)).
:- use_module(library(lists)).
:- use_module(library(dcg/basics)).
:- dynamic cluster/2.

:- if(\+current_predicate(string_trim/2)).
string_trim(In,Out):-string_codes(In,C),phrase(trimmed(T),C),string_codes(Out,T).

trimmed(T)-->blanks,string(T),blanks,eos.
:- endif.

cluster(6, [feature(brands, 'SAMSUNG'), feature(colors, 'Black'), feature(memory, '8'), feature(storage, '128'), feature(rating, '4.30'), feature(sellingprice, '16999.00'), feature(discountpercentage, '0.00'), feature(os, 'Android'), feature(sellersamount, '420'), feature(screensize, '5.70'), feature(batterysize, '3558'), feature(reviews, '48')]).
cluster(0, [feature(brands, 'SAMSUNG'), feature(colors, 'Black'), feature(memory, '4'), feature(storage, '64'), feature(rating, '4.30'), feature(sellingprice, '9999.00'), feature(discountpercentage, '0.00'), feature(os, 'Android'), feature(sellersamount, '319'), feature(screensize, '4.60'), feature(batterysize, '4177'), feature(reviews, '11')]).
cluster(7, [feature(brands, 'SAMSUNG'), feature(colors, 'Black'), feature(memory, '4'), feature(storage, '64'), feature(rating, '4.30'), feature(sellingprice, '9999.00'), feature(discountpercentage, '0.00'), feature(os, 'Android'), feature(sellersamount, '482'), feature(screensize, '6.40'), feature(batterysize, '5916'), feature(reviews, '78')]).
cluster(1, [feature(brands, 'SAMSUNG'), feature(colors, 'Black'), feature(memory, '4'), feature(storage, '64'), feature(rating, '4.30'), feature(sellingprice, '9999.00'), feature(discountpercentage, '0.00'), feature(os, 'Android'), feature(sellersamount, '998'), feature(screensize, '6.50'), feature(batterysize, '3809'), feature(reviews, '706')]).
cluster(3, [feature(brands, 'Apple'), feature(colors, 'Black'), feature(memory, '6'), feature(storage, '128'), feature(rating, '0.00'), feature(sellingprice, '6499.00'), feature(discountpercentage, '0.00'), feature(os, 'Android'), feature(sellersamount, '266'), feature(screensize, '5.30'), feature(batterysize, '5761'), feature(reviews, '27')]).
cluster(2, [feature(brands, 'SAMSUNG'), feature(colors, 'Black'), feature(memory, '4'), feature(storage, '64'), feature(rating, '4.30'), feature(sellingprice, '9999.00'), feature(discountpercentage, '0.00'), feature(os, 'Android'), feature(sellersamount, '134'), feature(screensize, '6.80'), feature(batterysize, '2679'), feature(reviews, '8')]).
cluster(5, [feature(brands, 'SAMSUNG'), feature(colors, 'Black'), feature(memory, '4'), feature(storage, '64'), feature(rating, '4.30'), feature(sellingprice, '9999.00'), feature(discountpercentage, '0.00'), feature(os, 'Android'), feature(sellersamount, '585'), feature(screensize, '4.70'), feature(batterysize, '2336'), feature(reviews, '374')]).
cluster(8, [feature(brands, 'Apple'), feature(colors, 'Silver'), feature(memory, '4'), feature(storage, '256'), feature(rating, '4.60'), feature(sellingprice, '64900.00'), feature(discountpercentage, '0.00'), feature(os, 'IOS'), feature(sellersamount, '5'), feature(screensize, '5.60'), feature(batterysize, '5284'), feature(reviews, '229')]).
cluster(4, [feature(brands, 'SAMSUNG'), feature(colors, 'Black'), feature(memory, '4'), feature(storage, '64'), feature(rating, '4.30'), feature(sellingprice, '10990.00'), feature(discountpercentage, '26.67'), feature(os, 'Android'), feature(sellersamount, '563'), feature(screensize, '6.20'), feature(batterysize, '3623'), feature(reviews, '246')]).

maybe_number(A,N):-catch(atom_number(A,N),_,fail).

numeric_similarity(U,C,S):-maybe_number(U,Un),maybe_number(C,Cn),
    D is abs(Un-Cn), S is 1/(1+D).  % 1/(1+|Δ|)

feature_similarity(feature(K,U),feature(K,C),S):-
    ( maybe_number(U,_),maybe_number(C,_) ->
        numeric_similarity(U,C,S)
    ; (U==C->S=1;S=0) ).

cluster_score(UFs,CID,Score):-
    cluster(CID,CFs),
    findall(S,(member(Fu,UFs),member(Fc,CFs),feature_similarity(Fu,Fc,S)),Ss),
    sum_list(Ss,Score).

epsilon(1.0e-6).
best_clusters(UFs,IDs,Best):-
    findall(S-C,(cluster_score(UFs,C,S)),Pairs),
    pairs_keys(Pairs,Scores),max_list(Scores,Best),epsilon(E),
    findall(C,(member(S-C,Pairs),abs(S-Best)=<E),IDs).

parse_input(UFs):-
    writeln('Enter facts as feature(key,value); feature(...).'),
    read_line_to_codes(user_input,Cs0),
    (append(Cs,[46],Cs0)->true;Cs=Cs0), % strip '.'
    atom_codes(A,Cs),atomic_list_concat(Atoms,';',A),
    findall(feature(K,V),(member(Raw,Atoms),
        atom_string(Raw,S0),string_trim(S0,S),S\='',
        atom_to_term(S,feature(K,V),_)),UFs).

query_from_input(UFs):-
    best_clusters(UFs,IDs,Best),
    format('Top score ~2f, clusters ~w~n',[Best,IDs]).

main:-
    parse_input(UFs),best_clusters(UFs,IDs,Best),
    format('Top score ~2f, clusters ~w~n',[Best,IDs]).

start:-
    catch((
        prolog_current_frame(Frame),
        prolog_frame_attribute(Frame,parent_goal,Goal),
        ( Goal == user -> main ; true )
    ), _, true).

:- initialization(start).
