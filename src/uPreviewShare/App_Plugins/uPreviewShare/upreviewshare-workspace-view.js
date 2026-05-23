import { LitElement, html, css } from '@umbraco-cms/backoffice/external/lit'
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api'
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/document'
import { AuthenticationHelper } from './auth-helper.js'
import { NotificationHelper } from './notification-helper.js'
import './components/upreviewshare-link-list.js'
import './components/upreviewshare-audit-panel.js'
import './components/upreviewshare-branding-panel.js'
import './components/upreviewshare-create-dialog.js'

export class uPreviewShareWorkspaceView extends UmbElementMixin(LitElement) {
  static properties = {
    _nodeId: { state: true },
    _activeTab: { state: true },
    _links: { state: true },
    _loading: { state: true },
    _showCreateDialog: { state: true },
    _stats: { state: true },
    _auditFilterLinkId: { state: true },
  }

  static styles = css`
    :host {
      display: block;
      padding: 0;
    }
    .header-banner {
      background: linear-gradient(135deg, #1e293b 0%, #2d1b4e 100%);
      padding: 24px 32px;
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 16px;
    }
    .header-left {
      display: flex;
      align-items: center;
      gap: 16px;
    }
    .header-icon {
      width: 48px;
      height: 48px;
      background: rgba(255, 255, 255, 0.1);
      border-radius: 12px;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    .header-icon uui-icon {
      font-size: 24px;
      color: #fff;
    }
    .header-title {
      font-size: 1.5rem;
      font-weight: 700;
      color: #fff;
      margin: 0;
    }
    .header-subtitle {
      font-size: 0.85rem;
      color: rgba(255, 255, 255, 0.7);
      margin: 4px 0 0 0;
    }
    .header-actions uui-button {
      --uui-button-background-color: #8b5cf6;
      --uui-button-border-color: #8b5cf6;
      --uui-button-font-color: #fff;
    }
    .content {
      padding: 20px 32px;
    }
    .stats-bar {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(160px, 1fr));
      gap: 10px;
      margin-bottom: 12px;
    }
    .stat-card {
      padding: 12px;
      background: var(--uui-color-surface);
      border: 2px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      text-align: center;
      transition:
        transform 0.2s,
        box-shadow 0.2s,
        border-color 0.2s;
    }
    .stat-card:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
      border-color: var(--uui-color-interactive);
    }
    .stat-value {
      font-size: 1.3rem;
      font-weight: bold;
      color: var(--uui-color-text);
    }
    .stat-label {
      font-size: 0.7rem;
      color: var(--uui-color-text-alt);
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }
    .tabs {
      display: flex;
      gap: 0;
      border-bottom: 2px solid var(--uui-color-border);
      margin-bottom: 20px;
    }
    .tab {
      padding: 10px 20px;
      cursor: pointer;
      font-size: 0.9rem;
      font-weight: 500;
      color: var(--uui-color-text-alt);
      border-bottom: 2px solid transparent;
      margin-bottom: -2px;
      transition:
        color 0.2s,
        border-color 0.2s;
      background: none;
      border-top: none;
      border-left: none;
      border-right: none;
    }
    .tab:hover {
      color: var(--uui-color-interactive);
    }
    .tab.active {
      color: var(--uui-color-interactive);
      border-bottom-color: var(--uui-color-interactive);
      font-weight: 600;
    }
    .tab-content {
      min-height: 200px;
    }
    .loading-state {
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 60px 20px;
      color: var(--uui-color-text-alt);
      font-size: 0.9rem;
    }
  `

  constructor() {
    super()
    this._nodeId = null
    this._activeTab = 'links'
    this._links = []
    this._loading = true
    this._showCreateDialog = false
    this._stats = { total: 0, active: 0, expired: 0, views: 0 }
    this._auditFilterLinkId = null
    this.authHelper = new AuthenticationHelper(this)
  }

  connectedCallback() {
    super.connectedCallback()
    this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (context) => {
      if (!context) return
      this.observe(context.unique, (unique) => {
        if (unique && unique !== this._nodeId) {
          this._nodeId = unique
          this._loadData()
        }
      })
    })
  }

  updated(changed) {
    if (changed.has('_nodeId') && this._nodeId) {
      this._auditFilterLinkId = null
      this._loadData()
    }
  }

  async _loadData() {
    if (!this._nodeId) return
    this._loading = true
    try {
      await this.authHelper.initialize()
      const response = await this.authHelper.makeAuthenticatedRequest(
        `/umbraco/management/api/v1/upreviewshare/links/${this._nodeId}`,
      )
      if (response.ok) {
        this._links = await response.json()
        this._computeStats()
      } else {
        this._links = []
        this._computeStats()
        NotificationHelper.showError(this, 'Failed to load preview links')
      }
    } catch (e) {
      this._links = []
      this._computeStats()
      NotificationHelper.showError(this, 'Network error loading preview links')
    }
    this._loading = false
  }

  _computeStats() {
    const total = this._links.length
    const active = this._links.filter((l) => l.status === 'Active').length
    const expired = this._links.filter((l) => l.status === 'Expired').length
    const views = this._links.reduce((sum, l) => sum + (l.viewCount || 0), 0)
    this._stats = { total, active, expired, views }
  }

  _setTab(tab) {
    this._activeTab = tab
    if (tab !== 'audit') {
      this._auditFilterLinkId = null
    }
  }
  _openCreateDialog() {
    this._showCreateDialog = true
  }
  _closeCreateDialog() {
    this._showCreateDialog = false
  }
  _onLinkCreated() {
    this._showCreateDialog = false
    this._loadData()
  }
  _onLinksChanged() {
    this._loadData()
  }
  _onViewLinkLogs(e) {
    this._auditFilterLinkId = e.detail.linkId
    this._activeTab = 'audit'
  }

  render() {
    return html`
      <div class="header-banner">
        <div class="header-left">
          <div class="header-icon">
            <img
              src="/App_Plugins/uPreviewShare/icon.png"
              alt="uPreviewShare"
              style="width:36px;height:36px;object-fit:contain;"
            />
          </div>
          <div>
            <div class="header-title">
              uPreviewShare
              <span
                style="font-size:0.6em;font-weight:400;opacity:0.7;vertical-align:middle;"
                >v1.0.0</span
              >
            </div>
            <div class="header-subtitle">
              Manage secure preview links for this content
            </div>
          </div>
        </div>
        <div class="header-actions">
          <uui-button
            look="primary"
            label="Create Link"
            @click=${this._openCreateDialog}
            ?disabled=${!this._nodeId}
          >
            <uui-icon name="icon-add"></uui-icon> Create Link
          </uui-button>
        </div>
      </div>
      <div class="content">
        ${this._loading
          ? html`<div class="loading-state"><uui-loader></uui-loader></div>`
          : html`
              <div class="stats-bar">
                <div class="stat-card">
                  <div class="stat-value">${this._stats.total}</div>
                  <div class="stat-label">Total Links</div>
                </div>
                <div class="stat-card">
                  <div class="stat-value">${this._stats.active}</div>
                  <div class="stat-label">Active</div>
                </div>
                <div class="stat-card">
                  <div class="stat-value">${this._stats.expired}</div>
                  <div class="stat-label">Expired</div>
                </div>
                <div class="stat-card">
                  <div class="stat-value">${this._stats.views}</div>
                  <div class="stat-label">Total Views</div>
                </div>
              </div>
              <div class="tabs">
                <button
                  class="tab ${this._activeTab === 'links' ? 'active' : ''}"
                  @click=${() => this._setTab('links')}
                >
                  Links
                </button>
                <button
                  class="tab ${this._activeTab === 'audit' ? 'active' : ''}"
                  @click=${() => this._setTab('audit')}
                >
                  Audit Log
                </button>
                <button
                  class="tab ${this._activeTab === 'branding' ? 'active' : ''}"
                  @click=${() => this._setTab('branding')}
                >
                  Branding
                </button>
              </div>
              <div class="tab-content">${this._renderTabContent()}</div>
            `}
      </div>
      ${this._showCreateDialog
        ? html`<upreviewshare-create-dialog
            .nodeId=${this._nodeId}
            .authHelper=${this.authHelper}
            @close=${this._closeCreateDialog}
            @created=${this._onLinkCreated}
          ></upreviewshare-create-dialog>`
        : ''}
    `
  }

  _renderTabContent() {
    switch (this._activeTab) {
      case 'links':
        return html`<upreviewshare-link-list
          .links=${this._links}
          .nodeId=${this._nodeId}
          .authHelper=${this.authHelper}
          @links-changed=${this._onLinksChanged}
          @view-link-logs=${this._onViewLinkLogs}
        ></upreviewshare-link-list>`
      case 'audit':
        return html`<upreviewshare-audit-panel
          .nodeId=${this._nodeId}
          .authHelper=${this.authHelper}
          .filterLinkId=${this._auditFilterLinkId}
          .links=${this._links}
        ></upreviewshare-audit-panel>`
      case 'branding':
        return html`<upreviewshare-branding-panel
          .authHelper=${this.authHelper}
        ></upreviewshare-branding-panel>`
      default:
        return ''
    }
  }

  disconnectedCallback() {
    super.disconnectedCallback()
    if (this.authHelper) this.authHelper.destroy()
  }
}

customElements.define(
  'upreviewshare-workspace-view',
  uPreviewShareWorkspaceView,
)
export default uPreviewShareWorkspaceView
