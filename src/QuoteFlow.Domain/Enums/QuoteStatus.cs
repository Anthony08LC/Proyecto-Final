namespace QuoteFlow.Domain.Enums;

/// <summary>Estado del ciclo de vida de una cotización.</summary>
public enum QuoteStatus
{
    /// <summary>Borrador: editable, aún no emitida.</summary>
    Draft = 1,

    /// <summary>Emitida: con número consecutivo y datos congelados.</summary>
    Issued = 2,

    /// <summary>Aceptada por el cliente.</summary>
    Accepted = 3,

    /// <summary>Rechazada por el cliente.</summary>
    Rejected = 4,

    /// <summary>Vencida: superó su fecha de validez.</summary>
    Expired = 5,

    /// <summary>Anulada.</summary>
    Cancelled = 6
}
