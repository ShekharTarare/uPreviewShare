import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification'

export class NotificationHelper {
  static async showSuccess(context, message) {
    try {
      const nc = await context.getContext(UMB_NOTIFICATION_CONTEXT)
      nc.peek('positive', { data: { message } })
    } catch {}
  }
  static async showError(context, message) {
    try {
      const nc = await context.getContext(UMB_NOTIFICATION_CONTEXT)
      nc.peek('danger', { data: { message } })
    } catch {}
  }
  static async showWarning(context, message) {
    try {
      const nc = await context.getContext(UMB_NOTIFICATION_CONTEXT)
      nc.peek('warning', { data: { message } })
    } catch {}
  }
  static async showInfo(context, message) {
    try {
      const nc = await context.getContext(UMB_NOTIFICATION_CONTEXT)
      nc.peek('default', { data: { message } })
    } catch {}
  }
}
