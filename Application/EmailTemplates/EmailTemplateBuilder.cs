using Application.Dto.EmailMessages.Requests;
using Application.Interfaces;

namespace Application.EmailTemplates;

public class EmailTemplateBuilder : IEmailTemplateBuilder
{
    public EmailContent BuildNewUserCredentials(string email, string password)
    {
        var html = $@"
            <h2>Данные для входа в систему BarsPnzSupportService</h2>
            <p>Логин: <b>{email}</b></p>
            <p>Пароль: <b>{password}</b></p>";

        return new EmailContent
        {
            Html = html,
            Subject = "Доступ к системе"
        };
    }

    public EmailContent BuildNewRequestStatus(string theme)
    {
        var html = $@"
            <h2>Статус по вашему обращению изменён</h2>
            <p>Тема: <b>{theme}</b></p>";

        return new EmailContent
        {
            Html = html,
            Subject = $"Заявка: {theme}"
        };
    }

    public EmailContent BuildResetPasswordCode(string email, string code)
    {
        var html = $@"
            <!DOCTYPE html>
            <html>
            <body style=""margin:0;padding:24px;background:#ffffff;font-family:'Segoe UI',Arial,sans-serif;color:#000000;"">
              <p style=""margin:0 0 24px;font-size:13px;color:#707070;"">Учетная запись BarsPnzSupportService</p>
              <h1 style=""margin:0 0 28px;font-size:28px;font-weight:400;color:#2F75B5;"">Код сброса пароля</h1>
              <p style=""margin:0 0 20px;font-size:15px;line-height:1.5;"">
                Используйте этот код, чтобы сбросить пароль для учетной записи BarsPnzSupportService {email}.
              </p>
              <p style=""margin:0 0 28px;font-size:15px;line-height:1.5;"">
                Вот ваш код: <b>{code}</b>
              </p>
              <p style=""margin:0;font-size:15px;line-height:1.5;"">С уважением,</p>
              <p style=""margin:4px 0 0;font-size:15px;line-height:1.5;"">Служба технической поддержки BarsPnzSupportService</p>
            </body>
            </html>";

        return new EmailContent
        {
            Html = html,
            Subject = "Код сброса пароля"
        };
    }
}