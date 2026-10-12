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
}