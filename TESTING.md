# uPreviewShare — Manual Testing Guide

## Prerequisites

- Test site running: `dotnet run` from `tests/uPreviewShare.TestSite`
- Login: `admin@example.com` / `SuperSecret1234!`
- Create at least one content node with a template assigned
- Have an incognito/private browser window ready for testing public preview links

---

## 1. First Run / Migrations

| #   | Test                                                     | Expected Result                                                                          | Pass |
| --- | -------------------------------------------------------- | ---------------------------------------------------------------------------------------- | ---- |
| 1.1 | Start the site fresh (delete `umbraco/Data/*.db*` first) | Logs show "uPreviewShare migration plan executed successfully"                           | ☐    |
| 1.2 | Verify DB tables exist (use DB browser or check logs)    | `uPreviewShare_Links`, `uPreviewShare_AuditLog`, `uPreviewShare_Branding` tables created | ☐    |
| 1.3 | Restart the site again                                   | No migration errors, "Already at" message in logs                                        | ☐    |

---

## 2. Workspace Tab UI

| #   | Test                                 | Expected Result                                             | Pass |
| --- | ------------------------------------ | ----------------------------------------------------------- | ---- |
| 2.1 | Open any content node in backoffice  | "uPreviewShare" tab appears with the violet U-arrow logo    | ☐    |
| 2.2 | Click the uPreviewShare tab          | Shows stats bar (Total: 0, Active: 0, Expired: 0, Views: 0) | ☐    |
| 2.3 | Verify three sub-tabs exist          | "Links", "Audit Log", "Branding" tabs visible               | ☐    |
| 2.4 | Check empty state on Links tab       | Shows "No preview links found" message with icon            | ☐    |
| 2.5 | Check empty state on Audit tab       | Shows "No audit entries found" message                      | ☐    |
| 2.6 | Navigate to a different content node | Stats reset, data reloads for new node                      | ☐    |

---

## 3. Link Creation — Happy Path

| #   | Test                                                | Expected Result                                              | Pass |
| --- | --------------------------------------------------- | ------------------------------------------------------------ | ---- |
| 3.1 | Click "Create Link" → submit with no options        | Link created, appears in list with "Active" status           | ☐    |
| 3.2 | Create link with expiry = 1 hour from now           | Link shows expiry date and countdown (e.g., "59m remaining") | ☐    |
| 3.3 | Create link with max views = 3                      | Link shows "0 / 3" in views field                            | ☐    |
| 3.4 | Create link with PIN "123456"                       | Link shows "PIN Protected" in security field                 | ☐    |
| 3.5 | Create link with all options (expiry + views + PIN) | All three constraints shown on the card                      | ☐    |
| 3.6 | Click "Copy Link" button                            | URL copied to clipboard, button shows "Copied!" briefly      | ☐    |
| 3.7 | Stats bar updates                                   | Total and Active counts increment                            | ☐    |

---

## 4. Link Creation — Validation Errors

| #   | Test                                          | Expected Result                                               | Pass |
| --- | --------------------------------------------- | ------------------------------------------------------------- | ---- |
| 4.1 | Create with expiry < 15 minutes in the future | Error: "Expiration must be at least 15 minutes in the future" | ☐    |
| 4.2 | Create with expiry in the past                | Error: "Expiration must be at least 15 minutes in the future" | ☐    |
| 4.3 | Create with PIN = "12345" (5 digits)          | Error: "PIN must be exactly 6 digits"                         | ☐    |
| 4.4 | Create with PIN = "1234567" (7 digits)        | Error: "PIN must be exactly 6 digits"                         | ☐    |
| 4.5 | Create with PIN = "abcdef" (letters)          | Error: "PIN must be exactly 6 digits"                         | ☐    |
| 4.6 | Create with max views = 0                     | Error: "Max views must be between 1 and 10,000"               | ☐    |
| 4.7 | Create with max views = 10001                 | Error: "Max views must be between 1 and 10,000"               | ☐    |
| 4.8 | Create with max views = -1                    | Error: "Max views must be between 1 and 10,000"               | ☐    |

---

## 5. Preview — Template Rendering (Primary Path)

| #   | Test                                               | Expected Result                                                         | Pass |
| --- | -------------------------------------------------- | ----------------------------------------------------------------------- | ---- |
| 5.1 | Copy link, open in incognito browser               | Page renders through actual Umbraco template (matches frontend)         | ☐    |
| 5.2 | Check floating bar at bottom                       | Dark bar with "Draft Preview" (blue badge) or "Published" (green badge) | ☐    |
| 5.3 | Verify "Powered by uPreviewShare" link in bar      | Links to GitHub repo                                                    | ☐    |
| 5.4 | Save content without publishing, access preview    | Shows latest saved (draft) content                                      | ☐    |
| 5.5 | Publish content, access preview                    | Shows published content, badge says "Published"                         | ☐    |
| 5.6 | Edit published content (save only), access preview | Shows draft edits, badge says "Draft Preview"                           | ☐    |
| 5.7 | View page source                                   | `<meta name="robots" content="noindex, nofollow">` present              | ☐    |

---

## 6. Preview — Fallback Renderer (No Template)

| #   | Test                                                               | Expected Result                                         | Pass |
| --- | ------------------------------------------------------------------ | ------------------------------------------------------- | ---- |
| 6.1 | Create content node with no template, create a link                | Preview shows property-dump renderer with h1 title      | ☐    |
| 6.2 | Add HTML content to a property (e.g., `<script>alert(1)</script>`) | HTML is escaped/encoded, no script execution (XSS safe) | ☐    |
| 6.3 | Check CSP header on fallback page                                  | `Content-Security-Policy` header present in response    | ☐    |

---

## 7. Preview — Error Cases

| #   | Test                                  | Expected Result                      | Pass |
| --- | ------------------------------------- | ------------------------------------ | ---- |
| 7.1 | Access with invalid/random token      | Returns 404 (no information leakage) | ☐    |
| 7.2 | Access with empty token parameter     | Returns 404                          | ☐    |
| 7.3 | Access with no token parameter at all | Returns 404                          | ☐    |
| 7.4 | Access a revoked link's token         | Returns 404                          | ☐    |
| 7.5 | Access an expired link's token        | Returns 404                          | ☐    |
| 7.6 | Access a soft-deleted link's token    | Returns 404                          | ☐    |

---

## 8. PIN Protection

| #    | Test                                           | Expected Result                               | Pass |
| ---- | ---------------------------------------------- | --------------------------------------------- | ---- |
| 8.1  | Access a PIN-protected link in incognito       | Shows PIN entry page (not content)            | ☐    |
| 8.2  | PIN page shows branding (if configured)        | Logo and colors applied                       | ☐    |
| 8.3  | Enter correct PIN "123456"                     | Redirects to preview, content visible         | ☐    |
| 8.4  | Refresh page after correct PIN                 | Still shows preview (session cookie active)   | ☐    |
| 8.5  | Enter wrong PIN                                | Error: "The PIN you entered is incorrect"     | ☐    |
| 8.6  | Shows remaining attempts                       | "4 attempts remaining" (or configured value)  | ☐    |
| 8.7  | Enter wrong PIN 5 times (default)              | Lockout page: "Too many failed attempts"      | ☐    |
| 8.8  | Check `Retry-After` header on lockout response | Header present with seconds value (e.g., 900) | ☐    |
| 8.9  | Wait for lockout to expire, try again          | PIN page accessible again                     | ☐    |
| 8.10 | After lockout expires, enter correct PIN       | Access granted                                | ☐    |

---

## 9. Max Views Limit

| #   | Test                                         | Expected Result                                                     | Pass |
| --- | -------------------------------------------- | ------------------------------------------------------------------- | ---- |
| 9.1 | Create link with max views = 2, access once  | Content shown, view count = 1 in workspace                          | ☐    |
| 9.2 | Access second time                           | Content shown, view count = 2 in workspace                          | ☐    |
| 9.3 | Access third time                            | Returns 404 (link exhausted)                                        | ☐    |
| 9.4 | Check workspace                              | Link still shows as "Active" with 2/2 views (or auto-revoked)       | ☐    |
| 9.5 | Max views with PIN: create link (max=1, PIN) | PIN entry → correct PIN → content shown → second access returns 404 | ☐    |

---

## 10. Time Expiration

| #    | Test                                    | Expected Result                                      | Pass |
| ---- | --------------------------------------- | ---------------------------------------------------- | ---- |
| 10.1 | Create link with 15-min expiry          | Countdown shows in workspace (e.g., "14m remaining") | ☐    |
| 10.2 | Access before expiry                    | Content shown                                        | ☐    |
| 10.3 | Wait for expiry (or manually update DB) | Access returns 404                                   | ☐    |
| 10.4 | Check workspace after cleanup runs      | Link status changes to "Expired"                     | ☐    |

---

## 11. Link Revocation

| #    | Test                             | Expected Result                                             | Pass |
| ---- | -------------------------------- | ----------------------------------------------------------- | ---- |
| 11.1 | Click "Revoke" on an active link | Confirm dialog: "This will revoke this preview link"        | ☐    |
| 11.2 | Confirm revoke                   | Link status → "Revoked", success notification               | ☐    |
| 11.3 | Access revoked link              | Returns 404                                                 | ☐    |
| 11.4 | Click "Revoke All" button        | Confirm dialog: "This will revoke ALL active preview links" | ☐    |
| 11.5 | Confirm revoke all               | All active links revoked, count shown in notification       | ☐    |

---

## 12. Soft Delete

| #    | Test                                | Expected Result                                                      | Pass |
| ---- | ----------------------------------- | -------------------------------------------------------------------- | ---- |
| 12.1 | Click trash icon on any link        | Confirm dialog: "Audit log entries will be preserved for compliance" | ☐    |
| 12.2 | Confirm delete                      | Link disappears from links list                                      | ☐    |
| 12.3 | Check audit log tab                 | Logs for deleted link still visible                                  | ☐    |
| 12.4 | Check link filter dropdown in audit | Deleted link shows with "(Deleted)" suffix                           | ☐    |
| 12.5 | Filter audit by the deleted link    | Only that link's events shown                                        | ☐    |
| 12.6 | Access deleted link's token         | Returns 404                                                          | ☐    |

---

## 13. Audit Log

| #     | Test                                       | Expected Result                                         | Pass |
| ----- | ------------------------------------------ | ------------------------------------------------------- | ---- |
| 13.1  | Access a preview link, check audit tab     | "Access" entry with timestamp, IP, user-agent           | ☐    |
| 13.2  | Fail a PIN attempt, check audit            | "Failed PIN" entry                                      | ☐    |
| 13.3  | Trigger lockout, check audit               | "Lockout" entry                                         | ☐    |
| 13.4  | Revoke a link, check audit                 | "Revocation" entry with "RevokedBy:{userId}" in details | ☐    |
| 13.5  | Filter by "Access" event type              | Only access events shown                                | ☐    |
| 13.6  | Filter by "Failed PIN" event type          | Only failed PIN events shown                            | ☐    |
| 13.7  | Filter by specific link (dropdown)         | Only that link's events shown                           | ☐    |
| 13.8  | Click "Logs" button on a link card         | Switches to audit tab, pre-filtered to that link        | ☐    |
| 13.9  | Click "✕ Clear Link Filter" button         | Filter removed, all events shown                        | ☐    |
| 13.10 | Navigate to different node                 | Audit filter resets                                     | ☐    |
| 13.11 | Create 51+ audit entries, check pagination | "Page 1 of 2", Next/Previous buttons work               | ☐    |
| 13.12 | Expand an audit card (click it)            | Shows Link ID and full user-agent                       | ☐    |

---

## 14. Branding

| #     | Test                                    | Expected Result                              | Pass |
| ----- | --------------------------------------- | -------------------------------------------- | ---- |
| 14.1  | Go to Branding tab                      | Shows color pickers and logo upload area     | ☐    |
| 14.2  | Set primary color to `#FF0000` (red)    | Preview in branding panel updates            | ☐    |
| 14.3  | Save, then access a PIN-protected link  | PIN page uses red for submit button          | ☐    |
| 14.4  | Set background color to `#000000`       | PIN page has black background                | ☐    |
| 14.5  | Upload PNG logo (< 500KB, < 1000x500px) | Logo appears in preview and on PIN page      | ☐    |
| 14.6  | Upload SVG logo                         | Logo appears, check response headers for CSP | ☐    |
| 14.7  | Upload file > 500KB                     | Error: file size exceeds maximum             | ☐    |
| 14.8  | Upload .jpg file                        | Error: only PNG/SVG accepted                 | ☐    |
| 14.9  | Upload PNG > 1000x500px                 | Error: dimensions exceed maximum             | ☐    |
| 14.10 | Enter invalid hex color "red"           | Validation error                             | ☐    |
| 14.11 | Enter invalid hex "#GGG"                | Validation error                             | ☐    |
| 14.12 | Click "Reset to Defaults"               | Confirm dialog, then all branding cleared    | ☐    |
| 14.13 | Access PIN page after reset             | Default violet branding applied              | ☐    |

---

## 15. Auto-Revocation on Content Events

| #    | Test                                                 | Expected Result                                                | Pass |
| ---- | ---------------------------------------------------- | -------------------------------------------------------------- | ---- |
| 15.1 | Create a link for a draft node, then delete the node | Link auto-revoked (verify in DB or recreate node)              | ☐    |
| 15.2 | Create a link, then publish the content              | Check if link is still active (current behavior: stays active) | ☐    |

---

## 16. Configuration Options (appsettings.json)

Test site config (`tests/uPreviewShare.TestSite/appsettings.json`) should have:

```json
{
  "uPreviewShare": {
    "MaxPinAttempts": 2,
    "LockoutDurationMinutes": 1,
    "AttemptWindowMinutes": 1,
    "SessionDurationMinutes": 1,
    "CleanupIntervalMinutes": 1
  }
}
```

### Verify config is loaded

| #    | Test               | Expected Result                                               | Pass |
| ---- | ------------------ | ------------------------------------------------------------- | ---- |
| 16.1 | Check startup logs | "ExpiredLinkCleanupService started. Running every 1 minutes." | ☐    |

### MaxPinAttempts: 2

| #    | Test                                             | Expected Result                                   | Pass |
| ---- | ------------------------------------------------ | ------------------------------------------------- | ---- |
| 16.2 | Create link with PIN "123456", open in incognito | PIN page shown                                    | ☐    |
| 16.3 | Enter wrong PIN "000000"                         | Error shown, "1 attempt remaining"                | ☐    |
| 16.4 | Enter wrong PIN again "111111"                   | Lockout page shown (after only 2 attempts, not 5) | ☐    |

### LockoutDurationMinutes: 1

| #    | Test                                       | Expected Result                              | Pass |
| ---- | ------------------------------------------ | -------------------------------------------- | ---- |
| 16.5 | After lockout, check timer on lockout page | Shows ~1:00 countdown (not 15:00)            | ☐    |
| 16.6 | Wait 1 minute, refresh the page            | PIN entry accessible again (lockout expired) | ☐    |

### AttemptWindowMinutes: 1

| #    | Test                                        | Expected Result                                                       | Pass |
| ---- | ------------------------------------------- | --------------------------------------------------------------------- | ---- |
| 16.7 | Enter wrong PIN once (1 of 2 attempts used) | Shows "1 attempt remaining"                                           | ☐    |
| 16.8 | Wait 1+ minute without entering anything    | —                                                                     | ☐    |
| 16.9 | Enter wrong PIN again                       | Shows "1 attempt remaining" again (first attempt expired from window) | ☐    |

### SessionDurationMinutes: 1

| #     | Test                                    | Expected Result                              | Pass |
| ----- | --------------------------------------- | -------------------------------------------- | ---- |
| 16.10 | Create link with PIN, enter correct PIN | Preview shown                                | ☐    |
| 16.11 | Refresh immediately                     | Preview still shown (session active)         | ☐    |
| 16.12 | Wait 1+ minute, then refresh            | Redirects back to PIN page (session expired) | ☐    |
| 16.13 | Enter correct PIN again                 | Preview shown again (new session)            | ☐    |

### CleanupIntervalMinutes: 1

| #     | Test                                                                                                                     | Expected Result                                | Pass |
| ----- | ------------------------------------------------------------------------------------------------------------------------ | ---------------------------------------------- | ---- |
| 16.14 | Create link with 15-min expiry                                                                                           | Link shows as "Active"                         | ☐    |
| 16.15 | Manually update DB: `UPDATE uPreviewShare_Links SET ExpiresAt = datetime('now', '-1 minute') WHERE Token = 'your-token'` | —                                              | ☐    |
| 16.16 | Wait ~1 minute, check console logs                                                                                       | "Cleanup completed: 1 links marked as expired" | ☐    |
| 16.17 | Refresh workspace                                                                                                        | Link status changed to "Expired"               | ☐    |

### Defaults (no config)

| #     | Test                                                            | Expected Result                             | Pass |
| ----- | --------------------------------------------------------------- | ------------------------------------------- | ---- |
| 16.18 | Remove entire `uPreviewShare` section from appsettings, restart | Startup log shows "Running every 5 minutes" | ☐    |
| 16.19 | Fail PIN 4 times                                                | Still not locked out (default is 5)         | ☐    |
| 16.20 | Fail PIN 5th time                                               | Lockout page with ~15:00 timer              | ☐    |

---

## 17. Security Tests

| #     | Test                                                                 | Expected Result                                                          | Pass |
| ----- | -------------------------------------------------------------------- | ------------------------------------------------------------------------ | ---- |
| 17.1  | Call management API without auth (e.g., curl without cookie)         | 401 Unauthorized                                                         | ☐    |
| 17.2  | Path traversal on logo: `/upreviewshare/logo/../../appsettings.json` | 404 (path sanitized)                                                     | ☐    |
| 17.3  | Path traversal: `/upreviewshare/logo/..%2F..%2Fappsettings.json`     | 404                                                                      | ☐    |
| 17.4  | Check preview page response headers                                  | `Content-Security-Policy` header present                                 | ☐    |
| 17.5  | Check PIN page response headers                                      | `Content-Security-Policy` header present                                 | ☐    |
| 17.6  | Check SVG logo response headers                                      | `Content-Security-Policy: default-src 'none'; style-src 'unsafe-inline'` | ☐    |
| 17.7  | Send request with `X-Forwarded-For: 1.2.3.4`                         | Audit log shows `1.2.3.4` as IP                                          | ☐    |
| 17.8  | Send request with `X-Forwarded-For: 1.2.3.4, 5.6.7.8`                | Audit log shows `1.2.3.4` (first IP only)                                | ☐    |
| 17.9  | Verify PIN is not in any API response                                | GET links endpoint shows `hasPin: true` but no PIN value                 | ☐    |
| 17.10 | Check cookie settings on PIN session                                 | HttpOnly, Secure, SameSite=Strict, Path=/upreviewshare/                  | ☐    |

---

## 18. Edge Cases

| #    | Test                                                         | Expected Result                                             | Pass |
| ---- | ------------------------------------------------------------ | ----------------------------------------------------------- | ---- |
| 18.1 | Create link for node, then create another link for same node | Both appear in list, both work independently                | ☐    |
| 18.2 | Very long user-agent (> 512 chars)                           | Truncated to 512 in audit log, no error                     | ☐    |
| 18.3 | Unicode content in node properties                           | Renders correctly in fallback renderer                      | ☐    |
| 18.4 | Access preview link from mobile device                       | Page responsive, floating bar visible                       | ☐    |
| 18.5 | Multiple rapid clicks on "Create Link"                       | Only one link created (or graceful handling)                | ☐    |
| 18.6 | Open two browser tabs with same PIN-protected link           | Both can enter PIN independently                            | ☐    |
| 18.7 | Token with special URL characters                            | Token uses URL-safe base64 (no +, /, =)                     | ☐    |
| 18.8 | Node with no properties at all                               | Fallback renderer shows "no properties with values" message | ☐    |

---

## 19. Performance / Load

| #    | Test                                   | Expected Result                                                  | Pass |
| ---- | -------------------------------------- | ---------------------------------------------------------------- | ---- |
| 19.1 | Create 20+ links for one node          | All display correctly, pagination works at 12 per page           | ☐    |
| 19.2 | Access same link rapidly (10 requests) | View count increments correctly, no errors                       | ☐    |
| 19.3 | Check that token validation uses cache | Second access to same token is faster (check logs for cache hit) | ☐    |

---

## 20. NuGet Package

| #    | Test                                                                | Expected Result                                                       | Pass |
| ---- | ------------------------------------------------------------------- | --------------------------------------------------------------------- | ---- |
| 20.1 | Run `dotnet pack src/uPreviewShare/uPreviewShare.csproj -c Release` | `.nupkg` created in `bin/Release`                                     | ☐    |
| 20.2 | Inspect package contents (rename to .zip)                           | Contains: `App_Plugins/`, `buildTransitive/`, `icon.png`, `README.md` | ☐    |
| 20.3 | Install package in a fresh Umbraco 18 site                          | Migrations run, tab appears, everything works                         | ☐    |
| 20.4 | Verify App_Plugins copied to consuming project on build             | `App_Plugins/uPreviewShare/` folder appears in consuming project      | ☐    |

---

## Test Summary

| Section                       | Total Tests | Passed | Failed |
| ----------------------------- | ----------- | ------ | ------ |
| 1. Migrations                 | 3           |        |        |
| 2. Workspace Tab              | 6           |        |        |
| 3. Link Creation (Happy)      | 7           |        |        |
| 4. Link Creation (Validation) | 8           |        |        |
| 5. Preview (Template)         | 7           |        |        |
| 6. Preview (Fallback)         | 3           |        |        |
| 7. Preview (Errors)           | 6           |        |        |
| 8. PIN Protection             | 10          |        |        |
| 9. Max Views                  | 5           |        |        |
| 10. Expiration                | 4           |        |        |
| 11. Revocation                | 5           |        |        |
| 12. Soft Delete               | 6           |        |        |
| 13. Audit Log                 | 12          |        |        |
| 14. Branding                  | 13          |        |        |
| 15. Auto-Revocation           | 2           |        |        |
| 16. Configuration             | 6           |        |        |
| 17. Security                  | 10          |        |        |
| 18. Edge Cases                | 8           |        |        |
| 19. Performance               | 3           |        |        |
| 20. NuGet Package             | 4           |        |        |
| **TOTAL**                     | **128**     |        |        |
