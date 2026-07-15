# Security policy

Report vulnerabilities privately through GitHub Security Advisories after the repository is published.

The application treats game archives and THTK output as untrusted input. Extraction is restricted to a random application staging directory, expected member names, expected output names, and supported source hashes. It never executes content extracted from a game archive.

Diagnostics must contain only version labels, relative virtual paths, hashes, sizes, and status codes. They must not contain absolute user paths, Steam account information, pixel data, or extracted files.
