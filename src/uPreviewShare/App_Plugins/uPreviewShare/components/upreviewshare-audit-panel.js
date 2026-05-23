import { LitElement, html, css } from '@umbraco-cms/backoffice/external/lit'
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api'
import { NotificationHelper } from '../notification-helper.js'

export class uPreviewShareAuditPanel extends UmbElementMixin(LitElement) {
  static properties = {
    nodeId: { type: String },
    authHelper: { type: Object },
    filterLinkId: { type: String },
    links: { type: Array },
    _allLinks: { state: true },
    _entries: { state: true },
    _loading: { state: true },
    _filter: { state: true },
    _linkFilter: { state: true },
    _page: { state: true },
    _totalPages: { state: true },
    _expandedIds: { state: true },
  }

  static styles = css`
    :host {
      display: block;
    }

    .filter-bar {
      display: flex;
      gap: 8px;
      margin-bottom: 12px;
      flex-wrap: wrap;
    }
    .filter-btn {
      padding: 6px 14px;
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface);
      color: var(--uui-color-text);
      font-size: 0.8rem;
      cursor: pointer;
      transition:
        background 0.2s,
        border-color 0.2s;
    }
    .filter-btn:hover {
      border-color: var(--uui-color-interactive);
      color: var(--uui-color-interactive);
    }
    .filter-btn.active {
      background: #8B5CF6;
      color: #fff;
      border-color: #8B5CF6;
    }

    .cards-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
      gap: 10px;
      margin-bottom: 12px;
    }

    .audit-card {
      padding: 14px 16px;
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface);
      cursor: pointer;
      transition:
        transform 0.2s,
        box-shadow 0.2s,
        border-color 0.2s;
    }
    .audit-card:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
      border-color: var(--uui-color-interactive);
    }

    .audit-card-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      margin-bottom: 8px;
    }

    .event-badge {
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
    .event-access {
      background: #2563eb;
    }
    .event-failedpin {
      background: #dc2626;
    }
    .event-revocation {
      background: #d97706;
    }
    .event-lockout {
      background: #6b7280;
    }

    .audit-link-id {
      font-family: monospace;
      font-size: 0.75rem;
      color: var(--uui-color-text-alt);
      max-width: 120px;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    .audit-card-body {
      display: flex;
      justify-content: space-between;
      align-items: center;
      font-size: 0.8rem;
      color: var(--uui-color-text-alt);
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

    .loading-state {
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 40px;
    }
  `

  constructor() {
    super()
    this.nodeId = null
    this.authHelper = null
    this.filterLinkId = null
    this.links = []
    this._allLinks = []
    this._entries = []
    this._loading = false
    this._filter = 'all'
    this._linkFilter = null
    this._page = 1
    this._totalPages = 1
    this._expandedIds = new Set()
  }

  connectedCallback() {
    super.connectedCallback()
    if (this.filterLinkId) {
      this._linkFilter = this.filterLinkId
    }
    if (this.nodeId && this.authHelper) {
      this._loadAllLinks()
      this._loadAudit()
    }
  }

  updated(changed) {
    if (changed.has('nodeId') && this.nodeId) {
      this._loadAllLinks()
      this._loadAudit()
    }
    if (changed.has('filterLinkId') && this.filterLinkId !== undefined) {
      this._linkFilter = this.filterLinkId
      this._page = 1
      this._loadAudit()
    }
  }

  async _loadAllLinks() {
    if (!this.nodeId || !this.authHelper) return
    try {
      await this.authHelper.initialize()
      const response = await this.authHelper.makeAuthenticatedRequest(
        `/umbraco/management/api/v1/upreviewshare/audit/${this.nodeId}/links`,
      )
      if (response.ok) {
        this._allLinks = await response.json()
      } else {
        this._allLinks = this.links || []
      }
    } catch (e) {
      this._allLinks = this.links || []
    }
  }

  async _loadAudit() {
    if (!this.nodeId || !this.authHelper) return
    this._loading = true
    try {
      await this.authHelper.initialize()
      let url = `/umbraco/management/api/v1/upreviewshare/audit/${this.nodeId}?page=${this._page}`
      if (this._filter !== 'all') {
        url += `&eventType=${encodeURIComponent(this._filter)}`
      }
      if (this._linkFilter) {
        url += `&linkId=${encodeURIComponent(this._linkFilter)}`
      }
      const response = await this.authHelper.makeAuthenticatedRequest(url)
      if (response.ok) {
        const data = await response.json()
        if (Array.isArray(data)) {
          this._entries = data
          this._totalPages = 1
        } else {
          this._entries = data.items || []
          this._totalPages = data.totalPages || 1
        }
      } else {
        this._entries = []
        this._totalPages = 1
        NotificationHelper.showError(this, 'Failed to load audit log')
      }
    } catch (e) {
      this._entries = []
      this._totalPages = 1
      NotificationHelper.showError(this, 'Network error loading audit log')
    }
    this._loading = false
  }

  _setFilter(filter) {
    this._filter = filter
    this._page = 1
    this._loadAudit()
  }

  _setLinkFilter(e) {
    const value = e.target.value
    this._linkFilter = value === 'all' ? null : value
    this._page = 1
    this._loadAudit()
  }

  _clearLinkFilter() {
    this._linkFilter = null
    this._page = 1
    this._loadAudit()
  }

  _prevPage() {
    if (this._page > 1) {
      this._page--
      this._loadAudit()
    }
  }

  _nextPage() {
    if (this._page < this._totalPages) {
      this._page++
      this._loadAudit()
    }
  }

  _getEventClass(eventType) {
    const type = String(eventType || '').toLowerCase()
    switch (type) {
      case 'access':
      case '0':
        return 'event-access'
      case 'failedpin':
      case '1':
        return 'event-failedpin'
      case 'revocation':
      case '2':
        return 'event-revocation'
      case 'lockout':
      case '3':
      case '4':
        return 'event-lockout'
      default:
        return 'event-access'
    }
  }

  _getEventLabel(eventType) {
    const type = String(eventType || '').toLowerCase()
    switch (type) {
      case 'access':
      case '0':
        return 'Access'
      case 'failedpin':
      case '1':
        return 'Failed PIN'
      case 'revocation':
      case '2':
        return 'Revocation'
      case 'lockout':
      case '3':
        return 'Lockout'
      case '4':
        return 'Lockout'
      default:
        return 'Unknown'
    }
  }

  _formatDate(dateStr) {
    if (!dateStr) return ''
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

  _truncateId(id) {
    if (!id) return ''
    const str = String(id)
    return str.length > 12 ? str.substring(0, 12) + '...' : str
  }

  _toggleExpand(entryId) {
    const newSet = new Set(this._expandedIds)
    if (newSet.has(entryId)) {
      newSet.delete(entryId)
    } else {
      newSet.add(entryId)
    }
    this._expandedIds = newSet
  }

  _parseUserAgent(ua) {
    if (!ua) return 'Unknown'
    if (ua.includes('Chrome') && !ua.includes('Edg')) return 'Chrome'
    if (ua.includes('Edg')) return 'Edge'
    if (ua.includes('Firefox')) return 'Firefox'
    if (ua.includes('Safari') && !ua.includes('Chrome')) return 'Safari'
    if (ua.includes('Opera') || ua.includes('OPR')) return 'Opera'
    return 'Other'
  }

  _isSystemEvent(eventType) {
    const type = String(eventType || '').toLowerCase()
    return type === 'revocation' || type === '2'
  }

  render() {
    if (this._loading) {
      return html`<div class="loading-state"><uui-loader></uui-loader></div>`
    }

    return html`
      <div class="filter-bar">
        <button
          class="filter-btn ${this._filter === 'all' ? 'active' : ''}"
          @click=${() => this._setFilter('all')}
        >
          All
        </button>
        <button
          class="filter-btn ${this._filter === 'Access' ? 'active' : ''}"
          @click=${() => this._setFilter('Access')}
        >
          Access
        </button>
        <button
          class="filter-btn ${this._filter === 'FailedPin' ? 'active' : ''}"
          @click=${() => this._setFilter('FailedPin')}
        >
          Failed PIN
        </button>
        <button
          class="filter-btn ${this._filter === 'Revocation' ? 'active' : ''}"
          @click=${() => this._setFilter('Revocation')}
        >
          Revocation
        </button>
        <button
          class="filter-btn ${this._filter === 'Lockout' ? 'active' : ''}"
          @click=${() => this._setFilter('Lockout')}
        >
          Lockout
        </button>
        <select
          class="filter-btn"
          style="min-width:160px;"
          @change=${this._setLinkFilter}
        >
          <option value="all" ?selected=${!this._linkFilter}>All Links</option>
          ${(this._allLinks || []).map(
            (link) => html`
              <option
                value="${link.id}"
                ?selected=${this._linkFilter === link.id}
              >
                ${link.token
                  ? link.token.substring(0, 12) + '...'
                  : link.id.substring(0, 8) + '...'}${link.status === 'Deleted'
                  ? ' (Deleted)'
                  : ''}
              </option>
            `,
          )}
        </select>
        ${this._linkFilter
          ? html`<button
              class="filter-btn"
              style="background:#ef4444;color:#fff;border-color:#ef4444;"
              @click=${this._clearLinkFilter}
            >
              ✕ Clear Link Filter
            </button>`
          : ''}
      </div>

      ${this._entries.length === 0
        ? html`
            <div class="empty-state">
              <uui-icon name="icon-audit-trail"></uui-icon>
              <p>No audit entries found</p>
              <p>
                Events are logged when preview links are accessed, PIN attempts
                fail, or links are revoked.
              </p>
            </div>
          `
        : html`
            <div class="cards-grid">
              ${this._entries.map(
                (entry) => html`
                  <div
                    class="audit-card"
                    @click=${() => this._toggleExpand(entry.id)}
                  >
                    <div class="audit-card-header">
                      <span
                        class="event-badge ${this._getEventClass(
                          entry.eventType,
                        )}"
                        >${this._getEventLabel(entry.eventType)}</span
                      >
                      <span
                        style="font-size:0.7rem;color:var(--uui-color-text-alt);"
                        >${this._isSystemEvent(entry.eventType)
                          ? 'Backoffice'
                          : this._parseUserAgent(entry.userAgent)}</span
                      >
                    </div>
                    <div class="audit-card-body">
                      <span>${this._formatDate(entry.timestamp)}</span>
                      <span
                        >${entry.ipAddress ||
                        (this._isSystemEvent(entry.eventType)
                          ? 'System'
                          : 'Unknown IP')}</span
                      >
                      <uui-icon
                        name="${this._expandedIds.has(entry.id)
                          ? 'icon-navigation-up'
                          : 'icon-navigation-down'}"
                        style="font-size:0.75rem;color:var(--uui-color-text-alt);opacity:0.6;"
                      ></uui-icon>
                    </div>
                    ${this._expandedIds.has(entry.id)
                      ? html`
                          <div
                            style="margin-top:8px;padding-top:8px;border-top:1px solid var(--uui-color-border);font-size:0.75rem;color:var(--uui-color-text-alt);"
                          >
                            <div style="margin-bottom:4px;">
                              <strong>Link ID:</strong> ${entry.linkId}
                            </div>
                            ${entry.userAgent
                              ? html`<div style="word-break:break-all;">
                                  <strong>User Agent:</strong>
                                  ${entry.userAgent}
                                </div>`
                              : ''}
                          </div>
                        `
                      : ''}
                  </div>
                `,
              )}
            </div>

            ${this._totalPages > 1
              ? html`
                  <div class="pagination">
                    <button
                      ?disabled=${this._page <= 1}
                      @click=${this._prevPage}
                    >
                      Previous
                    </button>
                    <span>Page ${this._page} of ${this._totalPages}</span>
                    <button
                      ?disabled=${this._page >= this._totalPages}
                      @click=${this._nextPage}
                    >
                      Next
                    </button>
                  </div>
                `
              : ''}
          `}
    `
  }
}

customElements.define('upreviewshare-audit-panel', uPreviewShareAuditPanel)
