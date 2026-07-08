namespace AdsoLabs.Application.Interfaces.Services;
public interface IEmailServices
{

    Task EnviarAsync(string correoDestino, string asunto, string cuerpoHtml);

}