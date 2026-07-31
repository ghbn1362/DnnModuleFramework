# DnnModuleFoundation

## Enterprise Foundation for DotNetNuke Module Development

DnnModuleFoundation is an architectural foundation designed to build professional, scalable, and maintainable DotNetNuke (DNN) modules.

The purpose of this project is not to replace DNN, but to provide a consistent development foundation on top of DNN by reducing repetitive infrastructure code, improving maintainability, and enforcing a clean development structure.

DnnModuleFoundation provides common building blocks required for modern DNN module development, including:

- Standard module architecture
- Centralized module definition management
- Shared base classes
- Context management
- Service infrastructure
- Security foundations
- Skin and asset management
- Common utilities and extensions

The goal is simple:

> Build DNN modules faster, cleaner, and with a consistent long-term architecture.

---

# Why DnnModuleFoundation?

Developing DNN modules without a common foundation usually creates several problems:

- Every module has its own base classes
- Similar infrastructure code is duplicated
- Business logic becomes mixed with DNN implementation details
- Module maintenance becomes harder over time
- Developers follow different architectural approaches
- Refactoring large solutions becomes expensive

DnnModuleFoundation solves these problems by providing a reusable foundation layer.

Instead of every module creating its own infrastructure, all modules can share the same architectural standards.

---

# Main Goals

DnnModuleFoundation is designed around these goals:

## 1. Consistent Architecture

Every module should follow the same structure and development principles.

Developers should focus on business features instead of rebuilding infrastructure.

---

## 2. Reduce DNN Coupling

DNN provides many powerful APIs, but direct usage everywhere creates strong dependency.

The foundation introduces abstraction layers to keep module code cleaner and easier to maintain.

---

## 3. Increase Maintainability

A module should be easy to understand and modify even years after development.

The foundation provides centralized locations for:

- Configuration
- Module information
- Context access
- Shared services
- Common behaviors

---

## 4. Improve Development Speed

Common requirements should already exist:

- Module lifecycle handling
- Security checks
- Portal information access
- Resource management
- Shared utilities

Developers should spend time building features, not repeating infrastructure.

---

# Architecture Overview

DnnModuleFoundation follows a layered architecture approach.

DnnModuleFoundation

│
├── Foundation.Core
│
│ ├── Contracts
│ ├── Interfaces
│ ├── Definitions
│ ├── Context Abstractions
│ └── Shared Components
│
│
├── Foundation.Infrastructure
│
│ ├── DNN Integration
│ ├── Portal Services
│ ├── Module Services
│ ├── Security
│ ├── Configuration
│ └── Platform Implementations
│
│
├── Foundation.Web
│
│ ├── Module Base Classes
│ ├── Page Base Classes
│ ├── User Controls
│ ├── Skin Management
│ └── Presentation Helpers
│
│
└── Foundation.Tests
└── Automated Tests

---

# Core Concepts

# Module Definition

Every module should have a centralized definition.

Instead of spreading module information across different files, the module provides a single definition object.

Example:

```csharp
public class ProductModuleDefinition : ModuleDefinition
{
    public override string Name =>
        "Product";

    public override string Version =>
        "1.0.0";
}


}

The definition is responsible for describing:

Module identity
Configuration information
Shared metadata
Module-level settings

Benefits:

Centralized module information
Easier version management
Consistent module initialization
Better maintainability
Module Base Architecture

DnnModuleFoundation provides common base classes for module development.

Example:

public class ProductView : ModuleBase
{

}

Instead of every module implementing the same infrastructure repeatedly, the base layer provides common capabilities.

Examples:

Module context
Portal information
Security access
Common services
Shared behaviors

This creates a consistent development experience across all modules.

Page Base Architecture

Pages inside modules can inherit from a common page foundation.

Example

public class ProductPage : BasePage
{

}

This provides:

Common page lifecycle handling
Shared security validation
Common initialization logic
Consistent behavior
Context Management

Accessing DNN objects directly everywhere creates strong dependency and makes testing difficult.

DnnModuleFoundation introduces context abstractions.

Instead of

PortalSettings
ModuleInfo
UserInfo

being accessed everywhere, modules can work through centralized contexts.

Example

public interface IModuleContext
{
    int ModuleId { get; }

    int PortalId { get; }
}

Benefits:

Cleaner code
Lower dependency
Better testability
Easier future changes
Service Architecture

Business operations should not live inside UI controls.

The recommended flow

Presentation Layer

        |
        |

Application Services

        |
        |

Infrastructure

        |
        |

Database

This separation provides:

Better organization
Reusable business logic
Easier testing
Cleaner maintenance
Skin Management

DNN skins usually contain many shared resources:

CSS
JavaScript
Images
Templates
Assets

DnnModuleFoundation provides a centralized approach for managing skin-related resources.

The goal is to avoid duplicated resource handling and keep presentation infrastructure organized


---

# Project Structure

A module built using DnnModuleFoundation should follow a predictable structure.

Example:




MyModule

│
├── Definition
│
│ └── MyModuleDefinition.cs
│
│
├── Controllers
│
│ └── Module Controllers
│
│
├── Services
│
│ └── Business Services
│
│
├── Models
│
│ └── Data Models
│
│
├── Data
│
│ └── Data Access Components
│
│
├── Pages
│
│ └── Module Pages
│
│
├── Controls
│
│ └── User Controls
│
│
└── Assets

├── CSS

├── JavaScript

└── Images

The foundation does not force a specific business implementation.

It provides the infrastructure and standards required for building modules consistently.

---

# Development Philosophy

DnnModuleFoundation is built around several important principles.

---

## Separation of Responsibilities

Each component should have a clear responsibility.

Examples:

Bad:


Module Control

|
|

Database Query

|
|

Business Logic


Good:


Module Control

|
|

Application Service

|
|

Data Layer


The UI layer should focus on presentation, not business processing.

---

# Dependency Management

Components should depend on abstractions instead of concrete implementations.

The foundation encourages:

- Interfaces
- Service abstractions
- Clear boundaries between layers

This reduces dependency and makes future changes easier.

---

# Reusability

Common functionality should exist in the foundation instead of being duplicated in every module.

Examples:

- Security handling
- Context access
- Common validations
- Shared utilities
- Resource management

---

# Long-Term Maintainability

DnnModuleFoundation is designed with long-term projects in mind.

A module should remain understandable and maintainable after years of development.

The architecture focuses on:

- Predictable structure
- Consistent patterns
- Reduced complexity
- Clear responsibilities

---

# Supported Environment

## Platform

- DotNetNuke (DNN) 10.x
- .NET Framework 4.7.2
- C#
- ASP.NET WebForms
- SQL Server


## Development Tools

Recommended:

- Visual Studio
- SQL Server Management Studio
- Git


---

# Getting Started

## Create a New Module

A module using DnnModuleFoundation should:

1. Reference the foundation assemblies.

2. Create a module definition.

3. Inherit from foundation base classes.

4. Implement business logic through services.

5. Keep UI components focused on presentation.


Example:

```csharp
public class ProductView : ModuleBase
{
    protected void Page_Load(
        object sender,
        EventArgs e)
    {

    }
}
Coding Standards

To keep modules consistent:

Naming

Use clear and meaningful names.

Example:

ProductService

CustomerRepository

OrderDefinition


Avoid unclear names:

Helper1

Manager

CommonClass
Responsibilities

Each class should have one clear purpose.

Avoid creating classes that:

Handle database operations
Manage UI
Process business rules
Handle security

all together.

Security

Security should always be considered part of module architecture.

The foundation provides common infrastructure for:

Permission handling
User context
Access validation

Modules should never assume that users have permission.

Performance Considerations

Modules built on this foundation should consider:

Efficient database access
Proper caching strategies
Avoiding unnecessary DNN API calls
Optimized resource loading

The foundation provides common patterns to support these goals.

Roadmap

Future improvements may include:

Dependency Injection

Improved dependency management and service registration.

Logging Abstraction

Centralized logging support for modules.

Advanced Configuration

Unified configuration management.

Caching Infrastructure

Standard caching mechanisms.

Module Templates

Tools for quickly creating new modules based on foundation standards.

More Automated Tests

Increasing test coverage for foundation components.

Architectural Rules

Before adding a new feature to DnnModuleFoundation, consider:

Does it reduce complexity?

The foundation should simplify development, not add unnecessary abstraction.

Does it improve consistency?

New features should help modules follow the same standards.

Does it belong in the foundation?

Not every reusable code belongs in the foundation.

Only infrastructure-level functionality should be added.

What DnnModuleFoundation Is Not

DnnModuleFoundation is not:

A replacement for DNN
A business framework
A complete application framework
A database ORM
A UI component library

It is an architectural foundation that helps developers build better DNN modules.

Versioning

DnnModuleFoundation follows semantic versioning principles.

Version format:

MAJOR.MINOR.PATCH

Example:

1.0.0

Breaking architectural changes should increase the major version.

Contribution

Contributions are welcome.

Before submitting changes, consider:

Does this follow the existing architecture?
Does this improve maintainability?
Does this reduce complexity?
Does this help future modules?

Architectural consistency is more important than adding features quickly.

License

MIT License

Copyright © DnnModuleFoundation Contributors