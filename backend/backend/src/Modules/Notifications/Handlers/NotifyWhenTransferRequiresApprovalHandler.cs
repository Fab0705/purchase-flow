using backend.Common.Contracts;
using backend.Modules.Notifications.Services;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace backend.Modules.Notifications.Handlers
{
    public class NotifyWhenTransferRequiresApprovalHandler : INotificationHandler<TransferApprovalRequiredIntegrationEvent>
    {
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        public NotifyWhenTransferRequiresApprovalHandler(IEmailSender emailSender, IConfiguration configuration)
        {
            _emailSender = emailSender;
            _configuration = configuration;
        }
        public async Task Handle(TransferApprovalRequiredIntegrationEvent notification, CancellationToken cancellationToken)
        {
            var subject = $"🟡 APROBACIÓN REQUERIDA: Traslado {notification.TrackingCode}";

            var body = $"Estimado Jefe de Logística,\n\n" +
                       $"Se ha registrado una nueva solicitud de traslado de inventario que requiere su aprobación para proceder con el despacho.\n\n" +
                       $"📝 DETALLES DEL TRASLADO:\n" +
                       $"- Código: {notification.TrackingCode}\n" +
                       $"- Origen: {notification.OriginLocationName}\n" +
                       $"- Destino: {notification.DestinationLocationName}\n" +
                       $"- Solicitado por: {notification.RequestedBy}\n\n" +
                       $"Por favor, ingrese al sistema backend para revisar los repuestos solicitados y emitir su aprobación o rechazo.";

            // Leemos el correo del Jefe de Logística desde los User Secrets o AppSettings
            var managerEmail = _configuration["Notifications:LogisticsManagerEmail"]
                ?? throw new InvalidOperationException("El correo del Jefe de Logística no está configurado.");

            await _emailSender.SendEmailAsync(managerEmail, subject, body);
        }
    }
}
