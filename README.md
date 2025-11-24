This is a testing project for interaction with a PostgreSQL database. Each version grows more complicated and abstract. The programme is run from the program.cs file.

Version descriptions:
- Version 1
 - All queries are hand written and no subroutines are used.
- Version 2
  - String interpolation is used in query creation.
  - Some subroutines are used to re use commonly used functions.
  - Contains a concatenated insertion statement with faulty parameters.
- Version 3
 - Most of the programme functionality is now contained in subroutines, the main programme function now just calls these other subroutines.
- Version 4
 - Table creation, insertion and update subroutines now work off of the class of handed in objects.
 - Different tables are created based of different classes using the same subroutines. These subroutines are now generic enough to not require custom subroutines for each class.
