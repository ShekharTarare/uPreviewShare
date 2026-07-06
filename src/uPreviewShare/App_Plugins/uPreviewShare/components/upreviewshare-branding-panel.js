import { LitElement, html, css } from '@umbraco-cms/backoffice/external/lit'
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api'
import { NotificationHelper } from '../notification-helper.js'

export class uPreviewShareBrandingPanel extends UmbElementMixin(LitElement) {
  static properties = {
    authHelper: { type: Object },
    nodeId: { type: String },
    _primaryColor: { state: true },
    _backgroundColor: { state: true },
    _textColor: { state: true },
    _autoTextColor: { state: true },
    _useAutoText: { state: true },
    _loading: { state: true },
    _saving: { state: true },
    _isCustom: { state: true },
    _logoPath: { state: true },
    _uploading: { state: true },
    _showResetConfirm: { state: true },
    _overrideEnabled: { state: true },
    _hasOverride: { state: true },
    _showOverrideDeleteConfirm: { state: true },
  }

  static styles = css`
    :host {
      display: block;
    }

    .override-section {
      margin-bottom: 20px;
      padding: 16px;
      background: var(--uui-color-surface-alt);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
    }

    .override-toggle-row {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .inheritance-indicator {
      margin-top: 10px;
      padding: 10px 14px;
      background: var(--uui-color-surface);
      border: 1px dashed var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      font-size: 0.8rem;
      color: var(--uui-color-text-alt);
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .branding-layout {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 24px;
      align-items: start;
    }

    @media (max-width: 768px) {
      .branding-layout {
        grid-template-columns: 1fr;
      }
    }

    .color-section {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    .color-field {
      display: flex;
      flex-direction: column;
      gap: 6px;
    }
    .color-field label {
      font-size: 0.8rem;
      font-weight: 600;
      color: var(--uui-color-text);
      text-transform: uppercase;
      letter-spacing: 0.3px;
    }
    .color-input-row {
      display: flex;
      gap: 8px;
      align-items: center;
    }
    .color-input-row input[type='color'] {
      width: 40px;
      height: 36px;
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      cursor: pointer;
      padding: 2px;
    }
    .color-input-row input[type='text'] {
      flex: 1;
      padding: 8px 12px;
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface);
      font-family: monospace;
      font-size: 0.85rem;
    }
    .color-input-row input[type='text']:focus {
      outline: none;
      border-color: var(--uui-color-interactive);
    }

    .preview-section {
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      overflow: hidden;
    }
    .preview-label {
      padding: 10px 14px;
      font-size: 0.75rem;
      font-weight: 600;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      color: var(--uui-color-text-alt);
      background: var(--uui-color-surface-alt);
      border-bottom: 1px solid var(--uui-color-border);
    }
    .preview-frame {
      padding: 24px;
      display: flex;
      align-items: center;
      justify-content: center;
      min-height: 180px;
      transition: background 0.3s;
      flex-direction: column;
    }
    .preview-card {
      width: 100%;
      max-width: 240px;
      border-radius: 8px;
      overflow: hidden;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    }
    .preview-card-header {
      padding: 16px;
      text-align: center;
      transition: background 0.3s;
    }
    .preview-card-header span {
      font-weight: 600;
      font-size: 0.85rem;
    }
    .preview-card-body {
      padding: 16px;
      background: #fff;
      text-align: center;
    }
    .preview-card-body .preview-input {
      width: 80%;
      height: 8px;
      background: #e5e7eb;
      border-radius: 4px;
      margin: 6px auto;
    }
    .preview-card-body .preview-btn {
      margin-top: 12px;
      padding: 6px 20px;
      border-radius: 4px;
      color: #fff;
      font-size: 0.75rem;
      font-weight: 600;
      display: inline-block;
      transition: background 0.3s;
    }

    .actions {
      display: flex;
      gap: 8px;
      margin-top: 20px;
    }

    .loading-state {
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 40px;
    }
  `

  constructor() {
    super()
    this.authHelper = null
    this.nodeId = null
    this._primaryColor = '#8B5CF6'
    this._backgroundColor = '#f8fafc'
    this._textColor = '#ffffff'
    this._autoTextColor = '#ffffff'
    this._useAutoText = true
    this._loading = true
    this._saving = false
    this._isCustom = false
    this._logoPath = null
    this._uploading = false
    this._showResetConfirm = false
    this._overrideEnabled = false
    this._hasOverride = false
    this._showOverrideDeleteConfirm = false
  }

  // W3C relative luminance formula for auto-contrast
  _getAutoTextColor(bgColor) {
    if (!bgColor || bgColor.length < 7) return '#1e293b'
    const r = parseInt(bgColor.slice(1, 3), 16) / 255
    const g = parseInt(bgColor.slice(3, 5), 16) / 255
    const b = parseInt(bgColor.slice(5, 7), 16) / 255
    const luminance = 0.2126 * r + 0.7152 * g + 0.0722 * b
    return luminance > 0.5 ? '#1e293b' : '#ffffff'
  }

  _updateAutoText() {
    this._autoTextColor = this._getAutoTextColor(this._primaryColor)
    if (this._useAutoText) {
      this._textColor = this._autoTextColor
    }
  }

  connectedCallback() {
    super.connectedCallback()
    this._loadBranding()
  }

  async _loadBranding(isInitialLoad = true) {
    if (!this.authHelper) return
    if (isInitialLoad) this._loading = true
    try {
      await this.authHelper.initialize()
      // If we have a nodeId, check if a per-page override exists
      const url = this.nodeId
        ? `/umbraco/management/api/v1/upreviewshare/branding?nodeKey=${this.nodeId}`
        : '/umbraco/management/api/v1/upreviewshare/branding'
      const response = await this.authHelper.makeAuthenticatedRequest(url)
      if (response.ok) {
        const data = await response.json()
        this._primaryColor = data.primaryColor || '#8B5CF6'
        this._backgroundColor = data.backgroundColor || '#f8fafc'
        this._textColor = data.textColor || '#ffffff'
        this._isCustom = data.isCustom || false
        this._logoPath = data.logoPath || null
        // Only set override toggle state on initial load (not when user explicitly toggles)
        if (isInitialLoad) {
          if (this.nodeId && data.isOverride) {
            this._overrideEnabled = true
            this._hasOverride = true
          } else {
            this._overrideEnabled = false
            this._hasOverride = false
          }
        } else {
          // When reloading after user action, only update _hasOverride
          this._hasOverride = !!(this.nodeId && data.isOverride)
        }
        // If textColor was explicitly saved, don't auto-calculate
        if (data.textColor) {
          this._useAutoText = false
        } else {
          this._useAutoText = true
          this._updateAutoText()
        }
      } else if (response.status === 404) {
        // Content not yet saved or not found — use defaults silently
      } else {
        NotificationHelper.showError(this, 'Failed to load branding settings')
      }
    } catch (e) {
      NotificationHelper.showError(this, 'Network error loading branding')
    }
    if (isInitialLoad) this._loading = false
  }

  async _onToggleOverride(e) {
    const enabled = e.target.checked
    if (!enabled && this._hasOverride) {
      // Show confirmation before deleting override
      this._showOverrideDeleteConfirm = true
      // Revert the toggle visually until confirmed
      e.target.checked = true
      return
    }
    this._overrideEnabled = enabled
    if (!enabled) {
      // Toggling off without a saved override — reload global branding to reset any unsaved changes
      await this._loadBranding(false)
    }
  }

  _closeOverrideDeleteConfirm() {
    this._showOverrideDeleteConfirm = false
  }

  async _confirmDeleteOverride() {
    this._showOverrideDeleteConfirm = false
    if (!this.authHelper || !this.nodeId) return
    try {
      const response = await this.authHelper.makeAuthenticatedRequest(
        `/umbraco/management/api/v1/upreviewshare/branding?nodeKey=${this.nodeId}`,
        { method: 'DELETE' },
      )
      if (response.ok) {
        this._overrideEnabled = false
        this._hasOverride = false
        // Reload global branding to display
        await this._loadBranding(false)
        NotificationHelper.showSuccess(this, 'Page branding override removed')
      } else {
        NotificationHelper.showError(this, 'Failed to remove page override')
      }
    } catch (e) {
      NotificationHelper.showError(this, 'Network error removing override')
    }
  }

  _onPrimaryColorPicker(e) {
    this._primaryColor = e.target.value
    this._updateAutoText()
  }

  _onPrimaryColorText(e) {
    const val = e.target.value
    if (/^#[0-9a-fA-F]{6}$/.test(val)) {
      this._primaryColor = val
      this._updateAutoText()
    }
  }

  _onBgColorPicker(e) {
    this._backgroundColor = e.target.value
    this._updateAutoText()
  }

  _onBgColorText(e) {
    const val = e.target.value
    if (/^#[0-9a-fA-F]{6}$/.test(val)) {
      this._backgroundColor = val
      this._updateAutoText()
    }
  }

  async _save() {
    if (!this.authHelper) return
    this._saving = true
    try {
      const baseUrl = '/umbraco/management/api/v1/upreviewshare/branding'
      const url =
        this._overrideEnabled && this.nodeId
          ? `${baseUrl}?nodeKey=${this.nodeId}`
          : baseUrl
      const response = await this.authHelper.makeAuthenticatedRequest(url, {
        method: 'PUT',
        body: JSON.stringify({
          primaryColor: this._primaryColor,
          backgroundColor: this._backgroundColor,
          textColor: this._textColor,
        }),
      })
      if (response.ok) {
        this._isCustom = true
        if (this._overrideEnabled && this.nodeId) {
          this._hasOverride = true
        }
        NotificationHelper.showSuccess(this, 'Branding saved successfully')
      } else {
        NotificationHelper.showError(this, 'Failed to save branding')
      }
    } catch (e) {
      NotificationHelper.showError(this, 'Network error while saving branding')
    }
    this._saving = false
  }

  async _reset() {
    this._showResetConfirm = true
  }

  _closeResetConfirm() {
    this._showResetConfirm = false
  }

  async _confirmReset() {
    this._showResetConfirm = false
    try {
      const baseUrl = '/umbraco/management/api/v1/upreviewshare/branding'
      const url =
        this._overrideEnabled && this.nodeId
          ? `${baseUrl}?nodeKey=${this.nodeId}`
          : baseUrl
      const response = await this.authHelper.makeAuthenticatedRequest(url, {
        method: 'DELETE',
      })
      if (response.ok) {
        this._primaryColor = '#8B5CF6'
        this._backgroundColor = '#f8fafc'
        this._textColor = '#ffffff'
        this._useAutoText = true
        this._isCustom = false
        this._logoPath = null
        if (this._overrideEnabled && this.nodeId) {
          this._hasOverride = false
          this._overrideEnabled = false
          // Reload to show global defaults
          await this._loadBranding(false)
        }
        NotificationHelper.showSuccess(this, 'Branding reset to defaults')
      } else {
        NotificationHelper.showError(this, 'Failed to reset branding')
      }
    } catch (e) {
      NotificationHelper.showError(
        this,
        'Network error while resetting branding',
      )
    }
  }

  async _uploadLogo(e) {
    const file = e.target.files[0]
    if (!file) return
    if (file.size > 512000) {
      NotificationHelper.showError(this, 'Logo must be under 500KB')
      return
    }
    const ext = file.name.split('.').pop().toLowerCase()
    if (!['png', 'svg'].includes(ext)) {
      NotificationHelper.showError(this, 'Only PNG and SVG files are accepted')
      return
    }
    this._uploading = true
    try {
      const formData = new FormData()
      formData.append('file', file)
      const token = await this.authHelper.getToken()
      const baseUrl = '/umbraco/management/api/v1/upreviewshare/branding/logo'
      const url =
        this._overrideEnabled && this.nodeId
          ? `${baseUrl}?nodeKey=${this.nodeId}`
          : baseUrl
      const response = await fetch(url, {
        method: 'POST',
        headers: { Authorization: 'Bearer ' + token },
        body: formData,
      })
      if (response.ok) {
        const data = await response.json()
        this._logoPath = data.logoPath
        this._isCustom = true
        if (this._overrideEnabled && this.nodeId) {
          this._hasOverride = true
        }
        NotificationHelper.showSuccess(this, 'Logo uploaded successfully')
      } else {
        const text = await response.text()
        NotificationHelper.showError(this, text || 'Failed to upload logo')
      }
    } catch (e) {
      NotificationHelper.showError(this, 'Network error uploading logo')
    }
    this._uploading = false
    if (e.target) e.target.value = ''
  }

  render() {
    if (this._loading) {
      return html`<div class="loading-state"><uui-loader></uui-loader></div>`
    }

    return html`
      ${this._renderOverrideSection()} ${this._renderBrandingEditor()}
      ${this._renderResetConfirmDialog()}
      ${this._renderOverrideDeleteConfirmDialog()}
    `
  }

  _renderOverrideSection() {
    if (!this.nodeId) return ''
    return html`
      <div class="override-section">
        <div class="override-toggle-row">
          <uui-toggle
            label="Override branding for this page"
            ?checked=${this._overrideEnabled}
            @change=${this._onToggleOverride}
          ></uui-toggle>
        </div>
        ${!this._overrideEnabled
          ? html`<div class="inheritance-indicator">
              <uui-icon name="icon-link"></uui-icon>
              <span>Using global branding defaults</span>
            </div>`
          : ''}
      </div>
    `
  }

  _renderBrandingEditor() {
    return html`
      <div class="branding-layout">
        <div class="color-section">
          <div class="color-field">
            <label>Primary Color</label>
            <div class="color-input-row">
              <input
                type="color"
                .value=${this._primaryColor}
                @input=${this._onPrimaryColorPicker}
              />
              <input
                type="text"
                .value=${this._primaryColor}
                @change=${this._onPrimaryColorText}
                placeholder="#8B5CF6"
              />
            </div>
          </div>

          <div class="color-field">
            <label>Background Color</label>
            <div class="color-input-row">
              <input
                type="color"
                .value=${this._backgroundColor}
                @input=${this._onBgColorPicker}
              />
              <input
                type="text"
                .value=${this._backgroundColor}
                @change=${this._onBgColorText}
                placeholder="#f8fafc"
              />
            </div>
          </div>

          <div class="color-field">
            <label
              >Text Color
              <span
                style="font-weight:400;font-size:0.7rem;color:var(--uui-color-text-alt)"
                >(for text on primary color)</span
              ></label
            >
            <div class="color-input-row">
              <input
                type="color"
                .value=${this._textColor}
                @input=${(e) => {
                  this._textColor = e.target.value
                  this._useAutoText = false
                }}
              />
              <input
                type="text"
                .value=${this._textColor}
                @change=${(e) => {
                  if (/^#[0-9a-fA-F]{6}$/.test(e.target.value)) {
                    this._textColor = e.target.value
                    this._useAutoText = false
                  }
                }}
                placeholder="#1e293b"
              />
              <button
                style="padding:4px 8px;border:1px solid var(--uui-color-border);border-radius:var(--uui-border-radius);background:var(--uui-color-surface);cursor:pointer;font-size:0.7rem;"
                @click=${() => {
                  this._useAutoText = true
                  this._updateAutoText()
                }}
              >
                Auto
              </button>
            </div>
          </div>

          <div class="color-field">
            <label
              >Logo
              <span
                style="font-weight:400;font-size:0.7rem;color:var(--uui-color-text-alt)"
                >(PNG or SVG, max 500KB)</span
              ></label
            >
            <div class="color-input-row">
              ${this._logoPath
                ? html`<img
                    src="/${this._logoPath}"
                    alt="Logo"
                    style="max-height:40px;max-width:160px;object-fit:contain;border:1px solid var(--uui-color-border);border-radius:var(--uui-border-radius);padding:4px;"
                  />`
                : html`<span
                    style="font-size:0.8rem;color:var(--uui-color-text-alt);"
                    >No logo uploaded</span
                  >`}
              <label
                style="padding:6px 12px;border:1px solid var(--uui-color-border);border-radius:var(--uui-border-radius);background:var(--uui-color-surface);cursor:pointer;font-size:0.8rem;"
              >
                ${this._uploading ? 'Uploading...' : 'Upload'}
                <input
                  type="file"
                  accept=".png,.svg"
                  style="display:none;"
                  @change=${this._uploadLogo}
                  ?disabled=${this._uploading}
                />
              </label>
            </div>
          </div>

          <div class="actions">
            <uui-button
              look="primary"
              label="Save"
              @click=${this._save}
              ?disabled=${this._saving}
            >
              ${this._saving ? 'Saving...' : 'Save'}
            </uui-button>
            <uui-button
              look="secondary"
              label="Reset"
              @click=${this._reset}
              ?disabled=${!this._isCustom}
            >
              Reset to Default
            </uui-button>
          </div>
        </div>

        <div class="preview-section">
          <div class="preview-label">Live Preview</div>
          <div
            class="preview-frame"
            style="background: ${this
              ._backgroundColor}; flex-direction: column; padding: 0; min-height: 280px;"
          >
            <div
              style="background: #fff; border-bottom: 1px solid #e2e8f0; padding: 8px 16px; text-align: center; width: 100%;"
            >
              ${this._logoPath
                ? html`<img
                    src="/${this._logoPath}"
                    alt="Logo"
                    style="max-height:28px;max-width:120px;object-fit:contain;"
                  />`
                : html`<span
                    style="font-size:0.8rem;font-weight:600;color:${this
                      ._primaryColor};"
                    >uPreviewShare</span
                  >`}
            </div>
            <div
              style="flex:1;display:flex;align-items:center;justify-content:center;padding:16px;width:100%;"
            >
              <div
                style="background:#fff;border-radius:10px;box-shadow:0 2px 8px rgba(0,0,0,0.08);padding:20px 16px;width:100%;max-width:200px;text-align:center;"
              >
                <div
                  style="width:32px;height:32px;background:${this
                    ._primaryColor};border-radius:50%;display:flex;align-items:center;justify-content:center;margin:0 auto 10px;"
                >
                  <svg
                    xmlns="http://www.w3.org/2000/svg"
                    viewBox="0 0 24 24"
                    style="width:16px;height:16px;fill:${this._textColor};"
                  >
                    <path
                      d="M18 8h-1V6c0-2.76-2.24-5-5-5S7 3.24 7 6v2H6c-1.1 0-2 .9-2 2v10c0 1.1.9 2 2 2h12c1.1 0 2-.9 2-2V10c0-1.1-.9-2-2-2zm-6 9c-1.1 0-2-.9-2-2s.9-2 2-2 2 .9 2 2-.9 2-2 2zm3.1-9H8.9V6c0-1.71 1.39-3.1 3.1-3.1s3.1 1.39 3.1 3.1v2z"
                    />
                  </svg>
                </div>
                <div
                  style="font-size:0.85rem;font-weight:700;color:#1e293b;margin-bottom:4px;"
                >
                  Enter PIN
                </div>
                <div style="font-size:0.6rem;color:#64748b;margin-bottom:10px;">
                  Enter the 6-digit PIN
                </div>
                <div
                  style="width:100%;height:24px;border:1.5px solid #e2e8f0;border-radius:4px;margin-bottom:10px;display:flex;align-items:center;justify-content:center;"
                >
                  <span
                    style="font-size:0.7rem;color:#94a3b8;letter-spacing:3px;"
                    >......</span
                  >
                </div>
                <div
                  style="width:100%;padding:6px;background:${this
                    ._primaryColor};color:${this
                    ._textColor};border-radius:4px;font-size:0.65rem;font-weight:600;text-align:center;"
                >
                  Verify PIN
                </div>
              </div>
            </div>
            <div
              style="background:#fff;border-top:1px solid #e2e8f0;padding:6px;text-align:center;width:100%;font-size:0.55rem;color:#64748b;"
            >
              Powered by
              <span style="color:${this._primaryColor};">uPreviewShare</span>
            </div>
          </div>
        </div>
      </div>
    `
  }

  _renderResetConfirmDialog() {
    if (!this._showResetConfirm) return ''
    return html`
      <div
        style="position:fixed;top:0;left:0;right:0;bottom:0;background:rgba(0,0,0,0.5);display:flex;align-items:center;justify-content:center;z-index:10000;"
        @click=${this._closeResetConfirm}
      >
        <div
          style="background:var(--uui-color-surface);padding:28px;border-radius:var(--uui-border-radius);max-width:420px;width:90%;box-shadow:0 8px 24px rgba(0,0,0,0.2);"
          @click=${(e) => e.stopPropagation()}
        >
          <h3 style="margin:0 0 12px 0;font-size:1.1rem;">Reset Branding</h3>
          <p
            style="margin:0 0 8px 0;font-size:0.9rem;color:var(--uui-color-text-alt);"
          >
            This will reset all branding settings (colors and logo) to defaults.
            The preview pages will use default uPreviewShare branding.
          </p>
          <p style="color:#991b1b;font-weight:500;font-size:0.85rem;">
            This action cannot be undone.
          </p>
          <div
            style="display:flex;gap:8px;justify-content:flex-end;margin-top:20px;"
          >
            <uui-button
              look="secondary"
              label="Cancel"
              @click=${this._closeResetConfirm}
              >Cancel</uui-button
            >
            <uui-button
              look="primary"
              color="danger"
              label="Reset"
              @click=${this._confirmReset}
              >Reset</uui-button
            >
          </div>
        </div>
      </div>
    `
  }

  _renderOverrideDeleteConfirmDialog() {
    if (!this._showOverrideDeleteConfirm) return ''
    return html`
      <div
        style="position:fixed;top:0;left:0;right:0;bottom:0;background:rgba(0,0,0,0.5);display:flex;align-items:center;justify-content:center;z-index:10000;"
        @click=${this._closeOverrideDeleteConfirm}
      >
        <div
          style="background:var(--uui-color-surface);padding:28px;border-radius:var(--uui-border-radius);max-width:420px;width:90%;box-shadow:0 8px 24px rgba(0,0,0,0.2);"
          @click=${(e) => e.stopPropagation()}
        >
          <h3 style="margin:0 0 12px 0;font-size:1.1rem;">
            Remove Page Branding Override
          </h3>
          <p
            style="margin:0 0 8px 0;font-size:0.9rem;color:var(--uui-color-text-alt);"
          >
            Remove page branding override? This page will revert to using global
            branding defaults.
          </p>
          <p style="color:#991b1b;font-weight:500;font-size:0.85rem;">
            This action cannot be undone.
          </p>
          <div
            style="display:flex;gap:8px;justify-content:flex-end;margin-top:20px;"
          >
            <uui-button
              look="secondary"
              label="Cancel"
              @click=${this._closeOverrideDeleteConfirm}
              >Cancel</uui-button
            >
            <uui-button
              look="primary"
              color="danger"
              label="Remove Override"
              @click=${this._confirmDeleteOverride}
              >Remove Override</uui-button
            >
          </div>
        </div>
      </div>
    `
  }
}

customElements.define(
  'upreviewshare-branding-panel',
  uPreviewShareBrandingPanel,
)
