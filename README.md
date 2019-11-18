# cb_orm
ORM Wrapper

Realized features:
- Compact and extendable Model Definition Language

Planned Features: (~In this sequence)
- ORM Wrapper optimized for Web Applications. (No WPF support for now)
- Customizeable Attribute Generation
- Customizeable Skalar-Field types/Type conversion chain for loading/saving.
- Custom Enum Types in mdl.
- Save on demand (only save what has changed)
- ObjectCache (only one instance per id)
- Storage in file system (XML based)
- Optimization for loading 1:N Relations and 1:1 reverse navigation in file system storage
- Relation 1:1 with cascade delete (on client side) (Navigation in both directions)
- Relation 1:N with cascade delete (on client side) (Navigation in both directions)
- Relation 1:1 Weak (Navigation in both directions)
- Relation 1:N Weak (Navigation in both directions)
- Encryption (=>Saving passwords)
- Polymorphy by MultiTableInheritance
- ObjectVersioning

- SQL DOM
- Storage in MS-SQL Server
- Extensible for other SQL Dialects, Databases and Storages.
- Database model population via mdl (no redundant user code)
- Model Update Definition Language (mud for changing DB Structure)
- Support for struct or class as skalar fields?

-------------------
- Maybe just for fun: (No usage atm)
- Compiler/Interpreter for Binary Model
- Binary Storage
- Lean and Mean C++ ORM Wrappers optimized for Embedded Systems

--------------------
LAAAAAAAAAAAAAAATER
- Gui for Model editor
- Tracking model changes with redesign-actions to auto generate mud...
- On-the-fly code generation and Iron Python scripting...