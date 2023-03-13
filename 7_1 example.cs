//редактирование ролей доступа пользователей
private async Task OnUpdateRow(User user)
{
    var userFromDb = await UserManager.FindByIdAsync(user.Id);
    if (userFromDb == null)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = "Пользователь не найден",
            Detail = "В базе данных не найден редактируемый пользователь",
            Duration = 3000
        });
        return;
    }
    var existingRoles = await UserManager.GetRolesAsync(userFromDb);
    var rolesToDelete = user.Roles
        .Where(r => r.IsSelected == false && existingRoles.Any(x => x == r.Name))
        .Select(x => x.Name)
        .ToList();
    var rolesToAdd = user.Roles
        .Where(r => r.IsSelected == true && existingRoles.All(x => x != r.Name))
        .Select(x => x.Name)
        .ToList();
    try
    {
        await UserManager.RemoveFromRolesAsync(userFromDb, rolesToDelete);
        await UserManager.AddToRolesAsync(userFromDb, rolesToAdd);
        userFromDb.Email = user.Email;
        userFromDb.Name = user.Name;
        var result = await UserManager.UpdateAsync(userFromDb);
        if (result.Succeeded)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Пользователь обновлён",
                Detail = "Пользователь " + user.Name + " обновлён",
                Duration = 3000
            });
        }
        else
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Ошибка обновления пользователя",
                Detail = result.Errors.FirstOrDefault()?.Description,
                Duration = 3000
            });
        }
        await _usersGrid.Reload();
    }
    catch (Exception ex)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = "Ошибка обновления пользователя",
            Detail = ex.Message,
            Duration = 3000
        });
    }
}