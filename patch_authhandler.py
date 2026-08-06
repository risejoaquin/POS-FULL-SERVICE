import re

with open('./PosCore/Services/AuthDelegatingHandler.cs', 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace(
'''        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            _sessionManager.ClearSession();
            System.Windows.Application.Current.Dispatcher.Invoke(() =>''',
'''        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            if (request.RequestUri != null && request.RequestUri.AbsolutePath.EndsWith("login", System.StringComparison.OrdinalIgnoreCase))
            {
                return response;
            }

            _sessionManager.ClearSession();
            System.Windows.Application.Current.Dispatcher.Invoke(() =>''')

with open('./PosCore/Services/AuthDelegatingHandler.cs', 'w', encoding='utf-8') as f:
    f.write(content)
