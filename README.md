# cb_orm
ORM Wrapper

Planned Features:
- ORM Wrapper optimized for Web Applications. (No WPF support for now)
- Compact Model Definition Language
- Database model population via mdl (no redundant code)
- Customizeable Attribute Generation
- Customizeable Field types.
- Save on demand (only save what has changed)
- ObjectCache (only one instance per id)
- Storage in file system (XML based)
- Optimization for loading 1:N Relations and 1:1 reverse navigation in file system storage
- Storage in MS-SQL Server
- Extensible for other SQL Dialects, Databases and Storages.
- Relation 1:1 with cascade delete (on client side) (Navigation in both directions)
- Relation 1:N with cascade delete (on client side) (Navigation in both directions)
- Relation 1:1 Weak (Navigation in both directions)
- Relation 1:N Weak (Navigation in both directions)
- Custom Enum Types
- Encryption (=>Saving passwords)
- Polymorphy by MultiTableInheritance
- ObjectVersioning
- Model Update Definition Language (mud for changing DB Structure)

-------------------
- Maybe just for fun:
- Compiler/Interpreter for Binary Model
- Binary Storage
- Lean and Mean C++ ORM Wrappers optimized for Embedded Systems

--------------------
LAAAAAAAAAAAAAAATER
- Gui for Model editor
- Tracking model changes with redesign-actions to auto generate mud...
- On-the-fly code generation and Iron Python scripting...
