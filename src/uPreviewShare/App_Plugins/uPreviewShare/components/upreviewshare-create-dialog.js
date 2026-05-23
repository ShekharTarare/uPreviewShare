import { LitElement, html, css } from '@umbraco-cms/backoffice/external/lit'
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api'
import { NotificationHelper } from '../notification-helper.js'

export class uPreviewShareCreateDialog extends UmbElementMixin(LitElement) {
  static properties = {
    nodeId: { type: String },
    authHelper: { type: Object },
    _expiration: { state: true },
    _maxViews: { state: true },
    _pin: { state: true },
    _creating: { state: true },
    _success: { state: true },
    _createdUrl: { state: true },
    _copyFeedback: { state: true },
    _error: { state: true },
  }

  static styles = css`
    :host {
      display: block;
    }

    .overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(0, 0, 0, 0.5);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 9999;
    }

    .dialog {
      background: var(--uui-color-surface);
      border-radius: var(--uui-border-radius);
      box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
      width: 100%;
      max-width: 440px;
      overflow: hidden;
    }

    .dialog-header {
      padding: 18px 24px;
      border-bottom: 1px solid var(--uui-color-border);
      display: flex;
      align-items: center;
      justify-content: space-between;
    }
    .dialog-header h3 {
      margin: 0;
      font-size: 1.1rem;
      font-weight: 600;
    }
    .dialog-close {
      background: none;
      border: none;
      cursor: pointer;
      padding: 4px;
      color: var(--uui-color-text-alt);
      font-size: 1.2rem;
    }
    .dialog-close:hover {
      color: var(--uui-color-text);
    }

    .dialog-body {
      padding: 24px;
      display: flex;
      flex-direction: column;
      gap: 18px;
    }

    .form-field {
      display: flex;
      flex-direction: column;
      gap: 6px;
    }
    .form-field label {
      font-size: 0.8rem;
      font-weight: 600;
      color: var(--uui-color-text);
    }
    .form-field select,
    .form-field input {
      padding: 10px 12px;
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface);
      font-size: 0.85rem;
    }
    .form-field select:focus,
    .form-field input:focus {
      outline: none;
      border-color: var(--uui-color-interactive);
    }
    .form-field .hint {
      font-size: 0.75rem;
      color: var(--uui-color-text-alt);
    }

    .dialog-footer {
      padding: 16px 24px;
      border-top: 1px solid var(--uui-color-border);
      display: flex;
      justify-content: flex-end;
      gap: 8px;
      background: var(--uui-color-surface-alt);
    }

    .error-msg {
      padding: 10px 14px;
      background: #fef2f2;
      border: 1px solid #fca5a5;
      border-radius: var(--uui-border-radius);
      color: #991b1b;
      font-size: 0.85rem;
    }

    .success-state {
      text-align: center;
      padding: 8px 0;
    }
    .success-state uui-icon {
      font-size: 40px;
      color: #16a34a;
      margin-bottom: 12px;
    }
    .success-state p {
      margin: 4px 0;
      font-size: 0.9rem;
      color: var(--uui-color-text);
    }
    .success-url {
      margin-top: 12px;
      padding: 10px 12px;
      background: var(--uui-color-surface-alt);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      font-family: monospace;
      font-size: 0.75rem;
      word-break: break-all;
      text-align: left;
    }
    .success-copy-btn {
      margin-top: 12px;
      display: inline-flex;
      align-items: center;
      gap: 6px;
      padding: 8px 18px;
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface);
      font-size: 0.85rem;
      cursor: pointer;
      transition:
        background 0.2s,
        border-color 0.2s;
    }
    .success-copy-btn:hover {
      background: var(--uui-color-surface-alt);
      border-color: var(--uui-color-interactive);
    }
    .success-copy-btn.copied {
      background: #dcfce7;
      border-color: #166534;
      color: #166534;
    }
  `

  constructor() {
    super()
    this.nodeId = null
    this.authHelper = null
    this._expiration = '24h'
    this._maxViews = ''
    this._pin = ''
    this._creating = false
    this._success = false
    this._createdUrl = ''
    this._copyFeedback = false
    this._error = ''
  }

  _close() {
    this.dispatchEvent(
      new CustomEvent('close', { bubbles: true, composed: true }),
    )
  }

  _getExpirationDate() {
    if (this._expiration === 'none') return null
    const now = new Date()
    switch (this._expiration) {
      case '1h':
        return new Date(now.getTime() + 3600000).toISOString()
      case '6h':
        return new Date(now.getTime() + 21600000).toISOString()
      case '24h':
        return new Date(now.getTime() + 86400000).toISOString()
      case '7d':
        return new Date(now.getTime() + 604800000).toISOString()
      case '30d':
        return new Date(now.getTime() + 2592000000).toISOString()
      default:
        return new Date(now.getTime() + 86400000).toISOString()
    }
  }

  async _create() {
    if (!this.nodeId || !this.authHelper) return

    // Validate PIN if provided
    if (this._pin.trim()) {
      if (!/^\d{6}$/.test(this._pin.trim())) {
        this._error = 'PIN must be exactly 6 digits (0-9)'
        return
      }
    }

    // Validate max views if provided
    if (
      this._maxViews !== '' &&
      this._maxViews !== null &&
      this._maxViews !== undefined
    ) {
      const views = parseInt(this._maxViews)
      if (isNaN(views) || views < 1 || views > 10000) {
        this._error = 'Max views must be between 1 and 10,000'
        return
      }
    }

    this._creating = true
    this._error = ''
    try {
      const body = {
        nodeKey: this.nodeId,
      }
      const expiresAt = this._getExpirationDate()
      if (expiresAt) body.expiresAt = expiresAt
      if (
        this._maxViews !== '' &&
        this._maxViews !== null &&
        this._maxViews !== undefined
      ) {
        const views = parseInt(this._maxViews)
        if (views >= 1) body.maxViews = views
      }
      if (this._pin.trim()) {
        body.pin = this._pin.trim()
      }

      const response = await this.authHelper.makeAuthenticatedRequest(
        '/umbraco/management/api/v1/upreviewshare/links',
        { method: 'POST', body: JSON.stringify(body) },
      )

      if (response.ok) {
        const data = await response.json()
        const token = data.token || data.Token
        this._createdUrl = `${window.location.origin}/upreviewshare/preview?token=${token}`
        this._success = true
        NotificationHelper.showSuccess(
          this,
          'Preview link created successfully',
        )
        this.dispatchEvent(
          new CustomEvent('created', { bubbles: true, composed: true }),
        )
      } else {
        const text = await response.text()
        this._error = text || 'Failed to create link'
        NotificationHelper.showError(this, this._error)
      }
    } catch (e) {
      this._error = 'Network error: ' + e.message
      NotificationHelper.showError(this, this._error)
    }
    this._creating = false
  }

  async _copyUrl() {
    try {
      await navigator.clipboard.writeText(this._createdUrl)
      this._copyFeedback = true
      NotificationHelper.showSuccess(this, 'Link copied to clipboard')
      setTimeout(() => {
        this._copyFeedback = false
        this.requestUpdate()
      }, 2000)
    } catch (e) {
      NotificationHelper.showError(this, 'Failed to copy link to clipboard')
    }
  }

  render() {
    return html`
      <div class="overlay" @click=${this._onOverlayClick}>
        <div class="dialog" @click=${(e) => e.stopPropagation()}>
          <div class="dialog-header">
            <h3>${this._success ? 'Link Created' : 'Create Preview Link'}</h3>
            <button class="dialog-close" @click=${this._close}>&times;</button>
          </div>

          <div class="dialog-body">
            ${this._success ? this._renderSuccess() : this._renderForm()}
          </div>

          ${this._success
            ? ''
            : html`
                <div class="dialog-footer">
                  <uui-button
                    look="secondary"
                    label="Cancel"
                    @click=${this._close}
                    >Cancel</uui-button
                  >
                  <uui-button
                    look="primary"
                    label="Create"
                    @click=${this._create}
                    ?disabled=${this._creating}
                  >
                    ${this._creating ? 'Creating...' : 'Create Link'}
                  </uui-button>
                </div>
              `}
        </div>
      </div>
    `
  }

  _onOverlayClick(e) {
    if (e.target === e.currentTarget) {
      this._close()
    }
  }

  _renderForm() {
    return html`
      ${this._error ? html`<div class="error-msg">${this._error}</div>` : ''}

      <div class="form-field">
        <label>Expiration</label>
        <select
          .value=${this._expiration}
          @change=${(e) => (this._expiration = e.target.value)}
        >
          <option value="1h">1 Hour</option>
          <option value="6h">6 Hours</option>
          <option value="24h" selected>24 Hours</option>
          <option value="7d">7 Days</option>
          <option value="30d">30 Days</option>
          <option value="none">No Expiration</option>
        </select>
      </div>

      <div class="form-field">
        <label>Max Views</label>
        <input
          type="number"
          min="1"
          placeholder="Unlimited"
          .value=${this._maxViews}
          @input=${(e) => (this._maxViews = e.target.value)}
        />
        <span class="hint">Leave empty for unlimited views</span>
      </div>

      <div class="form-field">
        <label>PIN Protection</label>
        <input
          type="text"
          maxlength="6"
          pattern="[0-9]{6}"
          inputmode="numeric"
          placeholder="6-digit PIN (optional)"
          .value=${this._pin}
          @input=${(e) => {
            this._pin = e.target.value.replace(/[^0-9]/g, '').slice(0, 6)
            e.target.value = this._pin
          }}
        />
        <span class="hint"
          >Exactly 6 digits required. Viewers must enter this PIN to access the
          preview.</span
        >
      </div>
    `
  }

  _renderSuccess() {
    return html`
      <div class="success-state">
        <uui-icon name="icon-check"></uui-icon>
        <p>Your secure preview link has been created.</p>
        <div class="success-url">${this._createdUrl}</div>
        <button
          class="success-copy-btn ${this._copyFeedback ? 'copied' : ''}"
          @click=${this._copyUrl}
        >
          <uui-icon
            name="${this._copyFeedback ? 'icon-check' : 'icon-documents'}"
          ></uui-icon>
          ${this._copyFeedback ? 'Copied!' : 'Copy Link'}
        </button>
      </div>
    `
  }
}

customElements.define('upreviewshare-create-dialog', uPreviewShareCreateDialog)
