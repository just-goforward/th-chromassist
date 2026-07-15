# Contributing

1. Read `AGENTS.md` and `README.md` before changing code.
2. Open an issue before adding a game adapter or changing a fairness boundary.
3. Use only synthetic fixtures and metadata in commits and CI.
4. Run `dotnet format --verify-no-changes`, `dotnet build -c Release`, and `dotnet test -c Release`.
5. Document every supported asset set with source, version label, hashes, virtual paths, and local validation evidence.
6. Do not attach original or transformed game files to issues, pull requests, releases, or diagnostics.

Pull requests must explain user-visible behavior, safety/fairness impact, validation performed, and any remaining uncertainty.
