# uPreviewShare — Secure Draft Preview Sharing for Umbraco

<p align="center">
  <img src="https://raw.githubusercontent.com/ShekharTarare/uPreviewShare/main/src/uPreviewShare/icon.png" alt="uPreviewShare Logo" width="80" />
</p>

<p align="center">
  <a href="https://www.nuget.org/packages/uPreviewShare"><img src="https://img.shields.io/nuget/v/uPreviewShare.svg" alt="NuGet" /></a>
  <a href="https://www.nuget.org/packages/uPreviewShare"><img src="https://img.shields.io/nuget/dt/uPreviewShare.svg" alt="NuGet Downloads" /></a>
  <a href="https://marketplace.umbraco.com/package/upreviewshare"><img src="https://img.shields.io/badge/Umbraco-Marketplace-8B5CF6" alt="Umbraco Marketplace" /></a>
  <a href="https://github.com/ShekharTarare/uPreviewShare/blob/main/LICENSE"><img src="https://img.shields.io/badge/License-MIT-green.svg" alt="License: MIT" /></a>
  <a href="https://umbraco.com/"><img src="https://img.shields.io/badge/Umbraco-17%2B-8B5CF6" alt="Umbraco 17+" /></a>
  <a href="https://dotnet.microsoft.com/"><img src="https://img.shields.io/badge/.NET-10-512BD4" alt=".NET 10" /></a>
</p>

A Bellissima-native package for **Umbraco 17+** that enables content editors to securely share unpublished draft nodes with external stakeholders — clients, legal reviewers, freelancers — without requiring Umbraco backoffice accounts. Reviewers see the **exact page** as it will appear when published, rendered through the actual Umbraco template.

---

## Features

| Feature                           | Description                                                                                         |
| --------------------------------- | --------------------------------------------------------------------------------------------------- |
| **True Draft Preview**            | Renders through Umbraco's actual template engine — reviewers see the real page, not a property dump |
| **Secure Token Links**            | Cryptographically random URLs with 128-bit entropy                                                  |
| **Time-Limited Expiration**       | Links expire after a configurable duration (minimum 15 minutes)                                     |
| **Max Views Limit**               | Auto-revoke links after a set number of views (1–10,000)                                            |
| **PIN Protection**                | 6-digit PIN with rate limiting and lockout                                                          |
| **Audit Logging**                 | Track all access events, failed PINs, revocations with IP and user-agent                            |
| **Link-Specific Audit Filtering** | Filter audit logs by individual link, including deleted links                                       |
| **Configurable Branding**         | Custom logo, colors on the PIN page                                                                 |
| **Auto-Revocation**               | Links revoked when content is deleted                                                               |
| **Soft Delete**                   | Deleted links preserve audit trail for compliance                                                   |
| **Draft/Published Badge**         | Floating bar shows reviewers the content status                                                     |
| **Workspace Tab**                 | Full management UI in the content editor                                                            |
| **Proxy-Aware**                   | Reads `X-Forwarded-For` for accurate IP tracking behind load balancers                              |

---

## Screenshots

![Workspace Tab](https://raw.githubusercontent.com/ShekharTarare/uPreviewShare/main/screenshots/workspace-tab.png)
![Results](https://raw.githubusercontent.com/ShekharTarare/uPreviewShare/main/screenshots/results.png)
![Create Link Dialog](https://raw.githubusercontent.com/ShekharTarare/uPreviewShare/main/screenshots/create-link.png)
![Audit Log](https://raw.githubusercontent.com/ShekharTarare/uPreviewShare/main/screenshots/audit-log.png)
![Branding Page](https://raw.githubusercontent.com/ShekharTarare/uPreviewShare/main/screenshots/branding-page.png)
![Preview Page](https://raw.githubusercontent.com/ShekharTarare/uPreviewShare/main/screenshots/preview-page.png)
![PIN Page](https://raw.githubusercontent.com/ShekharTarare/uPreviewShare/main/screenshots/pin-page.png)
![Access Lock Page](https://raw.githubusercontent.com/ShekharTarare/uPreviewShare/main/screenshots/access-lock-page.png)

---

## Installation

```bash
dotnet add package uPreviewShare
```

The package automatically:

- Registers all services via Umbraco's Composer pattern
- Runs database migrations on first startup
- Adds the **uPreviewShare** workspace tab to content nodes

---

## Requirements

- Umbraco 17.0+
- .NET 10.0+

---

## Getting Started

1. Install the package via NuGet
2. Restart your Umbraco application
3. Navigate to any content node in the backoffice
4. Click the **uPreviewShare** tab in the content workspace
5. Click **Create Link** to generate a secure preview link
6. Share the URL with your external reviewer

---

## Configuration

uPreviewShare works out of the box with sensible defaults. All settings are optional.

Add to your `appsettings.json`:

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
| `MaxPinAttempts`         | 5       | Failed PIN attempts before IP lockout                     |
| `LockoutDurationMinutes` | 15      | How long an IP is locked out after max attempts           |
| `AttemptWindowMinutes`   | 15      | Sliding window for tracking failed attempts               |
| `SessionDurationMinutes` | 30      | How long a PIN session cookie lasts                       |
| `CleanupIntervalMinutes` | 5       | How often the background service checks for expired links |

### Branding

Customize the PIN page appearance from the **Branding** tab in the workspace:

- **Primary Color** — Buttons, badges, and accents
- **Background Color** — PIN page background
- **Text Color** — Text on primary-colored elements (auto-calculated if not set)
- **Logo** — Upload a PNG or SVG (max 500KB)

---

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

- ✅ Reviewers see the **exact same page** as the frontend
- ✅ Works with any template complexity (Block Grid, Block List, custom components)
- ✅ Works for never-published content (drafts)
- ✅ Falls back to a property renderer if no template is assigned

---

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

---

## Database Tables

uPreviewShare creates the following tables automatically via migrations:

- `uPreviewShare_Links` — Share link metadata (token, expiry, views, PIN hash, status)
- `uPreviewShare_AuditLog` — Access events, failed PINs, revocations
- `uPreviewShare_Branding` — Custom branding configuration

---

## API Endpoints

### Management API (requires backoffice authentication)

| Method | Endpoint                                                            | Description                                                   |
| ------ | ------------------------------------------------------------------- | ------------------------------------------------------------- |
| POST   | `/umbraco/management/api/v1/upreviewshare/links`                    | Create a new link                                             |
| GET    | `/umbraco/management/api/v1/upreviewshare/links/{nodeKey}`          | Get links for a node                                          |
| DELETE | `/umbraco/management/api/v1/upreviewshare/links/{linkId}`           | Revoke a link                                                 |
| DELETE | `/umbraco/management/api/v1/upreviewshare/links/{linkId}/permanent` | Soft-delete a link                                            |
| DELETE | `/umbraco/management/api/v1/upreviewshare/links/node/{nodeKey}`     | Revoke all links for a node                                   |
| GET    | `/umbraco/management/api/v1/upreviewshare/audit/{nodeKey}`          | Get audit log (supports `?linkId=` and `?eventType=` filters) |
| GET    | `/umbraco/management/api/v1/upreviewshare/audit/{nodeKey}/links`    | Get all links including deleted (for audit filter)            |
| GET    | `/umbraco/management/api/v1/upreviewshare/branding`                 | Get branding config                                           |
| PUT    | `/umbraco/management/api/v1/upreviewshare/branding`                 | Update branding colors                                        |
| DELETE | `/umbraco/management/api/v1/upreviewshare/branding`                 | Reset branding to defaults                                    |
| POST   | `/umbraco/management/api/v1/upreviewshare/branding/logo`            | Upload logo                                                   |

### Public Endpoints (no authentication required)

| Method | Endpoint                               | Description                                   |
| ------ | -------------------------------------- | --------------------------------------------- |
| GET    | `/upreviewshare/preview?token={token}` | View draft preview (renders through template) |
| GET    | `/upreviewshare/pin?token={token}`     | PIN entry page                                |
| POST   | `/upreviewshare/pin/verify`            | Verify PIN                                    |
| GET    | `/upreviewshare/logo/{filename}`       | Serve uploaded logo                           |

---

## Known Limitations

- **Single-instance only** — Rate limiting and cleanup use in-process memory. In a load-balanced setup, rate limits are per-server (not shared across instances).
- **No bulk sharing** — Each link is for a single content node. Multi-node sharing is not yet supported.
- **Template required for true preview** — If a content type has no template assigned, the preview falls back to a basic property renderer (not the actual page design).
- **No email notifications** — The package does not send emails when links are accessed. Monitor via the Audit Log tab.
- **Cleanup is eventual** — Expired links are marked as expired by a background service running at a configurable interval (default: 5 minutes). During that window, the link validation still catches expiry on access.

---

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.

---

## License

MIT License — see [LICENSE](LICENSE) for details.

---

## Author

**[Shekhar Tarare](https://github.com/ShekharTarare)** · [shekhartarare.dev](https://shekhartarare.dev)
