% Expert System rules generated from cluster summaries

cluster(6, [feature(brands, 'SAMSUNG'), feature(colors, 'Black'), feature(memory, '8'), feature(storage, '128'), feature(rating, '4.30'), feature(sellingprice, '16999.00'), feature(discountpercentage, '0.00'), feature(os, 'Android'), feature(sellersamount, '420'), feature(screensize, '5.70'), feature(batterysize, '3558'), feature(reviews, '48')]).
cluster(0, [feature(brands, 'SAMSUNG'), feature(colors, 'Black'), feature(memory, '4'), feature(storage, '64'), feature(rating, '4.30'), feature(sellingprice, '9999.00'), feature(discountpercentage, '0.00'), feature(os, 'Android'), feature(sellersamount, '319'), feature(screensize, '4.60'), feature(batterysize, '4177'), feature(reviews, '11')]).
cluster(7, [feature(brands, 'SAMSUNG'), feature(colors, 'Black'), feature(memory, '4'), feature(storage, '64'), feature(rating, '4.30'), feature(sellingprice, '9999.00'), feature(discountpercentage, '0.00'), feature(os, 'Android'), feature(sellersamount, '482'), feature(screensize, '6.40'), feature(batterysize, '5916'), feature(reviews, '78')]).
cluster(1, [feature(brands, 'SAMSUNG'), feature(colors, 'Black'), feature(memory, '4'), feature(storage, '64'), feature(rating, '4.30'), feature(sellingprice, '9999.00'), feature(discountpercentage, '0.00'), feature(os, 'Android'), feature(sellersamount, '998'), feature(screensize, '6.50'), feature(batterysize, '3809'), feature(reviews, '706')]).
cluster(3, [feature(brands, 'Apple'), feature(colors, 'Black'), feature(memory, '6'), feature(storage, '128'), feature(rating, '0.00'), feature(sellingprice, '6499.00'), feature(discountpercentage, '0.00'), feature(os, 'Android'), feature(sellersamount, '266'), feature(screensize, '5.30'), feature(batterysize, '5761'), feature(reviews, '27')]).
cluster(2, [feature(brands, 'SAMSUNG'), feature(colors, 'Black'), feature(memory, '4'), feature(storage, '64'), feature(rating, '4.30'), feature(sellingprice, '9999.00'), feature(discountpercentage, '0.00'), feature(os, 'Android'), feature(sellersamount, '134'), feature(screensize, '6.80'), feature(batterysize, '2679'), feature(reviews, '8')]).
cluster(5, [feature(brands, 'SAMSUNG'), feature(colors, 'Black'), feature(memory, '4'), feature(storage, '64'), feature(rating, '4.30'), feature(sellingprice, '9999.00'), feature(discountpercentage, '0.00'), feature(os, 'Android'), feature(sellersamount, '585'), feature(screensize, '4.70'), feature(batterysize, '2336'), feature(reviews, '374')]).
cluster(8, [feature(brands, 'Apple'), feature(colors, 'Silver'), feature(memory, '4'), feature(storage, '256'), feature(rating, '4.60'), feature(sellingprice, '64900.00'), feature(discountpercentage, '0.00'), feature(os, 'IOS'), feature(sellersamount, '5'), feature(screensize, '5.60'), feature(batterysize, '5284'), feature(reviews, '229')]).
cluster(4, [feature(brands, 'SAMSUNG'), feature(colors, 'Black'), feature(memory, '4'), feature(storage, '64'), feature(rating, '4.30'), feature(sellingprice, '10990.00'), feature(discountpercentage, '26.67'), feature(os, 'Android'), feature(sellersamount, '563'), feature(screensize, '6.20'), feature(batterysize, '3623'), feature(reviews, '246')]).

% --------------------------------------------------------------------
% Backward Chaining Expert System Rules (New Version)
% --------------------------------------------------------------------

get_numeric_feature(Cluster, Field, NumVal) :-
    cluster(Cluster, Features),
    member(feature(Field, Val), Features),
    atom_number(Val, NumVal).

get_string_feature(Cluster, Field, StrVal) :-
    cluster(Cluster, Features),
    member(feature(Field, Val), Features),
    atom_string(Val, StrVal).

compute_closest_clusters(Requirements, FieldToClusterMap) :-
    include([R]>>(R = requirement(_,_,numeric)), Requirements, NumericReqs),
    maplist(get_closest_cluster_for_field, NumericReqs, Pairs),
    dict_create(FieldToClusterMap, _, Pairs).

get_closest_cluster_for_field(requirement(Field, Desired, numeric), Field-BestCluster) :-
    findall(Diff-C, (
         cluster(C, _),
         get_numeric_feature(C, Field, NumVal),
         Diff is abs(Desired - NumVal)
    ), List),
    sort(List, [_-BestCluster | _]).

% A requirement is represented as requirement(Field, Desired, Type),
% where Type is either 'text' or 'numeric'.

% For text requirements: Exact (case-insensitive) match.
triggered(Cluster, requirement(Field, Desired, text), _, true) :-
    get_string_feature(Cluster, Field, Val),
    downcase_atom(Val, LVal),
    downcase_atom(Desired, LDesired),
    LVal = LDesired.

% For numeric requirements: Use the precomputed field-to-cluster map.
triggered(Cluster, requirement(Field, _, numeric), FieldToClusterMap, true) :-
    get_dict(Field, FieldToClusterMap, Cluster), !.

triggered(_, _, _, false).

% score_cluster(+Cluster, +Requirements, +FieldToClusterMap, -Score)
% Score is the count of requirements triggered by the cluster.
score_cluster(Cluster, Requirements, FieldToClusterMap, Score) :-
    maplist(triggered(Cluster), Requirements, FieldToClusterMap, TriggeredList),
    include(==(true), TriggeredList, Filtered),
    length(Filtered, Score).

% best_cluster(+Requirements, -BestCluster)
best_cluster(Requirements, BestCluster) :-
    setof(C, Fs^(cluster(C, Fs)), Clusters),
    compute_closest_clusters(Requirements, FieldMap),
    findall(Score-C, (member(C, Clusters), score_cluster(C, Requirements, FieldMap, Score)), ScorePairs),
    sort(1, @>=, ScorePairs, [_-BestCluster | _]).

% parse_requirements(+InputString, -Requirements)
% Input is a string with requirements separated by ";".
% Each requirement is formatted as: "Field,Desired".
parse_requirements(InputString, Requirements) :-
    split_string(InputString, ";", " ", ReqStrings),
    maplist(parse_requirement, ReqStrings, Requirements).

parse_requirement(ReqStr, requirement(Field, Desired, Type)) :-
    split_string(ReqStr, ",", " ", Parts),
    ( Parts = [FieldString, DesiredString] ->
          ( number_string(Num, DesiredString) ->
                Type = numeric, Desired = Num, Field = FieldString
          ;
                Type = text, Desired = DesiredString, Field = FieldString
          )
    ;
       Field = "", Desired = "", Type = text
    ).

% Main predicate for backward chaining testing.
main :-
    read_line_to_string(user_input, Input),
    parse_requirements(Input, Requirements),
    best_cluster(Requirements, BestCluster),
    format("Best matching cluster: ~w~n", [BestCluster]),
    halt.

:- initialization(main).
