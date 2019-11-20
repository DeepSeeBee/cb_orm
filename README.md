# cb_orm
ORM Wrapper

GettingStarted
- See the GettingStarted.txt in the root folder.

Concept:
- Similar to Microsoft EntityFramework
- ORM Wrapper for loading and saving object models consisting of composed, polymorph entity objects
- Compact and extendable Model Definition Language
- Code Generation for Model classes, SQL-Queries and database table population
- Loading objects on demand by navigating over object model.
- 1:1 and 1:N object relations with reverse navigation (Child->Parent)
- Fast Metadata handling.
- Extensible for other SQL Dialects, Databases and Storages.

Restrictions:
- Each object is identified by a Guid, also in the database tables.
- Cache doesnt use weak references. So loaded objects will stay in ram until the objectcontext is closed. (Suitable for WebApplications or DesktopApps using Lifetime per Workflow pattern or less)
- To avoid name conflicts you should avoid identifiers with underscore ("_") for class names and property names.
- Your database model should be normalized to allow creation of composed objects, however you can use weak references to break these rule.

Realized features:
- Compact and extendable Model Definition Language
- Code Generation of Model.
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
- Custom Enum Types in mdl by using existing enums.
- Custom Enum Types in mdl with code generation

Planned Features: (~In this sequence)
- Relation 1:1: Weak: Navigation from Parent to Child ***
- Encryption (=>Saving passwords)

- Optimization for loading 1:1 reverse navigation in file system storage
- Relation 1:1: Weak: Navigation from Child to Parent
- Relation 1:N: Weak: Navigation from Parent to Child
- Relation 1:N: Weak: Navigation from Child to Parent
- Polymorphy by MultiTableInheritance
- ObjectVersioning (Protection against concurrent overwrites of modified data)

- Customizeable Attribute Generation
- Customizeable Skalar-Field types/Type conversion chain for loading/saving.
- Storage in MS-SQL Server
- T-SQL: SQL Code DOM
- T-SQL: Code Generation for Insert
- T-SQL: Code Generation for Select
- T-SQL: Code Generation for Update
- T-SQL: Code Generation for Delete
- T-SQL: Code Generation for 1:N Parent->Child
- T-SQL: Code Generation for 1:N Child-Parent
- T-SQL: Code Generation for 1:1 Parent->Child
- T-SQL: Code Generation for 1:1 Child-Parent
- T-SQL: Code Generation for Database Table Population
- Database model population via mdl (no redundant user code)
- MS-SQL: Use transactions for atomar saving of object data
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
