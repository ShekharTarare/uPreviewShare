# Changelog

All notable changes to uPreviewShare will be documented in this file.

## [2.2.0] - 2026-07-06

### Added

- Per-page branding overrides — configure distinct colors and logo per content node
- Override toggle UI with inheritance indicator ("Using global branding defaults")
- Confirmation dialog when removing per-page overrides
- Branding API now accepts optional `?nodeKey=<guid>` query parameter for per-page operations

### Changed

- ExpiredLinkCleanupService migrated to Umbraco's `IRecurringBackgroundJob` (fixes scope conflicts)
- AddCultureColumn migration uses `Create.Column().OnTable()` pattern (Umbraco-recommended)
- Workspace view tab positioned after Content, before Info (weight 150)
- Global branding cache invalidation now flushes all per-node fallback entries

### Fixed

- Ambient scope conflicts in background cleanup service on SQLite
- Error notification on unsaved content nodes (404 handled gracefully)

## [2.1.0] - 2026-07-04

### Added

- Culture/variant support — share specific language versions of multilingual content
- Language picker in Create Link dialog for variant document types
- Variant badge shown on link cards in the workspace
- New DB migration: `Culture` column added to `uPreviewShare_Links` table

## [2.0.0] - 2026-06-29

### Changed

- Upgraded to Umbraco 18 (version range `[18.0.0,19.0.0)`)
- Updated test site dependencies (Umbraco 18.0.0, RuntimeCompilation 10.0.7)
- Bumped package version to 2.0.0

### Notes

- The `main` branch now targets Umbraco 18
- For Umbraco 17, use the `v17/dev` branch (version 1.x)
- No breaking API changes; existing links continue to work as before

## [1.0.2] - 2026-06-15

### Fixed

- Minor bug fixes and stability improvements

## [1.0.0] - 2026-05-22

### Added

- Secure token link generation with 128-bit cryptographic randomness
- Time-limited expiration (1h, 6h, 24h, 7d, 30d, or no expiration)
- Max views limit (1-10,000) with atomic view counting
- PIN protection with 6-digit numeric PIN (BCrypt hashed)
- Rate limiting: 5 failed PIN attempts triggers 15-minute lockout per IP
- Encrypted session cookies (30-minute duration) after successful PIN entry
- Individual and bulk link revocation
- Audit logging with IP address, user-agent, and millisecond timestamps
- Configurable branding (primary color, background color, text color, logo)
- Auto-contrast text color calculation for accessibility
- Auto-revocation on content publish or delete via notification handlers
- Draft/Published badge on preview page based on content state
- Dedicated Workspace Tab with stats cards, link management, audit log, and branding
- Card-based UI with search, filter, and pagination
- Umbraco-native toast notifications for all operations
- Background service for expired link cleanup
- NuGet package with buildTransitive targets for App_Plugins deployment
- Full Management API with Swagger documentation
- Inline HTML rendering (no Razor view dependencies)
