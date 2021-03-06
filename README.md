# PegLeg
A PEG parser for C#, using incremental code generators

# Goals

- Support *maintainable* grammars as much as possible. Do everything possible so that grammar maintainers don't have to change their grammar at all due to parser generator limitations.
- Work exclusively on `ReadOnlyMemory<char>` instead of strings and substrings.
- Implementation as a code generator: consumers define a `.peg` file and we generate the parser code on build with full cross-platform support.
- Be reasonably efficient, both in terms of memory usage and execution speed.
  - UTF8 support, if reasonably possible.

## Doc links

[TatSu](https://tatsu.readthedocs.io/en/stable/) has great docs. Note that TatSu [specifically disallows semantic actions](https://tatsu.readthedocs.io/en/stable/semantics.html), which in turn prevents code-based matching.

### Left recursion

Automatic handling of left-recursive grammars require memoization. The default for most PEG generators these days is to default memoization to *on*, but allow turning it off on a per-rule basis.

[Packrat Parsers Can Support Left Recursion (Warth, et al)](http://www.vpri.org/pdf/tr2007002_packrat.pdf)

[TatSu docs](https://tatsu.readthedocs.io/en/stable/syntax.html#left-recursion)

[Pegged docs](https://github.com/PhilippeSigaud/Pegged/wiki/Left-Recursion)

### Cut operator

[Packrat Parsers Can Handle Practical Grammars in Mostly Constant Space (Mizushima, et al)](https://kmizu.github.io/papers/paste513-mizushima.pdf)

[Pegen: Validate implemenation of cut operator](https://github.com/we-like-parsers/pegen_experiments/issues/49)

[Discussion on cut in Python](https://discuss.python.org/t/preparing-for-new-python-parsing/1550/43)

### State

[Is Stateful Packrat Parsing Really Linear in Practice? A Counter-Example, an Improved Grammar, and Its Parsing Algorithms (Chida, et al)](https://dl.acm.org/doi/pdf/10.1145/3377555.3377898)

### Examples

[Pegasus PEG grammar](https://github.com/otac0n/Pegasus/blob/1b094e30e6044ad898d65bb91f59b870b6092ee6/Pegasus/Parser/PegParser.peg)

### Other

[Practical Packrat Parsing (Grimm)](https://www.math.nyu.edu/media/mathfin/publications/TR2004-854.pdf)

[Packrat Parsing: a Practical Linear-Time Algorithm with Backtracking (Ford)](https://pdos.csail.mit.edu/~baford/packrat/thesis/thesis.pdf)

[Packrat Parsing: Simple, Powerful, Lazy, Linear Time (Ford)](https://bford.info/pub/lang/packrat-icfp02.pdf)

[Parsing Expression Grammars: A Recognition-Based Syntactic Foundation (Ford)](https://bford.info/pub/lang/peg.pdf)

[Pika parsing: reformulating packrat parsing as a dynamic programming algorithm solves the left recursion and error recovery problems (Hutchison)](https://arxiv.org/pdf/2005.06444.pdf) - interesting approach with good error recovery, but doesn't seem easily extendable via state to encompass some context-sensitivie grammars like PEG is.
