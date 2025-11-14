# MD5 Helper Class Test Plan

## 1. Setup
- Create a temporary folder for test files.
- Use `File.WriteAllText` or `File.WriteAllBytes` to create small test files with known content.
- Known test content can be simple, e.g., `"Hello World"` or binary data.
- Compute the expected MD5 hash ahead of time using a trusted tool or library.

## 2. Tests for `ComputeMD5`

1. **Basic correctness**
   - Create a file with known content.
   - Call `ComputeMD5(filePath)`.
   - Assert that the returned MD5 matches the precomputed hash.

2. **Empty file**
   - Create an empty file.
   - Verify that `ComputeMD5` returns the correct MD5 for an empty input (`d41d8cd98f00b204e9800998ecf8427e`).

3. **File not found**
   - Pass a non-existent file path.
   - Assert that the method throws `FileNotFoundException`.

4. **Large file**
   - Optionally create a large file (e.g., 10MB) to ensure the method handles streaming correctly without loading the entire file into memory.

## 3. Tests for `ValidateMD5`

1. **Valid checksum**
   - Pass a file and its correct MD5 hash.
   - Assert that `ValidateMD5` returns `true`.

2. **Invalid checksum**
   - Pass the file with an incorrect MD5 string.
   - Assert that `ValidateMD5` returns `false`.

3. **Case-insensitivity**
   - Test that `ValidateMD5` works regardless of case (upper/lowercase MD5 strings).

4. **File not found**
   - Pass a non-existent file path.
   - Decide whether it should throw an exception or return `false`. Test accordingly.

5. **Empty file**
   - Validate that the MD5 for an empty file is correct.

## 4. Edge Cases
- Whitespace in file content: ensure it affects the hash correctly.
- Unicode characters: if the file contains UTF-8 characters, make sure the checksum is consistent.
- Binary files: verify that the checksum works for non-text files.

## 5. Cleanup
- Ensure all temp files are deleted after each test (use `try/finally` or Xunit fixtures).

## 6. Optional Extras
- Multiple files validation: Test that you can loop through multiple files and compute/validate checksums in bulk.
- Integration test: Generate a BagIt manifest with one or two files and validate their MD5 hashes using your helper class.

