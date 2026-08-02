From: Add DI bootstrap, DNN adapter and ManifestService implementation

This branch adds a minimal DI bootstrap and foundational services to make the
project more testable and modular. It does not change ModuleBase/BasePage
behavior yet; that will be done in a follow-up PR to keep changes small and
reviewable.

Changes:
- Add IManifestService and IDnnFacade contracts
- Add XmlManifestReader(Stream) constructor to support streaming/async reads
- Add DnnFacade adapter wrapping common DNN APIs
- Add ServiceProviderAccessor for WebForms-friendly DI access
- Add ServiceCollectionExtensions to register foundation services
- Add ManifestService implementation (async wrapper over XmlManifestReader)
- Add example Global.asax.cs bootstrap snippet
- Add unit test for ManifestService (NUnit)
