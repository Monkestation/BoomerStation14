cmd-openurl-help = openurl <url>

cmd-setmap-desc = Sets the map for a finite number of rounds (default 1)
cmd-setmap-help = Usage: setmap <map> [duration]
cmd-setmap-hint = <map>
cmd-setmap-hint-2 = [duration]
cmd-setmap-map-not-found = No eligible map exists with name { $map }.
cmd-setmap-optional-argument-not-integer = If argument 2 is provided it must be a number.
cmd-setmap-set-immediate = Map set to { $map } for { $rounds } { $rounds ->
    [one] round.
   *[other] rounds.
}
cmd-setmap-set-delayed = Map set to { $map } for { $rounds } { $rounds ->
    [one] round
   *[other] rounds
} starting next round.
cmd-setmap-reset-immediate = Cleared set map
cmd-setmap-reset-delayed = Cleared set map for next round
