How to use it:

In addition to just encrypting the string, this library allows you to add hashing before encryption.
Hashing needs another "salt" value to be provided along with the hash algorithm.

A sample use:
Encrypt("This is a text to be encrypted","passphraseforencryption","saltvalueforencryption","SHA1");