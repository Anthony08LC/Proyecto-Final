namespace QuoteFlow.Application.Abstractions.Messaging;

/// <summary>Comando que modifica el estado del sistema y no devuelve datos.</summary>
public interface ICommand;

/// <summary>Comando que modifica el estado del sistema y devuelve un resultado (por ejemplo, el id creado).</summary>
public interface ICommand<TResponse>;
