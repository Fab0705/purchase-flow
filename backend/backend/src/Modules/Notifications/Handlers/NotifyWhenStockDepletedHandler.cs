using backend.Common.Contracts;
using backend.Modules.Notifications.Services;
using MediatR;

namespace backend.Modules.Notifications.Handlers
{
    public class NotifyWhenStockDepletedHandler : INotificationHandler<StockDepletedIntegrationEvent>
    {
        private readonly IEmailSender _emailSender;

        public NotifyWhenStockDepletedHandler(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public async Task Handle(StockDepletedIntegrationEvent notification, CancellationToken cancellationToken)
        {
            var subject = $"⚠️ ALERTA DE STOCK CRÍTICO";
            var body = $"Atención: El repuesto con ID {notification.SparePartId} ha caído a un nivel crítico de {notification.CurrentQuantity} unidades en el almacén.\n" +
                       $"Se requiere generar una solicitud de reabastecimiento urgente.";

            await _emailSender.SendEmailAsync("supervisor.logistica@natryx.com", subject, body, cancellationToken);
        }
    }
}
