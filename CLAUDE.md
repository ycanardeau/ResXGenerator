# Code style

- Order definitions bottom-up (dependency-first), like a functional-programming source file where you can't reference something before it's bound: define a function/method/property only after everything it depends on is already defined above it. The entry point / top-level caller goes last, the leaves (depending on nothing else local) go first. Applies to properties and fields as well as functions and methods — order them by the same dependency rule (e.g. a computed property goes after the properties it reads).

    Example — correct order:

    ```
    void C() { }

    void B() { C(); }

    void A() { B(); }
    ```

    Incorrect (caller-first) order:

    ```
    void A() { B(); }

    void B() { C(); }

    void C() { }
    ```

    Apply this whenever touching a file: when writing new files, when adding new members to existing ones, and when editing an existing file whose members are out of order — reorder the existing members too so the whole file follows dependency order, not just the new additions.
