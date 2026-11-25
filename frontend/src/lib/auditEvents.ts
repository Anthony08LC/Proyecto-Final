import { quoteEventLabel } from './quoteStatus'

/** Nombres en español de los eventos de auditoría (spec 10.3). */
const EVENT_LABELS: Record<string, string> = {
  LoginSucceeded: 'Inicio de sesión',
  LoginFailed: 'Inicio de sesión fallido',
  UserCreated: 'Usuario creado',
  UserUpdated: 'Usuario actualizado',
  UserRoleChanged: 'Rol de usuario cambiado',
  UserDeactivated: 'Usuario desactivado',
  UserReactivated: 'Usuario reactivado',
  UserPasswordChanged: 'Contraseña cambiada',
  UserPasswordReset: 'Contraseña restablecida',
  CustomerCreated: 'Cliente registrado',
  CustomerUpdated: 'Cliente actualizado',
  CustomerVipChanged: 'Condición VIP cambiada',
  CustomerDeactivated: 'Cliente desactivado',
  CustomerReactivated: 'Cliente reactivado',
  CategoryCreated: 'Categoría creada',
  CategoryUpdated: 'Categoría actualizada',
  CategoryDeactivated: 'Categoría desactivada',
  CategoryReactivated: 'Categoría reactivada',
  ProductCreated: 'Producto creado',
  ProductUpdated: 'Producto actualizado',
  ProductPriceChanged: 'Precio de producto cambiado',
  ProductDeactivated: 'Producto desactivado',
  ProductReactivated: 'Producto reactivado',
  SettingsUpdated: 'Configuración actualizada',
}

export function auditEventLabel(eventType: string): string {
  if (eventType.startsWith('Quote')) {
    const label = quoteEventLabel(eventType)
    return label === eventType ? eventType : `Cotización: ${label.toLowerCase()}`
  }
  return EVENT_LABELS[eventType] ?? eventType
}
