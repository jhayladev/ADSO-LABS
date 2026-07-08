using Microsoft.AspNetCore.Components.Server.Circuits;

namespace AdsoLabs.Web.Services
{

    public class ErrorCircuitHandler : CircuitHandler
    {
        public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken ct)
        {
            Console.WriteLine($"Circuito cerrado: {circuit.Id}");
            return base.OnCircuitClosedAsync(circuit, ct);
        }

        public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken ct)
        {
            Console.WriteLine($"Conexión caída: {circuit.Id}");
            return base.OnConnectionDownAsync(circuit, ct);
        }
    }
}
