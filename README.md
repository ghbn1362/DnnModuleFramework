# DnnModuleFoundation

> A modern, extensible and lightweight application foundation for building enterprise-grade DotNetNuke (DNN) modules.

DnnModuleFoundation is **not a framework that replaces DNN**.

It is a foundation that sits on top of the standard DNN module model and provides a consistent architecture for developing, maintaining and extending DNN modules while preserving the native capabilities of the DNN platform.

The primary goal of this project is to remove repetitive infrastructure code from every module and provide a clean, maintainable and highly extensible foundation that every DNN module can inherit from.

---

## Philosophy

Every DNN project eventually repeats the same infrastructure:

- Module initialization
- Context creation
- Service access
- Template loading
- Dashboard pages
- Resource management
- Utility classes
- Common helpers
- Client-side rendering
- Server-side rendering
- Security checks
- Skin management

Instead of implementing these concerns repeatedly inside every module, DnnModuleFoundation centralizes them into a reusable core.

Business logic belongs to your module.

Infrastructure belongs to DnnModuleFoundation.

---

## Design Goals

The foundation has been designed around a few simple principles.

### Preserve Native DNN

A module created with this foundation must never lose the capabilities that a normal DNN module already provides.

Everything available through the standard DNN development model should remain available.

The foundation adds capabilities.

It never replaces or hides native DNN functionality.

---

### Infrastructure First

A module developer should focus on solving business problems.

Infrastructure concerns such as context creation, template loading, dashboard rendering, utility methods and common services should already exist.

---

### Convention over Configuration

A new module should require as little configuration as possible.

Only module-specific information should be provided:

- Module Name
- Friendly Name
- Definition Name
- Installation Folder
- Template Locations
- Dashboard Settings
- Skin Configuration

Everything else should be automatically handled by the framework.

---

### Extensibility

Every component inside the framework is designed to be replaceable or extendable.

A developer should be able to customize:

- Rendering
- Templates
- Dashboard
- Skin
- Services
- Definitions
- Helpers

without modifying the framework itself.

---

### Low Coupling

The framework intentionally keeps external dependencies to an absolute minimum.

It primarily depends on the DNN platform itself.

This reduces maintenance cost and avoids dependency conflicts between modules.

---

### Long-Term Maintainability

This project is intended to become the common foundation of all future DNN modules.

Therefore every architectural decision prioritizes:

- readability
- consistency
- simplicity
- maintainability
- long-term evolution

over short-term convenience.

# Core Architecture

DnnModuleFoundation is designed as a layered architecture rather than a collection of helper classes.

Each layer has a single responsibility and can evolve independently without affecting the others.

```
                 +----------------------+
                 |    DNN Platform      |
                 +----------+-----------+
                            |
                 +----------v-----------+
                 | DnnModuleFoundation  |
                 +----------+-----------+
                            |
      +---------------------+----------------------+
      |                     |                      |
+-----v-----+        +------v------+       +-------v-------+
| Definition|        |   Context   |       | Infrastructure|
+-----------+        +-------------+       +---------------+
      |                     |                      |
      +-----------+---------+----------------------+
                  |
          +-------v--------+
          | Module Runtime |
          +-------+--------+
                  |
      +-----------+-----------+
      |                       |
+-----v------+         +------v------+
| Server View|         | Client View |
+------------+         +-------------+
                  |
          +-------v-------+
          | Business Code |
          +---------------+
```

The framework itself contains **no business logic**.

Its responsibility is to provide a reusable infrastructure that every module can inherit from.

---

# Architectural Principles

The architecture follows several important principles.

## Single Responsibility

Each component has one responsibility.

For example:

- ModuleDefinition describes a module.
- PageContext describes the current execution environment.
- ModuleBase provides infrastructure.
- SkinDefinition manages dashboard skins.
- Services encapsulate reusable functionality.
- Templates render the user interface.

Responsibilities never overlap.

---

## Open for Extension

The framework is designed to be extended rather than modified.

When building a new module you should rarely change the framework itself.

Instead, you extend its abstractions.

Typical extension points include:

- Module Definition
- Pages
- Dashboard
- Skins
- Templates
- Services
- Helpers

---

## Preserve DNN

The framework intentionally avoids hiding DNN.

Every DNN object remains available.

Examples include:

- ModuleInfo
- PortalSettings
- UserInfo
- Request
- Response
- ViewState
- Session
- Localization
- SkinPath

Developers familiar with DNN should immediately feel comfortable.

---

## Infrastructure over Inheritance

The framework provides infrastructure.

It does not attempt to replace the DNN programming model.

A developer still writes normal DNN modules.

The framework simply removes repetitive work.

---

# Runtime Components

The framework consists of several core components.

Each component has a clearly defined responsibility.

```

---

# Module Definition

`ModuleDefinition` is the heart of every module.

Instead of scattering constants across the project, all module metadata is centralized into a single definition object.

Typical information includes:

- Module Name
- Friendly Name
- Definition Name
- Installation Folder
- Resource Location
- Template Location
- Dashboard Configuration
- Skin Configuration
- Asset Locations

The framework uses this definition to configure the module runtime.

This means every derived module only needs to describe itself.

Everything else is handled automatically.

---

# Page Context

The execution context is represented by `PageContext`.

Rather than repeatedly accessing DNN objects throughout the codebase, the framework exposes a unified context object.

Typical runtime information includes:

- Portal
- Module
- User
- Tab
- Request
- Settings
- Services

The context acts as the bridge between DNN and the framework.

---

# Module Base

`ModuleBase` is the common base class for module controls.

It provides infrastructure shared by every page.

Examples include:

- Context creation
- Definition access
- Service resolution
- Utility methods
- Common helper functions
- Shared framework behavior

Business logic should never be placed inside ModuleBase.

Its responsibility is infrastructure only.

---

# Page Classes

Pages inherit from ModuleBase.

Each page represents a single functional part of the module.

Examples include:

- View
- Edit
- Settings
- Dashboard
- Management

Pages should remain lightweight.

Complex business logic belongs inside services.

---

# Service Layer

Business operations should be implemented through services.

This provides:

- better separation
- easier testing
- code reuse
- smaller page classes

Pages orchestrate.

Services execute.

---

# Rendering Layer

The rendering system supports multiple rendering strategies.

## Server-side Rendering

Traditional ASP.NET controls.

Examples:

- ASCX
- UserControls
- Razor (where applicable)

---

## Client-side Rendering

The framework also supports client-driven interfaces.

Typical technologies include:

- JavaScript
- Vue
- React
- Handlebars
- Mustache

The rendering engine should remain independent of business logic.

---

# Dashboard Architecture

The framework allows a module to expose an administrative dashboard.

Unlike standard module pages, dashboards may use a dedicated skin.

This allows administrative interfaces to have their own layout without affecting the public-facing module.

The dashboard infrastructure is part of the framework rather than being implemented separately inside every module.

---

# Skin System

Administrative pages often require a completely different layout.

Instead of embedding layout logic inside pages, the framework introduces dedicated skin definitions.

A skin definition controls:

- Assets
- CSS
- JavaScript
- Layout
- Shared components

Pages remain focused on application logic.

Skins focus on presentation.

---

# Template System

The template system allows the UI to be customized independently from the module logic.

Templates may be:

- Server-side
- Client-side

This separation allows the same module to support different visual implementations without changing business code.

---

# Utility Layer

Frequently used functionality is centralized into reusable helper classes.

Typical examples include:

- Path utilities
- URL helpers
- Localization
- Reflection
- Resource loading
- Security helpers
- HTML helpers

The goal is to eliminate duplicated code across modules.

# Creating Your First Module

One of the primary goals of DnnModuleFoundation is to minimize the amount of infrastructure code required when creating a new module.

A developer should only describe the module.

The framework builds the runtime around it.

---

# Step 1 — Create a Module Definition

Every module starts with a definition.

The definition represents the identity of the module.

```csharp
public sealed class MyModuleDefinition : ModuleDefinition
{
}
```

This single class describes everything that is unique about the module.

Typical configuration includes:

- Module Name
- Friendly Name
- Definition Name
- Installation Folder
- Resource Files
- Template Locations
- Dashboard Settings
- Asset Locations

Nothing else should need to know these values.

---

# Step 2 — Create a Base Page

Every module usually contains multiple pages.

Instead of duplicating infrastructure, they inherit from a common base.

```csharp
public abstract class MyModulePage : ModuleBase
{
}
```

This immediately provides:

- Runtime Context
- Module Definition
- Service Access
- Utility Functions
- Shared Infrastructure

---

# Step 3 — Create Pages

Each page focuses on a single responsibility.

Examples include:

```
View
Edit
Settings
Dashboard
Reports
Management
```

Pages should remain lightweight.

Heavy logic belongs inside services.

---

# Step 4 — Add Business Services

Business logic should never live inside UI pages.

Instead:

```
View
        │
        ▼
Application Service
        │
        ▼
Repository
        │
        ▼
Database
```

This separation makes the module easier to maintain and test.

---

# Step 5 — Create Templates

The UI can be implemented using different rendering approaches.

For example:

Server-side

```
ASCX
Razor
```

Client-side

```
Vue
React
Handlebars
Mustache
```

The rendering engine is independent of business logic.

---

# Step 6 — Run

Once the definition and pages exist, the framework provides:

- Runtime Context
- Module Infrastructure
- Dashboard Support
- Skin Support
- Template Resolution
- Utilities
- Helpers
- Resource Loading

without additional configuration.

---

# Typical Project Structure

A module built with DnnModuleFoundation generally follows this structure.

```
MyModule
│
├── Definition
│       MyModuleDefinition.cs
│
├── Pages
│       View.ascx
│       Edit.ascx
│       Dashboard.ascx
│
├── Services
│       ProductService.cs
│       UserService.cs
│
├── Templates
│       Default
│       Modern
│
├── Skin
│       Dashboard
│
├── Models
│
├── Repositories
│
├── Helpers
│
└── Resources
```

The exact organization is flexible.

The framework encourages consistency rather than enforcing a rigid folder hierarchy.

---

# What the Framework Provides

When a module inherits from DnnModuleFoundation, it automatically gains access to a common infrastructure.

This includes:

✔ Module Context

✔ Runtime Information

✔ Service Resolution

✔ Dashboard Infrastructure

✔ Skin Infrastructure

✔ Template Management

✔ Resource Management

✔ Common Utilities

✔ Shared Helpers

✔ Path Resolution

✔ Asset Management

✔ Localization Support

✔ DNN Integration

The module developer can therefore focus almost entirely on business requirements.

---

# What the Module Provides

The module itself is responsible only for:

- Business Logic
- Domain Models
- Database Access
- User Interface
- Business Services

Everything else belongs to the framework.

---

# Design Recommendation

A module should never reimplement functionality that already exists inside the framework.

If multiple modules require the same infrastructure, that functionality belongs in DnnModuleFoundation rather than being copied into individual projects.

This keeps every module smaller, more consistent and significantly easier to maintain.

---

# Extending the Framework

The framework has been intentionally designed around extension rather than modification.

Instead of changing the framework, create new implementations for the appropriate extension points.

Typical extension scenarios include:

- Custom Module Definitions
- Custom Page Types
- Custom Dashboard Implementations
- Custom Skin Definitions
- Custom Template Providers
- Custom Services
- Custom Helpers

This approach allows the framework to evolve while remaining backward compatible with existing modules.

# Framework Concepts

Understanding the philosophy behind DnnModuleFoundation is more important than learning its API.

The framework is intentionally designed around a small number of architectural concepts.

Once these concepts are understood, the rest of the framework becomes predictable and easy to use.

---

# Module Identity

Every module has a unique identity.

That identity is represented by a single object:

```
ModuleDefinition
```

Instead of spreading configuration across dozens of constants and helper classes, all module metadata belongs in one place.

The definition is the source of truth for the entire framework.

Typical information includes:

- Module Name
- Friendly Name
- Definition Name
- Installation Directory
- Resource Files
- Client Assets
- Server Templates
- Client Templates
- Dashboard Configuration
- Skin Configuration

The framework should never need to ask for these values elsewhere.

If the identity changes, only the definition changes.

---

# Runtime Context

Business code should not directly depend on DNN infrastructure.

Instead, the framework creates a runtime context representing the current execution environment.

The context typically contains:

- Portal
- Module
- Tab
- User
- Request
- Response
- Settings

Pages, services and infrastructure access runtime information through this context.

This creates a cleaner separation between application logic and the hosting platform.

---

# Infrastructure vs Business

One of the primary architectural goals is to separate infrastructure from business code.

Infrastructure includes:

- Template Resolution
- Asset Management
- Localization
- Dashboard
- Skin
- Utilities
- Context
- Services
- Resource Loading

Business code includes:

- Orders
- Products
- Users
- Reports
- Messages
- Domain Logic

The framework owns infrastructure.

The module owns business.

---

# Convention over Configuration

Most modules are structurally identical.

They differ only in:

- Name
- Resources
- Templates
- Business Logic

The framework therefore favors conventions rather than excessive configuration.

A developer should describe the module.

The framework should assemble everything else.

---

# Dashboard Isolation

Administrative interfaces have different requirements from public pages.

A dashboard may require:

- Different layout
- Additional JavaScript
- Custom CSS
- Administration menus
- Separate navigation

Instead of mixing dashboard behavior into normal pages, DnnModuleFoundation isolates dashboard functionality.

Dashboard pages become first-class citizens.

---

# Skin Isolation

Presentation should never leak into business code.

A skin represents visual infrastructure.

Pages describe behavior.

Skins describe presentation.

This separation allows administrative interfaces to evolve independently without modifying page logic.

---

# Rendering Independence

Rendering technology should not dictate architecture.

The framework supports multiple rendering approaches.

Examples include:

Server-side

- ASCX
- Razor

Client-side

- Vue
- React
- Handlebars

Business logic remains unchanged regardless of rendering technology.

---

# Extensibility

The framework is designed around extension points.

Developers extend behavior rather than modifying framework code.

Typical extension points include:

- Module Definitions
- Base Pages
- Services
- Dashboard
- Skin Definitions
- Template Providers
- Helpers

This makes upgrading significantly easier.

---

# Minimal Dependencies

Every dependency becomes part of every module.

Therefore the framework intentionally keeps external dependencies to an absolute minimum.

Whenever possible, it relies only on:

- .NET Framework
- DNN Platform

This improves:

- Compatibility
- Stability
- Upgradeability
- Long-term maintenance

---

# Preserve Native DNN

DnnModuleFoundation is not an alternative to DNN.

It is an architectural layer above DNN.

Developers retain access to all standard DNN features.

Nothing is hidden.

Nothing is removed.

The framework simply provides a better organization for building modules.

---

# Scalability

The framework is designed to support modules of every size.

Whether a module contains:

- one page

or

- hundreds of pages

the architecture remains the same.

This consistency significantly reduces maintenance costs over time.

---

# Why ModuleDefinition Exists

Without a definition object, every module repeats the same constants throughout the project.

Examples include:

- Module Name
- Friendly Name
- Folder
- Resource Path
- Template Path

This duplication creates maintenance problems.

ModuleDefinition centralizes these values into a single source of truth.

---

# Why ModuleBase Exists

Every DNN page requires access to common infrastructure.

Without a shared base class, every page would repeatedly implement:

- Context creation
- Service access
- Helpers
- Resource loading
- Utility methods

ModuleBase eliminates this duplication.

---

# Why Services Exist

Pages should coordinate.

Services should execute.

This separation keeps user interface code small while allowing business logic to grow independently.

---

# Why Templates Exist

Business logic should survive UI redesigns.

Templates allow user interfaces to evolve without affecting application logic.

The same module can support multiple visual implementations.

---

# Why the Framework Exists

The purpose of DnnModuleFoundation is not to simplify one module.

Its purpose is to standardize every module.

Every project built on top of the framework should look familiar.

Every developer should immediately recognize the architecture.

Every module should share the same infrastructure while remaining completely independent in terms of business functionality.


License

MIT License

Copyright © DnnModuleFoundation Contributors