# BRC2

### Introduction

Bard code 2 (BRC2) is a symmetric encryption system, using a user chosen string of bytes as a key.

### Functionality

BRC2 was designed with security in mind. Both the length of the text and the contents of the text is hidden from anyone who does not have the key.

### Usage

When using BRC2, it is important to ensure that you provide a full path, instead of providing the path to a directory.

### Workings

To begin the encryption process, BRC2 does simple byte obfuscation. It then does the main process which relies on the SHA-512 hashing algorithm to hash the key. This hash is then used to modify the existing text. 

The final hash comes from thousands of iterations of hashing, this is to help prevent brute force attacks.
