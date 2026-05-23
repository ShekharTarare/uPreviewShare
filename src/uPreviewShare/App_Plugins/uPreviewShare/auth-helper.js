import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth'

export class AuthenticationHelper {
  constructor(host) {
    this.host = host
    this.authContext = null
    this.cachedToken = null
    this.tokenExpiry = null
    this.initPromise = null
    this.refreshPromise = null
  }

  async initialize() {
    if (this.initPromise) return this.initPromise
    this.initPromise = new Promise((resolve, reject) => {
      const timeout = setTimeout(() => reject(new Error('Auth timeout')), 10000)
      this.host.consumeContext(UMB_AUTH_CONTEXT, (ctx) => {
        if (ctx) {
          clearTimeout(timeout)
          this.authContext = ctx
          resolve()
        }
      })
    })
    return this.initPromise
  }

  async getToken() {
    if (!this.authContext) await this.initialize()
    if (
      this.cachedToken &&
      this.tokenExpiry &&
      Date.now() < this.tokenExpiry - 60000
    )
      return this.cachedToken
    return this.refreshToken()
  }

  async refreshToken() {
    if (this.refreshPromise) return this.refreshPromise
    this.refreshPromise = (async () => {
      try {
        let token = null
        if (typeof this.authContext.getLatestToken === 'function')
          token = await this.authContext.getLatestToken()
        if (
          !token &&
          typeof this.authContext.getOpenApiConfiguration === 'function'
        ) {
          const config = this.authContext.getOpenApiConfiguration()
          if (config && typeof config.token === 'function')
            token = await config.token()
        }
        if (!token) throw new Error('Token unavailable')
        this.cachedToken = token
        try {
          const payload = JSON.parse(atob(token.split('.')[1]))
          this.tokenExpiry = payload.exp
            ? payload.exp * 1000
            : Date.now() + 300000
        } catch {
          this.tokenExpiry = Date.now() + 300000
        }
        return this.cachedToken
      } finally {
        this.refreshPromise = null
      }
    })()
    return this.refreshPromise
  }

  async makeAuthenticatedRequest(url, options = {}) {
    const token = await this.getToken()
    const headers = {
      'Content-Type': 'application/json',
      ...options.headers,
      Authorization: 'Bearer ' + token,
    }
    let response = await fetch(url, { ...options, headers })
    if (response.status === 401) {
      this.cachedToken = null
      const newToken = await this.getToken()
      headers.Authorization = 'Bearer ' + newToken
      response = await fetch(url, { ...options, headers })
    }
    return response
  }

  destroy() {
    this.cachedToken = null
    this.authContext = null
    this.initPromise = null
  }
}
