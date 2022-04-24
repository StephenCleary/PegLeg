# Syntax

Type: `<` type `>` (note: [may be cut](./types.md))

Attributes: `@` name ` ` value

Literal characters: `'x'` / `"x"` (note that `""` is valid and always successfully matches the empty string, even at the end of input) (note: strings can have escape chars, too)

Nonterminal reference: `rule`

Sequence: `e1 e2`

Ordered choice: `e1 / e2`

Zero or more: `e*` (note: greedy)

One or more: `e+` (note: greedy)

Optional: `e?` (note: greedy)

And-predicate: `&e` (positive lookahead assertion)

Not-predicate: `!e` (negative lookahead assertion)

Grouping: `(e)`

Naming: `name:e`

Code: `{c}`

Regex: `/regex/flags` (note: character ranges are represented as regexes, e.g., `/[a-z]/i`)

Quantifier: `e<0,>` / `e<0,5>` / `e1<0,,e2>` / `e1<0,5,e2>`

Comments: `//` or `/* ... */` (note: the latter should nest)

Wildcard: `.`

Cut: `~`

## Notes

Begin/end pairs that may contain arbitrary text (Literal Characters and Code and possibly Regex) might change to use a C# raw string-style delimiter strategy (allowing arbitrary length), e.g., `"""string"""`, `{{{{code}}}}`, or `//regex//flags`.
