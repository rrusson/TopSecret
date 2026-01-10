# TopSecret
This is a very basic password keeper using .NET MAUI to provide multi-platform support.

**Features include:** extra paranoid encryption, automatic shutdown, and a streamlined UI for viewing your account credentials.

Notes: 
- I ported it from an app I made for my Windows Phone 8 (RIP).
- Master password is set on first use (and can be updated).
- Master password is never stored, only a hash.
- After ten bad guesses, there's a deliberate, complete data wipe of [all]() password data.