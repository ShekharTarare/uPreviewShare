# Changelog

All notable changes to uPreviewShare will be documented in this file.

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
