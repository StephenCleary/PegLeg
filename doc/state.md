# State

State is a way for PEG grammars to extend just a bit into contextual grammars.

## Use cases for state

- Matching the most-recently-seen tag name in XML.
- Keeping track of forward-declared identifiers.
- Multi-quoted strings (e.g., C# raw strings).
- Multi-delimited regions (e.g., C# raw string interpolation).
- Significant whitespace (e.g., Python). (TODO: work out example of this)
- Avoid grammar bifurcation.

## Problems with state

- Arbitrary state modifications (e.g., from an oracle, or even from the same state) can cause infinite left-recursion.
- Global state can cause performance problems, exploding the memoizing cache while also preventing the cache benefits. This can result in exponential time and space, even on non-pathological inputs.

E.g., Pegasus has both problems, as it allows arbitrary state modification.

## Partial solution

See "Is Stateful Packrat Parsing Really Linear in Practice? A Counter-Example, an Improved Grammar, and Its Parsing Algorithms", which introduces V-PEG.

Problems with V-PEG:

- V-PEG has two "environments", which is awkward to think about. Both environments are handled the same way in the paper, but both have different operators and semantics.
- Most backreference usage just wants to match the same literal string (or a trivially transformed string). V-PEG allows matching an *expression* against the original literal string. I'm not sure what the utility of that is.
- V-PEG bifurcates the grammar based on whether it can contain environment operators.

# Proposed solution

There are three types of state (what V-PEG calls "environments"): `backref` (a simpler form of V-PEG's `match` environment), `set` (equivalent to a properly-scoped and memoized V-PEG `exists` environment), and `bool` (not present in V-PEG).

Proposed syntax and semantics, where `name` is a name identifier, `e` is an expression (TODO: possibly restrained from some state operations), and `c` is a code block.

## Backref State

Purpose: allow backrefs, which are matched as literal strings. For advanced scenarios, the literal string may be transformed by a code block.

Use cases:
- Matching the most-recently-seen tag name in XML.
- Multi-quoted strings (e.g., C# raw strings).
- Multi-delimited regions (e.g., C# raw string interpolation).

Syntax:

- `#\name=e` - Set the backref `name` to the result of evaluating `e`.
- `#\name` - Match the literal string consumed by the `e` for `name`. The result of this parse is the literal string.
- `#\(e)` - Establish a backref scope. All backrefs set in `e` do not exist outside the scope.
- `#\name{c}` - Take the literal string consumed by the `e` for `name`, pass it through the code `c`, and match the resulting literal string. The result of this parse is the literal string.

`ImmutableDictionary<VariableName, string>`

## Set State

Purpose: provide a collection (set) of strings.

Use cases:
- Keeping track of forward-declared identifiers.

Syntax:

- `#~name+=e` - Add the result of evaluating `e` to the set `name`. This is a set operation, so this is ignored if the string value already exists.
- `#~name?e` - Match `e` to the input, and determine if its string value is in `name`. If it is not in `name`, then this parse will fail.
- `#~(e)` - Establish a set scope. All set modifications in `e` do not propagate outside the scope.

`ImmutableDictionary<VariableName, ImmutableHashSet<string>>`

## Bool State

Purpose: provide contextual flags that lower-level rules can respond to.

Use cases:
- Avoid grammar bifurcation.

Syntax:

- `#?name+(e)` - Establish a bool state scope in which `name` is set.
- `#?name-(e)` - Establish a bool state scope in which `name` is not set. TODO: Not sure if we need this.
- `#?name?` - Zero-width assertion that succeeds if `name` is set and fails if `name` is not set.

Note: you can invert the logic by using a standard `!`, e.g., `!#?name?` will fail if `name` is set and succeed if `name` is not set.

`ImmutableHashSet<VariableName>`

## Memoization notes and open issues

- V-PEG doesn't include the `exists` environment (Set State) in memoization at all. I'm not sure if that's correct if `scope` is used with backtracking.
  - Turns out they globally memoize `exists`, which makes it less than useful and makes the `scope` semantics potentially extremely confusing, since `exists` may or may not be called on various branches within a `scope`. See below.
- For Backref and Bool State, we can "flatten" the state so that only the most-recent values for each name are present (similar to V-PEG).
- Comparing strings: variable names are a finite set and can be `static` instances compared by reference.
- The state doesn't actually need to be stored. For a sufficiently good hash (e.g., SHA1), storing the *hash* of the state is sufficient. This effectively removes the environment part of the space requirements of packrat parsers, at the cost of updating/recalculating hashes on state changes.

Example grammar showing confusion around `exists` with `scope`; the following will succeed in matching the *complete* input `xx`:

```
a: scope(b) / scope(d)
b: binde(decls,identifier) c !''
c: exists(decls,identifier)
d: binde(decls,'') identifier c
identifier: 'x'
```

The equivalent PegLeg grammar behaves in a more expected fashion, matching only the first `x` in `xx`:

```
a: #~(b) / #~(d)
b: #~decls+=identifier c !''
c: #~decls?identifier
d: #~decls+='' identifier c
identifier: 'x'
```

However, in order to do this, it must include the entire set state in its memoization.
