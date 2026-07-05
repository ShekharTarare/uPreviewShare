# uPreviewShare - Secure Draft Preview Sharing for Umbraco

[![NuGet](https://img.shields.io/nuget/v/uPreviewShare?color=blue)](https://www.nuget.org/packages/uPreviewShare)
[![NuGet Downloads](https://img.shields.io/nuget/dt/uPreviewShare)](https://www.nuget.org/packages/uPreviewShare)
[![Umbraco Marketplace](https://img.shields.io/badge/Umbraco-Marketplace-3544B1?logo=umbraco)](https://marketplace.umbraco.com/package/upreviewshare)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://github.com/ShekharTarare/uPreviewShare/blob/main/LICENSE)
[![Umbraco 17+](https://img.shields.io/badge/Umbraco-17%2B-orange)](https://umbraco.com)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple)](https://dotnet.microsoft.com)

![uPreviewShare Logo](https://raw.githubusercontent.com/ShekharTarare/uPreviewShare/main/src/uPreviewShare/icon.png)

Securely share unpublished draft content with external stakeholders — clients, legal reviewers, freelancers — without requiring Umbraco backoffice accounts. Reviewers see the **exact page** rendered through the actual Umbraco template.

## Features

| Feature                       | Description                                                                    |
| ----------------------------- | ------------------------------------------------------------------------------ |
| True Draft Preview            | Renders through Umbraco's actual template engine — reviewers see the real page |
| Secure Token Links            | Cryptographically random URLs with 128-bit entropy                             |
| Time-Limited Expiration       | Links expire after a configurable duration (minimum 15 minutes)                |
| Max Views Limit               | Auto-revoke links after a set number of views (1–10,000)                       |
| PIN Protection                | 6-digit PIN with rate limiting and lockout                                     |
| Audit Logging                 | Track all access events, failed PINs, revocations with IP and user-agent       |
| Link-Specific Audit Filtering | Filter audit logs by individual link, including deleted links                  |
| Configurable Branding         | Custom logo and colors on the PIN page                                         |
| Auto-Revocation               | Links revoked when content is deleted                                          |
| Soft Delete                   | Deleted links preserve audit trail for compliance                              |
| Draft/Published Badge         | Floating bar shows reviewers the content status                                |
| Workspace Tab                 | Full management UI in the content editor                                       |
| Proxy-Aware                   | Reads `X-Forwarded-For` for accurate IP tracking behind load balancers         |
| Culture Variant Support       | Share specific language versions of multilingual content                       |

## Screenshots

![Workspace Tab](https://raw.githubusercontent.com/ShekharTarare/uPreviewShare/main/screenshots/workspace-tab.png)
![Link Results](https://raw.githubusercontent.com/ShekharTarare/uPreviewShare/main/screenshots/results.png)
![Create Link Dialog](https://raw.githubusercontent.com/ShekharTarare/uPreviewShare/main/screenshots/create-link.png)
![Audit Log](https://raw.githubusercontent.com/ShekharTarare/uPreviewShare/main/screenshots/audit-log.png)
![Branding Page](https://raw.githubusercontent.com/ShekharTarare/uPreviewShare/main/screenshots/branding-page.png)
![Preview Page](https://raw.githubusercontent.com/ShekharTarare/uPreviewShare/main/screenshots/preview-page.png)
![PIN Page](https://raw.githubusercontent.com/ShekharTarare/uPreviewShare/main/screenshots/pin-page.png)
![Access Lock Page](https://raw.githubusercontent.com/ShekharTarare/uPreviewShare/main/screenshots/access-lock-page.png)

## Installation

### Version Compatibility

| uPreviewShare Version | Umbraco Version | Branch    |
| --------------------- | --------------- | --------- |
| 2.1.x                 | 18              | `main`    |
| 1.1.x                 | 17              | `v17/dev` |

### Umbraco 18

```bash
dotnet add package uPreviewShare --version 2.1.0
```

### Umbraco 17

```bash
dotnet add package uPreviewShare --version 1.1.0
```

The package automatically:

- Registers all services via Umbraco's Composer pattern
- Runs database migrations on first startup
- Adds the **uPreviewShare** workspace tab to content nodes

## Getting Started

After installing the package and restarting your Umbraco site:

1. Log in to the Umbraco backoffice
2. Navigate to any content node
3. Click the **uPreviewShare** tab in the content workspace
4. Click **Create Link** to generate a secure preview link
5. Share the URL with your external reviewer

## Configuration

Add to your `appsettings.json` (optional — defaults are provided):

```json
{
  "uPreviewShare": {
    "MaxPinAttempts": 5,
    "LockoutDurationMinutes": 15,
    "AttemptWindowMinutes": 15,
    "SessionDurationMinutes": 30,
    "CleanupIntervalMinutes": 5
  }
}
```

| Setting                  | Default | Description                                               |
| ------------------------ | ------- | --------------------------------------------------------- |
| `MaxPinAttempts`         | `5`     | Failed PIN attempts before IP lockout                     |
| `LockoutDurationMinutes` | `15`    | How long an IP is locked out after max attempts           |
| `AttemptWindowMinutes`   | `15`    | Sliding window for tracking failed attempts               |
| `SessionDurationMinutes` | `30`    | How long a PIN session cookie lasts                       |
| `CleanupIntervalMinutes` | `5`     | How often the background service checks for expired links |

### Branding

Customize the PIN page appearance from the **Branding** tab in the workspace:

- **Primary Color** — Buttons, badges, and accents
- **Background Color** — PIN page background
- **Text Color** — Text on primary-colored elements (auto-calculated if not set)
- **Logo** — Upload a PNG or SVG (max 500KB)

## How It Works

1. **Create a link** — Set expiration, max views, and optional PIN protection
2. **Share the URL** — Send the generated link to your external reviewer
3. **Reviewer accesses** — They see the draft content rendered through the actual template
4. **Floating status bar** — Shows "Draft Preview" or "Published" at the bottom of the page
5. **PIN gate** — If PIN-protected, reviewer must enter the PIN first
6. **Monitor access** — Check the Audit Log tab for all access events
7. **Revoke when done** — Manually revoke, or let auto-expiration handle it

### Preview Rendering

uPreviewShare uses Umbraco's `IPublishedContentCache` with `preview: true` to fetch draft content and renders it through the content's assigned template. This means:

- Reviewers see the exact same page as the frontend
- Works with any template complexity (Block Grid, Block List, custom components)
- Works for never-published content (drafts)
- Falls back to a property renderer if no template is assigned

## Requirements

- Umbraco 17.0+
- .NET 10.0+
- SQLite or SQL Server

## Security

- Tokens use `RandomNumberGenerator` for cryptographic randomness (128-bit entropy)
- PINs are stored as HMAC-SHA256 hashes (never plaintext)
- Rate limiting: configurable failed attempts → IP lockout
- Session cookies are encrypted via ASP.NET Core Data Protection
- Cookies are `HttpOnly`, `Secure`, `SameSite=Strict`, scoped to `/upreviewshare/`
- Audit logging is fail-secure (access denied if logging fails)
- All preview pages include `noindex, nofollow` meta tags
- Soft-delete preserves audit trail for compliance
- Proxy-aware IP detection via `X-Forwarded-For`
- Content-Security-Policy headers on all public pages

## Database Tables

uPreviewShare creates the following tables automatically via migrations:

- `uPreviewShare_Links` — Share link metadata (token, expiry, views, PIN hash, status)
- `uPreviewShare_AuditLog` — Access events, failed PINs, revocations
- `uPreviewShare_Branding` — Custom branding configuration

## API Endpoints

All management endpoints require backoffice authentication and are available under:
`/umbraco/management/api/v1/upreviewshare/`

| Route Prefix             | Description                                          |
| ------------------------ | ---------------------------------------------------- |
| `upreviewshare/links`    | Create, get, revoke, and delete preview links        |
| `upreviewshare/audit`    | Audit log queries with link and event type filtering |
| `upreviewshare/branding` | Branding configuration (colors, logo)                |

Public endpoints (no authentication):

| Route                                  | Description                                   |
| -------------------------------------- | --------------------------------------------- |
| `/upreviewshare/preview?token={token}` | View draft preview (renders through template) |
| `/upreviewshare/pin?token={token}`     | PIN entry page                                |
| `/upreviewshare/pin/verify`            | Verify PIN                                    |
| `/upreviewshare/logo/{filename}`       | Serve uploaded logo                           |

## Known Limitations

- Rate limiting uses in-process memory. In a load-balanced setup, rate limits are per-server (not shared across instances).
- No bulk sharing — each link is for a single content node.
- Template required for true preview — if no template is assigned, falls back to a basic property renderer.
- Cleanup is eventual — expired links are marked by a background service at a configurable interval.

## Documentation

- [Contributing](https://github.com/ShekharTarare/uPreviewShare/blob/main/CONTRIBUTING.md)

## License

MIT License — see [LICENSE](https://github.com/ShekharTarare/uPreviewShare/blob/main/LICENSE) for details.
