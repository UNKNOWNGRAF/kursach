using System.Text;
using System.Text.Json;

var client = new HttpClient { BaseAddress = new Uri("http://localhost:7092/") };
string? currentToken = null;

Console.WriteLine("Добро пожаловать в консольное приложение «Шифр Виженера»");
Console.WriteLine("  О шифре Виженера:");
Console.WriteLine("Это метод полиалфавитного шифрования, использующий ключевое слово для сдвига букв исходного текста.");
Console.WriteLine("Поддерживаются русский (А-Я, а-я, Ёё) и английский (A-Z, a-z) алфавиты.");
Console.WriteLine("\n Пример:");
Console.WriteLine("Текст:    \"Привет Мир\"");
Console.WriteLine("Ключ:     \"СЕКРЕТ\"");
Console.WriteLine("Результат:  \"Иъщгшц Цщъ\"");
Console.WriteLine("\nНачните с регистрации или входа в аккаунт.\n");

while (true)
{
    Console.WriteLine("\n=== Консольный клиент шифрования ===");
    if (currentToken == null)
    {
        Console.WriteLine("1. Регистрация");
        Console.WriteLine("2. Вход");
        Console.WriteLine("0. Выход");
    }
    else
    {
        Console.WriteLine("3. Создать текст");
        Console.WriteLine("4. Просмотреть все тексты");
        Console.WriteLine("5. Просмотреть текст по ID");
        Console.WriteLine("6. Изменить текст");
        Console.WriteLine("7. Удалить текст");
        Console.WriteLine("8. Зашифровать текст");
        Console.WriteLine("9. Расшифровать текст");
        Console.WriteLine("10. История запросов");
        Console.WriteLine("11. Удалить запись истории по ID");
        Console.WriteLine("12. Очистить всю историю");
        Console.WriteLine("13. Изменить пароль");
        Console.WriteLine("14. Зашифровать текст (без сохранения)");
        Console.WriteLine("15. Расшифровать текст (без сохранения)");
        Console.WriteLine("16. Выйти из аккаунта");
    }

    Console.Write("Выберите действие: ");
    var choice = Console.ReadLine();

    try
    {
        switch (choice)
        {
            case "1" when currentToken == null:
                await RegisterAsync(client);
                break;
            case "2" when currentToken == null:
                currentToken = await LoginAsync(client);
                break;
            case "3" when currentToken != null:
                await CreateTextAsync(client, currentToken);
                break;
            case "4" when currentToken != null:
                await GetAllTextsAsync(client, currentToken);
                break;
            case "5" when currentToken != null:
                await GetTextByIdAsync(client, currentToken);
                break;
            case "6" when currentToken != null:
                await UpdateTextAsync(client, currentToken);
                break;
            case "7" when currentToken != null:
                await DeleteTextAsync(client, currentToken);
                break;
            case "8" when currentToken != null:
                await EncryptTextAsync(client, currentToken);
                break;
            case "9" when currentToken != null:
                await DecryptTextAsync(client, currentToken);
                break;
            case "10" when currentToken != null:
                await GetHistoryAsync(client, currentToken);
                break;
            case "11" when currentToken != null:
                await DeleteHistoryItemAsync(client, currentToken);
                break;
            case "12" when currentToken != null:
                await ClearHistoryAsync(client, currentToken);
                break;
            case "13" when currentToken != null:
                await ChangePasswordAsync(client, currentToken);
                break;
            case "14":
                await EncryptArbitraryTextAsync(client);
                break;
            case "15":
                await DecryptArbitraryTextAsync(client);
                break;
            case "16" when currentToken != null:
                currentToken = null;
                Console.Clear();
                Console.WriteLine("Вы вышли из аккаунта.");
                break;
            case "0":
                return;
            default:
                Console.WriteLine("Неверный выбор или действие недоступно.");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка: {ex.Message}");
    }
}

static async Task ChangePasswordAsync(HttpClient client, string token)
{
    Console.Write("Старый пароль: ");
    var oldPassword = ReadPassword();
    Console.Write("Новый пароль: ");
    var newPassword = ReadPassword();

    var request = new { OldPassword = oldPassword, NewPassword = newPassword };
    var json = JsonSerializer.Serialize(request);
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var response = await client.PatchAsync("api/auth/change-password", content);
    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("Пароль успешно изменён!");
    }
    else
    {
        var error = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Ошибка смены пароля: {error}");
    }
}

static async Task EncryptArbitraryTextAsync(HttpClient client)
{
    Console.Write("Текст для шифрования: ");
    var text = Console.ReadLine();
    Console.Write("Ключ шифрования (только буквы): ");
    var key = Console.ReadLine();

    var request = new { Text = text, Key = key };
    var json = JsonSerializer.Serialize(request);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    var response = await client.PostAsync("api/texts/encrypt-text", content);
    if (response.IsSuccessStatusCode)
    {
        var jsonResp = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(jsonResp);
        var result = doc.RootElement.GetProperty("result").GetString();
        Console.WriteLine($"Зашифрованный текст: {result}");
    }
    else
    {
        var error = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Ошибка: {error}");
    }
}

static async Task DecryptArbitraryTextAsync(HttpClient client)
{
    Console.Write("Текст для дешифрования: ");
    var text = Console.ReadLine();
    Console.Write("Ключ дешифрования (только буквы): ");
    var key = Console.ReadLine();

    var request = new { Text = text, Key = key };
    var json = JsonSerializer.Serialize(request);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    var response = await client.PostAsync("api/texts/decrypt-text", content);
    if (response.IsSuccessStatusCode)
    {
        var jsonResp = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(jsonResp);
        var result = doc.RootElement.GetProperty("result").GetString();
        Console.WriteLine($"Расшифрованный текст: {result}");
    }
    else
    {
        var error = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Ошибка: {error}");
    }
}

static async Task DeleteTextAsync(HttpClient client, string token)
{
    Console.Write("ID текста для удаления: ");
    if (int.TryParse(Console.ReadLine(), out int id))
    {
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var response = await client.DeleteAsync($"api/texts/{id}");
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Текст успешно удалён!");
        }
        else
        {
            Console.WriteLine("Не удалось удалить текст. Возможно, он не существует или не принадлежит вам.");
        }
    }
    else
    {
        Console.WriteLine("Неверный ID.");
    }
}

static async Task RegisterAsync(HttpClient client)
{
    Console.Write("Логин: ");
    var username = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(username))
    {
        Console.WriteLine("Ошибка: Логин не может быть пустым.");
        return;
    }
    Console.Write("Email: ");
    var email = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(email))
    {
        Console.WriteLine("Ошибка: Email не может быть пустым.");
        return;
    }

    Console.Write("Пароль: ");
    var password = ReadPassword();
    if (string.IsNullOrWhiteSpace(password))
    {
        Console.WriteLine("Ошибка: Пароль не может быть пустым.");
        return;
    }

    var request = new { Username = username, Email = email, Password = password };
    var json = JsonSerializer.Serialize(request);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    var response = await client.PostAsync("api/auth/register", content);
    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("Регистрация успешна!");
    }
    else
    {
        var error = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Ошибка регистрации: {error}");
    }
}

static async Task<string?> LoginAsync(HttpClient client)
{
    Console.Write("Логин: ");
    var username = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(username))
    {
        Console.WriteLine("Ошибка: Логин не может быть пустым.");
        return null;
    }
    Console.Write("Пароль: ");
    var password = ReadPassword();
    if (string.IsNullOrWhiteSpace(password))
    {
        Console.WriteLine("Ошибка: Пароль не может быть пустым.");
        return null;
    }

    var request = new { Username = username, Password = password };
    var json = JsonSerializer.Serialize(request);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    var response = await client.PostAsync("api/auth/login", content);
    if (response.IsSuccessStatusCode)
    {
        var jsonResp = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(jsonResp);
        var token = doc.RootElement.GetProperty("token").GetString();
        Console.WriteLine("Вход выполнен!");
        return token;
    }
    else
    {
        var error = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Ошибка входа: {error}");
        return null;
    }
}

static async Task CreateTextAsync(HttpClient client, string token)
{
    Console.Write("Заголовок (можно оставить пустым): ");
    var title = Console.ReadLine();
    Console.Write("Текст: ");
    var content = Console.ReadLine();
    Console.Write("Зашифровать сразу? (y/n): ");
    var encryptNow = Console.ReadLine()?.ToLower() == "y";
    string? key = null;
    if (encryptNow)
    {
        Console.Write("Ключ шифрования (только буквы, Enter — без ключа(Ключ по умолчанию: для RU — СЕКРЕТНЫЙКЛЮЧ, для EN — SECRETKEY)): ");
        key = Console.ReadLine();
    }
    var request = new
    {
        Title = title,
        Content = content,
        EncryptNow = encryptNow,
        Key = key
    };
    var json = JsonSerializer.Serialize(request);
    var contentHttp = new StringContent(json, Encoding.UTF8, "application/json");
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var response = await client.PostAsync("api/texts", contentHttp);
    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("Текст создан!");
    }
    else
    {
        var error = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Ошибка: {error}");
    }
}

static async Task GetAllTextsAsync(HttpClient client, string token)
{
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    var response = await client.GetAsync("api/texts");
    if (response.IsSuccessStatusCode)
    {
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var texts = doc.RootElement.EnumerateArray();
        if (!texts.Any())
        {
            Console.WriteLine("У вас нет текстов.");
            return;
        }
        foreach (var text in texts)
        {
            var id = text.GetProperty("id").GetInt32();
            var title = text.GetProperty("title").GetString();
            var isEncrypted = text.GetProperty("isEncrypted").GetBoolean();
            Console.WriteLine($"ID: {id}, Заголовок: {title}, Зашифрован: {isEncrypted}");
        }
    }
    else
    {
        Console.WriteLine("Не удалось загрузить тексты.");
    }
}

static async Task GetTextByIdAsync(HttpClient client, string token)
{
    Console.Write("ID текста: ");
    if (int.TryParse(Console.ReadLine(), out int id))
    {
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var response = await client.GetAsync($"api/texts/{id}");
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var title = doc.RootElement.GetProperty("title").GetString();
            var content = doc.RootElement.GetProperty("content").GetString();
            var isEncrypted = doc.RootElement.GetProperty("isEncrypted").GetBoolean();
            var encryptCon = doc.RootElement.GetProperty("encryptCon").GetString();

            Console.WriteLine($"\n--- Текст ---");
            Console.WriteLine($"Заголовок: {title}");
            Console.WriteLine($"Содержимое: {content}");
            if (isEncrypted) Console.WriteLine($"Зашифрованная версия: {encryptCon}");
            Console.WriteLine($"Статус: {(isEncrypted ? "зашифрован" : "обычный")}");
        }
        else
        {
            Console.WriteLine("Текст не найден.");
        }
    }
}

static async Task UpdateTextAsync(HttpClient client, string token)
{
    Console.Write("ID текста: ");
    if (int.TryParse(Console.ReadLine(), out int id))
    {
        Console.Write("Новый заголовок (Enter — пропустить): ");
        var title = Console.ReadLine();
        Console.Write("Новое содержимое (Enter — пропустить): ");
        var content = Console.ReadLine();

        var request = new
        {
            Title = string.IsNullOrWhiteSpace(title) ? null : title,
            Content = string.IsNullOrWhiteSpace(content) ? null : content
        };

        var json = JsonSerializer.Serialize(request);
        var contentHttp = new StringContent(json, Encoding.UTF8, "application/json");
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await client.PatchAsync($"api/texts/{id}", contentHttp);
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Текст обновлён!");
        }
        else
        {
            Console.WriteLine("Не удалось обновить текст.");
        }
    }
}

static async Task EncryptTextAsync(HttpClient client, string token)
{
    Console.Write("ID текста: ");
    if (int.TryParse(Console.ReadLine(), out int id))
    {
        Console.Write("Ключ шифрования (только буквы, Enter — без ключа(Ключ по умолчанию: для RU — СЕКРЕТНЫЙКЛЮЧ, для EN — SECRETKEY)): ");
        var key = Console.ReadLine();

        var request = new { Key = key };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsync($"api/texts/{id}/encrypt", content);
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Текст зашифрован!");
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Ошибка: {error}");
        }
    }
}

static async Task DecryptTextAsync(HttpClient client, string token)
{
    Console.Write("ID текста: ");
    if (int.TryParse(Console.ReadLine(), out int id))
    {
        Console.Write("Ключ дешифрования (только буквы, Enter — без ключа): ");
        var key = Console.ReadLine();

        var request = new { Key = key };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsync($"api/texts/{id}/decrypt", content);
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Текст расшифрован!");
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Ошибка: {error}");
        }
    }
}

static async Task GetHistoryAsync(HttpClient client, string token)
{
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    var response = await client.GetAsync("api/history");
    if (response.IsSuccessStatusCode)
    {
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var history = doc.RootElement.EnumerateArray();
        if (!history.Any())
        {
            Console.WriteLine("История пуста.");
            return;
        }
        foreach (var entry in history)
        {
            var endpoint = entry.GetProperty("endpoint").GetString();
            var method = entry.GetProperty("method").GetString();
            var createdAt = entry.GetProperty("createdAt").GetString();
            Console.WriteLine($"{createdAt}: {method} {endpoint}");
        }
    }
    else
    {
        Console.WriteLine("Не удалось загрузить историю.");
    }
}

static async Task DeleteHistoryItemAsync(HttpClient client, string token)
{
    Console.Write("ID записи истории для удаления: ");
    if (int.TryParse(Console.ReadLine(), out int id))
    {
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var response = await client.DeleteAsync($"api/history/{id}");
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Запись истории удалена!");
        }
        else
        {
            Console.WriteLine("Не удалось удалить запись. Возможно, она не существует или не принадлежит вам.");
        }
    }
    else
    {
        Console.WriteLine("Неверный ID.");
    }
}

static async Task ClearHistoryAsync(HttpClient client, string token)
{
    Console.Write("Вы уверены, что хотите очистить всю историю? (y/n): ");
    if (Console.ReadLine()?.ToLower() == "y")
    {
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var response = await client.DeleteAsync("api/history");
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("История запросов очищена!");
        }
        else
        {
            Console.WriteLine("Не удалось очистить историю.");
        }
    }
}

static string ReadPassword()
{
    var password = new StringBuilder();
    while (true)
    {
        var key = Console.ReadKey(intercept: true);
        if (key.Key == ConsoleKey.Enter) break;
        if (key.Key == ConsoleKey.Backspace && password.Length > 0)
        {
            password.Remove(password.Length - 1, 1);
            Console.Write("\b \b");
        }
        else if (key.KeyChar != '\u0000')
        {
            password.Append(key.KeyChar);
            Console.Write("*");
        }
    }
    Console.WriteLine();
    return password.ToString();
}