# 🧾 BagIt 1.0 (RFC 8493) Compliance Checklist

## 📁 Bag Structure
- [ ] Bag base directory exists.
- [ ] Contains a `data/` directory with payload files.
- [ ] Contains `bagit.txt` file at root with:
  - [ ] `BagIt-Version: 1.0`
  - [ ] `Tag-File-Character-Encoding: UTF-8`
- [ ] At least one payload manifest (`manifest-<algorithm>.txt`).
- [ ] No required file missing, no duplicate file paths.

## 🪪 Bag Declaration (`bagit.txt`)
- [ ] Exactly two lines, properly terminated (`\r\n` or `\n` acceptable per platform).
- [ ] Keys case-sensitive (`BagIt-Version`, `Tag-File-Character-Encoding`).
- [ ] Encoding name must be a valid IANA encoding label (e.g. UTF-8).

## 🧩 Payload Manifests
- [ ] Each `manifest-<alg>.txt` matches `<alg>` supported by implementation (e.g., `sha256`, `sha512`).
- [ ] Each payload file appears **once** per manifest.
- [ ] Each line: `<checksum><space><space><relative path>`.
- [ ] Paths use **forward slashes (`/`)**, not backslashes (`\`).
- [ ] Paths are relative to the bag root, not `data/`.
- [ ] Checksums use lowercase hex, no separators.

## 🏷 Tag Files (Optional)
- [ ] `bag-info.txt` parsed as key-value pairs.
- [ ] Optional `fetch.txt` entries validated for URI format.
- [ ] Optional `tagmanifest-<alg>.txt` entries, if present, verified like payload manifests.
- [ ] Tag files themselves excluded from payload manifests.

## 🧮 Algorithms & Checksums
- [ ] Must support **SHA-256**.
- [ ] Should support **SHA-512**.
- [ ] May support MD5 or SHA-1 for legacy bags.
- [ ] When validating, check that manifest’s algorithm matches filename.
- [ ] For new bags, use consistent algorithm for all manifests.

## 🧠 Validation Rules
- [ ] Every file listed in manifest exists and matches checksum.
- [ ] No payload file omitted from manifests.
- [ ] No extra files outside `data/` except allowed tag files.
- [ ] If multiple manifests exist, all must be internally consistent.
- [ ] All tag manifests (if present) validate.
- [ ] Report warnings for optional but missing files (like `bag-info.txt`).

## 🌍 Interoperability & Encoding
- [ ] Treat `Tag-File-Character-Encoding` as authoritative for reading tag files.
- [ ] Handle UTF-8 BOMs gracefully.
- [ ] Normalize Unicode filenames for comparison (NFC recommended).
- [ ] Use `/` path separators even on Windows.
- [ ] Ignore trailing whitespace and blank lines in manifests.
- [ ] Preserve relative paths as-is (no unintended canonicalization).

## 🧱 Definitions
- **Complete** = all required files present and properly structured.
- **Valid** = complete + all checksums match.

## 🧰 Suggested Features for `bagit.net`
- [ ] `bagit.net --version`
- [ ] `bagit.net --validate <path>`
- [ ] `bagit.net <path>` → create new bag.
- [ ] `bagit.net --algorithm sha512` → choose digest algorithm.
- [ ] Verbose / quiet modes for progress.
- [ ] Human-readable validation report.
- [ ] JSON validation report output (optional).
- [ ] Option to “repair” missing manifests (rebuild from existing payload).
