import { LitElement, html, css } from '@umbraco-cms/backoffice/external/lit'
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api'
import { NotificationHelper } from '../notification-helper.js'

export class uPreviewShareLinkList extends UmbElementMixin(LitElement) {
  static properties = {
    links: { type: Array },
    nodeId: { type: String },
    authHelper: { type: Object },
    _searchTerm: { state: true },
    _filterStatus: { state: true },
    _currentPage: { state: true },
    _copyFeedback: { state: true },
    _showConfirmDialog: { state: true },
    _confirmAction: { state: true },
    _confirmTarget: { state: true },
  }

  static styles = css`
    :host {
      display: block;
    }

    .primary-controls {
      display: flex;
      gap: 12px;
      align-items: center;
      margin-bottom: 12px;
      flex-wrap: wrap;
      padding: 12px;
      background: var(--uui-color-surface-alt);
      border-radius: var(--uui-border-radius);
      border: 1px solid var(--uui-color-border);
    }

    .search-box {
      flex: 1;
      min-width: 200px;
      position: relative;
    }
    .search-box input {
      width: 100%;
      padding: 8px 12px;
      padding-left: 32px;
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface);
      color: var(--uui-color-text);
      font-size: 0.85rem;
      box-sizing: border-box;
    }
    .search-box input:focus {
      outline: none;
      border-color: var(--uui-color-interactive);
    }
    .search-box uui-icon {
      position: absolute;
      left: 8px;
      top: 50%;
      transform: translateY(-50%);
      color: var(--uui-color-text-alt);
    }

    .filter-select {
      padding: 8px 12px;
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface);
      color: var(--uui-color-text);
      font-size: 0.85rem;
    }

    .summary {
      display: flex;
      align-items: center;
      gap: 12px;
      margin-bottom: 12px;
      padding: 10px 12px;
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      font-size: 0.85rem;
      color: var(--uui-color-text-alt);
    }

    .cards-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(340px, 1fr));
      gap: 12px;
      margin-bottom: 12px;
    }

    .link-card {
      transition:
        transform 0.2s,
        box-shadow 0.2s,
        border-color 0.2s;
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface);
      overflow: hidden;
    }
    .link-card:hover {
      transform: translateY(-4px);
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
      border-color: var(--uui-color-interactive);
    }

    .card-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 12px 16px;
      border-bottom: 1px solid var(--uui-color-border);
      background: var(--uui-color-surface-alt);
    }
    .card-token {
      font-family: monospace;
      font-size: 0.8rem;
      color: var(--uui-color-text-alt);
      max-width: 180px;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    .status-badge {
      display: inline-flex;
      align-items: center;
      gap: 4px;
      padding: 4px 10px;
      border-radius: 9999px;
      font-size: 0.7rem;
      font-weight: 600;
      text-transform: uppercase;
      letter-spacing: 0.3px;
      color: #fff;
    }
    .status-active {
      background: #059669;
    }
    .status-revoked {
      background: #dc2626;
    }
    .status-expired {
      background: #d97706;
    }

    .card-body {
      padding: 14px 16px;
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 10px;
    }
    .card-field {
      display: flex;
      flex-direction: column;
      gap: 2px;
    }
    .card-field-label {
      font-size: 0.7rem;
      color: var(--uui-color-text-alt);
      text-transform: uppercase;
      letter-spacing: 0.3px;
    }
    .card-field-value {
      font-size: 0.85rem;
      color: var(--uui-color-text);
      font-weight: 500;
    }

    .card-footer {
      display: flex;
      align-items: center;
      justify-content: flex-end;
      gap: 8px;
      padding: 10px 16px;
      border-top: 1px solid var(--uui-color-border);
      background: var(--uui-color-surface-alt);
    }

    .btn-copy {
      display: inline-flex;
      align-items: center;
      gap: 4px;
      padding: 6px 12px;
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface);
      color: var(--uui-color-text);
      font-size: 0.78rem;
      cursor: pointer;
      transition:
        background 0.2s,
        border-color 0.2s;
    }
    .btn-copy:hover {
      background: var(--uui-color-surface-alt);
      border-color: var(--uui-color-interactive);
    }
    .btn-copy.copied {
      background: var(--uui-color-positive);
      border-color: var(--uui-color-positive);
      color: #fff;
    }

    .btn-logs {
      display: inline-flex;
      align-items: center;
      gap: 4px;
      padding: 6px 12px;
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface);
      color: var(--uui-color-text);
      font-size: 0.78rem;
      cursor: pointer;
      transition:
        background 0.2s,
        border-color 0.2s;
    }
    .btn-logs:hover {
      background: var(--uui-color-surface-alt);
      border-color: var(--uui-color-interactive);
      color: var(--uui-color-interactive);
    }

    .btn-revoke {
      display: inline-flex;
      align-items: center;
      gap: 4px;
      padding: 6px 12px;
      border: 1px solid var(--uui-color-danger);
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface);
      color: var(--uui-color-danger);
      font-size: 0.78rem;
      cursor: pointer;
      transition: background 0.2s;
    }
    .btn-revoke:hover {
      background: var(--uui-color-surface-alt);
    }

    .btn-delete {
      display: inline-flex;
      align-items: center;
      padding: 6px 8px;
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface);
      color: var(--uui-color-text-alt);
      cursor: pointer;
      transition:
        background 0.2s,
        color 0.2s;
    }
    .btn-delete:hover {
      background: var(--uui-color-surface-alt);
      color: var(--uui-color-danger);
      border-color: var(--uui-color-danger);
    }

    .pagination {
      display: flex;
      justify-content: center;
      align-items: center;
      gap: 12px;
      margin-top: 12px;
      padding: 12px;
      background: var(--uui-color-surface-alt);
      border-radius: var(--uui-border-radius);
    }
    .pagination button {
      padding: 6px 14px;
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface);
      color: var(--uui-color-text);
      cursor: pointer;
      font-size: 0.8rem;
    }
    .pagination button:disabled {
      opacity: 0.4;
      cursor: not-allowed;
    }
    .pagination span {
      font-size: 0.85rem;
      color: var(--uui-color-text-alt);
    }

    .empty-state {
      text-align: center;
      padding: 48px 20px;
      color: var(--uui-color-text-alt);
    }
    .empty-state uui-icon {
      font-size: 48px;
      margin-bottom: 12px;
      opacity: 0.4;
    }
    .empty-state p {
      margin: 4px 0;
      font-size: 0.9rem;
    }

    .countdown {
      font-size: 0.75rem;
      color: #b45309;
      font-weight: 500;
    }

    .dialog-overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(0, 0, 0, 0.5);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 10000;
    }
    .dialog {
      background: var(--uui-color-surface);
      padding: 28px;
      border-radius: var(--uui-border-radius);
      max-width: 420px;
      width: 90%;
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.2);
    }
    .dialog h3 {
      margin: 0 0 12px 0;
      font-size: 1.1rem;
    }
    .dialog p {
      margin: 0 0 8px 0;
      font-size: 0.9rem;
      color: var(--uui-color-text-alt);
    }
    .dialog-warning {
      color: #991b1b;
      font-weight: 500;
      font-size: 0.85rem;
    }
    .dialog-actions {
      display: flex;
      gap: 8px;
      justify-content: flex-end;
      margin-top: 20px;
    }
  `

  constructor() {
    super()
    this.links = []
    this.nodeId = null
    this.authHelper = null
    this._searchTerm = ''
    this._filterStatus = 'all'
    this._currentPage = 1
    this._copyFeedback = null
    this._pageSize = 12
    this._showConfirmDialog = false
    this._confirmAction = null
    this._confirmTarget = null
  }

  get _filteredLinks() {
    let result = [...(this.links || [])]
    if (this._filterStatus !== 'all') {
      result = result.filter(
        (l) => l.status.toLowerCase() === this._filterStatus,
      )
    }
    if (this._searchTerm) {
      const term = this._searchTerm.toLowerCase()
      result = result.filter(
        (l) =>
          l.token.toLowerCase().includes(term) ||
          l.status.toLowerCase().includes(term),
      )
    }
    return result
  }

  get _pagedLinks() {
    const start = (this._currentPage - 1) * this._pageSize
    return this._filteredLinks.slice(start, start + this._pageSize)
  }

  get _totalPages() {
    return Math.max(1, Math.ceil(this._filteredLinks.length / this._pageSize))
  }

  _onSearch(e) {
    this._searchTerm = e.target.value
    this._currentPage = 1
  }

  _onFilterChange(e) {
    this._filterStatus = e.target.value
    this._currentPage = 1
  }

  _prevPage() {
    if (this._currentPage > 1) this._currentPage--
  }

  _nextPage() {
    if (this._currentPage < this._totalPages) this._currentPage++
  }

  async _copyLink(link) {
    const url = `${window.location.origin}/upreviewshare/preview?token=${link.token}`
    try {
      await navigator.clipboard.writeText(url)
      this._copyFeedback = link.id
      NotificationHelper.showSuccess(this, 'Link copied to clipboard')
      setTimeout(() => {
        this._copyFeedback = null
        this.requestUpdate()
      }, 2000)
    } catch (e) {
      NotificationHelper.showError(this, 'Failed to copy link to clipboard')
    }
  }

  async _revokeLink(link) {
    this._confirmAction = 'revoke-single'
    this._confirmTarget = link
    this._showConfirmDialog = true
  }

  async _revokeAll() {
    this._confirmAction = 'revoke-all'
    this._confirmTarget = null
    this._showConfirmDialog = true
  }

  _deleteLink(link) {
    this._confirmAction = 'delete-single'
    this._confirmTarget = link
    this._showConfirmDialog = true
  }

  _closeConfirmDialog() {
    this._showConfirmDialog = false
    this._confirmAction = null
    this._confirmTarget = null
  }

  async _executeConfirmedAction() {
    if (this._confirmAction === 'revoke-single' && this._confirmTarget) {
      try {
        const response = await this.authHelper.makeAuthenticatedRequest(
          `/umbraco/management/api/v1/upreviewshare/links/${this._confirmTarget.id}`,
          { method: 'DELETE' },
        )
        if (response.ok) {
          NotificationHelper.showSuccess(this, 'Link revoked successfully')
          this.dispatchEvent(
            new CustomEvent('links-changed', { bubbles: true, composed: true }),
          )
        } else {
          NotificationHelper.showError(this, 'Failed to revoke link')
        }
      } catch (e) {
        NotificationHelper.showError(this, 'Network error while revoking link')
      }
    } else if (this._confirmAction === 'revoke-all') {
      try {
        const response = await this.authHelper.makeAuthenticatedRequest(
          `/umbraco/management/api/v1/upreviewshare/links/node/${this.nodeId}`,
          { method: 'DELETE' },
        )
        if (response.ok) {
          const data = await response.json()
          const count = data.count || 0
          NotificationHelper.showSuccess(
            this,
            count === 1
              ? '1 link revoked successfully'
              : `${count} links revoked successfully`,
          )
          this.dispatchEvent(
            new CustomEvent('links-changed', { bubbles: true, composed: true }),
          )
        } else {
          NotificationHelper.showError(this, 'Failed to revoke links')
        }
      } catch (e) {
        NotificationHelper.showError(this, 'Network error while revoking links')
      }
    } else if (this._confirmAction === 'delete-single' && this._confirmTarget) {
      try {
        const response = await this.authHelper.makeAuthenticatedRequest(
          `/umbraco/management/api/v1/upreviewshare/links/${this._confirmTarget.id}/permanent`,
          { method: 'DELETE' },
        )
        if (response.ok) {
          NotificationHelper.showSuccess(this, 'Link deleted successfully')
          this.dispatchEvent(
            new CustomEvent('links-changed', { bubbles: true, composed: true }),
          )
        } else {
          NotificationHelper.showError(this, 'Failed to delete link')
        }
      } catch (e) {
        NotificationHelper.showError(this, 'Network error while deleting link')
      }
    }
    this._closeConfirmDialog()
  }

  _refresh() {
    this.dispatchEvent(
      new CustomEvent('links-changed', { bubbles: true, composed: true }),
    )
  }

  _viewLogs(link) {
    this.dispatchEvent(
      new CustomEvent('view-link-logs', {
        bubbles: true,
        composed: true,
        detail: { linkId: link.id },
      }),
    )
  }

  _hasAnyCulture() {
    return this.links && this.links.some((l) => l.culture)
  }

  _formatDate(dateStr) {
    if (!dateStr) return 'Never'
    let utcDate = dateStr
    if (
      !utcDate.endsWith('Z') &&
      !utcDate.includes('+') &&
      !utcDate.includes('-', 10)
    ) {
      utcDate += 'Z'
    }
    const d = new Date(utcDate)
    return d.toLocaleDateString(undefined, {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    })
  }

  _getCountdown(dateStr) {
    if (!dateStr) return null
    let utcDate = dateStr
    if (
      !utcDate.endsWith('Z') &&
      !utcDate.includes('+') &&
      !utcDate.includes('-', 10)
    ) {
      utcDate += 'Z'
    }
    const now = new Date()
    const expires = new Date(utcDate)
    const diff = expires - now
    if (diff <= 0) return 'Expired'
    const days = Math.floor(diff / 86400000)
    const hours = Math.floor((diff % 86400000) / 3600000)
    const minutes = Math.floor((diff % 3600000) / 60000)
    if (days > 0) return `${days}d ${hours}h remaining`
    if (hours > 0) return `${hours}h ${minutes}m remaining`
    return `${minutes}m remaining`
  }

  _getStatusClass(status) {
    switch (status) {
      case 'Active':
        return 'status-active'
      case 'Revoked':
        return 'status-revoked'
      case 'Expired':
        return 'status-expired'
      default:
        return ''
    }
  }

  render() {
    return html`
      <div class="primary-controls">
        <div class="search-box">
          <uui-icon name="icon-search"></uui-icon>
          <input
            type="text"
            placeholder="Search by token..."
            .value=${this._searchTerm}
            @input=${this._onSearch}
          />
        </div>
        <select class="filter-select" @change=${this._onFilterChange}>
          <option value="all">All Status</option>
          <option value="active">Active</option>
          <option value="revoked">Revoked</option>
          <option value="expired">Expired</option>
        </select>
        <uui-button
          look="secondary"
          label="Revoke All"
          @click=${this._revokeAll}
        >
          Revoke All
        </uui-button>
        <uui-button look="secondary" label="Refresh" @click=${this._refresh}>
          <uui-icon name="icon-refresh"></uui-icon>
        </uui-button>
      </div>

      <div class="summary">
        <span
          >${this._filteredLinks.length}
          link${this._filteredLinks.length !== 1 ? 's' : ''} found</span
        >
      </div>

      ${this._filteredLinks.length === 0
        ? html`
            <div class="empty-state">
              <uui-icon name="icon-link"></uui-icon>
              <p>No preview links found</p>
              <p>
                Create a secure link to share draft content with external
                reviewers.
              </p>
            </div>
          `
        : html`
            <div class="cards-grid">
              ${this._pagedLinks.map((link) => this._renderCard(link))}
            </div>

            ${this._totalPages > 1
              ? html`
                  <div class="pagination">
                    <button
                      ?disabled=${this._currentPage <= 1}
                      @click=${this._prevPage}
                    >
                      Previous
                    </button>
                    <span
                      >Page ${this._currentPage} of ${this._totalPages}</span
                    >
                    <button
                      ?disabled=${this._currentPage >= this._totalPages}
                      @click=${this._nextPage}
                    >
                      Next
                    </button>
                  </div>
                `
              : ''}
          `}
      ${this._showConfirmDialog ? this._renderConfirmDialog() : ''}
    `
  }

  _renderConfirmDialog() {
    const isAll = this._confirmAction === 'revoke-all'
    const isDelete = this._confirmAction === 'delete-single'
    let title, message, buttonLabel

    if (isDelete) {
      title = 'Delete Link'
      message =
        'This will delete this link from the list. Audit log entries will be preserved for compliance purposes.'
      buttonLabel = 'Delete'
    } else if (isAll) {
      title = 'Revoke All Links'
      message =
        'This will revoke ALL active preview links for this content. External reviewers will no longer be able to access any shared drafts.'
      buttonLabel = 'Revoke'
    } else {
      title = 'Revoke Link'
      message =
        'This will revoke this preview link. The external reviewer will no longer be able to access the shared draft.'
      buttonLabel = 'Revoke'
    }

    return html`
      <div class="dialog-overlay" @click=${this._closeConfirmDialog}>
        <div class="dialog" @click=${(e) => e.stopPropagation()}>
          <h3>${title}</h3>
          <p>${message}</p>
          <p class="dialog-warning">This action cannot be undone.</p>
          <div class="dialog-actions">
            <uui-button
              look="secondary"
              label="Cancel"
              @click=${this._closeConfirmDialog}
            >
              Cancel
            </uui-button>
            <uui-button
              look="primary"
              color="danger"
              label="${buttonLabel}"
              @click=${this._executeConfirmedAction}
            >
              ${buttonLabel}
            </uui-button>
          </div>
        </div>
      </div>
    `
  }

  _renderCard(link) {
    const isCopied = this._copyFeedback === link.id
    return html`
      <div class="link-card">
        <div class="card-header">
          <span class="status-badge ${this._getStatusClass(link.status)}"
            >${link.status}</span
          >
          <span class="card-token" title="${link.token}">${link.token}</span>
        </div>
        <div class="card-body">
          <div class="card-field">
            <span class="card-field-label">Security</span>
            <span class="card-field-value"
              >${link.hasPin ? 'PIN Protected' : 'Link Only'}</span
            >
          </div>
          <div class="card-field">
            <span class="card-field-label">Created</span>
            <span class="card-field-value"
              >${this._formatDate(link.createdAt)}</span
            >
          </div>
          <div class="card-field">
            <span class="card-field-label">Expires</span>
            <span class="card-field-value"
              >${link.expiresAt
                ? this._formatDate(link.expiresAt)
                : 'Never'}</span
            >
            ${link.expiresAt && link.status === 'Active'
              ? html`<span class="countdown"
                  >${this._getCountdown(link.expiresAt)}</span
                >`
              : ''}
          </div>
          <div class="card-field">
            <span class="card-field-label">Views</span>
            <span class="card-field-value"
              >${link.viewCount || 0}${link.maxViews
                ? ' / ' + link.maxViews
                : ''}</span
            >
          </div>
          ${link.culture
            ? html`
                <div class="card-field">
                  <span class="card-field-label">Variant</span>
                  <span class="card-field-value">${link.culture}</span>
                </div>
              `
            : this._hasAnyCulture()
              ? html`
                  <div class="card-field">
                    <span class="card-field-label">Variant</span>
                    <span
                      class="card-field-value"
                      style="color: var(--uui-color-text-alt)"
                      >Default</span
                    >
                  </div>
                `
              : ''}
        </div>
        <div class="card-footer">
          <button
            class="btn-copy ${isCopied ? 'copied' : ''}"
            @click=${() => this._copyLink(link)}
          >
            <uui-icon
              name="${isCopied ? 'icon-check' : 'icon-documents'}"
            ></uui-icon>
            ${isCopied ? 'Copied!' : 'Copy Link'}
          </button>
          <button class="btn-logs" @click=${() => this._viewLogs(link)}>
            <uui-icon name="icon-activity"></uui-icon>
            Logs
          </button>
          ${link.status === 'Active'
            ? html`
                <button
                  class="btn-revoke"
                  @click=${() => this._revokeLink(link)}
                >
                  <uui-icon name="icon-delete"></uui-icon>
                  Revoke
                </button>
              `
            : ''}
          <button class="btn-delete" @click=${() => this._deleteLink(link)}>
            <uui-icon name="icon-trash"></uui-icon>
          </button>
        </div>
      </div>
    `
  }
}

customElements.define('upreviewshare-link-list', uPreviewShareLinkList)
