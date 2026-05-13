namespace FloraLink.Application.Interfaces;

public interface IEmailService
{
    Task SendPlantAlertAsync(string toEmail, string ownerName, string plantName, string message, string severity);
}
