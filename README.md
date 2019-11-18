# cb_orm
ORM Wrapper

Realized features:
- Compact and extendable Model Definition Language
- Save on demand (only save modified objects)
- ObjectCache (only one instance per id)
- Storage in file system (XML based)
- Blops for huge Binary Data.
- Relation 1:N: Navigation from Parent to Child
- Relation 1:N: Navigation from Child to Parent
- Relation 1:N: cascade delete (on client side) 
- Relation 1:N: cascade create
- Relation 1:1: Navigation from Parent to Child
- Relation 1:1: Navigation from Child To Parent
- Relation 1:1: cascade delete (on client side)
- Relation 1:1: cascade create
- Optimization for loading 1:N Relations in file system storage

Planned Features: (~In this sequence)
- Custom Enum Types in mdl.
- Relation 1:1: Weak: Navigation from Parent to Child ***
- Encryption (=>Saving passwords)

- Optimization for loading 1:1 reverse navigation in file system storage
- Relation 1:1: Weak: Navigation from Child to Parent
- Relation 1:N: Weak: Navigation from Parent to Child
- Relation 1:N: Weak: Navigation from Child to Parent
- Polymorphy by MultiTableInheritance
- ObjectVersioning

- Customizeable Attribute Generation
- Customizeable Skalar-Field types/Type conversion chain for loading/saving.
- SQL DOM
- Storage in MS-SQL Server
- Extensible for other SQL Dialects, Databases and Storages.
- Database model population via mdl (no redundant user code)
- Model Update Definition Language (mud for changing DB Structure)
- Support for struct or class as skalar fields? (MS-SQL Server)

-------------------
- Maybe just for fun: (No usage atm)
- Compiler/Interpreter for Binary Model
- Binary Storage
- Lean and Mean C++ ORM Wrappers optimized for Embedded Systems
- Protobuf adapter
- Soap adapter
--------------------
LAAAAAAAAAAAAAAATER
- Gui for Model editor
- Tracking model changes with redesign-actions to auto generate mud...
- On-the-fly code generation and Iron Python scripting...
