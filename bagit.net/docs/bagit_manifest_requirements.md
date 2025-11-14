# Distilled Requirements for a Valid BagIt Manifest (RFC 8493)

## 1. Manifest Filenames

-   Must be named: `manifest-<algorithm>.txt`
-   At least one manifest file is required.

## 2. Manifest File Content Format

Each line must contain:

    <checksum><SP><pathname>

Where: - `<checksum>` is a lowercase hex digest. - `<SP>` is exactly one
space. - `<pathname>` is a POSIX path beginning with `data/`.

## 3. Manifest Line Rules

-   Checksums must be lowercase hexadecimal.
-   Exactly one space separates checksum and path.
-   Leading whitespace allowed (tabs/spaces), but **only one space** may
    separate checksum and path.
-   No trailing spaces.
-   Paths must be POSIX (`/`), no backslashes.
-   Paths must not contain absolute paths, `..`, or isolated `.`
    components.
-   Paths must reference **payload files** under `data/`.

## 4. Duplicate and Missing Entries

-   A file must appear **exactly once** in any given manifest.
-   All payload files must appear in every manifest file.

## 5. Multiple Manifest Consistency

If multiple manifests exist (e.g., md5 + sha256): - All must list
identical file paths.

## 6. Encoding Requirements

-   Manifest files must be UTF-8 encoded.
-   Text must be normalized to Unicode NFC.

## 7. Checksum Byte Rules

-   Checksums must represent the raw bytes of the file.

## 8. Ordering

-   File ordering is not normative (any order allowed).

## 9. Tag Manifests

-   Follow identical rules but reference tag files instead of payload
    files.
