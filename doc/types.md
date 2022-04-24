# Types

With natural delegate types for lambdas, `<type>` may not be necessary in the [syntax](./syntax.md).

Note that although "string" is commonly used, the *actual* runtime type for "string" is `ReadOnlyMemory<char>`. Similarly, `T[]` is a shorthand for `IReadOnlyList<T>`.

## Natural types

If possible, it would be nice to have all (most?) rules imply their types.

- Literal Chars, Regex: `string`
- Rule: `<type>` if present; otherwise, inferred type.
- Sequence:
  - All types are `string`: `string`
  - All types are `T`: `T[]`
  - All types except `string` are `T` and there's only one `T`: `T`
  - All types except `string` are `T` and there's more than one `T`: `T[]`
  - Otherwise: `string`
- Choice:
  - All types are `T`: `T`
  - Otherwise: `string`
- Zero/one/optional/quantify: `T[]`
