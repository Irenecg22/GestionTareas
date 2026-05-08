using System;
using System.Collections.Generic;
using System.Text;
namespace GestionTareas.Services;
public static class ApiConfig
{
    public static string BaseUrl
    {
        get
        {
#if ANDROID
            return "http://10.0.2.2:8000";
#else
            return "http://127.0.0.1:8000";
#endif
        }
    }
}
